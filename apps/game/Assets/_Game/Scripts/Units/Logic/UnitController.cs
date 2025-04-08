using System;
using System.Collections.Generic;
using DanielKreitsch;
using UniRx;
using UnityEngine;
using UnityEngine.AI;
using Zenject;

namespace VoxelCommand.Client
{
    /// <summary>
    /// Controls unit behavior including movement, combat, and AI decision-making.
    /// This controller manages the tactical execution of combat decisions made by the CombatSystem.
    /// </summary>
    public class UnitController : DisposableMonoBehaviour
    {
        [Inject]
        private IMessageBroker _messageBroker;

        [Inject]
        private IPathfindingService _pathfindingService;

        [Header("References")]
        [SerializeField]
        private NavMeshAgent _navMeshAgent;
        public NavMeshAgent NavMeshAgent => _navMeshAgent;

        [Header("Movement Settings")]
        [SerializeField, Tooltip("Distance at which unit considers destination reached")]
        private float _destinationReachedThreshold = 0.2f;
        public float DestinationReachedThreshold => _destinationReachedThreshold;

        [Header("Combat Settings")]
        [SerializeField, Tooltip("Maximum distance at which unit can attack targets")]
        private float _attackRange = 1f;
        public float AttackRange => _attackRange;

        [SerializeField, Tooltip("Maximum distance at which unit can detect enemies")]
        private float _detectionRange = 5f;
        public float DetectionRange => _detectionRange;

        [SerializeField, Tooltip("Seconds between attack attempts")]
        private float _attackCooldown = 1f;
        public float AttackCooldown => _attackCooldown;

        [SerializeField, Tooltip("Duration of crowd control effects when hit")]
        private float _crowdControlDuration = 0.75f;

        [SerializeField, Tooltip("Reference to the attack trigger collider")]
        private AttackTrigger _attackTrigger;
        public AttackTrigger AttackTrigger => _attackTrigger;

        [SerializeField, Tooltip("Force applied to target when hit")]
        private float _knockbackForce = 5f;

        // Variables
        private Unit _self;
        private Unit _opponent;
        private readonly List<Vector3> _currentPath = new();
        private bool _isMoving;
        private bool _isUnderManualControl;
        private bool _isInCombat;
        private float _lastAttackTime;
        private bool _isCrowdControlled;
        private IDisposable _crowdControlTimer;

        public Unit Self => _self;

        public Unit Opponent => _opponent;

        public bool IsMoving
        {
            get => _isMoving;
            set => _isMoving = value;
        }

        public float LastAttackTime => _lastAttackTime;

        public bool IsUnderManualControl
        {
            get => _isUnderManualControl;
            set => _isUnderManualControl = value;
        }

        public bool IsInCombat
        {
            get => _isInCombat;
            set => _isInCombat = value;
        }

        public bool IsCrowdControlled => _isCrowdControlled;

        // State machine
        private IUnitState _currentState;
        private readonly IdleState _idleState;
        private readonly ScoutingState _scoutingState;
        private readonly CombatState _combatState;
        private readonly DeadState _deadState;
        private readonly PreparingState _preparingState;
        private readonly VictoryState _victoryState;

        public IUnitState CurrentState => _currentState;
        public IdleState IdleState => _idleState;
        public ScoutingState ScoutingState => _scoutingState;
        public CombatState CombatState => _combatState;
        public DeadState DeadState => _deadState;
        public PreparingState PreparingState => _preparingState;
        public VictoryState VictoryState => _victoryState;

        public UnitController()
        {
            _idleState = new IdleState(this);
            _scoutingState = new ScoutingState(this);
            _combatState = new CombatState(this);
            _deadState = new DeadState(this);
            _preparingState = new PreparingState(this);
            _victoryState = new VictoryState(this);
        }

        /// <summary>
        /// Initializes the controller with unit data and sets up initial state
        /// </summary>
        /// <param name="unit">The unit this controller will manage</param>
        public void Initialize(Unit unit)
        {
            _self = unit;

            _navMeshAgent.enabled = true;
            _lastAttackTime = 0f;
            _isInCombat = false;
            _opponent = null;
            _isUnderManualControl = false;
            _isCrowdControlled = false;

            // Synchronize movement speed to NavMeshAgent
            _self
                .State.MovementSpeed.Subscribe(speed =>
                {
                    _navMeshAgent.speed = speed;
                })
                .AddTo(_disposables);
            _navMeshAgent.speed = _self.State.MovementSpeed.Value;

            // Handle death
            _self
                .State.Health.Subscribe(health =>
                {
                    if (health <= 0 && _currentState != _deadState)
                    {
                        TransitionToState(_deadState);
                        _messageBroker.Publish(new UnitDeathEvent(_self, _self.State.LastAttacker.Value));
                    }
                })
                .AddTo(_disposables);

            // Initialize with idle state
            TransitionToState(_idleState);
        }

