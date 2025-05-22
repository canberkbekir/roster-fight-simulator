using Genes.Base;
using Genes.Base.ScriptableObjects;
using Genes.Features.Base;
using Roosters.Components;
using UnityEngine;

namespace Genes.Features
{ 
    [CreateAssetMenu(fileName = "StatGeneFeature", menuName = "Genes/StatGeneFeature", order = 0)]
    public class StatGeneFeatureData : GeneFeatureData ,IStatGeneFeature
    {
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
