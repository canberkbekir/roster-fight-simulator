using Creatures.Genes.Base;
using Creatures.Genes.Base.ScriptableObjects;
using Creatures.Genes.Features.Base;
using UnityEngine;

namespace Creatures.Genes.Features
{
    public enum SkillGeneType
    {
        Passive,
        Active,
    }

    [CreateAssetMenu(fileName = "SkillGeneFeature", menuName = "Genes/SkillGeneFeature", order = 0)]
    public class SkillGeneFeatureData : GeneFeatureData,ISkillGeneFeature
    {
        [HideInInspector] public new GeneFeatureType GeneFeatureType => GeneFeatureType.Skill;
        
        [SerializeField] private SkillGeneType skillGeneType;
        [SerializeField] private string skillName;
        [SerializeField] private string skillDescription;
        [SerializeField] private int skillLevel;
        [SerializeField] private int skillCooldown;
        [SerializeField] private int skillDuration;
        
        public SkillGeneType SkillGeneType => skillGeneType;
        public string SkillName => skillName;
        public string SkillDescription => skillDescription;
        public int SkillLevel => skillLevel;
        public int SkillCooldown => skillCooldown;
        public int SkillDuration => skillDuration;

        public override GeneFeature CreateFeature()
        {
            var feature = new SkillGeneFeature
            {
                SkillName = SkillName,
                SkillDescription = SkillDescription,
                SkillLevel = SkillLevel,
                SkillCooldown = SkillCooldown,
                SkillDuration = SkillDuration,
                SkillGeneType = SkillGeneType
            };
            return feature;
        }

       
    }
}
