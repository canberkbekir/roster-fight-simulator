using System;
using Genes;
using Genes.Base;

namespace Roosters
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