using System;
using UniRx;
using UnityEngine;
using Zenject;

namespace VoxelCommand.Client
{
    public class UnitManager : MonoBehaviour
    {
        [Inject] private UnitFactory _unitFactory;
        [Inject] private UnitConfig _defaultUnitConfig;

        private readonly ReactiveCollection<Unit> _activeUnits = new();
        public IReadOnlyReactiveCollection<Unit> ActiveUnits => _activeUnits;

        private readonly Subject<Unit> _onAnyUnitSkillPointGained = new();
        public IObservable<Unit> OnAnyUnitSkillPointGained => _onAnyUnitSkillPointGained;

        private readonly CompositeDisposable _disposables = new();

        private void Awake()
        {
            _activeUnits
                .ObserveAdd()
                .Subscribe(evt =>
                {
                    var unit = evt.Value;
                    unit.OnSkillPointGained
                        .Subscribe(_onAnyUnitSkillPointGained.OnNext)
                        .AddTo(unit.Disposables)
                        .AddTo(_disposables);
                })
                .AddTo(_disposables);
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
            _onAnyUnitSkillPointGained.OnCompleted();
            _onAnyUnitSkillPointGained.Dispose();
            _activeUnits.Dispose();
        }

        public Unit CreateUnit(Vector3 position, Quaternion rotation, Team team, string name, UnitConfig config = null)
        {
            Unit unit = _unitFactory.Create();
            unit.name = name;
            unit.transform.position = position;
            unit.transform.rotation = rotation;
            unit.Initialize(config ?? _defaultUnitConfig, team, name);
            
            _activeUnits.Add(unit);
            
            unit.Disposables.Add(Disposable.Create(() => {
                if (_activeUnits.Contains(unit))
                {
                    _activeUnits.Remove(unit);
                }
            }));

            return unit;
        }

        public void DestroyUnit(Unit unit)
        {
            if (unit != null && _activeUnits.Contains(unit))
            {
                _activeUnits.Remove(unit);
                unit.Dispose();
            }
        }
    }
}
