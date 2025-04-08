using DanielKreitsch;
using UniRx;
using UnityEngine;
using Zenject;

namespace VoxelCommand.Client
{
    [RequireComponent(typeof(Rigidbody))]
    public class Unit : DisposableMonoBehaviour
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
        public UnitVisuals Visuals => _visuals;

        [SerializeField]
        private Collider _playerInteractionCollider;
        public Collider PlayerInteractionCollider => _playerInteractionCollider;

        [SerializeField]
        private Collider _vulnerableCollider;
        public Collider VulnerableCollider => _vulnerableCollider;

        [Inject]
        private IUnitStatsCalculator _statsCalculator;

        private Team _team;
        public Team Team => _team;

        private string _name;
        public string Name => _name;

        public ReactiveProperty<bool> IsSelected { get; } = new(false);

        private void Awake()
        {
            // Ensure Rigidbody is properly set up
            var rigidbody = GetComponent<Rigidbody>();
            rigidbody.isKinematic = false;
            rigidbody.useGravity = true;
            rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        }

        public void Initialize(UnitConfig config, Team team, string name)
        {
            _config = config;
            _team = team;
            _name = name;

            // Set GameObject name
            this.name = name;

            // Set player interaction collider layer based on the team
            string layerName = _team == Team.Player ? "PlayerUnit" : "EnemyUnit";
            int layer = LayerMask.NameToLayer(layerName);
            if (layer != -1)
                _playerInteractionCollider.gameObject.layer = layer;
            else
                Debug.LogWarning($"Layer '{layerName}' not found. Please define it in the Unity Tag Manager.");

            _controller.Initialize(this);
            _visuals.Initialize(this);

            SetupSubscriptions();

            // Initialize skill points and health
            _state.HealthSkill.Value = 0;
            _state.StrengthSkill.Value = 0;
            _state.DefenseSkill.Value = 0;
            _state.SpeedSkill.Value = 0;
            _state.Health.Value = _state.MaxHealth.Value;
        }

        private void SetupSubscriptions()
        {
            // Handle selection changes reactively
            IsSelected
                .Subscribe(isSelected =>
                {
                    _visuals.SetOutlineActive(isSelected);
                })
                .AddTo(_disposables);

            // Subscribe to experience changes to update level
            _state
                .Experience.Subscribe(exp =>
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
            _state
                .Level.Pairwise()
                .Subscribe(pair =>
                {
                    int previousLevel = pair.Previous;
                    int currentLevel = pair.Current;

                    // Calculate stat points based on level change (can be positive or negative)
                    int levelDifference = currentLevel - previousLevel;
                    _state.AvailableSkillPoints.Value += levelDifference;
                })
                .AddTo(_disposables);

            _state
                .HealthSkill.Subscribe(healthRank =>
                {
                    _state.MaxHealth.Value = _statsCalculator.CalculateMaxHealth(Config, _state);
                })
                .AddTo(_disposables);

            _state
                .StrengthSkill.Subscribe(damageRank =>
                {
                    _state.DamageOutput.Value = _statsCalculator.CalculateDamageOutput(Config, _state);
                })
                .AddTo(_disposables);

            _state
                .DefenseSkill.Subscribe(defenseRank =>
                {
                    _state.IncomingDamageReduction.Value = _statsCalculator.CalculateIncomingDamageReduction(Config, _state);
                })
                .AddTo(_disposables);

            _state
                .SpeedSkill.Subscribe(speedRank =>
                {
                    _state.MovementSpeed.Value = _statsCalculator.CalculateMovementSpeed(Config, _state);
                })
                .AddTo(_disposables);
        }

        public bool IsAlly(Unit otherUnit)
        {
            return _team == otherUnit.Team;
        }
    }
}
