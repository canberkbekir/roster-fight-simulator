using System;
using Genes.Base.ScriptableObjects;

namespace Genes.Base
{
    [Serializable]
    public abstract class GeneFeature : IGeneFeature
    {
        public GeneFeatureType GeneFeatureType { get; set; }
        public string Name { get; set; }
    }
    
    public interface IGeneFeature
    {
        GeneFeatureType GeneFeatureType { get; set; }
        string Name { get; set; }
        
    }
}