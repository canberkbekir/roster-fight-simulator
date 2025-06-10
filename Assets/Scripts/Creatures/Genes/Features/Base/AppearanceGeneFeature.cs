using System;
using Creatures.Genes.Base;
using UnityEngine;

namespace Creatures.Genes.Features.Base
{
    [Serializable]
    public class AppearanceGeneFeature :GeneFeature, IAppearanceGeneFeature
    {
        [SerializeField] private Color colorValue;
        [SerializeField] private Material materialValue;
        [SerializeField] private float sizeValue;
        [SerializeField] private Texture textureValue;
        [SerializeField] private AppearanceGeneType appearanceGeneType;
        [SerializeField] private AppearanceEffectType appearanceEffectType;

        public Color ColorValue
        {
            get => colorValue;
            set => colorValue = value;
        }

        public Material MaterialValue
        {
            get => materialValue;
            set => materialValue = value;
        }

        public float SizeValue
        {
            get => sizeValue;
            set => sizeValue = value;
        }

        public Texture TextureValue
        {
            get => textureValue;
            set => textureValue = value;
        }

        public AppearanceGeneType AppearanceGeneType
        {
            get => appearanceGeneType;
            set => appearanceGeneType = value;
        }

        public AppearanceEffectType AppearanceEffectType
        {
            get => appearanceEffectType;
            set => appearanceEffectType = value;
        }
    }

    public interface IAppearanceGeneFeature : IGeneFeature
    {
        public Color ColorValue { get; }
        public Material MaterialValue{ get; }
        public float SizeValue{ get;  }
        public Texture TextureValue { get;  }
        public AppearanceGeneType AppearanceGeneType { get;  }
        public AppearanceEffectType AppearanceEffectType { get;  }
    }
}