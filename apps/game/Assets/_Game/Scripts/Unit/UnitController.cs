using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.AI;
using Zenject;

namespace VoxelCommand.Client
{
    public class UnitController : DisposableComponent
    {
        private Unit _unit;
        private readonly List<Vector3> _currentPath = new();
        private bool _isMoving;
        private bool _isUnderManualControl; // Added for player commands

        [SerializeField]
        private UnitAnimationController _animationController;

        [SerializeField]
        private NavMeshAgent _navMeshAgent;

        [SerializeField]
        private float _pathingRefreshRate = 0.5f;

        [SerializeField]
        private float _destinationReachedThreshold = 0.2f;

        [SerializeField]
        private float _rotationSpeed = 10f;

        // Combat parameters
        [SerializeField, Tooltip("Distance from which unit can attack")]
        private float _attackRange = 1f;

        [SerializeField, Tooltip("Distance at which unit detects enemies")]
        private float _detectionRange = 5f;

        [SerializeField, Tooltip("Time between attacks")]
        private float _attackCooldown = 1f;

        private Unit _currentTarget;
        private float _lastAttackTime;
        public bool IsInCombat { get; private set; }
        public Unit CurrentTarget => _currentTarget;

        // Flag to indicate manual control
        public bool IsUnderManualControl => _isUnderManualControl;

        [Inject]
        private IPathfindingService _pathfindingService;

        public void Initialize(Unit unit)
        {
            _unit = unit;

            SetupSubscriptions();
            _navMeshAgent.enabled = true;
            _lastAttackTime = 0f;
            IsInCombat = false;
            _currentTarget = null;
            _isUnderManualControl = false; // Initialize flag

            _animationController.Initialize(_unit);
        }

        private void SetupSubscriptions()
        {
            // Update speed when unit speed stat changes
            _unit.State.MovementSpeed.Subscribe(speed => _navMeshAgent.speed = speed).AddTo(_disposables);
        }

        public bool MoveToPosition(Vector3 destination)
        {
            if (!_pathfindingService.CalculatePath(transform.position, destination, _currentPath))
            {
                return false;
            }

            _isMoving = true;
            // Ensure stopping distance is appropriate for general movement
            _navMeshAgent.stoppingDistance = _destinationReachedThreshold;
            _navMeshAgent.SetDestination(destination);
            return true;
        }

        // Method for player-issued move commands
        public bool ManualMoveToPosition(Vector3 destination)
        {
            Debug.Log($"[{_unit.name}] Received manual move command to {destination}");
            _isUnderManualControl = true;
            IsInCombat = false; // Break combat
            _currentTarget = null; // Clear target
            StopMoving(); // Stop current path/action
            return MoveToPosition(destination); // Start new move
        }

        public bool MoveToRandomPositionNear(Vector3 center, float radius)
        {
            if (!_pathfindingService.TryGetRandomPointNear(center, radius, out Vector3 randomPoint))
            {
                return false;
            }

            return MoveToPosition(randomPoint);
        }

        public void StopMoving()
        {
            _isMoving = false;
            _navMeshAgent.ResetPath();
            _currentPath.Clear();
        }

        public bool IsDestinationReachable(Vector3 destination)
        {
            return _pathfindingService.IsReachable(transform.position, destination);
        }

        // Find nearest enemy within detection range
        public Unit FindNearestEnemy(List<Unit> potentialTargets)
        {
            Unit nearestEnemy = null;
            float closestDistance = _detectionRange;

            foreach (Unit potentialTarget in potentialTargets)
            {
                // Skip if null, dead, or same team
                if (potentialTarget == null || potentialTarget.State.Health.Value <= 0 || _unit.IsAlly(potentialTarget))
                {
                    continue;
                }

                float distance = Vector3.Distance(transform.position, potentialTarget.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    nearestEnemy = potentialTarget;
                }
            }

            return nearestEnemy;
        }

        // Engage a target
        public void EngageTarget(Unit target)
        {
            if (target == null)
                return;

            _currentTarget = target;
            IsInCombat = true;

            // Set stopping distance to attack range when engaging
            _navMeshAgent.stoppingDistance = _attackRange * 0.8f;
        }

        // Attack the current target
        public void AttackTarget()
        {
            if (_currentTarget == null || _currentTarget.State.Health.Value <= 0 || Time.time - _lastAttackTime < _attackCooldown)
            {
                return;
            }

            // Make sure we're facing the target
            Vector3 targetDirection = (_currentTarget.transform.position - transform.position).normalized;
            targetDirection.y = 0; // Keep rotation on the horizontal plane

            // Only rotate if we have a direction
            if (targetDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(targetDirection),
                    _rotationSpeed * Time.deltaTime
                );
            }

            float distanceToTarget = Vector3.Distance(transform.position, _currentTarget.transform.position);

