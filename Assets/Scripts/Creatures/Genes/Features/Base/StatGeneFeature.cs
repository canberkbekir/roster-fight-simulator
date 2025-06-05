using Creatures.Chickens.Base.Components;
using Creatures.Genes.Base;

namespace Creatures.Genes.Features.Base
{
    public class StatGeneFeature : GeneFeature, IStatGeneFeature
    {
        public StatType StatType { get; set; }
        public int Value { get; set; }
    } 
    
    public interface IStatGeneFeature : IGeneFeature
    {
        public StatType StatType { get; }
        public int Value { get; }
    }
}