using UnityEngine;
using UnityEngine.AI;

namespace VoxelCommand.Client
{
    /// <summary>
    /// State when unit is preparing for a round by moving away from other units
    /// </summary>
    public class PreparingState : IUnitState
    {
        private readonly UnitController _controller;

        public PreparingState(UnitController controller)
        {
            _controller = controller;
        }
        
        public void Enter(IUnitState previousState)
        {
            _controller.IsMoving = true;
            FindSafePosition();
        }
        
        public void Update()
        {
            // Check if destination reached
            NavMeshAgent agent = _controller.NavMeshAgent;
            if (
                agent.enabled
                && agent.isOnNavMesh
                && !agent.pathPending
                && agent.remainingDistance <= agent.stoppingDistance
            )
            {
                _controller.StopMoving();
                _controller.TransitionToState(_controller.IdleState);
            }
        }
        
        public void Exit(IUnitState nextState)
        {
            // Nothing specific to clean up
        }

        private void FindSafePosition()
        {
            // Find a position away from other units
            Vector3 currentPosition = _controller.transform.position;
            float preparationDistance = 5f;
            Vector3 randomDirection = Random.insideUnitSphere * preparationDistance;
            randomDirection.y = 0; // Keep on the same plane
            
            Vector3 targetPosition = currentPosition + randomDirection;
            
            // Use the pathfinding service to find a valid position
            if (_controller.MoveToPosition(targetPosition))
            {
                _controller.NavMeshAgent.stoppingDistance = 0.1f; // Be precise about reaching the preparation position
            }
            else
            {
                // If we can't find a valid position, just go to idle
                _controller.TransitionToState(_controller.IdleState);
            }
        }
    }
} 