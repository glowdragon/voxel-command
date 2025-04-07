using UnityEngine;
using Zenject;

namespace VoxelCommand.Client
{
    /// <summary>
    /// Project-level dependency installer for the Zenject DI container.
    /// Handles registering services that should persist throughout the entire game.
    /// </summary>
    public class ProjectInstaller : MonoInstaller
    {
        [SerializeField]
        private UnitConfig _defaultUnitConfig;

        /// <summary>
        /// Installs all project-level service bindings into the Zenject container.
        /// </summary>
        public override void InstallBindings()
        {
            Container.BindInstance(_defaultUnitConfig).AsSingle();
            
            Container.Bind<IUnitStatsCalculator>().To<UnitStatsCalculator>().AsSingle();
        }
    }
}
