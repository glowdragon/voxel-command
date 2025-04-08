using System;
using UniRx;
using UnityEngine;
using UnityEngine.AI;

namespace VoxelCommand.Client
{
    /// <summary>
    /// State when unit is in combat
    /// </summary>
    public class CombatState : IUnitState
    {
        private readonly UnitController _controller;

        public CombatState(UnitController controller)
        {
            _controller = controller;
        }

        public void Enter(IUnitState previousState)
        {
            _controller.IsInCombat = true;
        }

        public void Update()
        {
            if (!_controller.IsTargetValid())
            {
                _controller.TransitionToState(_controller.IdleState);
                return;
            }

            float distanceToTarget = Vector3.Distance(_controller.transform.position, _controller.Opponent.transform.position);

            if (distanceToTarget <= _controller.AttackRange)
            {
                // In attack range - stop and attack
                _controller.StopMoving();
                Observable
                    .Timer(TimeSpan.FromSeconds(UnityEngine.Random.value * 3f))
                    .Subscribe(_ =>
                    {
                        _controller.AttackTarget();
                    });
            }
            else
            {
                // Not in attack range - move towards target
                NavMeshAgent agent = _controller.NavMeshAgent;
                if (agent.enabled && agent.isOnNavMesh)
                {
                    _controller.MoveToPosition(_controller.Opponent.transform.position);
                }
            }
        }

        public void Exit(IUnitState nextState)
        {
            // Nothing specific to clean up
        }
    }
}
