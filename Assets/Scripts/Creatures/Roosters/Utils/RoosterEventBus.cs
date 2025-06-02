using System;
using Creatures.Genes.Base;

namespace Creatures.Roosters.Utils
{
    public class RoosterEventBus
    {
        public event Action<Gene[]> OnGeneInstancesUpdated; 
        
        public void RaiseGeneInstancesUpdated(Gene[] genes)
        {
            OnGeneInstancesUpdated?.Invoke(genes);
        }
    }
}