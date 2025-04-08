using System;
using UniRx;
using UnityEngine;

namespace VoxelCommand.Client
{
    public class UnitState : MonoBehaviour
    {
        public IntReactiveProperty Experience = new(0);
        public IntReactiveProperty Level = new(1);
        public IntReactiveProperty AvailableStatPoints = new(0);

        public IntReactiveProperty HealthRank = new(0);
        public IntReactiveProperty DamageRank = new(0);
        public IntReactiveProperty DefenseRank = new(0);
        public IntReactiveProperty SpeedRank = new(0);

        public FloatReactiveProperty MaxHealth = new(1);
        public FloatReactiveProperty DamageOutput = new(1);
        public FloatReactiveProperty IncomingDamageReduction = new(0);
        public FloatReactiveProperty MovementSpeed = new(1);

        public FloatReactiveProperty Health = new(1);

        // Battle statistics
        public FloatReactiveProperty DamageDealt = new(0);
        public IntReactiveProperty Kills = new(0);

        public bool IsDead => Health.Value <= 0;

        public int GetStatRank(StatType statType)
        {
            switch (statType)
            {
                case StatType.Health:
                    return HealthRank.Value;
                case StatType.Damage:
                    return DamageRank.Value;
                case StatType.Defense:
                    return DefenseRank.Value;
                case StatType.Speed:
                    return SpeedRank.Value;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
