using UnityEngine;

namespace Genes.Base.ScriptableObjects
{
    public enum GeneFeatureType
    {
        Appearance,
        Skill,
        Stat
    }
    public abstract class GeneFeatureData : ScriptableObject,IGeneFeature
    { 
        public abstract GeneFeature CreateFeature();
        public GeneFeatureType GeneFeatureType { get; set; }
        public string Name { get; set; }
    }

   
}
