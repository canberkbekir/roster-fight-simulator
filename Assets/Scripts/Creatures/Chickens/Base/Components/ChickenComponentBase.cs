using Mirror;

namespace Creatures.Chickens.Base.Components
{
    /// <summary>
    /// Base class for all chicken-related network components.
    /// Provides a common Init method and owner reference.
    /// </summary>
    public abstract class ChickenComponentBase : NetworkBehaviour, IChickenComponent
    {
        protected ChickenEntity Owner { get; private set; }

        public virtual void Init(ChickenEntity owner)
        {
            Owner = owner;
        }
    }
}
