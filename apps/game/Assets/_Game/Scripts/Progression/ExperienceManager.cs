using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace VoxelCommand.Client
{
    public class ExperienceManager : MonoBehaviour
    {
        [Inject]
        private UnitManager _unitManager;

        /// <summary>
        /// Awards experience to all units based on their performance
        /// </summary>
        public void AwardExperience()
        {
            AwardExperienceToUnits(_unitManager.Units.ToList());
        }

        /// <summary>
        /// Awards experience to all units based on their performance
        /// </summary>
        private void AwardExperienceToUnits(List<Unit> units)
        {
            foreach (Unit unit in units)
            {
                if (unit != null)
                {
                    // Calculate XP based on damage dealt and kills
                    int damageXp = Mathf.RoundToInt(unit.State.DamageDealt.Value * 0.5f);
                    int killXp = unit.State.Kills.Value * 100;
                    int totalXp = damageXp + killXp;

                    // Award XP
                    unit.AddExperience(totalXp);

                    Debug.Log($"{unit.name} earned {totalXp} XP (Damage: {unit.State.DamageDealt.Value}, Kills: {unit.State.Kills.Value})");

                    // Reset stats for next round
                    unit.ResetBattleStats();
                }
            }
        }
    }
} 