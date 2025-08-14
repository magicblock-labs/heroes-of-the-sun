using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using Connectors;
using Model;
using Smartobjecttokenlauncher.Accounts;
using Solana.Unity.Wallet;
using UnityEngine;
using Utils.Injection;
using View.Exploration.SmartObjectTypes;
using World.Program;

namespace View.Exploration
{
    [Serializable]
    public class ComponentRenderer
    {
        public string componentAddress;
        public GameObject prefab;
    }

    public class RenderSmartObject : InjectableBehaviour
    {
        [Inject] private SmartObjectLocationConnector _connector;
        [Inject] private PathfindingModel _pathfinding;
        [Inject] private SmartObjectModel _model;

        private SmartObjectLocation.Accounts.SmartObjectLocation _data;


        [Inject] private SmartObjectDeityConnector _deity;
        [Inject] private SmartObjectTokenLauncherConnector _tokenLauncher;

        [SerializeField] private ComponentRenderer[] renderers;

        public const string CachePrefix = nameof(CachePrefix);

        public async Task SetDataAddress(string value)
        {
            _connector.SetDataAddress(value);
            var data = await _connector.LoadData();

            var cachedComponentAddress = PlayerPrefs.GetString($"{CachePrefix}:{data.Entity}", null);
            if (!string.IsNullOrEmpty(cachedComponentAddress)){
                var componentAddress = new PublicKey(cachedComponentAddress);
                var smartObjectRenderer = renderers.FirstOrDefault(r => r.componentAddress == componentAddress);
                if (smartObjectRenderer == null)
                    return;

                Instantiate(smartObjectRenderer.prefab, transform);
            }

            else
            {
                foreach (var smartObjectRenderer in renderers)
                {
                    Instantiate(smartObjectRenderer.prefab, transform);
                }
            }

           

            OnDataUpdate(data);
            foreach (var smartObjectInstance in gameObject.GetComponentsInChildren<ISmartObject>())
                await smartObjectInstance.SetEntity(data.Entity);
            await _connector.Subscribe(OnDataUpdate);
        }

        private void OnDataUpdate(SmartObjectLocation.Accounts.SmartObjectLocation value)
        {
            _data = value;
            _model.Set(new Vector2Int(_data.X, _data.Y), _data.Entity);

            StopAllCoroutines();
            StartCoroutine(UpdatePosition());
        }

        private IEnumerator UpdatePosition()
        {
            while (true)
            {
                var pos = ConfigModel.GetWorldCellPosition(_data.X, _data.Y);
                pos.y = _pathfinding.GetY(new Vector2Int(_data.X, _data.Y)) + ConfigModel.CellSize;

                transform.localPosition = pos;

                yield return new WaitForSeconds(1);
            }
        }

        public void UpdateData()
        {
            foreach (var smartObjectInstance in gameObject.GetComponentsInChildren<ISmartObject>())
                smartObjectInstance.UpdateData();
        }
    }
}