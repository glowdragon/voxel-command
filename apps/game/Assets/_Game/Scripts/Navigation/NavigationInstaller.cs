using UnityEngine;
using Zenject;

namespace VoxelCommand.Client
{
    public class NavigationInstaller : MonoInstaller
    {
        [SerializeField]
        private PathfindingService _pathfindingService;
        
        public override void InstallBindings()
        {
            // Bind PathfindingService
            Container.Bind<IPathfindingService>()
                .FromInstance(_pathfindingService)
                .AsSingle();
        }
    }
} 