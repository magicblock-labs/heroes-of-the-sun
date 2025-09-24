using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Model;
using UnityEngine;
using UnityEngine.Rendering;
using Utils.Injection;

namespace View.Exploration
{
    public class RenderRandomChunk : InjectableBehaviour
    {
        [Header("Tile meshes")] public Mesh[] tileMeshes;

        [Header("Shared material (Enable GPU Instancing = ON)")]
        public Material instancedMat;

        [SerializeField] private Transform tileContainer;
        [SerializeField] private RenderTile tilePrefab;

        [SerializeField] private MeshFilter waterMesh;
        [Inject] private PathfindingModel _pathfinder;

        // === Chunked visibility (no-GC per frame) ===
        private const int ChunkSize = 16;                       // tiles per chunk edge
        private const int InitialVisibleCapacity = 4096;        // tweak as needed

        private struct Chunk
        {
            public Vector3 center;           // world-space center
            public Vector3 halfExtents;      // world-space half extents (x,z from chunk size; y from min..max tile heights)
            public List<int> meshIdx;        // per-instance mesh index
            public List<Vector3> pos;        // per-instance world position
        }

        private Dictionary<Vector2Int, Chunk> _chunks;          // chunk grid
        private bool _chunksBuilt;

        // Reusable visible buffers (no ToArray per frame)
        private int[] _visibleMeshIdx;
        private Vector3[] _visiblePos;
        private int _visibleCount;

        // Reusable math helpers
        private static readonly Vector3 WorldRight = Vector3.right;
        private static readonly Vector3 WorldUp = Vector3.up;
        private static readonly Vector3 WorldForward = Vector3.forward;

        const int BatchSize = 1023; // Unity hard cap
        private Matrix4x4[][] _batchBuffers; // [meshIndex][0..1022]
        private int[] _batchCounts; // how many filled in current buffer per mesh
        private MaterialPropertyBlock _mpb;

        protected override void Awake()
        {
            base.Awake();

            if (instancedMat == null)
            {
                Debug.LogError("Assign instancedMat");
                enabled = false;
                return;
            }

            if (tileMeshes == null || tileMeshes.Length == 0)
            {
                Debug.LogError("Assign tileMeshes");
                enabled = false;
                return;
            }

            instancedMat.enableInstancing = true;

            // Preallocate one fixed buffer per mesh (no per-frame allocs)
            _batchBuffers = new Matrix4x4[tileMeshes.Length][];
            _batchCounts = new int[tileMeshes.Length];

            for (int i = 0; i < tileMeshes.Length; i++)
                _batchBuffers[i] = new Matrix4x4[BatchSize]; // reused forever

            _mpb = new MaterialPropertyBlock(); // reuse
            _cam = Camera.main;

            _visibleMeshIdx = new int[InitialVisibleCapacity];
            _visiblePos = new Vector3[InitialVisibleCapacity];
            _visibleCount = 0;
            _chunks = new Dictionary<Vector2Int, Chunk>(256);
            _chunksBuilt = false;
        }

        public void Create(Vector2Int offset, int size, Vector2 scale, bool instant)
        {
            foreach (Transform child in tileContainer)
                Destroy(child.gameObject);


            GenerateWaterMesh(size, size * ConfigModel.CellSize);

            StartCoroutine(GenerateTiles(offset, size, scale, instant));

            _chunksBuilt = false; // tiles changed; rebuild chunks when ready
        }

