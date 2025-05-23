using Sirenix.OdinInspector;
using UnityEngine;

namespace Genes.Base.ScriptableObjects
{
    [InlineEditor(InlineEditorModes.FullEditor)]
    [CreateAssetMenu(fileName = "GeneData", menuName = "Genes/GeneData", order = 0)]
    public class GeneData : ScriptableObject,IGene
    {
        [Header("Gene Data")]
        [SerializeField] private int geneId;
        [SerializeField] private string geneName;
        [SerializeField] private string geneDescription;
        [SerializeField] private Sprite geneIcon;
        [SerializeField] private float baseGenePassingChance;
        [SerializeField] private GeneFeatureData[] geneFeatures; 
        
        public int GeneId => geneId;
        public GeneFeatureData[] GeneFeatures => geneFeatures;
        public string GeneName => geneName;
        public string GeneDescription => geneDescription; 
        public Sprite GeneIcon => geneIcon;
        public float GenePassingChance => baseGenePassingChance;
        
        
    } 
}
