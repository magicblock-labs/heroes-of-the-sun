using System.Collections;
using System.Threading.Tasks;
using Connectors;
using Model;
using Smartobjectlocation.Accounts;
using UnityEngine;
using Utils.Injection;
using View.Exploration.SmartObjectTypes;

namespace View.Exploration
{
    public class RenderSmartObject : InjectableBehaviour
    {
        [Inject] private SmartObjectLocationConnector _connector;
        [Inject] private PathfindingModel _pathfinding;
        [Inject] private SmartObjectModel _model;

        private SmartObjectLocation _data;
        

        public async Task SetDataAddress(string value)
        {
            _connector.SetDataAddress(value);
            var data = await _connector.LoadData();

            await gameObject.AddComponent<RenderSmartObjectDeity>().SetEntity(data.Entity);
            
            OnDataUpdate(data);

            await _connector.Subscribe(OnDataUpdate);
        }

        private void OnDataUpdate(SmartObjectLocation value)
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
    }
}