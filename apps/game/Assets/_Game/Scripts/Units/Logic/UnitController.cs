using System;
using System.Collections.Generic;
using DanielKreitsch;
using UniRx;
using UnityEngine;
using UnityEngine.AI;
using Zenject;

namespace VoxelCommand.Client
{
    public class UnitController : DisposableMonoBehaviour
    {
        [Inject]
        private IPathfindingService _pathfindingService;

        private Unit _unit;
        private readonly List<Vector3> _currentPath = new();
        private bool _isMoving;
        private bool _isUnderManualControl;
        private float _lastAttackTime;

        [Header("References")]
        [SerializeField]
        private UnitAnimationController _animationController;
        public UnitAnimationController AnimationController => _animationController;

        [SerializeField]
        private NavMeshAgent _navMeshAgent;

        [Header("Movement Settings")]
        [SerializeField]
        private float _destinationReachedThreshold = 0.2f;

        [SerializeField]
        private float _rotationSpeed = 10f;

        [Header("Combat Settings")]
        [SerializeField, Tooltip("Distance from which unit can attack")]
        private float _attackRange = 1f;

        [SerializeField, Tooltip("Distance at which unit detects enemies")]
        private float _detectionRange = 5f;

        [SerializeField, Tooltip("Time between attacks")]
        private float _attackCooldown = 1f;

        private Unit _currentTarget;

        // Properties
        public bool IsInCombat { get; private set; }
        public Unit CurrentTarget => _currentTarget;
        public bool IsUnderManualControl => _isUnderManualControl;

        public void Initialize(Unit unit)
        {
            _unit = unit;

            SetupSubscriptions();
            _navMeshAgent.enabled = true;
            _lastAttackTime = 0f;
            IsInCombat = false;
            _currentTarget = null;
            _isUnderManualControl = false;

            _animationController.Initialize(_unit);
        }

        private void SetupSubscriptions()
        {
            _unit.State.MovementSpeed.Subscribe(speed => _navMeshAgent.speed = speed).AddTo(_disposables);
        }

        public bool MoveToPosition(Vector3 destination)
        {
            if (!_pathfindingService.CalculatePath(transform.position, destination, _currentPath))
            {
                return false;
            }

            _isMoving = true;
            _navMeshAgent.stoppingDistance = _destinationReachedThreshold;
            _navMeshAgent.SetDestination(destination);
            return true;
        }

        public bool ManualMoveToPosition(Vector3 destination)
        {
            _isUnderManualControl = true;
            IsInCombat = false;
            _currentTarget = null;
            StopMoving();
            return MoveToPosition(destination);
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
            // _navMeshAgent.ResetPath();
            _currentPath.Clear();
        }

        public bool IsDestinationReachable(Vector3 destination)
        {
            return _pathfindingService.IsReachable(transform.position, destination);
        }

        public Unit FindNearestEnemy(IEnumerable<Unit> potentialTargets)
        {
            Unit nearestEnemy = null;
            float closestDistance = _detectionRange;

            foreach (Unit potentialTarget in potentialTargets)
            {
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

        public void EngageTarget(Unit target)
        {
            if (target == null)
                return;

            _currentTarget = target;
            IsInCombat = true;
            _navMeshAgent.stoppingDistance = _attackRange * 0.8f;
        }

        public void AttackTarget()
        {
            if (_currentTarget == null || _currentTarget.State.Health.Value <= 0 || Time.time - _lastAttackTime < _attackCooldown)
            {
                return;
            }

            FaceTarget();

            float distanceToTarget = Vector3.Distance(transform.position, _currentTarget.transform.position);

            if (distanceToTarget <= _attackRange)
            {
                PerformAttack();
            }
            else
            {
                MoveToPosition(_currentTarget.transform.position);
            }
        }

        private void PerformAttack()
        {
            _lastAttackTime = Time.time;
            _animationController?.PlayAttackAnimation();
            Observable
                .Timer(TimeSpan.FromSeconds(0.2f))
                .Subscribe(_ =>
                {
                    if (_currentTarget != null && _currentTarget.transform)
                    {
                        _currentTarget.Controller.AnimationController?.PlayHitReactionAnimation();
                    }
                })
                .AddTo(_disposables);

            float damage = _unit.State.DamageOutput.Value;
            float targetHealth = _currentTarget.State.Health.Value;
            float targetDamageReduction = _currentTarget.State.IncomingDamageReduction.Value;
            float finalDamage = Mathf.Max(1, damage * (1 - targetDamageReduction));

            _currentTarget.State.Health.Value = Mathf.Max(0, targetHealth - finalDamage);
            _unit.RecordDamageDealt(finalDamage);

            if (_currentTarget.State.Health.Value <= 0)
            {
                if (_currentTarget != null && _currentTarget.transform)
                {
                    _currentTarget.Controller.AnimationController?.PlayDeathAnimation();
                }

                _unit.RecordKill();
                _currentTarget = null;
                IsInCombat = false;
            }
        }

        private void FaceTarget()
        {
            if (_currentTarget == null)
                return;

            Vector3 targetDirection = (_currentTarget.transform.position - transform.position).normalized;
            targetDirection.y = 0;

            if (targetDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(targetDirection),
                    _rotationSpeed * Time.deltaTime
                );
            }
        }

        private bool IsTargetValid()
        {
            return _currentTarget != null
                && _currentTarget.State.Health.Value > 0
                && Vector3.Distance(transform.position, _currentTarget.transform.position) <= _detectionRange;
        }

        private void Update()
        {
            if (_unit == null || _unit.State.Health.Value <= 0)
            {
                return;
            }

            if (_isUnderManualControl)
            {
                HandleManualControl();
            }
            else
            {
                HandleAILogic();
            }
        }

        private void HandleManualControl()
        {
            if (!_isMoving)
                return;

            if (!_navMeshAgent.pathPending && _navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance)
            {
                StopMoving();
                _isUnderManualControl = false;
            }
        }

        private void HandleAILogic()
        {
            if (IsInCombat && IsTargetValid())
            {
                HandleCombat();
            }
            else
            {
                HandleNonCombat();
            }
        }

        private void HandleCombat()
        {
            float distanceToTarget = Vector3.Distance(transform.position, _currentTarget.transform.position);

            if (distanceToTarget <= _attackRange)
            {
                StopMoving();
                AttackTarget();
            }
            else
            {
                if (!_navMeshAgent.hasPath || _navMeshAgent.isPathStale || Time.frameCount % 10 == 0)
                {
                    MoveToPosition(_currentTarget.transform.position);
                }
            }
        }

        private void HandleNonCombat()
        {
            IsInCombat = false;
            _currentTarget = null;
            _navMeshAgent.stoppingDistance = _destinationReachedThreshold;

            if (_isMoving && !_navMeshAgent.pathPending && _navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance)
            {
                StopMoving();
            }
        }

        public void PlayVictoryAnimation()
        {
            if (_animationController == null)
                return;

            if (_navMeshAgent != null && _navMeshAgent.enabled)
            {
                _navMeshAgent.isStopped = true;
                // _navMeshAgent.ResetPath();
            }

            _isMoving = false;
            _animationController.PlayVictoryAnimation();
        }

        public void ResetCombatState()
        {
            StopMoving();
            _currentTarget = null;
            IsInCombat = false;
            _isUnderManualControl = false;
            _lastAttackTime = 0f;
            _navMeshAgent.stoppingDistance = _destinationReachedThreshold;

            _animationController?.PlayReviveAnimation();
        }

        public new void Dispose()
        {
            base.Dispose();
            _currentTarget = null;
            IsInCombat = false;
            _currentPath.Clear();
        }
    }
}
