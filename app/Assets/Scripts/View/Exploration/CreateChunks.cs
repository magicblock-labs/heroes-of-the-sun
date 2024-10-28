using System.Collections.Generic;
using Model;
using UnityEngine;

public class CreateChunks : MonoBehaviour
{
    [SerializeField] int chunkSize;
    [SerializeField] RenderRandomChunk prefab;

    private Dictionary<Vector2Int, RenderRandomChunk> _visibleChunks = new();
    private Camera _camera;
    private Transform _cameraTransform;
    private Vector2 _cameraAngleOffset;

    private void Start()
    {
        _camera = Camera.main;
        _cameraTransform = _camera.transform;

        _cameraAngleOffset = _camera.ViewportToWorldPoint(new Vector3(.5f, .5f, 40f));
    }

    private void Update()
    {
        var startingChunkX =
            (int)((_cameraAngleOffset.x + _cameraTransform.position.x) / (chunkSize * ConfigModel.CellSize));
        var startingChunkY =
            (int)((_cameraAngleOffset.y + _cameraTransform.position.z) / (chunkSize * ConfigModel.CellSize));

        var pass = 0;

        while (pass < 2)
        {
            for (var chunkX = -pass; chunkX <= pass; chunkX++)
            for (var chunkY = -pass; chunkY <= pass; chunkY++)
            {
                //edges only
                if (chunkX > -pass && chunkX < pass && chunkY > -pass && chunkY < pass) continue;

                var offsetChunkLocation = new Vector2Int(startingChunkX + chunkX, startingChunkY + chunkY);

                if (!_visibleChunks.ContainsKey(offsetChunkLocation))
                {
                    _visibleChunks[offsetChunkLocation] =
                        Instantiate(prefab,
                            new Vector3(startingChunkX + chunkX, 0, startingChunkY + chunkY),
                            Quaternion.identity,
                            transform).Create(offsetChunkLocation * chunkSize, chunkSize,
                            new Vector2(chunkSize, chunkSize) * .1f);
                }
            }

            pass++;
        }
    }
}