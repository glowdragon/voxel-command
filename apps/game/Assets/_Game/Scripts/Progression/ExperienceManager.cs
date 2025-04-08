using UniRx;
using UnityEngine;
using Zenject;

namespace VoxelCommand.Client
{
    public class ExperienceManager : MonoBehaviour
    {
        [Inject]
        private IMessageBroker _messageBroker;

        private void Start()
        {
            // Award experience for attacks
            _messageBroker
                .Receive<UnitDamagedEvent>()
                .Subscribe(e => AwardExperienceForAttack(e.SourceUnit, e.TargetUnit, e.DamageAmount));

            // Award experience for kills
            _messageBroker //
                .Receive<UnitDeathEvent>()
                .Subscribe(e => AwardExperienceForKill(e.Killer, e.Victim));
        }

        private void AwardExperienceForAttack(Unit attacker, Unit target, float damageAmount)
        {
            var victimLevel = target.State.Level.Value;
            var xp = Mathf.RoundToInt(attacker.Config.ExperiencePerDamage * damageAmount * victimLevel);
            attacker.State.Experience.Value += xp;
        }

        private void AwardExperienceForKill(Unit killer, Unit victim)
        {
            var victimLevel = victim.State.Level.Value;
            var xp = Mathf.RoundToInt(killer.Config.ExperiencePerKill * victimLevel);
            killer.State.Experience.Value += xp;
        }
    }
}
