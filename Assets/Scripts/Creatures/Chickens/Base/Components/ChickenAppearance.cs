using Creatures.Chickens.Base.Handlers;
using Creatures.Genes.Base;
using Creatures.Genes.Features.Base;
using Mirror;
using UnityEngine;

namespace Creatures.Chickens.Base.Components
{
    public class ChickenAppearance: NetworkBehaviour, IChickenComponent
    { 
        [Header("Handlers")] [SerializeField] private ChickenAppearanceHandler appearanceHandler;
        
        private ChickenEntity _owner;
        
        public void Init(ChickenEntity entity)
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