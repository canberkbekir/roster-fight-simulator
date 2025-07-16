using System;

namespace Creatures.Chickens.Base.Components
{
    /// <summary>
    /// Interface for all chicken-related components.
    /// Provides common functionality and validation methods.
    /// </summary>
    public interface IChickenComponent
    {
        /// <summary>
        /// Gets whether the component is properly initialized.
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// Validates the component's data integrity.
        /// </summary>
        /// <returns>True if the component data is valid, false otherwise.</returns>
        bool IsValid();

        /// <summary>
        /// Gets a human-readable description of the component's current state.
        /// </summary>
        /// <returns>A string describing the component state.</returns>
        string GetStateDescription();
    }
}