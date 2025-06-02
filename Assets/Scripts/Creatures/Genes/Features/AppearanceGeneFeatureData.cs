using Creatures.Genes.Base;
using Creatures.Genes.Base.ScriptableObjects;
using Creatures.Genes.Features.Base;
using UnityEngine;

namespace Creatures.Genes.Features
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
    public class AppearanceGeneFeatureData : GeneFeatureData, IAppearanceGeneFeature
    {
        [HideInInspector] public new GeneFeatureType GeneFeatureType => GeneFeatureType.Appearance;
        
        [Header("Appearance Gene Feature")]
        [SerializeField] private AppearanceGeneType appearanceGeneType;
        [SerializeField] private AppearanceEffectType appearanceEffectType;

        [Space, Header("Values")]
        [SerializeField] private Color colorValue;
        [SerializeField] private Material materialValue;
        [SerializeField] private float sizeValue;
        [SerializeField] private Texture textureValue;

        // expose read‑only properties
        public AppearanceGeneType AppearanceGeneType => appearanceGeneType;
        public AppearanceEffectType AppearanceEffectType => appearanceEffectType;
        public Color ColorValue => colorValue;
        public Material MaterialValue => materialValue;
        public float SizeValue => sizeValue;
        public Texture TextureValue => textureValue;

        public override GeneFeature CreateFeature()
        {
            var feature = new AppearanceGeneFeature
            {
                ColorValue = ColorValue,
                MaterialValue = MaterialValue,
                SizeValue = SizeValue,
                TextureValue = TextureValue,
                AppearanceGeneType = AppearanceGeneType,
                AppearanceEffectType = AppearanceEffectType, 
            };
            return feature; 
        }
    }

}
