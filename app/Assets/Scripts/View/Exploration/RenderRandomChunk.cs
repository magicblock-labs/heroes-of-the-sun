using System;
using System.Collections;
using System.Collections.Generic;
using Model;
using UnityEngine;

namespace View.Exploration
{
    public class RenderRandomChunk : MonoBehaviour
    {
        [SerializeField] private Transform tileContainer;
        [SerializeField] private RenderTile tile;

        [SerializeField] private MeshFilter waterMesh;

        public void Create(Vector2Int offset, int size, Vector2 scale)
        {
            foreach (Transform child in tileContainer)
                Destroy(child.gameObject);


            GenerateWaterMesh(size, size * ConfigModel.CellSize);
            
            StartCoroutine(GenerateTiles(offset, size, scale));
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

        IEnumerator GenerateTiles(Vector2Int offset, int size, Vector2 scale)
        {
            for (var x = 0; x < size; x++)
            for (var y = 0; y < size; y++)
            {
                var sampleX = (float)(x + offset.x) / size * scale.x;
                var sampleY = (float)(y + offset.y) / size * scale.y;

                var perlinNoiseSample = Mathf.PerlinNoise(sampleX, sampleY);
                Instantiate(tile, tileContainer).Create(
                    offset, 
                    new Vector2Int(x, y), 
                    perlinNoiseSample,
                    x == 0 || y == 0 || x == size - 1 || y == size - 1);
                yield return null;
            }
        }
    }
}