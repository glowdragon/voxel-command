using UnityEngine;
using Zenject;

namespace VoxelCommand.Client
{
    public class BattleInstaller : MonoInstaller
    {
        [SerializeField]
        private BattleManager _battleManager;

        [SerializeField]
        private RoundManager _roundManager;

        [SerializeField]
        private TeamManager _teamManager;

        [SerializeField]
        private UnitSpawner _unitSpawner;

        [SerializeField]
        private CombatSystem _combatSystem;

        public override void InstallBindings()
        {
            Container.BindInstance(_battleManager).AsSingle();
            Container.BindInstance(_teamManager).AsSingle();
            Container.BindInstance(_roundManager).AsSingle();
            Container.BindInstance(_unitSpawner).AsSingle();
            Container.BindInstance(_combatSystem).AsSingle();
        }
    }
}
