using System;
using Mirror;
using UnityEngine;

namespace Creatures.Chickens.Base.Components
{
    /// <summary>
    /// Handles hunger logic for a chicken. Hunger decreases over time and
    /// other components can check <see cref="IsFed"/> before performing actions.
    /// </summary>
    public class ChickenHungerHandler : ChickenComponentBase
    {
        [Header("Hunger Settings")]
        [SerializeField] private float maxHunger = 100f;
        [SerializeField] private float hungerDecayRate = 1f;
        [SerializeField] private float fedThreshold = 50f;

        [SyncVar] private float currentHunger;

        /// <summary>
        /// True when the chicken has enough hunger to perform actions.
        /// </summary>
        public bool IsFed => currentHunger >= fedThreshold;

        /// <summary>
        /// True when hunger has reached zero.
        /// </summary>
        public bool IsStarved => currentHunger <= 0f;

        /// <summary>
        /// Percentage of remaining hunger (0-1 range).
        /// </summary>
        public float HungerPercent => maxHunger > 0f ? Mathf.Clamp01(currentHunger / maxHunger) : 0f;

        public event Action OnStarved;

        public override void Init(ChickenEntity owner)
        {
            base.Init(owner);
            currentHunger = maxHunger;
        }

        [ServerCallback]
        private void Update()
        {
            if (maxHunger <= 0f) return;

            currentHunger -= hungerDecayRate * Time.deltaTime;
            if (currentHunger <= 0f)
            {
                currentHunger = 0f;
                OnStarved?.Invoke();
            }
        }

        /// <summary>
        /// Feed the chicken by the specified amount.
        /// </summary>
        [Server]
        public void Feed(float amount)
        {
            if (amount <= 0f) return;
            currentHunger = Mathf.Clamp(currentHunger + amount, 0f, maxHunger);
        }
    }
}
