using System;
using Genes;

namespace Roosters
{
    public class RoosterEventBus
    {
        public event Action<GeneInstance[]> OnGeneInstancesUpdated; 
        
        public void RaiseGeneInstancesUpdated(GeneInstance[] geneInstances)
        {
            OnGeneInstancesUpdated?.Invoke(geneInstances);
        }
    }
}