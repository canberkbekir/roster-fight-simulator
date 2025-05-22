using Genes;
using Genes.Base;
using Genes.Features;
using Genes.Features.Base;
using Mirror;
using Roosters.Handlers;
using UnityEngine; 

namespace Roosters.Components
{
    public class RoosterAppearance: NetworkBehaviour, IRoosterComponent
    { 
        [Header("Handlers")] [SerializeField] private RoosterAppearanceHandler appearanceHandler;
        
        private RoosterEntity _owner;
        
        public void Init(RoosterEntity entity)
        {
            _owner = entity;
            entity.EventBus.OnGeneInstancesUpdated += OnGeneInstancesUpdated;
            
        }

        private void OnGeneInstancesUpdated(Gene[] genes)
        {
            foreach (var gene in genes)
            {
                foreach (var geneFeature in gene.GeneFeatures)
                {
                    if (geneFeature is AppearanceGeneFeature feature)
                    {
                        appearanceHandler.HandleAppearanceGeneFeature(feature);
                    }
                }
            }
        }
    }
}