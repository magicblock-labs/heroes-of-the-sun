using System.Collections;
using Model;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Utils.Injection;

namespace View.Exploration
{
    public class RenderMinimap : InjectableBehaviour
    {
        [Inject] private PlayerHeroModel _playerHero;
        [Inject] private PathfindingModel _pathfinding;

        [SerializeField] private Color[] colors;

        [SerializeField] private RawImage _target;

        private const int TextureSize = 256; // final texture resolution
        private const int SampleSpan = 128; // number of samples per axis (-64..63)
        private const int PixelsPerSample = 2; // 2x2 pixels per sample

        private Texture2D _minimapTexture;
        private Color32[] _pixels;

        private IEnumerator Start()
        {
            _minimapTexture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Point
            };

            _pixels = new Color32[TextureSize * TextureSize];

            if (_target != null)
            {
                _target.texture = _minimapTexture;
            }

            while (true)
            {
                yield return new WaitForSeconds(1f);
                RedrawMinimap();
            }
        }

        void RedrawMinimap()
        {
            if (_playerHero?.Get() == null)
                return;
            
            var heroPosition = new Vector2(_playerHero.Get().X, Mathf.Abs(_playerHero.Get().Y));

            var halfSampleSpan = SampleSpan / 2;
            for (var x = -halfSampleSpan; x < halfSampleSpan; x++)
            for (var y = -halfSampleSpan; y < halfSampleSpan; y++)
            {
                var sample = _pathfinding.GetY(new Vector2Int(x + (int)(heroPosition.x/ConfigModel.CellSize), y + (int)(heroPosition.y/ConfigModel.CellSize)));

                var colorIndex = Mathf.Min(
                    (int)((sample + 6.5f) / 4f),
                    colors.Length - 1
                );

                // Map color index to color and paint a 2x2 block in the buffer.
                // Convert loop-space (-64..63) into texture-space (0..255) with 2x2 pixels per sample.
                colorIndex = Mathf.Clamp(colorIndex, 0, colors.Length - 1);
                var col = (Color32)colors[colorIndex];

                var tx = (x + halfSampleSpan) * PixelsPerSample;
                var ty = (y + halfSampleSpan) * PixelsPerSample;

                // Write a 2x2 block at (tx, ty)
                var row0 = ty * TextureSize;
                var row1 = (ty + 1) * TextureSize;

                _pixels[row0 + tx] = col;
                _pixels[row0 + tx + 1] = col;
                _pixels[row1 + tx] = col;
                _pixels[row1 + tx + 1] = col;
            }

            // Push buffer to texture once per frame
            _minimapTexture.SetPixels32(_pixels);
            _minimapTexture.Apply(false, false);

            // Ensure the RawImage has the texture assigned (covers the case when assigned at runtime)
            if (_target != null && _target.texture != _minimapTexture)
                _target.texture = _minimapTexture;
        }
    }
}