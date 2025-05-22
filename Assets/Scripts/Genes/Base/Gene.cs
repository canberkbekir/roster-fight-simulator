using System;
using System.Linq;
using Genes.Base.ScriptableObjects;
using UnityEngine;

namespace Genes.Base
{
    [Serializable]
    public class Gene : IGene
    {
        [SerializeField] private string geneName;
        [SerializeField] private string geneDescription;
        [SerializeField] private float genePassingChance;
        [SerializeField] private GeneFeature[] geneFeatures;
        [NonSerialized] private Sprite geneIcon;

        public Gene() { }

        public Gene(GeneData data)
        {
            geneName = data.GeneName;
            geneDescription = data.GeneDescription;
            genePassingChance = data.GenePassingChance;
            geneIcon = data.GeneIcon;
            geneFeatures = data.GeneFeatures
                .Select(fd => fd.CreateFeature())
                .ToArray();
        }

        public string GeneName => geneName;
        public string GeneDescription => geneDescription;
        public float GenePassingChance => genePassingChance;
        public Sprite GeneIcon => geneIcon;
        public GeneFeature[] GeneFeatures => geneFeatures;
    }

    public interface IGene
    {
        string GeneName { get; }
        string GeneDescription { get; }
        Sprite GeneIcon { get; }
        float GenePassingChance { get; }
    }
}