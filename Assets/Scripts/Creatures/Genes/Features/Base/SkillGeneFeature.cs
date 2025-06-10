using Creatures.Genes.Base;
using UnityEngine;
namespace Creatures.Genes.Features.Base
{
    [System.Serializable]
    public class SkillGeneFeature : GeneFeature,ISkillGeneFeature
    {
        [SerializeField] private string skillName;
        [SerializeField] private string skillDescription;
        [SerializeField] private int skillLevel;
        [SerializeField] private int skillCooldown;
        [SerializeField] private int skillDuration;
        [SerializeField] private SkillGeneType skillGeneType;

        public string SkillName
        {
            get => skillName;
            set => skillName = value;
        }

        public string SkillDescription
        {
            get => skillDescription;
            set => skillDescription = value;
        }

        public int SkillLevel
        {
            get => skillLevel;
            set => skillLevel = value;
        }

        public int SkillCooldown
        {
            get => skillCooldown;
            set => skillCooldown = value;
        }

        public int SkillDuration
        {
            get => skillDuration;
            set => skillDuration = value;
        }

        public SkillGeneType SkillGeneType
        {
            get => skillGeneType;
            set => skillGeneType = value;
        }
    }
    
    public interface ISkillGeneFeature : IGeneFeature
    {
        public string SkillName { get; }
        public string SkillDescription { get; }
        public int SkillLevel { get; }
        public int SkillCooldown { get; }
        public int SkillDuration { get; }
        public SkillGeneType SkillGeneType { get; }
    }
}