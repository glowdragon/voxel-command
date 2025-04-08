using UnityEngine;

namespace VoxelCommand.Client
{
    /// <summary>
    /// Interface for all unit states
    /// </summary>
    public interface IUnitState
    {
        void Enter(IUnitState previousState);
        void Update();
        void Exit(IUnitState nextState);
    }
} 