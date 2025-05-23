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
        public GeneFeatureType GeneFeatureType;
        public abstract GeneFeature CreateFeature();
    }

   
}
