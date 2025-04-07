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
        [SerializeField]
        private float _attackRange = 1f; // Distance from which unit can attack

        [SerializeField]
        private float _detectionRange = 5f; // Distance at which unit detects enemies

        [SerializeField]
        private float _attackCooldown = 1f; // Time between attacks

        private Unit _currentTarget;
        private float _lastAttackTime;
        public bool IsInCombat { get; private set; }
        public Unit CurrentTarget => _currentTarget;

        [Inject]
        private IPathfindingService _pathfindingService;

        public void Initialize(Unit unit)
        {
            Dispose();
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

                // Apply damage to target after a small delay (matching animation)
                Observable
                    .Timer(System.TimeSpan.FromSeconds(0.12f))
                    .Subscribe(_ => ApplyDamage(_currentTarget, damage))
                    .AddTo(_disposables);
            }
            else
            {
                // Move closer to target if out of range
                MoveToPosition(_currentTarget.transform.position);
            }
        }

        // Apply damage to a target
        private void ApplyDamage(Unit target, float damage)
        {
            if (target == null || target.State.Health.Value <= 0)
                return;

            // Calculate final damage after reduction
            float damageReduction = target.State.IncomingDamageReduction.Value;
            float finalDamage = Mathf.Max(1f, damage - damageReduction);

            // Apply damage
            target.State.Health.Value -= finalDamage;

            Debug.Log($"{_unit.name} dealt {finalDamage} damage to {target.name}");
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
