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
        private GameObject _renderer;

        private const string SmartObjectComponentAddress = nameof(SmartObjectComponentAddress);

        public async Task SetDataAddress(string value)
        {
            _connector.SetDataAddress(value);
            var data = await _connector.LoadData();

            var cachedComponentAddress = PlayerPrefs.GetString($"{SmartObjectComponentAddress}:{data.Entity}", null);
            PublicKey componentAddress = null;
            if (!string.IsNullOrEmpty(cachedComponentAddress))
                componentAddress = new PublicKey(cachedComponentAddress);

            else
            {
                componentAddress = await TryInitSmartObject(data.Entity, _deity);

                if (componentAddress == null)
                    componentAddress = await TryInitSmartObject(data.Entity, _tokenLauncher);

                //no match
                if (componentAddress == null)
                {
                    Destroy(gameObject);
                    return;
                }
            }

            var smartObjectRenderer = renderers.FirstOrDefault(r => r.componentAddress == componentAddress);
            if (smartObjectRenderer == null)
                return;

            _renderer = smartObjectRenderer.prefab;
            Instantiate(_renderer, transform);

            OnDataUpdate(data);
            await gameObject.GetComponentInChildren<ISmartObject>().SetEntity(data.Entity);
            PlayerPrefs.SetString($"{SmartObjectComponentAddress}:{data.Entity}", componentAddress);
            await _connector.Subscribe(OnDataUpdate);
        }

        private static async Task<PublicKey> TryInitSmartObject<T>(PublicKey entity, BaseComponentConnector<T> connector)
        {
            await connector.SetEntityPda(entity, false);
            var smartObjectData = await connector.LoadData();
            return smartObjectData == null ? null : connector.GetComponentProgramAddress();
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
            _renderer.GetComponent<ISmartObject>().UpdateData();
        }
    }
}