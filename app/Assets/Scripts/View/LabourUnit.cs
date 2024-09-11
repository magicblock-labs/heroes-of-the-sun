using System.Collections;
using System.Linq;
using Model;
using Service;
using UnityEngine;
using UnityEngine.AI;
using Utils.Injection;
using BuildingType = Settlement.Types.BuildingType;

namespace View
{
    public class LabourUnit : InjectableBehaviour
    {
        [Inject] private SettlementModel _model;
        [Inject] private ResourceLocationModel _locations;

        private int _index;

        private Vector2Int? _actionPoint;
        private Vector2Int? _collectionPoint;
        private NavMeshAgent _agent;
        private Animator _anim;
        private static readonly int TargetDistance = Animator.StringToHash("TargetDistance");
        private static readonly int Action = Animator.StringToHash("Action");

        void Start()
        {
            _agent = GetComponent<NavMeshAgent>();
            _anim = GetComponent<Animator>();
            _model.Updated.Add(ResetLogic);
            ResetLogic();
        }

        public void SetIndex(int value)
        {
            _index = value;
        }


        private void ResetLogic()
        {
            StopAllCoroutines();
            StartCoroutine(ResetLogicCoroutine());
        }

        private IEnumerator ResetLogicCoroutine()
        {
            Vector2Int? GetClosestLocationFor(ResourceType resource, Vector2Int target)
            {
                var locations = _locations.Get(resource);
                return locations.Any()
                    ? locations.OrderBy(loc =>
                        Vector3.Distance(ConfigModel.GetWorldCellPosition(loc.x, loc.y),
                            ConfigModel.GetWorldCellPosition(target.x, target.y))).First()
                    : null;
            }
            
            yield return null;//allow locations to be populated
            
            var settlement = _model.Get();
            var labourAllocation = settlement.LabourAllocation[_index];

            if (labourAllocation < 0 || labourAllocation >= settlement.Buildings.Length)
            {
                _collectionPoint = null;
                _actionPoint = null;
            }
            else
            {
                var building = settlement.Buildings[labourAllocation];

                if (building.DaysToBuild > 0)
                {
                    //still building, so action point should be at buildings location
                    _actionPoint = new Vector2Int(building.X, building.Y);
                }
                else
                {
                    //whatever resource we are collecting, should be brought back to the building
                    _collectionPoint = new Vector2Int(building.X, building.Y);

                    _actionPoint = building.Id switch
                    {
                        BuildingType.FoodCollector => GetClosestLocationFor(ResourceType.Food, _collectionPoint.Value),
                        BuildingType.WoodCollector => GetClosestLocationFor(ResourceType.Wood, _collectionPoint.Value),
                        _ => _actionPoint
                    };
                }
            }

            yield return StartCoroutine(PerformLogic());
        }

        private IEnumerator PerformLogic()
        {
            var currentTarget = Vector2Int.zero;

            if (!_collectionPoint.HasValue && !_actionPoint.HasValue)
                yield break;

            //set uyor first target
            if (_actionPoint.HasValue)
                currentTarget = _actionPoint.Value;
            else if (_collectionPoint.HasValue)
                currentTarget = _collectionPoint.Value;

            while (true)
            {
                _agent.SetDestination(
                    ConfigModel.GetWorldCellPosition(currentTarget.x, currentTarget.y));

                yield return null;//allow to recalculate the distance
                var distance = _agent.remainingDistance;
                _anim.SetFloat(TargetDistance, distance);

                while (distance > 1)
                {
                    distance = _agent.remainingDistance;
                    _anim.SetFloat(TargetDistance, distance);
                    yield return null; //no need to do this every frame though
                }

                if (_actionPoint.HasValue && currentTarget == _actionPoint.Value)
                {
                    //reached actionable point
                    _anim.SetBool(Action, true);
                    yield return new WaitForSeconds(3);
                    _anim.SetBool(Action, false);

                    if (_collectionPoint.HasValue)
                        currentTarget = _collectionPoint.Value;
                }
                else if (_actionPoint.HasValue)
                    currentTarget = _actionPoint.Value;
                
                yield return null; 
            }
        }

        private void OnDestroy()
        {
            _model.Updated.Remove(ResetLogic);
        }
    }
}