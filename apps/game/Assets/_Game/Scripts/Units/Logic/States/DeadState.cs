using UnityEngine;

namespace VoxelCommand.Client
{
    /// <summary>
    /// State when unit is dead
    /// </summary>
    public class DeadState : IUnitState
    {
        private readonly UnitController _controller;
        
        public DeadState(UnitController controller)
        {
            _controller = controller;
        }
        
        public void Enter(IUnitState previousState)
        {
            _controller.PlayDeathAnimation();
        }
        
        public void Update()
        {
            // Dead units don't do anything
        }

        public void Exit(IUnitState nextState)
        {
            // Nothing to clean up, dead is permanent
        }
    }
} 