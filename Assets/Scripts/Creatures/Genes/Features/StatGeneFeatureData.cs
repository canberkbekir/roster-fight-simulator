using Creatures.Chickens.Base.Components;
using Creatures.Genes.Base;
using Creatures.Genes.Base.ScriptableObjects;
using Creatures.Genes.Features.Base;
using UnityEngine;

namespace Creatures.Genes.Features
{ 
    [CreateAssetMenu(fileName = "StatGeneFeature", menuName = "Genes/StatGeneFeature", order = 0)]
    public class StatGeneFeatureData : GeneFeatureData ,IStatGeneFeature
    {
        [HideInInspector] public new GeneFeatureType GeneFeatureType => GeneFeatureType.Stat;
        
        [SerializeField] private StatType statType;
        [SerializeField] private int value;

        public StatType StatType => statType;
        public int Value => value;
        public override GeneFeature CreateFeature()
        {
            var feature = new StatGeneFeature
            {
                StatType = StatType,
                Value = Value
            };
            return feature;
        }
    }
}
