using System;
using Mirror;
using UnityEngine;

namespace Creatures.Chickens.Base.Components
{
    public enum StatType
    {
        Strength,
        Agility,
        Endurance,
        Intelligence,
        Health
    }
    
    public class ChickenStats : ChickenComponentBase
    {
        [Header("Stat Configuration")]
        [SerializeField] private int minStatValue = 0;
        [SerializeField] private int maxStatValue = 100;
        [SerializeField] private int defaultStatValue = 50;
        
        [Header("Current Stats")]
        [SyncVar(hook = nameof(OnStrengthChanged))] public int strength;
        [SyncVar(hook = nameof(OnAgilityChanged))] public int agility;
        [SyncVar(hook = nameof(OnEnduranceChanged))] public int endurance;
        [SyncVar(hook = nameof(OnIntelligenceChanged))] public int intelligence;
        [SyncVar(hook = nameof(OnHealthChanged))] public int health;

        // Events for stat changes
        public event Action<StatType, int, int> OnStatChanged; // StatType, oldValue, newValue

        public override void Init(ChickenEntity owner)
        {
            base.Init(owner);
            ValidateStatConfiguration();
            InitializeDefaultStats();
        }

        /// <summary>
        /// Validates the stat configuration for valid ranges.
        /// </summary>
        private void ValidateStatConfiguration()
        {
            if (minStatValue >= maxStatValue)
            {
                Debug.LogError($"[ChickenStats:{Owner?.name}] Invalid stat configuration: minStatValue ({minStatValue}) must be less than maxStatValue ({maxStatValue})");
                minStatValue = 0;
                maxStatValue = 100;
            }

            if (defaultStatValue < minStatValue || defaultStatValue > maxStatValue)
            {
                Debug.LogWarning($"[ChickenStats:{Owner?.name}] Default stat value ({defaultStatValue}) is outside valid range [{minStatValue}-{maxStatValue}]. Using min value.");
                defaultStatValue = minStatValue;
            }
        }

        /// <summary>
        /// Initializes stats with default values if they haven't been set.
        /// </summary>
        private void InitializeDefaultStats()
        {
            if (strength == 0) strength = defaultStatValue;
            if (agility == 0) agility = defaultStatValue;
            if (endurance == 0) endurance = defaultStatValue;
            if (intelligence == 0) intelligence = defaultStatValue;
            if (health == 0) health = defaultStatValue;
        }

        /// <summary>
        /// Sets a stat value with validation and clamping.
        /// </summary>
        /// <param name="statType">The type of stat to set.</param>
        /// <param name="value">The value to set.</param>
        /// <returns>True if the value was set successfully, false otherwise.</returns>
        public bool SetStat(StatType statType, int value)
        {
            if (!ValidateStatValue(value))
            {
                Debug.LogWarning($"[ChickenStats:{Owner?.name}] Invalid stat value {value} for {statType}. Value must be between {minStatValue} and {maxStatValue}.");
                return false;
            }

            int clampedValue = Mathf.Clamp(value, minStatValue, maxStatValue);
            
            switch (statType)
            {
                case StatType.Strength:
                    strength = clampedValue;
                    break;
                case StatType.Agility:
                    agility = clampedValue;
                    break;
                case StatType.Endurance:
                    endurance = clampedValue;
                    break;
                case StatType.Intelligence:
                    intelligence = clampedValue;
                    break;
                case StatType.Health:
                    health = clampedValue;
                    break;
                default:
                    Debug.LogError($"[ChickenStats:{Owner?.name}] Unknown stat type: {statType}");
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Gets a stat value by type.
        /// </summary>
        /// <param name="statType">The type of stat to get.</param>
        /// <returns>The current value of the stat.</returns>
        public int GetStat(StatType statType)
        {
            return statType switch
            {
                StatType.Strength => strength,
                StatType.Agility => agility,
                StatType.Endurance => endurance,
                StatType.Intelligence => intelligence,
                StatType.Health => health,
                _ => throw new ArgumentException($"Unknown stat type: {statType}", nameof(statType))
            };
        }

        /// <summary>
        /// Validates that a stat value is within the valid range.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <returns>True if the value is valid, false otherwise.</returns>
        private bool ValidateStatValue(int value)
        {
            return value >= minStatValue && value <= maxStatValue;
        }

        /// <summary>
        /// Clamps a stat value to the valid range.
        /// </summary>
        /// <param name="value">The value to clamp.</param>
        /// <returns>The clamped value.</returns>
        private int ClampStatValue(int value)
        {
            return Mathf.Clamp(value, minStatValue, maxStatValue);
        }

        // SyncVar hooks
        void OnStrengthChanged(int oldVal, int newVal)
        {
            if (oldVal != newVal)
            {
                OnStatChanged?.Invoke(StatType.Strength, oldVal, newVal);
                Debug.Log($"[ChickenStats:{Owner?.name}] Strength changed from {oldVal} to {newVal}");
            }
        }

        void OnAgilityChanged(int oldVal, int newVal)
        {
            if (oldVal != newVal)
            {
                OnStatChanged?.Invoke(StatType.Agility, oldVal, newVal);
                Debug.Log($"[ChickenStats:{Owner?.name}] Agility changed from {oldVal} to {newVal}");
            }
        }

        void OnEnduranceChanged(int oldVal, int newVal)
        {
            if (oldVal != newVal)
            {
                OnStatChanged?.Invoke(StatType.Endurance, oldVal, newVal);
                Debug.Log($"[ChickenStats:{Owner?.name}] Endurance changed from {oldVal} to {newVal}");
            }
        }

        void OnIntelligenceChanged(int oldVal, int newVal)
        {
            if (oldVal != newVal)
            {
                OnStatChanged?.Invoke(StatType.Intelligence, oldVal, newVal);
                Debug.Log($"[ChickenStats:{Owner?.name}] Intelligence changed from {oldVal} to {newVal}");
            }
        }

        void OnHealthChanged(int oldVal, int newVal)
        {
            if (oldVal != newVal)
            {
                OnStatChanged?.Invoke(StatType.Health, oldVal, newVal);
                Debug.Log($"[ChickenStats:{Owner?.name}] Health changed from {oldVal} to {newVal}");
            }
        }

        /// <summary>
        /// Gets the minimum allowed stat value.
        /// </summary>
        public int MinStatValue => minStatValue;

        /// <summary>
        /// Gets the maximum allowed stat value.
        /// </summary>
        public int MaxStatValue => maxStatValue;

        /// <summary>
        /// Gets the default stat value.
        /// </summary>
        public int DefaultStatValue => defaultStatValue;
    }
}