        private void Update()
        {
            _currentState.Update();

            // Debug UI
            var debugText = "";
            var stateName = _currentState.GetType().Name;
            // debugText += stateName.Substring(0, stateName.Length - 5) + "\n";
            _self.Visuals.DebugText = debugText;
        }

        /// <summary>
        /// Transitions to a new state
        /// </summary>
        /// <param name="newState">The state to transition to</param>
        public void TransitionToState(IUnitState newState)
        {
            var previousState = _currentState;
            Debug.Log($"[{_self.name}] {previousState?.GetType()?.Name} -> {newState.GetType().Name}");
            _currentState?.Exit(newState);
            _currentState = newState;
            _currentState.Enter(previousState);
        }

        /// <summary>
        /// Moves the unit to a specific position using pathfinding
        /// </summary>
        /// <param name="destination">Target position to move to</param>
        /// <returns>True if path was successfully calculated</returns>
        public bool MoveToPosition(Vector3 destination)
        {
            if (_self.State.IsDead || _isCrowdControlled)
                return false;

            if (!_pathfindingService.CalculatePath(transform.position, destination, _currentPath))
                return false;

            _isMoving = true;
            _navMeshAgent.stoppingDistance = _destinationReachedThreshold;
            if (_navMeshAgent.enabled && _navMeshAgent.isOnNavMesh)
            {
                _navMeshAgent.SetDestination(destination);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Handles player-initiated movement, overriding AI control
        /// </summary>
        /// <param name="destination">Target position to move to</param>
        /// <returns>True if movement was initiated successfully</returns>
        public bool ManualMoveToPosition(Vector3 destination)
        {
            if (_self.State.IsDead || _isCrowdControlled)
                return false;

            _isUnderManualControl = true;
            _isInCombat = false;
            _opponent = null;
            StopMoving();
            return MoveToPosition(destination);
        }

        /// <summary>
        /// Moves the unit to a random position within a radius of the center point
        /// </summary>
        /// <param name="center">Center position for random movement</param>
        /// <param name="radius">Maximum distance from center</param>
        /// <returns>True if movement was initiated successfully</returns>
        public bool MoveToRandomPositionNear(Vector3 center, float radius)
        {
            if (!_pathfindingService.TryGetRandomPointNear(center, radius, out Vector3 randomPoint))
                return false;

            return MoveToPosition(randomPoint);
        }

        /// <summary>
        /// Stops the unit's current movement
        /// </summary>
        /// <param name="idle">Whether to transition to idle state after stopping</param>
        public void StopMoving(bool idle = false)
        {
            _isMoving = false;
            if (_navMeshAgent.enabled && _navMeshAgent.isOnNavMesh)
            {
                _navMeshAgent.ResetPath();
            }
            _currentPath.Clear();

            if (idle && _currentState != _combatState && _currentState != _deadState)
            {
                TransitionToState(_idleState);
            }
        }

        /// <summary>
        /// Finds the nearest enemy unit within detection range
        /// </summary>
        /// <param name="potentialTargets">Collection of potential enemy units</param>
        /// <returns>Nearest enemy unit or null if none found in range</returns>
        public Unit FindNearestEnemy(IEnumerable<Unit> potentialTargets)
        {
            Unit nearestEnemy = null;
            float closestDistance = _detectionRange;

            foreach (Unit potentialTarget in potentialTargets)
            {
                if (potentialTarget == null || potentialTarget.State.Health.Value <= 0 || _self.IsAlly(potentialTarget))
                    continue;

                float distance = Vector3.Distance(transform.position, potentialTarget.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    nearestEnemy = potentialTarget;
                }
            }

            return nearestEnemy;
        }

        /// <summary>
        /// Sets a target for combat engagement
        /// </summary>
        /// <param name="target">Unit to target for combat</param>
        public void EngageTarget(Unit target)
        {
            if (target == null)
                return;

            _opponent = target;
            _isInCombat = true;
            _navMeshAgent.stoppingDistance = _attackRange * 0.8f;
            TransitionToState(_combatState);
        }

        /// <summary>
        /// Attempts to attack the current target if in range
        /// </summary>
        public void AttackTarget()
        {
            if (_isCrowdControlled || _opponent == null || _opponent.State.IsDead || Time.time - _lastAttackTime < _attackCooldown)
                return;

            FaceTarget();

            float distanceToTarget = Vector3.Distance(transform.position, _opponent.transform.position);

            if (distanceToTarget <= _attackRange)
            {
                _lastAttackTime = Time.time;
                _self.Visuals.Animations.PlayAttackAnimation();
            }
            else
            {
                MoveToPosition(_opponent.transform.position);
            }
        }

        /// <summary>
        /// Handles attack hit detection and applies damage to the target
        /// </summary>
        /// <param name="target">The unit that was hit</param>
        public void OnAttackHit(Unit target)
        {
            if (_self.State.IsDead)
                return;

            // Apply damage and effects
            float damage = _self.State.DamageOutput.Value;
            float targetHealth = target.State.Health.Value;
            float targetDamageReduction = target.State.IncomingDamageReduction.Value;
            float finalDamage = Mathf.Max(1, damage * (1 - targetDamageReduction));

            target.State.LastAttacker.Value = _self;
            target.State.Health.Value = Mathf.Max(0, targetHealth - finalDamage);
            _messageBroker.Publish(new UnitDamagedEvent(target, _self, finalDamage));

            // Play hit reaction
            target.Visuals.Animations.PlayHitReactionAnimation();

            // Apply crowd control
            var opponentController = target.GetComponent<UnitController>();
            if (opponentController != null)
            {
                opponentController.ApplyCrowdControl();
            }

            // Update last attack time
            _lastAttackTime = Time.time;

            // Clear target if it died
            if (target.State.IsDead)
            {
                _opponent = null;
                _isInCombat = false;
            }
        }

        /// <summary>
        /// Rotates the unit to face the current target
        /// </summary>
        private void FaceTarget()
        {
            if (_opponent == null)
                return;

            Vector3 targetDirection = (_opponent.transform.position - transform.position).normalized;
            targetDirection.y = 0;

            if (targetDirection != Vector3.zero)
            {
                float angleOffset = 0f; // TODO: Use for random inaccuracy
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection) * Quaternion.Euler(0, angleOffset, 0);
                transform.rotation = targetRotation;
            }
        }

        /// <summary>
        /// Checks if the current target is still valid for engagement
        /// </summary>
        /// <returns>True if target is alive and in detection range</returns>
        public bool IsTargetValid()
        {
            return _opponent != null
                && _opponent.State.IsAlive
                && Vector3.Distance(transform.position, _opponent.transform.position) <= _detectionRange;
        }

        /// <summary>
        /// Applies crowd control effect to this unit, preventing actions for a duration
        /// </summary>
        public void ApplyCrowdControl()
        {
            _isCrowdControlled = true;

            StopMoving();

            // Dispose existing timer if any
            _crowdControlTimer?.Dispose();

            // Set timer to end CC after duration
            _crowdControlTimer = Observable
                .Timer(TimeSpan.FromSeconds(_crowdControlDuration))
                .Subscribe(_ =>
                {
                    _isCrowdControlled = false;
                })
                .AddTo(_disposables);
        }

        /// <summary>
        /// Stops movement and plays victory animation
        /// </summary>
        public void PlayVictoryAnimation()
        {
            StopMoving();
            _self.Visuals.Animations.PlayVictoryAnimation();
        }

        /// <summary>
        /// Stops movement and plays death animation
        /// </summary>
        public void PlayDeathAnimation()
        {
            StopMoving();
            _self.Visuals.Animations.PlayDeathAnimation();
        }

        /// <summary>
        /// Resets the unit's combat state and control status
        /// </summary>
        public void ResetCombatState()
        {
            StopMoving();
            _opponent = null;
            _isInCombat = false;
            _isUnderManualControl = false;
            _lastAttackTime = 0f;
            if (_navMeshAgent.enabled)
            {
                _navMeshAgent.stoppingDistance = _destinationReachedThreshold;
            }
            TransitionToState(_idleState);
        }

        /// <summary>
        /// Cleans up resources and resets state
        /// </summary>
        public new void Dispose()
        {
            base.Dispose();
            _opponent = null;
            _isInCombat = false;
            _currentPath.Clear();
            _crowdControlTimer?.Dispose();
        }
    }
}
