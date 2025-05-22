using UnityEngine;

namespace Genes.Base.ScriptableObjects
{
    [CreateAssetMenu(fileName = "GeneData", menuName = "Genes/GeneData", order = 0)]
    public class GeneData : ScriptableObject,IGene
    {
        [SerializeField] private string geneName;
        [SerializeField] private string geneDescription;
        [SerializeField] private Sprite geneIcon;
        [SerializeField] private float baseGenePassingChance;
        [SerializeField] private GeneFeatureData[] geneFeatures; 
        
        public GeneFeatureData[] GeneFeatures => geneFeatures;
        public string GeneName => geneName;
        public string GeneDescription => geneDescription; 
        public Sprite GeneIcon => geneIcon;
        public float GenePassingChance => baseGenePassingChance;
        
        
    } 
}
