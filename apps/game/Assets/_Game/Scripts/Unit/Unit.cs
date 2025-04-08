using System;
using UniRx;
using UnityEngine;
using Zenject;
using MiniTools.BetterGizmos;

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

        private string _name;
        public string Name => _name;

        private bool _isSelected;
        public bool IsSelected 
        { 
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    _visuals.SetSelected(value);
                    OnSelectionChanged?.Invoke(this, value);
                }
            }
        }

        public event Action<Unit, bool> OnSelectionChanged;

        private readonly Subject<Unit> _onSkillPointGained = new();
        public IObservable<Unit> OnSkillPointGained => _onSkillPointGained;
        
        public void Initialize(UnitConfig config, Team team, string name)
        {
            _config = config;
            _team = team;
            _name = name;

            // Set the GameObject layer based on the team
            string layerName = _team == Team.Player ? "PlayerUnit" : "EnemyUnit";
            int layer = LayerMask.NameToLayer(layerName);
            if (layer == -1)
            {
                Debug.LogWarning($"Layer '{layerName}' not found. Please define it in the Unity Tag Manager.");
            }
            else
            {
                gameObject.layer = layer;
                // also all children
                foreach (Transform child in transform)
                {
                    child.gameObject.layer = layer;
                }
            }

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
            
            // Initialize battle stats
            ResetBattleStats();
        }

        // Implement the OnDispose hook instead
        protected override void OnDispose()
        {
            _onSkillPointGained.OnCompleted();
            _onSkillPointGained.Dispose();
            // Base call is handled by DisposableComponent's Dispose
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

                    // Fire event if skill points were gained
                    if (levelDifference > 0)
                    {
                        _onSkillPointGained.OnNext(this);
                    }
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

        public void AddExperience(int amount)
        {
            if (amount <= 0) return;
            
            _state.Experience.Value += amount;
            Debug.Log($"{name} gained {amount} experience! Total: {_state.Experience.Value}");
        }
        
        public void RecordDamageDealt(float damage)
        {
            _state.DamageDealt.Value += damage;
        }
        
        public void RecordKill()
        {
            _state.Kills.Value += 1;
        }
        
        public void ResetBattleStats()
        {
            _state.DamageDealt.Value = 0;
            _state.Kills.Value = 0;
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

        private void OnDrawGizmos()
        {
            if (IsSelected)
            {
                // Draw a circle around the unit when selected
                Color selectionColor = _team == Team.Player ? Color.cyan : Color.yellow;
                BetterGizmos.DrawCircle2D(selectionColor, transform.position, Vector3.up, 1.5f);
            }
        }
    }
}
