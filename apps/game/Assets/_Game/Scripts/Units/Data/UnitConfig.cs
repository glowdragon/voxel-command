namespace VoxelCommand.Client
{
    using UnityEngine;
    
    [CreateAssetMenu(fileName = "UnitConfig", menuName = "Game/Unit Config")]
    public class UnitConfig : ScriptableObject
    {
        [Header("Experience Settings")]
        [Tooltip("Base experience required for level 1")]
        public int BaseExperience = 100;
        
        [Tooltip("Growth factor for experience curve (higher = steeper progression)")]
        [Range(1.0f, 3.0f)]
        public float ExperienceGrowthFactor = 1.5f;
        
        [Tooltip("Experience points gained per damage point dealt")]
        public float ExperiencePerDamage = 0.5f;
        
        [Tooltip("Experience points gained per kill")]
        public int ExperiencePerKill = 25;
        
        [Header("Health Settings")]
        [Tooltip("Base health value")]
        public float BaseHealth = 100f;
        
        [Tooltip("Health points gained per stat point")]
        public float HealthPerPoint = 10f;
        
        [Header("Damage Settings")]
        [Tooltip("Base damage value")]
        public float BaseDamage = 10f;
        
        [Tooltip("Damage gained per stat point")]
        public float DamagePerPoint = 2f;
        
        [Header("Defense Settings")]
        [Tooltip("Base defense reduction")]
        public float BaseDefense = 5f;
        
        [Tooltip("Defense reduction gained per stat point")]
        public float DefensePerPoint = 2f;
        
        [Header("Speed Settings")]
        [Tooltip("Base movement speed")]
        public float BaseSpeed = 3f;
        
        [Tooltip("Speed gained per stat point")]
        public float SpeedPerPoint = 0.5f;
    }
} 