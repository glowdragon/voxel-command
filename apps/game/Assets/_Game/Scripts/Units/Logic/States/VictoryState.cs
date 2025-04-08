using UnityEngine;

namespace VoxelCommand.Client
{
    /// <summary>
    /// State when unit is celebrating victory
    /// </summary>
    public class VictoryState : IUnitState
    {
        private readonly UnitController _controller;
        private float _victoryDuration = 3f; // Duration of victory celebration
        private float _timeInState;
        
        public VictoryState(UnitController controller)
        {
            _controller = controller;
        }
        
        public void Enter(IUnitState previousState)
        {
            _controller.IsMoving = false;
            _controller.PlayVictoryAnimation();
            _timeInState = 0f;
        }

        public void Update()
        {
            _timeInState += Time.deltaTime;

            // After victory duration, return to idle
            if (_timeInState >= _victoryDuration)
            {
                _controller.TransitionToState(_controller.PreparingState);
            }
        }
        
        public void Exit(IUnitState nextState)
        {
            // Nothing specific to clean up
        }
    }
} 