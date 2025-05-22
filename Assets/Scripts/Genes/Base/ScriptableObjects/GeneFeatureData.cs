using UnityEngine;

namespace Genes.Base.ScriptableObjects
{
    public abstract class GeneFeatureData : ScriptableObject,IGeneFeature
    {
        public abstract GeneFeature CreateFeature();
    }

   
}
