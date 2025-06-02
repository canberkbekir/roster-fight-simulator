using System.Linq;
using Creatures.Genes.Base;
using Mirror;

namespace Creatures.Roosters.Components
{
    public class RoosterGenome : NetworkBehaviour, IRoosterComponent
    {
        private RoosterEntity _owner;
        private Gene[] _genes;
        public Gene[] Genes => _genes;
        
        public void Init(RoosterEntity owner,Gene[] genes)
        { 
            if(_owner) return;
            _owner = owner;
            
            if(!genes.Any()) return; 
            SetGeneInstances(genes);
        }
        
        public void SetGeneInstances(Gene[] newGenes)
        {
            _genes = newGenes;
            _owner.EventBus?.RaiseGeneInstancesUpdated(_genes);
        }
    }
}