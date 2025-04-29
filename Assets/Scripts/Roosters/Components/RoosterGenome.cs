using System.Linq;
using Genes;
using Mirror;

namespace Roosters.Components
{
    public class RoosterGenome : NetworkBehaviour, IRoosterComponent
    {
        private RoosterEntity _owner;
        private GeneInstance[] _geneInstances;
        public GeneInstance[] GeneInstances => _geneInstances;
        
        public void Init(RoosterEntity entity)
        {
            _owner = entity;
            if (entity.preReadyGenes == null || !entity.preReadyGenes.Any()) return;
            
            _geneInstances = entity.preReadyGenes
                .Select(preReadyGene => new GeneInstance(preReadyGene))
                .ToArray();
                                  
            SetGeneInstances(_geneInstances);
        }
        
        public void SetGeneInstances(GeneInstance[] geneInstances)
        {
            _geneInstances = geneInstances;
            _owner.EventBus?.RaiseGeneInstancesUpdated(_geneInstances);
        }
    }
}