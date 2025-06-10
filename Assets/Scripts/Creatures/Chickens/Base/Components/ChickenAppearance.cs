using Creatures.Chickens.Base.Handlers;
using Creatures.Genes.Base;
using Creatures.Genes.Features.Base;
using Mirror;
using UnityEngine;

namespace Creatures.Chickens.Base.Components
{
    public class ChickenAppearance: ChickenComponentBase
    {
        [Header("Handlers")] [SerializeField] private ChickenAppearanceHandler appearanceHandler;

        public override void Init(ChickenEntity entity)
        {
            base.Init(entity);
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