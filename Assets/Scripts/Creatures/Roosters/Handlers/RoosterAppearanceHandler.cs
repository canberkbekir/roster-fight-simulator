using System;
using Creatures.Genes.Features;
using Creatures.Genes.Features.Base;
using Creatures.Roosters.Utils;
using Mirror;
using UnityEngine;

namespace Creatures.Roosters.Handlers
{
    public class RoosterAppearanceHandler : NetworkBehaviour
    {
        [Header("Head")] [SerializeField] private BodyPart Head;
        [SerializeField] private BodyPart Beak;
        [SerializeField] private BodyPart Eye;

        [Space] [Header("Body")] [SerializeField]
        private BodyPart Body;

        [SerializeField] private BodyPart LeftWing;
        [SerializeField] private BodyPart RightWing;
        [SerializeField] private BodyPart Tail;

        [Space] [Header("Legs")] [SerializeField]
        private BodyPart LeftLeg;

        [SerializeField] private BodyPart RightLeg;

        public void Init()
        {
        }

        public void HandleAppearanceGeneFeature(AppearanceGeneFeature geneFeature)
        {
            switch (geneFeature.AppearanceGeneType)
            {
                case AppearanceGeneType.Head:
                    Head.HandleAppearanceGeneFeature(geneFeature);
                    break;
                case AppearanceGeneType.Body:
                    Body.HandleAppearanceGeneFeature(geneFeature);
                    break;
                case AppearanceGeneType.Wing:
                    LeftWing.HandleAppearanceGeneFeature(geneFeature);
                    RightWing.HandleAppearanceGeneFeature(geneFeature);
                    break;
                case AppearanceGeneType.Tail:
                    Tail.HandleAppearanceGeneFeature(geneFeature);
                    break;
                case AppearanceGeneType.Leg:
                    LeftLeg.HandleAppearanceGeneFeature(geneFeature);
                    RightLeg.HandleAppearanceGeneFeature(geneFeature);
                    break;
                case AppearanceGeneType.Beak:
                    Beak.HandleAppearanceGeneFeature(geneFeature);
                    break;
                case AppearanceGeneType.Eye:
                    Eye.HandleAppearanceGeneFeature(geneFeature);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}