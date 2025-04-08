using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace VoxelCommand.Client
{
    public class UnitState : MonoBehaviour
    {
        public IntReactiveProperty Experience = new(0);
        public IntReactiveProperty Level = new(1);
        public IntReactiveProperty AvailableSkillPoints = new(0);

        public Dictionary<SkillType, IntReactiveProperty> Skills = new();
        public IntReactiveProperty HealthSkill = new(0);
        public IntReactiveProperty StrengthSkill = new(0);
        public IntReactiveProperty DefenseSkill = new(0);
        public IntReactiveProperty SpeedSkill = new(0);

        public FloatReactiveProperty MaxHealth = new(1);
        public FloatReactiveProperty DamageOutput = new(1);
        public FloatReactiveProperty IncomingDamageReduction = new(0);
        public FloatReactiveProperty MovementSpeed = new(1);

        public FloatReactiveProperty Health = new(1);
        public ReactiveProperty<Unit> LastAttacker = new();

        public bool IsAlive => Health.Value > 0;
        public bool IsDead => Health.Value <= 0;

        private void Awake()
        {
            Skills.Add(SkillType.Health, HealthSkill);
            Skills.Add(SkillType.Strength, StrengthSkill);
            Skills.Add(SkillType.Defense, DefenseSkill);
            Skills.Add(SkillType.Speed, SpeedSkill);
        }

        public int GetSkillLevel(SkillType statType)
        {
            return Skills[statType].Value;
        }
    }
}
