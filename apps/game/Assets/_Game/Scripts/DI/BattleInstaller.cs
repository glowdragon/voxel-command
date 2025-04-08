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
        private CombatSystem _combatSystem;

        [SerializeField]
        private CombatLogManager _combatLogManager;

        public override void InstallBindings()
        {
            Container.BindInstance(_battleManager).AsSingle();
            Container.BindInstance(_roundManager).AsSingle();
            Container.BindInstance(_combatSystem).AsSingle();
            Container.BindInstance(_combatLogManager).AsSingle();
        }
    }
}
