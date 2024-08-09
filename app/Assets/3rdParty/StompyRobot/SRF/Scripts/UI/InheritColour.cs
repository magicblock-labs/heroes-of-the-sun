using StompyRobot.SRF.Internal;
using StompyRobot.SRF.Scripts.Components;
using UnityEngine;
using UnityEngine.UI;

namespace StompyRobot.SRF.Scripts.UI
{
    [RequireComponent(typeof (Graphic))]
    [ExecuteInEditMode]
    [AddComponentMenu(ComponentMenuPaths.InheritColour)]
    public class InheritColour : SRMonoBehaviour
    {
        private Graphic _graphic;
        public Graphic From;

        private Graphic Graphic
        {
            get
            {
                if (_graphic == null)
                {
                    _graphic = GetComponent<Graphic>();
                }

                return _graphic;
            }
        }

        private void Refresh()
        {
            if (From == null)
            {
                return;
            }

            Graphic.color = From.canvasRenderer.GetColor();
        }

        private void Update()
        {
            Refresh();
        }

        private void Start()
        {
            Refresh();
        }
    }
}
