using UnityEngine;

namespace Genes.Base
{
    [CreateAssetMenu(fileName = "GeneData", menuName = "Genes/GeneData", order = 0)]
    public class GeneData : ScriptableObject
    {
        [SerializeField] private string geneName;
        [SerializeField] private string geneDescription;
        [SerializeField] private GeneFeature[] geneFeatures;
        
        public GeneFeature[] GeneFeatures => geneFeatures;
        public string GeneName => geneName;
        public string GeneDescription => geneDescription; 
    }
}
