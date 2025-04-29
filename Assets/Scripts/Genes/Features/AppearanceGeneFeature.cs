using Genes.Base;
using UnityEngine;

namespace Genes.Features
{
    public enum AppearanceGeneType
    {
        Head,
        Body,
        Wing,
        Tail,
        Leg, 
        Eye,
        Beak,   
    }
    
    public enum AppearanceEffectType
    {
        Color,
        Material,
        Texture,
        Size, 
    }  
    
    [CreateAssetMenu(fileName = "AppearanceGeneFeature", menuName = "Genes/AppearanceGeneFeature", order = 0)]
    public class AppearanceGeneFeature : GeneFeature
    {
        [Header("Appearance Gene Feature")]
        [SerializeField] private AppearanceGeneType appearanceGeneType;
        [SerializeField] private AppearanceEffectType appearanceEffectType;
        
        [Space]
        [Header("Values")]
        public Color colorValue;
        public Material materialValue;
        public float sizeValue;
        public Texture textureValue; 
        
        public AppearanceGeneType AppearanceGeneType => appearanceGeneType;
        public AppearanceEffectType AppearanceEffectType => appearanceEffectType;
         
    }
}
