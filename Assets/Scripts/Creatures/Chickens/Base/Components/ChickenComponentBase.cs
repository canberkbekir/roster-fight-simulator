using System;
using Mirror;
using UnityEngine;

namespace Creatures.Chickens.Base.Components
{
    /// <summary>
    /// Base class for all chicken-related network components.
    /// Provides a common Init method and owner reference with validation.
    /// </summary>
    public abstract class ChickenComponentBase : NetworkBehaviour, IChickenComponent
    {
        protected ChickenEntity Owner { get; private set; }
        public bool IsInitialized { get; private set; }

        /// <summary>
        /// Initializes the component with the provided owner.
        /// </summary>
        /// <param name="owner">The chicken entity that owns this component. Must not be null.</param>
        /// <exception cref="ArgumentNullException">Thrown when owner is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when trying to initialize an already initialized component.</exception>
        public virtual void Init(ChickenEntity owner)
        {
            if (IsInitialized)
            {
                Debug.LogWarning($"[{GetType().Name}:{name}] Component already initialized. Skipping re-initialization.");
                return;
            }

            if (owner == null)
            {
                throw new ArgumentNullException(nameof(owner), $"Cannot initialize {GetType().Name}: owner cannot be null.");
            }

            if (!owner.isActiveAndEnabled)
            {
                Debug.LogWarning($"[{GetType().Name}:{name}] Initializing component with inactive owner: {owner.name}");
            }

            Owner = owner;
            IsInitialized = true;
            
            Debug.Log($"[{GetType().Name}:{name}] Successfully initialized with owner: {owner.name}");
        }

        /// <summary>
        /// Validates the component's data integrity.
        /// Override in derived classes to provide specific validation logic.
        /// </summary>
        /// <returns>True if the component data is valid, false otherwise.</returns>
        public virtual bool IsValid()
        {
            if (!IsInitialized)
            {
                Debug.LogWarning($"[{GetType().Name}:{name}] Component not initialized.");
                return false;
            }

            if (Owner == null)
            {
                Debug.LogError($"[{GetType().Name}:{name}] Owner reference is null.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets a human-readable description of the component's current state.
        /// Override in derived classes to provide specific state information.
        /// </summary>
        /// <returns>A string describing the component state.</returns>
        public virtual string GetStateDescription()
        {
            return $"Component: {GetType().Name}, Initialized: {IsInitialized}, Owner: {GetOwnerName()}, Valid: {IsValid()}";
        }

        /// <summary>
        /// Validates that the component is properly initialized before performing operations.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the component is not initialized.</exception>
        protected void ValidateInitialization()
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException($"{GetType().Name} {name} is not initialized. Call Init() first.");
            }

            if (Owner == null)
            {
                throw new InvalidOperationException($"{GetType().Name} {name} has null owner reference.");
            }
        }

        /// <summary>
        /// Validates that the component is properly initialized and the owner is valid.
        /// </summary>
        /// <returns>True if the component is ready for operations, false otherwise.</returns>
        protected bool IsReadyForOperations()
        {
            if (!IsInitialized)
            {
                Debug.LogWarning($"[{GetType().Name}:{name}] Component not initialized. Cannot perform operations.");
                return false;
            }

            if (Owner == null)
            {
                Debug.LogError($"[{GetType().Name}:{name}] Owner reference is null. Component may be in invalid state.");
                return false;
            }

            if (!Owner.isActiveAndEnabled)
            {
                Debug.LogWarning($"[{GetType().Name}:{name}] Owner {Owner.name} is not active. Operations may not work correctly.");
            }

            return true;
        }

        /// <summary>
        /// Gets the name of the owner for logging purposes.
        /// </summary>
        /// <returns>The owner name, or "Unknown" if owner is null.</returns>
        protected string GetOwnerName()
        {
            return Owner?.name ?? "Unknown";
        }

        /// <summary>
        /// Logs a debug message with component context.
        /// </summary>
        /// <param name="message">The message to log.</param>
        protected void LogDebug(string message)
        {
            Debug.Log($"[{GetType().Name}:{name}] {message}");
        }

        /// <summary>
        /// Logs a warning message with component context.
        /// </summary>
        /// <param name="message">The message to log.</param>
        protected void LogWarning(string message)
        {
            Debug.LogWarning($"[{GetType().Name}:{name}] {message}");
        }

        /// <summary>
        /// Logs an error message with component context.
        /// </summary>
        /// <param name="message">The message to log.</param>
        protected void LogError(string message)
        {
            Debug.LogError($"[{GetType().Name}:{name}] {message}");
        }

        /// <summary>
        /// Called when the component is destroyed. Override to perform cleanup.
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (IsInitialized)
            {
                Debug.Log($"[{GetType().Name}:{name}] Component destroyed. Owner: {GetOwnerName()}");
            }
        }
    }
}
