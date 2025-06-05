using System;
using Creatures.Genes.Base;

namespace Creatures.Chickens.Base.Utils
{
    public class ChickenEventBus
    {
        public event Action<Gene[]> OnGeneInstancesUpdated; 
        
        public void RaiseGeneInstancesUpdated(Gene[] genes)
        {
            OnGeneInstancesUpdated?.Invoke(genes);
        }
    }
}