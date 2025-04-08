using System;
using UnityEngine;

namespace VoxelCommand.Client
{
    public interface IUnitStatsCalculator
    {
        int CalculateExperienceForLevel(UnitConfig config, UnitState state);
        int CalculateExperienceForLevel(UnitConfig config, int level);
        int CalculateExperienceForNextLevel(UnitConfig config, UnitState state);
        int CalculateExperienceForNextLevel(UnitConfig config, int level);
        int CalculateLevelFromExperience(UnitConfig config, UnitState state);
        float CalculateMaxHealth(UnitConfig config, UnitState state);
        float CalculateMaxHealth(UnitConfig config, int healthRank);
        float CalculateDamageOutput(UnitConfig config, UnitState state);
        float CalculateDamageOutput(UnitConfig config, int damageRank);
        float CalculateIncomingDamageReduction(UnitConfig config, UnitState state);
        float CalculateIncomingDamageReduction(UnitConfig config, int defenseRank);
        float CalculateMovementSpeed(UnitConfig config, UnitState state);
        float CalculateMovementSpeed(UnitConfig config, int speedRank);
    }

    public class UnitStatsCalculator : IUnitStatsCalculator
    {
        public int CalculateExperienceForLevel(UnitConfig config, UnitState state)
        {
            return CalculateExperienceForLevel(config, state.Level.Value);
        }

        public int CalculateExperienceForLevel(UnitConfig config, int level)
        {
            if (level <= 1) return 0;
            
            return Mathf.RoundToInt(config.BaseExperience * Mathf.Pow(level - 1, config.ExperienceGrowthFactor));
        }

        public int CalculateExperienceForNextLevel(UnitConfig config, UnitState state)
        {
            return CalculateExperienceForNextLevel(config, state.Level.Value);
        }

        public int CalculateExperienceForNextLevel(UnitConfig config, int level)
        {
            if (level <= 0) return 0;
            
            return Mathf.RoundToInt(config.BaseExperience * Mathf.Pow(level, config.ExperienceGrowthFactor));
        }

        public int CalculateLevelFromExperience(UnitConfig config, UnitState state)
        {
            int experience = state.Experience.Value;
            if (experience <= 0) return 1;
            
            int level = 1;
            while (CalculateExperienceForNextLevel(config, level) <= experience)
            {
                level++;
            }
            
            return level;
        }

        public float CalculateMaxHealth(UnitConfig config, UnitState state)
        {
            return CalculateMaxHealth(config, state.HealthRank.Value);
        }

        public float CalculateMaxHealth(UnitConfig config, int healthRank)
        {
            return config.BaseHealth + (healthRank * config.HealthPerPoint);
        }

        public float CalculateDamageOutput(UnitConfig config, UnitState state)
        {
            return CalculateDamageOutput(config, state.DamageRank.Value);
        }

        public float CalculateDamageOutput(UnitConfig config, int damageRank)
        {
            return config.BaseDamage + (damageRank * config.DamagePerPoint);
        }

        public float CalculateIncomingDamageReduction(UnitConfig config, UnitState state)
        {
            return CalculateIncomingDamageReduction(config, state.DefenseRank.Value);
        }

        public float CalculateIncomingDamageReduction(UnitConfig config, int defenseRank)
        {
            return config.BaseDefense + (defenseRank * config.DefensePerPoint);
        }

        public float CalculateMovementSpeed(UnitConfig config, UnitState state)
        {
            return CalculateMovementSpeed(config, state.SpeedRank.Value);
        }

        public float CalculateMovementSpeed(UnitConfig config, int speedRank)
        {
            return config.BaseSpeed + (speedRank * config.SpeedPerPoint);
        }
    }
}
