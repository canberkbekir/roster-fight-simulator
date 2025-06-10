using System;
using Creatures.Genes.Base.ScriptableObjects;
using UnityEngine;

namespace Creatures.Genes.Base
{
    [Serializable]
    public abstract class GeneFeature : IGeneFeature
    {
        [SerializeField] private GeneFeatureType geneFeatureType;
        [SerializeField] private string name;

        public GeneFeatureType GeneFeatureType
        {
            get => geneFeatureType;
            set => geneFeatureType = value;
        }

        public string Name
        {
            get => name;
            set => name = value;
        }
    }
    
    public interface IGeneFeature
    {
        GeneFeatureType GeneFeatureType { get; set; }
        string Name { get; set; }
        
    }
}