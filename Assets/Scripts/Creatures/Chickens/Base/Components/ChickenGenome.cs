using System.Linq;
using Creatures.Genes.Base;
using Mirror;

namespace Creatures.Chickens.Base.Components
{
    public class ChickenGenome : NetworkBehaviour, IChickenComponent
    {
        private ChickenEntity _owner;
        private Gene[] _genes;
        public Gene[] Genes => _genes;
        
        public void Init(ChickenEntity owner,Gene[] genes)
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