        private void GenerateWaterMesh(int subdivisions, int scale)
        {
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            for (var y = 0; y < subdivisions; y++)
            for (var x = 0; x < subdivisions; x++)
            {
                var tx = x / (float)(subdivisions - 1);
                var ty = y / (float)(subdivisions - 1);
                vertices.Add(new Vector3(tx, 0, ty) * scale);
                normals.Add(Vector3.up);
                uvs.Add(new Vector2(tx, ty));
            }

            var indices = new List<int>();
            for (var y = 0; y < subdivisions - 1; y++)
            for (var x = 0; x < subdivisions - 1; x++)
            {
                var quad = y * subdivisions + x;

                indices.Add(quad);
                indices.Add(quad + subdivisions);
                indices.Add(quad + subdivisions + 1);

                indices.Add(quad);
                indices.Add(quad + subdivisions + 1);
                indices.Add(quad + 1);
            }

            waterMesh.sharedMesh = new Mesh
            {
                vertices = vertices.ToArray(),
                triangles = indices.ToArray(),
                uv = uvs.ToArray(),
                normals = normals.ToArray()
            };
        }

        private IEnumerator GenerateTiles(Vector2Int offset, int size, Vector2 scale, bool instant)
        {
            _tileMap = new Dictionary<int, List<Vector3>>();

            for (var x = 0; x < size; x++)
            for (var y = 0; y < size; y++)
            {
                var sampleX = (float)(x + offset.x) / size * scale.x;
                var sampleY = (float)(y + offset.y) / size * scale.y;

                var perlinNoiseSample = Mathf.PerlinNoise(sampleX, sampleY);
                var position = new Vector2Int(x, y);
                var tile = Instantiate(tilePrefab, tileContainer).Create(
                    offset,
                    position);


                var yPos = (int)(perlinNoiseSample * tileMeshes.Length * 4) - 6.5f;

                _pathfinder.AddPoint(tile.Location, yPos);

                var tileIndex = Mathf.Min((int)(tileMeshes.Length * perlinNoiseSample), tileMeshes.Length - 1);

                tile.transform.localScale = Vector3.one * 2;
                tile.transform.localPosition = ConfigModel.GetWorldCellPosition(position.x, position.y) +
                                               Vector3.up * yPos;

                if (!_tileMap.ContainsKey(tileIndex))
                    _tileMap.Add(tileIndex, new List<Vector3>());

                _tileMap[tileIndex].Add(ConfigModel.GetWorldCellPosition(position.x + offset.x, position.y + offset.y) +
                                        Vector3.up * yPos);

                //edge fall
                if (x == 0 || y == 0 || x == size - 1 || y == size - 1)
                {
                    for (var i = 1; i < yPos; i++)
                        _tileMap[tileIndex]
                            .Add(ConfigModel.GetWorldCellPosition(position.x + offset.x, position.y + offset.y) +
                                 Vector3.up * (yPos - i));
                }

                if (!instant)
                    yield return null;
            }

            BuildChunksFromTileMap();
            UpdateBatchData();
        }

        // Partition flat _tileMap (meshIndex -> positions) into spatial chunks to avoid full scans each frame
        private void BuildChunksFromTileMap()
        {
            _chunks.Clear();

            // Derive chunk world size from ConfigModel.CellSize
            float cell = ConfigModel.CellSize;
            float chunkWorld = ChunkSize * cell;

            // temp per-chunk min/max Y to compute vertical extents
            var minY = new Dictionary<Vector2Int, float>(128);
            var maxY = new Dictionary<Vector2Int, float>(128);

            foreach (var (tileIndex, tilePositions) in _tileMap)
            {
                for (int i = 0; i < tilePositions.Count; i++)
                {
                    Vector3 p = tilePositions[i];
                    int cx = Mathf.FloorToInt(p.x / chunkWorld);
                    int cz = Mathf.FloorToInt(p.z / chunkWorld);
                    var key = new Vector2Int(cx, cz);

                    if (!_chunks.TryGetValue(key, out var chunk))
                    {
                        chunk = new Chunk
                        {
                            meshIdx = new List<int>(256),
                            pos = new List<Vector3>(256)
                        };
                        _chunks.Add(key, chunk);
                        minY[key] = p.y;
                        maxY[key] = p.y;
                    }
                    else
                    {
                        if (p.y < minY[key]) minY[key] = p.y;
                        if (p.y > maxY[key]) maxY[key] = p.y;
                    }

                    _chunks[key].meshIdx.Add(tileIndex);
                    _chunks[key].pos.Add(p);
                }
            }

            // finalize bounds for each chunk (snapshot keys to avoid modifying during enumeration)
            var keys = new List<Vector2Int>(_chunks.Keys);
            for (int i = 0; i < keys.Count; i++)
            {
                var key = keys[i];
                var c = _chunks[key];
                float y0 = minY[key];
                float y1 = maxY[key];
                float ex = chunkWorld * 0.5f;
                float ez = chunkWorld * 0.5f;
                float ey = Mathf.Max(0.5f, (y1 - y0) * 0.5f);

                // world-space center of this chunk footprint (XZ), Y at middle of min..max
                Vector3 center = new Vector3((key.x + 0.5f) * chunkWorld, y0 + ey, (key.y + 0.5f) * chunkWorld);

                c.center = center;
                c.halfExtents = new Vector3(ex, ey, ez);
                _chunks[key] = c; // write back struct
            }

            _chunksBuilt = true;
        }

