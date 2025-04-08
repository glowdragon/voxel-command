using UnityEngine;
using UnityEngine.AI;

namespace VoxelCommand.Client
{
    /// <summary>
    /// State when unit is scouting or exploring the area
    /// </summary>
    public class ScoutingState : IUnitState
    {
        private readonly UnitController _controller;
        
        public ScoutingState(UnitController controller)
        {
            _controller = controller;
        }
        
        public void Enter(IUnitState previousState)
        {
            _controller.IsMoving = true;
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
                if (!_controller.IsUnderManualControl)
                {
                    _controller.TransitionToState(_controller.IdleState);
                }
                else
                {
                    _controller.IsUnderManualControl = false;
                }
            }
        }
        
        public void Exit(IUnitState nextState)
        {
            // Nothing specific to clean up
        }
    }
} 