            // Only attack if within range
            if (distanceToTarget <= _attackRange)
            {
                // Perform attack
                _lastAttackTime = Time.time;

                // Play attack animation
                if (_unit != null && _animationController != null)
                {
                    _animationController.PlayAttackAnimation();
                }

                // Calculate damage
                float damage = _unit.State.DamageOutput.Value;

                // Apply damage to target
                float targetHealth = _currentTarget.State.Health.Value;
                float targetDamageReduction = _currentTarget.State.IncomingDamageReduction.Value;
                
                // Calculate final damage after reduction
                float finalDamage = Mathf.Max(1, damage * (1 - targetDamageReduction));
                
                // Apply damage to target's health
                _currentTarget.State.Health.Value = Mathf.Max(0, targetHealth - finalDamage);
                
                // Record damage dealt for XP calculation
                _unit.RecordDamageDealt(finalDamage);
                
                // If target died from this attack, record the kill
                if (_currentTarget.State.Health.Value <= 0)
                {
                    // Record kill for XP calculation
                    _unit.RecordKill();
                    
                    // Clear current target since it's dead
                    _currentTarget = null;
                    IsInCombat = false;
                }
            }
            else
            {
                // Move closer to target if not in range
                _navMeshAgent.SetDestination(_currentTarget.transform.position);
                _isMoving = true;
            }
        }

        // Check if current target is valid and in range
        private bool IsTargetValid()
        {
            return _currentTarget != null
                && _currentTarget.State.Health.Value > 0
                && Vector3.Distance(transform.position, _currentTarget.transform.position) <= _detectionRange;
        }

        private void Update()
        {
            // Don't update if unit is dead
            if (_unit == null || _unit.State.Health.Value <= 0)
            {
                return;
            }

            // Handle Manual Movement Completion
            if (_isUnderManualControl && _isMoving)
            {
                // Check if the agent has reached the destination set by manual command
                if (!_navMeshAgent.pathPending && _navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance)
                {
                    Debug.Log($"[{_unit.name}] Reached manual destination.");
                    StopMoving(); // Important to reset _isMoving flag
                    _isUnderManualControl = false; // Allow AI to take over again
                    // No need to find a target here, CombatSystem's AI loop will handle it
                }
                // If manually moving, skip AI combat/targeting logic below
                return;
            }

            // --- Existing AI/Combat Logic --- (Ensure this runs only if NOT under manual control)
            if (!_isUnderManualControl)
            {
                // If in combat, prioritize attacking or moving towards target
                if (IsInCombat && IsTargetValid())
                {
                    float distanceToTarget = Vector3.Distance(transform.position, _currentTarget.transform.position);

                    // If target is in range, attack
                    if (distanceToTarget <= _attackRange)
                    {
                        StopMoving(); // Stop moving when in attack range
                        AttackTarget();
                    }
                    // If target is not in range, move towards them
                    else
                    {
                        // Update destination only periodically or if path is invalid
                        if (!_navMeshAgent.hasPath || _navMeshAgent.isPathStale || Time.frameCount % 10 == 0) // Example check
                        {
                            MoveToPosition(_currentTarget.transform.position);
                        }
                    }
                }
                else
                {
                    // No longer in combat or target is invalid, reset combat state
                    IsInCombat = false;
                    _currentTarget = null;
                    // Reset stopping distance to default movement threshold when not engaging
                    _navMeshAgent.stoppingDistance = _destinationReachedThreshold;

                    // If we were moving (_isMoving will be true from MoveToPosition call),
                    // check if we have arrived at a non-combat destination.
                    if (_isMoving && !_navMeshAgent.pathPending && _navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance)
                    {
                        StopMoving();
                    }
                }
            }

            // Update Animations (regardless of manual control, should reflect current state)
            // _animationController.UpdateAnimations(_isMoving, IsInCombat); // Removed: UnitAnimationController updates movement itself
        }

        /// <summary>
        /// Resets combat state for the unit, used when reviving between rounds
        /// </summary>
        public void ResetCombatState()
        {
            StopMoving();
            _currentTarget = null;
            IsInCombat = false;
            _isUnderManualControl = false; // Ensure manual control is reset on round changes/revival
            _lastAttackTime = 0f;
            _navMeshAgent.stoppingDistance = _destinationReachedThreshold;
            if (_animationController != null)
            {
                // _animationController.ResetState(); // Replaced with PlayReviveAnimation
                _animationController.PlayReviveAnimation();
            }
        }

        /// <summary>
        /// Makes the unit perform a victory dance
        /// </summary>
        public void PlayVictoryAnimation()
        {
            if (_animationController != null)
            {
                // Stop movement
                if (_navMeshAgent != null && _navMeshAgent.enabled)
                {
                    _navMeshAgent.isStopped = true;
                    _navMeshAgent.ResetPath();
                }
                
                _isMoving = false;
                
                // Play victory animation
                _animationController.PlayVictoryAnimation();
            }
        }

        // Use 'new' keyword since base Dispose is not virtual
        new public void Dispose()
        {
            base.Dispose();
            _currentTarget = null;
            IsInCombat = false;
            _currentPath.Clear();
        }
    }
}
