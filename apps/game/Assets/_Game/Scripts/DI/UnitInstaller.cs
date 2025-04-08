using UnityEngine;
using Zenject;

namespace VoxelCommand.Client
{
    public class UnitInstaller : MonoInstaller
    {
        [SerializeField]
        private UnitManager _unitManager;

        [SerializeField]
        private TeamManager _teamManager;

        [SerializeField]
        private UnitSpawner _unitSpawner;

        [SerializeField]
        private GameObject _unitPrefab;

        public override void InstallBindings()
        {
            Container.BindInstance(_unitManager).AsSingle();
            Container.BindInstance(_teamManager).AsSingle();
            Container.BindInstance(_unitSpawner).AsSingle();
            Container.BindFactory<Unit, UnitFactory>().FromComponentInNewPrefab(_unitPrefab).UnderTransformGroup("Units").AsSingle();
        }
    }
}
