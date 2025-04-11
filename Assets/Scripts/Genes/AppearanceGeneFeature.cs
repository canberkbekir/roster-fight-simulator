using Genes.Base;
using UnityEngine;

namespace Genes
{
    public enum AppearanceGeneType
    {
        Head,
        Body,
        Wing,
        Tail,
        Leg, 
        Eye,
        Mouth,   
    }
    [CreateAssetMenu(fileName = "AppearanceGeneFeature", menuName = "Genes/AppearanceGeneFeature", order = 0)]
    public class AppearanceGeneFeature : GeneFeature
    {
        [SerializeField] private AppearanceGeneType appearanceGeneType;
        
        public AppearanceGeneType AppearanceGeneType => appearanceGeneType;
         
    }
}
