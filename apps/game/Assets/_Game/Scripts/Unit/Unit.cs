using UniRx;
using UnityEngine;
using Zenject;

namespace VoxelCommand.Client
{
    public class Unit : DisposableComponent
    {
        [SerializeField]
        private UnitConfig _config;
        public UnitConfig Config => _config;

        [SerializeField]
        private UnitState _state;
        public UnitState State => _state;

        [SerializeField]
        private UnitController _controller;
        public UnitController Controller => _controller;

        [SerializeField]
        private UnitVisuals _visuals;

        [Inject]
        private IUnitStatsCalculator _statsCalculator;

        private Team _team;
        public Team Team => _team;
        
        public void Initialize(UnitConfig config, Team team)
        {
            Dispose();
            
            _config = config;
            _team = team;

            if (_controller != null)
            {
                _controller.Initialize(this);
            }

            if (_visuals != null)
            {
                _visuals.Initialize(this);
            }

            SetupSubscriptions();
            
            // Initialize stat ranks
            _state.HealthRank.Value = 0;
            _state.DamageRank.Value = 0;
            _state.DefenseRank.Value = 0;
            _state.SpeedRank.Value = 0;
            
            // Initialize health
            _state.Health.Value = _state.MaxHealth.Value;
        }
        
        private void SetupSubscriptions()
        {
            // Subscribe to experience changes to update level
            _state.Experience
                .Subscribe(exp =>
                {
                    // Calculate new level based on current experience
                    int newLevel = _statsCalculator.CalculateLevelFromExperience(Config, _state);

                    // Update level if changed
                    if (_state.Level.Value != newLevel)
                    {
                        _state.Level.Value = newLevel;
                    }
                })
                .AddTo(_disposables);

            // Use pairwise to compare previous and current level values
            _state.Level
                .Pairwise()
                .Subscribe(pair =>
                {
                    int previousLevel = pair.Previous;
                    int currentLevel = pair.Current;
                    
                    // Calculate stat points based on level change (can be positive or negative)
                    int levelDifference = currentLevel - previousLevel;
                    _state.AvailableStatPoints.Value += levelDifference;
                })
                .AddTo(_disposables);

            _state.HealthRank
                .Subscribe(healthRank =>
                {
                    _state.MaxHealth.Value = _statsCalculator.CalculateMaxHealth(Config, _state);
                })
                .AddTo(_disposables);

            _state.DamageRank
                .Subscribe(damageRank =>
                {
                    _state.DamageOutput.Value = _statsCalculator.CalculateDamageOutput(Config, _state);
                })
                .AddTo(_disposables);

            _state.DefenseRank
                .Subscribe(defenseRank =>
                {
                    _state.IncomingDamageReduction.Value = _statsCalculator.CalculateIncomingDamageReduction(Config, _state);
                })
                .AddTo(_disposables);

            _state.SpeedRank
                .Subscribe(speedRank =>
                {
                    _state.MovementSpeed.Value = _statsCalculator.CalculateMovementSpeed(Config, _state);
                })
                .AddTo(_disposables);
        }

        // Methods for player to allocate stat points
        public bool AllocatePointToHealth()
        {
            return AllocatePointToStat(_state.HealthRank);
        }

        public bool AllocatePointToDamage()
        {
            return AllocatePointToStat(_state.DamageRank);
        }

        public bool AllocatePointToDefense()
        {
            return AllocatePointToStat(_state.DefenseRank);
        }

        public bool AllocatePointToSpeed()
        {
            return AllocatePointToStat(_state.SpeedRank);
        }

        private bool AllocatePointToStat(IntReactiveProperty stat)
        {
            if (_state.AvailableStatPoints.Value <= 0)
                return false;

            stat.Value++;
            _state.AvailableStatPoints.Value--;
            return true;
        }
        
        public bool IsAlly(Unit otherUnit)
        {
            return _team == otherUnit.Team;
        }
    }
}
