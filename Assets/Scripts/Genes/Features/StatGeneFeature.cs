using Genes.Base;
using Roosters;
using Roosters.Components;
using UnityEngine;

namespace Genes
{ 
    [CreateAssetMenu(fileName = "StatGeneFeature", menuName = "Genes/StatGeneFeature", order = 0)]
    public class StatGeneFeature : GeneFeature
    {
        [SerializeField] private StatType statType;
        [SerializeField] private int value;

        public StatType StatType => statType;
        public int Value => value; 
    }
}
