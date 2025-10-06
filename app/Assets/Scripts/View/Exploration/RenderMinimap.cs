using System.Collections;
using Model;
using UnityEngine;
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

        private const int TextureSize = 128; // final texture resolution
        private const int PixelsPerSample = 2; // 2x2 pixels per sample
        private const int SampleSpan = TextureSize/PixelsPerSample; // number of samples per axis (-64..63)

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
            
            var heroPosition = _playerHero.ImmediatePosition;

            var halfSampleSpan = SampleSpan / 2;
            for (var x = -halfSampleSpan; x < halfSampleSpan; x++)
            for (var y = -halfSampleSpan; y < halfSampleSpan; y++)
            {

                var colorIndex = 2;
                var samplePos = new Vector2Int((x + (int)(heroPosition.x)), (y + (int)(heroPosition.y)));
                if (_pathfinding.Has(samplePos))
                {
                    var sample = _pathfinding.GetY(samplePos);
                    colorIndex = Mathf.Min(
                        (int)((sample + 6.5f) / 4f),
                        colors.Length - 1
                    );

                }

                // Map color index to color and paint a 2x2 block in the buffer.
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