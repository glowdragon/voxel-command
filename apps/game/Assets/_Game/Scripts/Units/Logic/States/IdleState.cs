namespace VoxelCommand.Client
{
    /// <summary>
    /// State when unit is idle
    /// </summary>
    public class IdleState : IUnitState
    {
        private readonly UnitController _controller;
        
        public IdleState(UnitController controller)
        {
            _controller = controller;
        }
        
        public void Enter(IUnitState previousState)
        {
            _controller.IsMoving = false;
            _controller.IsInCombat = false;
        }
        
        public void Update()
        {
            // Idle state just waits for commands or AI decisions
        }
        
        public void Exit(IUnitState nextState)
        {
            // Nothing specific to clean up
        }
    }
} 