using System;
using DanielKreitsch;
using UniRx;
using UnityEngine;
using Zenject;

namespace VoxelCommand.Client
{
    public class UnitManager : DisposableMonoBehaviour
    {
        [Inject]
        private UnitFactory _unitFactory;

        [SerializeField]
        private UnitConfig _playerUnitConfig;

        [SerializeField]
        private UnitConfig _enemyUnitConfig;

        private readonly ReactiveCollection<Unit> _units = new();
        public IReadOnlyReactiveCollection<Unit> Units => _units;

        private readonly Subject<Unit> _onAnyUnitSkillPointGained = new();
        public IObservable<Unit> OnAnyUnitSkillPointGained => _onAnyUnitSkillPointGained;

        private void Awake()
        {
            _units
                .ObserveAdd()
                .Subscribe(evt =>
                {
                    var unit = evt.Value;
                    unit.OnSkillPointGained.Subscribe(_onAnyUnitSkillPointGained.OnNext).AddTo(unit.Disposables).AddTo(_disposables);
                })
                .AddTo(_disposables);
        }

        protected override void OnDispose()
        {
            _onAnyUnitSkillPointGained.OnCompleted();
            _onAnyUnitSkillPointGained.Dispose();
            _units.Dispose();
        }

        public Unit CreateUnit(Vector3 position, Quaternion rotation, Team team, string name)
        {
            Unit unit = _unitFactory.Create();
            unit.name = name;
            unit.transform.SetPositionAndRotation(position, rotation);
            unit.Initialize(team == Team.Player ? _playerUnitConfig : _enemyUnitConfig, team, name);

            _units.Add(unit);

            unit.Disposables.Add(
                Disposable.Create(() =>
                {
                    if (_units.Contains(unit))
                    {
                        _units.Remove(unit);
                    }
                })
            );

            return unit;
        }
    }
}
