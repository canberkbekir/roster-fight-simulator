using System.Linq;
using Creatures.Genes.Base;
using Mirror;

namespace Creatures.Chickens.Base.Components
{
    public class ChickenGenome : ChickenComponentBase
    {
        private Gene[] _genes;
        public Gene[] Genes => _genes;

        public void Init(ChickenEntity owner,Gene[] genes)
        {
            if (Owner != null) return;
            base.Init(owner);

            if(!genes.Any()) return;
            SetGeneInstances(genes);
        }
        
        public void SetGeneInstances(Gene[] newGenes)
        {
            _genes = newGenes;
            Owner.EventBus?.RaiseGeneInstancesUpdated(_genes);
        }
    }
}