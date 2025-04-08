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

        private void Start() { }

        public Unit InstantiateUnit(Vector3 position, Quaternion rotation, Team team, string name)
        {
            Unit unit = _unitFactory.Create();
            unit.transform.SetPositionAndRotation(position, rotation);
            unit.Initialize(team == Team.Player ? _playerUnitConfig : _enemyUnitConfig, team, name);

            // Add to the units list
            _units.Add(unit);

            // Remove from the units list when the unit is destroyed
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

        public void ReviveUnit(Unit unit)
        {
            unit.gameObject.SetActive(true);
            unit.State.Health.Value = unit.State.MaxHealth.Value;
            unit.Controller.ResetCombatState();
            unit.Visuals.Animations.PlayReviveAnimation();
        }
    }
}
