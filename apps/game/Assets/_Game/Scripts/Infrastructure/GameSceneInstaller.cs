using UnityEngine;
using VoxelCommand.Client;
using Zenject;

public class GameSceneInstaller : MonoInstaller
{
    [SerializeField]
    private BattleManager _battleManager;

    [SerializeField]
    private UnitManager _unitManager;

    [SerializeField]
    private GameObject _unitPrefab;

    public override void InstallBindings()
    {
        // Bind scene-specific references
        Container.BindInstance(_battleManager).AsSingle();
        Container.BindInstance(_unitManager).AsSingle();

        // Factory for creating units
        Container.BindFactory<Unit, UnitFactory>().FromComponentInNewPrefab(_unitPrefab).UnderTransformGroup("Units").AsSingle();
    }
}
