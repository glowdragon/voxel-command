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
            _navMeshAgent.SetDestination(destination);
            return true;
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
            if (_unit == null || !_navMeshAgent.enabled)
                return;

            // Check if we've reached the destination for movement logic
            if (_isMoving && !_navMeshAgent.pathPending && _navMeshAgent.remainingDistance <= _destinationReachedThreshold)
            {
                _isMoving = false;
            }

            // Combat logic
            if (IsInCombat)
            {
                // If target is invalid, stop combat
                if (!IsTargetValid())
                {
                    IsInCombat = false;
                    _currentTarget = null;
                    _navMeshAgent.stoppingDistance = 0.1f; // Reset stopping distance
                    return;
                }

                // Attack if possible
                AttackTarget();
            }
        }

        /// <summary>
        /// Resets combat state for the unit, used when reviving between rounds
        /// </summary>
        public void ResetCombatState()
        {
            // Clear combat flags
            _currentTarget = null;
            IsInCombat = false;
            _lastAttackTime = 0f;
            
            // Stop any navigation in progress
            if (_navMeshAgent != null)
            {
                // Re-enable the NavMeshAgent if it was disabled (e.g., when the unit died)
                _navMeshAgent.enabled = true;
                
                _navMeshAgent.ResetPath();
                _navMeshAgent.isStopped = true;
                _navMeshAgent.isStopped = false;
                _navMeshAgent.stoppingDistance = 0.1f;
            }
            
            _isMoving = false;
            
            // Play revive animation if the unit was dead
            if (_animationController != null)
            {
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
