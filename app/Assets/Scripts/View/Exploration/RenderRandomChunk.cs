using System;
using System.Collections;
using System.Collections.Generic;
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
        }

        public void Create(Vector2Int offset, int size, Vector2 scale, bool instant)
        {
            foreach (Transform child in tileContainer)
                Destroy(child.gameObject);


            GenerateWaterMesh(size, size * ConfigModel.CellSize);

            StartCoroutine(GenerateTiles(offset, size, scale, instant));
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
            var tileMap = new Dictionary<int, List<Vector3>>();
            
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
                
                if (!tileMap.ContainsKey(tileIndex))
                    tileMap.Add(tileIndex, new List<Vector3>());
                
                tileMap[tileIndex].Add(ConfigModel.GetWorldCellPosition(position.x + offset.x, position.y + offset.y) + Vector3.up * yPos);

                //edge fall
                if (x == 0 || y == 0 || x == size - 1 || y == size - 1)
                {
                    for (var i = 1; i < yPos; i++)
                        tileMap[tileIndex].Add(ConfigModel.GetWorldCellPosition(position.x + offset.x, position.y + offset.y) + Vector3.up * (yPos - i));

                }
                
                if (!instant)
                    yield return null;
            }

            var tileIndexes = new List<int>();
            var positions = new List<Vector3>();
            foreach (var (tileIndex, tilePositions) in tileMap)
            {
                foreach (var position in tilePositions)
                {
                    tileIndexes.Add(tileIndex);
                    positions.Add(position);
                }
            }
            
            _count = tileIndexes.Count;
            _tileIndexes = tileIndexes.ToArray();
            _positions = positions.ToArray();
        }

        private int _count;
        private int[] _tileIndexes;
        private Vector3[] _positions;

        private void Update()
        {
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
            // Note: we pass the preallocated array and a 'count' (no copies, no allocs).
            Graphics.DrawMeshInstanced(
                tileMeshes[meshIndex], 0, instancedMat,
                _batchBuffers[meshIndex], count, _mpb,
                ShadowCastingMode.Off, /*receiveShadows*/ false,
                0, /*camera*/ null, LightProbeUsage.Off, /*lppv*/ null);
        }
    }
}