        private void UpdateBatchData()
        {
            if (_tileMap == null) return;

            // Rebuild chunks once after generation if needed
            if (!_chunksBuilt)
            {
                BuildChunksFromTileMap();
            }

            _visibleCount = 0; // reset write cursor

            // Camera half extents in local camera space
            float halfH = _cam.orthographicSize;
            float halfW = _cam.orthographicSize * _cam.aspect;
            float zNear = _cam.nearClipPlane;
            float zFar = _cam.farClipPlane;

            // Iterate only intersecting chunks
            foreach (var kv in _chunks)
            {
                var c = kv.Value;
                if (!ChunkIntersectsCameraBox(_cam, c.center, c.halfExtents, halfW, halfH, zNear, zFar))
                    continue;

                // Narrow phase: per-instance test using fast ortho check
                var m = c.meshIdx;
                var p = c.pos;
                int n = p.Count;
                for (int i = 0; i < n; i++)
                {
                    Vector3 wp = p[i];
                    if (!IsInOrthoCamBox(_cam, wp))
                        continue;

                    // ensure capacity
                    if (_visibleCount >= _visiblePos.Length)
                    {
                        GrowVisibleBuffers(_visibleCount << 1);
                    }

                    _visiblePos[_visibleCount] = wp;
                    _visibleMeshIdx[_visibleCount] = m[i];
                    _visibleCount++;
                }
            }

            _count = _visibleCount;
            _tileIndexes = _visibleMeshIdx; // expose buffers directly (no copies)
            _positions = _visiblePos;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void GrowVisibleBuffers(int newCapacity)
        {
            int cap = Mathf.Max(newCapacity, _visiblePos.Length * 2);
            Array.Resize(ref _visiblePos, cap);
            Array.Resize(ref _visibleMeshIdx, cap);
        }

        // Conservative and GC-free test of chunk OBB vs camera-aligned ortho box.
        // Projects the world-aligned chunk extents onto the camera axes and checks interval overlap.
        private static bool ChunkIntersectsCameraBox(Camera cam, Vector3 center, Vector3 halfExtents,
                                                     float camHalfW, float camHalfH, float zNear, float zFar)
        {
            // Transform chunk center to camera local space
            Vector3 lc = cam.transform.InverseTransformPoint(center);

            // Project world-aligned extents onto camera axes (absolute dot products)
            Vector3 rAxisX = cam.transform.right;
            Vector3 rAxisY = cam.transform.up;
            Vector3 rAxisZ = cam.transform.forward;

            // Chunk is axis-aligned in world (x,y,z). Its half extents in world are hx, hy, hz.
            float hx = halfExtents.x, hy = halfExtents.y, hz = halfExtents.z;

            float rx = Mathf.Abs(Vector3.Dot(rAxisX, WorldRight)) * hx +
                       Mathf.Abs(Vector3.Dot(rAxisX, WorldUp))    * hy +
                       Mathf.Abs(Vector3.Dot(rAxisX, WorldForward)) * hz;

            float ry = Mathf.Abs(Vector3.Dot(rAxisY, WorldRight)) * hx +
                       Mathf.Abs(Vector3.Dot(rAxisY, WorldUp))    * hy +
                       Mathf.Abs(Vector3.Dot(rAxisY, WorldForward)) * hz;

            float rz = Mathf.Abs(Vector3.Dot(rAxisZ, WorldRight)) * hx +
                       Mathf.Abs(Vector3.Dot(rAxisZ, WorldUp))    * hy +
                       Mathf.Abs(Vector3.Dot(rAxisZ, WorldForward)) * hz;

            // Interval overlap tests in camera local space
            if (lc.x + rx < -camHalfW || lc.x - rx > camHalfW) return false;
            if (lc.y + ry < -camHalfH || lc.y - ry > camHalfH) return false;
            if (lc.z + rz < zNear     || lc.z - rz > zFar)     return false;
            return true;
        }

        // Returns true if a world position is inside the ortho camera's box (with optional padding in world units).
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsInOrthoCamBox(Camera cam, Vector3 worldPos, float paddingX = 0f, float paddingY = 0f)
        {
            // Move point into the camera's local space (camera at origin, looking +Z).
            Vector3 p = cam.transform.InverseTransformPoint(worldPos);

            float halfH = cam.orthographicSize + paddingY;
            float halfW = cam.orthographicSize * cam.aspect + paddingX;

            // x/y within the orthographic rectangle, z within clip range
            return p.x >= -halfW && p.x <= halfW &&
                   p.y >= -halfH && p.y <= halfH &&
                   p.z >= cam.nearClipPlane && p.z <= cam.farClipPlane;
        }

        private int _count;
        private int[] _tileIndexes;
        private Vector3[] _positions;
        private Camera _cam;
        private Dictionary<int, List<Vector3>> _tileMap;

        private void Update()
        {
            UpdateBatchData();

            Render(_positions, _tileIndexes, _count);
        }

        /// Render all visible tiles for this frame with zero GC.
        /// positions: world positions of tiles
        /// meshIndices: which mesh (index into tileMeshes) each tile uses
        /// count: number of tiles to render (use <= positions.Length and meshIndices.Length)
        public void Render(Vector3[] positions, int[] meshIndices, int count)
        {
            if (positions == null || meshIndices == null) return;
            if (count <= 0) return;

            // STREAM tiles → fill per-mesh buffers; draw when full.
            for (int i = 0; i < count; i++)
            {
                int mi = meshIndices[i];
                // (Optional) bounds check in dev builds
#if UNITY_EDITOR
                if ((uint)mi >= (uint)tileMeshes.Length)
                {
                    Debug.LogWarning($"meshIndex {mi} out of range");
                    continue;
                }
#endif
                int c = _batchCounts[mi];
                _batchBuffers[mi][c] = Matrix4x4.TRS(positions[i], Quaternion.identity, Vector3.one * 2);
                c++;

                if (c == BatchSize)
                {
                    DrawBatch(mi, BatchSize);
                    c = 0; // reset buffer
                }

                _batchCounts[mi] = c;
            }

            // FLUSH leftovers for each mesh
            for (int mi = 0; mi < tileMeshes.Length; mi++)
            {
                int c = _batchCounts[mi];
                if (c > 0)
                {
                    DrawBatch(mi, c);
                    _batchCounts[mi] = 0;
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough]
        private void DrawBatch(int meshIndex, int count)
        {
            Graphics.DrawMeshInstanced(
                tileMeshes[meshIndex], 0, instancedMat,
                _batchBuffers[meshIndex], count, _mpb,
                ShadowCastingMode.Off, /*receiveShadows*/ false,
                0, /*layer*/ _cam, LightProbeUsage.Off, /*lppv*/ null);
        }
    }
}