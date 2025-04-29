using Genes;
using Genes.Features;
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

        private void OnGeneInstancesUpdated(GeneInstance[] obj)
        {
            foreach (var geneInstance in obj)
            {
                foreach (var geneFeature in geneInstance.geneData.GeneFeatures)
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