using UnityEngine;
using Zenject;

namespace VoxelCommand.Client
{
    public class UnitInstaller : MonoInstaller
    {
        [SerializeField]
        private UnitManager _unitManager;

        [SerializeField]
        private GameObject _unitPrefab;

        public override void InstallBindings()
        {
            Container.BindInstance(_unitManager).AsSingle();
            Container.BindFactory<Unit, UnitFactory>().FromComponentInNewPrefab(_unitPrefab).UnderTransformGroup("Units").AsSingle();
        }
    }
}
