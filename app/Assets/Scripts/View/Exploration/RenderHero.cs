using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Connectors;
using Model;
using Solana.Unity.Wallet;
using TMPro;
using UnityEngine;
using Utils;
using Utils.Injection;

namespace View.Exploration
{
    public class RenderHero : InjectableBehaviour
    {
        [Inject] private PlayerConnector _player;
        [Inject] private HeroConnector _connector;
        [Inject] private PlayerSettlementConnector _settlement;
        [Inject] private LootDistributionConnector _lootConnector;
        
        [Inject] private PathfindingModel _pathfinding;
        [Inject] private PlayerHeroModel _playerHero;
        [Inject] private LootModel _loot;
        
        [SerializeField] private TMP_Text keyLabel;

        private LineRenderer _line;

        private List<Vector2Int> _path = new();

        private Vector3? _currentTarget;
        private float _mouseDownTime;
        private bool _initialised;


        private Hero.Accounts.Hero _data;
        private Vector2Int _position;

        private void Start()
        {
            _line = GetComponent<LineRenderer>();
        }

        public async Task SetDataAddress(string value)
        {
            _connector.SetDataAddress(value);
            _data = await _connector.LoadData();
            keyLabel.text = _data.Owner.ToString()[..4];

            //this should be used after the rollup
            //todo move to point_and_click and rename to player hero controller?
            //await _connector.Subscribe((_, _, hero) => { _playerHero.Set(hero); });

            //todo remove this after subscription is enabled (this is basically client prediction code)
            if (_data.Owner == _player.DataAddress)
            {
                _playerHero.Set(_data);
                
                gameObject.AddComponent<PointAndClickMovement>().SetDataAddress(value, pos =>
                {
                    var hero = new Hero.Accounts.Hero()
                    {
                        Owner = _data.Owner,
                        X = pos.x,
                        Y = pos.y,
                        LastActivity = Web3Utils.GetNodeTime()
                    };
                    _playerHero.Set(hero);
                    OnDataUpdate(hero);
                });
            }

            MoveToNextPoint();
            _initialised = true;
        }

        private void OnDataUpdate(Hero.Accounts.Hero value)
        {
            _path = _pathfinding.FindPath(_position, new Vector2Int(value.X, value.Y));
            _data = value;
            ApplyPathToLine();
            MoveToNextPoint();
        }

        void Update()
        {
            if (!_initialised)
                return;

            if (!_currentTarget.HasValue) return;

            var diff = _currentTarget.Value - transform.position;
            transform.forward = diff;

            if (diff.magnitude < .1f)
            {
                _position = _path[0];
                _path.RemoveAt(0);
                MoveToNextPoint();

                if (!_currentTarget.HasValue)
                    _line.positionCount = 0;
                else
                    ApplyPathToLine();

                return;
            }

            transform.position += diff.normalized * (Time.deltaTime * 5f);
            if (_line.positionCount > 0)
                _line.SetPosition(0, transform.position);
        }

        private void ApplyPathToLine()
        {
            var pathWorld = new List<Vector3> { transform.position + Vector3.up }
                .Concat(_path.Select(v =>
                    ConfigModel.GetWorldCellPosition(v.x, v.y) + Vector3.up * (_pathfinding.GetY(v) + 2f)))
                .ToArray();

            _line.positionCount = pathWorld.Length;
            _line.SetPositions(pathWorld);
        }

        private void MoveToNextPoint()
        {
            if (_path.Count > 0)
            {
                _currentTarget = ConfigModel.GetWorldCellPosition(_path[0].x, _path[0].y) +
                                 Vector3.up * (_pathfinding.GetY(_path[0]) + 1f);
            }
            else
            {
                _currentTarget = null;
                transform.position =
                    ConfigModel.GetWorldCellPosition(_data.X, _data.Y) +
                    Vector3.up * (_pathfinding.GetY(new Vector2Int(_data.X, _data.Y)) + 1f);
                
                _position = new Vector2Int(_data.X, _data.Y);
                transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);

                if (_data.Owner == _player.DataAddress && _loot.HasLootAt(_position, out var index))
                    
                    /*#[system_input]
                       pub struct Components {
                           pub loot: LootDistribution, << this is the caller entity
                           pub hero: Hero,
                           pub settlement: Settlement,
                       }
                       */
                    _=_lootConnector.Claim(index, new Dictionary<PublicKey, PublicKey>()
                    {
                        { new PublicKey(_player.EntityPda), _connector.GetComponentProgramAddress() },
                        { new PublicKey(_settlement.EntityPda), _settlement.GetComponentProgramAddress() },
                    });
            }
        }
    }
}