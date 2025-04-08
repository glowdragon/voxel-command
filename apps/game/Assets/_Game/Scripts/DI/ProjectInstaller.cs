using UnityEngine;
using Zenject;

namespace VoxelCommand.Client
{
    public class ProjectInstaller : MonoInstaller
    {
        [SerializeField]
        private NameGenerator _nameGenerator;

        public override void InstallBindings()
        {
            Container.Bind<INameGenerator>().FromInstance(_nameGenerator).AsSingle();
            Container.Bind<IUnitStatsCalculator>().To<UnitStatsCalculator>().AsSingle();
        }
    }
}
