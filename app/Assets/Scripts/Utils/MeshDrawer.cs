using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Model;
using UnityEngine;
using UnityEngine.Rendering;

namespace Utils
{
    public class MeshDrawer
    {
        public void Init(Mesh[] tileMeshes, Material instancedMat)
        {
            _tileMeshes = tileMeshes;
            _instancedMat = instancedMat;
            
            // Preallocate one fixed buffer per mesh (no per-frame allocs)
            _batchBuffers = new Matrix4x4[_tileMeshes.Length][];
            _batchCounts = new int[_tileMeshes.Length];

            for (int i = 0; i < _tileMeshes.Length; i++)
                _batchBuffers[i] = new Matrix4x4[BatchSize]; // reused forever

            _mpb = new MaterialPropertyBlock(); // reuse
            _cam = Camera.main;

            _visibleMeshIdx = new int[InitialVisibleCapacity];
            _visiblePos = new Vector3[InitialVisibleCapacity];
            _visibleCount = 0;
            _chunks = new Dictionary<Vector2Int, Chunk>(256);
            Invalidate();
        }

        public void Invalidate()
        {
            _chunksBuilt = false;
        }


        // === Chunked visibility (no-GC per frame) ===
        private const int ChunkSize = 16; // tiles per chunk edge
        private const int InitialVisibleCapacity = 4096; // tweak as needed

        private struct Chunk
        {
            public Vector3 Center; // world-space center
            public Vector3 HalfExtents; // world-space half extents (x,z from chunk size; y from min..max tile heights)
            public List<int> MeshIdx; // per-instance mesh index
            public List<Vector3> Pos; // per-instance world position
        }

        private Dictionary<Vector2Int, Chunk> _chunks; // chunk grid
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

        // Partition flat _tileMap (meshIndex -> positions) into spatial chunks to avoid full scans each frame
        public void BuildChunksFromTileMap(Dictionary<int, List<Vector3>> tileMap)
        {
            _tileMap = tileMap;
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
                            MeshIdx = new List<int>(256),
                            Pos = new List<Vector3>(256)
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

                    _chunks[key].MeshIdx.Add(tileIndex);
                    _chunks[key].Pos.Add(p);
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

                c.Center = center;
                c.HalfExtents = new Vector3(ex, ey, ez);
                _chunks[key] = c; // write back struct
            }

            _chunksBuilt = true;
        }

        public void UpdateBatchData()
        {
            if (_tileMap == null) return;

            // Rebuild chunks once after generation if needed
            if (!_chunksBuilt)
            {
                BuildChunksFromTileMap(_tileMap);
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
                if (!ChunkIntersectsCameraBox(_cam, c.Center, c.HalfExtents, halfW, halfH, zNear, zFar))
                    continue;

                // Narrow phase: per-instance test using fast ortho check
                var m = c.MeshIdx;
                var p = c.Pos;
                int n = p.Count;
                for (int i = 0; i < n; i++)
                {
                    Vector3 wp = p[i];
                    if (!IsInOrthoCamBox(_cam, wp, 50, 50))
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
            _meshIndices = _visibleMeshIdx; // expose buffers directly (no copies)
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
                       Mathf.Abs(Vector3.Dot(rAxisX, WorldUp)) * hy +
                       Mathf.Abs(Vector3.Dot(rAxisX, WorldForward)) * hz;

            float ry = Mathf.Abs(Vector3.Dot(rAxisY, WorldRight)) * hx +
                       Mathf.Abs(Vector3.Dot(rAxisY, WorldUp)) * hy +
                       Mathf.Abs(Vector3.Dot(rAxisY, WorldForward)) * hz;

            float rz = Mathf.Abs(Vector3.Dot(rAxisZ, WorldRight)) * hx +
                       Mathf.Abs(Vector3.Dot(rAxisZ, WorldUp)) * hy +
                       Mathf.Abs(Vector3.Dot(rAxisZ, WorldForward)) * hz;

            // Interval overlap tests in camera local space
            if (lc.x + rx < -camHalfW || lc.x - rx > camHalfW) return false;
            if (lc.y + ry < -camHalfH || lc.y - ry > camHalfH) return false;
            if (lc.z + rz < zNear || lc.z - rz > zFar) return false;
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
        private int[] _meshIndices;
        private Vector3[] _positions;
        private Camera _cam;
        private Dictionary<int, List<Vector3>> _tileMap;
        private Mesh[] _tileMeshes;
        private Material _instancedMat;

        /// Render all visible tiles for this frame with zero GC.
        /// positions: world positions of tiles
        /// meshIndices: which mesh (index into tileMeshes) each tile uses
        /// count: number of tiles to render (use <= positions.Length and meshIndices.Length)
        public void Render(float tileScale = 2f)
        {
            if (_positions == null || _meshIndices == null) return;
            if (_count <= 0) return;

            // STREAM tiles → fill per-mesh buffers; draw when full.
            for (int i = 0; i < _count; i++)
            {
                int mi = _meshIndices[i];
                // (Optional) bounds check in dev builds
#if UNITY_EDITOR
                if ((uint)mi >= (uint)_tileMeshes.Length)
                {
                    Debug.LogWarning($"meshIndex {mi} out of range");
                    continue;
                }
#endif
                int c = _batchCounts[mi];
                _batchBuffers[mi][c] = Matrix4x4.TRS(_positions[i], Quaternion.identity, Vector3.one * tileScale);
                c++;

                if (c == BatchSize)
                {
                    DrawBatch(mi, BatchSize);
                    c = 0; // reset buffer
                }

                _batchCounts[mi] = c;
            }

            // FLUSH leftovers for each mesh
            for (int mi = 0; mi < _tileMeshes.Length; mi++)
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
                _tileMeshes[meshIndex], 0, _instancedMat,
                _batchBuffers[meshIndex], count, _mpb,
                ShadowCastingMode.Off, /*receiveShadows*/ false,
                0, /*layer*/ _cam, LightProbeUsage.Off, /*lppv*/ null);
        }

    }
}