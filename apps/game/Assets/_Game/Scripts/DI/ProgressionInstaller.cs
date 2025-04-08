using UnityEngine;
using Zenject;

namespace VoxelCommand.Client
{
    public class ProgressionInstaller : MonoInstaller
    {
        [SerializeField]
        private ExperienceManager _experienceManager;

        [SerializeField]
        private SkillPointAllocationManager _skillPointAllocationManager;

        public override void InstallBindings()
        {
            Container.BindInstance(_experienceManager).AsSingle();
            Container.BindInstance(_skillPointAllocationManager).AsSingle();
        }
    }
}
