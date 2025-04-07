using System;
using UnityEngine;

namespace VoxelCommand.Client
{
    public interface IUnitStatsCalculator
    {
        int CalculateExperienceForLevel(UnitConfig config, UnitState state);
        int CalculateLevelFromExperience(UnitConfig config, UnitState state);
        float CalculateMaxHealth(UnitConfig config, UnitState state);
        float CalculateDamageOutput(UnitConfig config, UnitState state);
        float CalculateIncomingDamageReduction(UnitConfig config, UnitState state);
        float CalculateMovementSpeed(UnitConfig config, UnitState state);
    }

    public class UnitStatsCalculator : IUnitStatsCalculator
    {
        public int CalculateExperienceForLevel(UnitConfig config, UnitState state)
        {
            int level = state.Level.Value;
            if (level <= 1) return 0;
            
            return Mathf.RoundToInt(config.BaseExperience * Mathf.Pow(level - 1, config.ExperienceGrowthFactor));
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

        private int CalculateExperienceForNextLevel(UnitConfig config, int level)
        {
            if (level <= 0) return 0;
            
            return Mathf.RoundToInt(config.BaseExperience * Mathf.Pow(level, config.ExperienceGrowthFactor));
        }

        public float CalculateMaxHealth(UnitConfig config, UnitState state)
        {
            return config.BaseHealth + (state.HealthRank.Value * config.HealthPerPoint);
        }

        public float CalculateDamageOutput(UnitConfig config, UnitState state)
        {
            return config.BaseDamage + (state.DamageRank.Value * config.DamagePerPoint);
        }

        public float CalculateIncomingDamageReduction(UnitConfig config, UnitState state)
        {
            return config.BaseDefense + (state.DefenseRank.Value * config.DefensePerPoint);
        }

        public float CalculateMovementSpeed(UnitConfig config, UnitState state)
        {
            return config.BaseSpeed + (state.SpeedRank.Value * config.SpeedPerPoint);
        }
    }
}
