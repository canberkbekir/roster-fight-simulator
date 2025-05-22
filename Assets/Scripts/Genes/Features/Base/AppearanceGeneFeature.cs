using System;
using Genes.Base;
using UnityEngine;

namespace Genes.Features.Base
{
    [Serializable]
    public class AppearanceGeneFeature :GeneFeature, IAppearanceGeneFeature
    { 
       [SerializeField] public Color ColorValue { get; set; }
       [SerializeField]public Material MaterialValue { get; set; }
       [SerializeField]public float SizeValue { get; set; }
       [SerializeField]public Texture TextureValue { get; set; }
       [SerializeField]public AppearanceGeneType AppearanceGeneType { get; set; }
       [SerializeField]public AppearanceEffectType AppearanceEffectType { get; set; }
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