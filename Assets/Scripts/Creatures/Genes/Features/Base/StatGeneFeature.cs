using Creatures.Chickens.Base.Components;
using Creatures.Genes.Base;
using UnityEngine;

namespace Creatures.Genes.Features.Base
{
    [System.Serializable]
    public class StatGeneFeature : GeneFeature, IStatGeneFeature
    {
        [SerializeField] private StatType statType;
        [SerializeField] private int value;

        public StatType StatType
        {
            get => statType;
            set => statType = value;
        }

        public int Value
        {
            get => value;
            set => this.value = value;
        }
    }
    
    public interface IStatGeneFeature : IGeneFeature
    {
        public StatType StatType { get; }
        public int Value { get; }
    }
}