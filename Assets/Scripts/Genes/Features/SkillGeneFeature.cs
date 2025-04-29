using Genes.Base;
using UnityEngine;

namespace Genes
{
    public enum SkillGeneType
    {
        Passive,
        Active,
    }

    [CreateAssetMenu(fileName = "SkillGeneFeature", menuName = "Genes/SkillGeneFeature", order = 0)]
    public class SkillGeneFeature : GeneFeature
    {
        [SerializeField] private SkillGeneType skillGeneType;
        [SerializeField] private string skillName;
        [SerializeField] private string skillDescription;
        [SerializeField] private int skillLevel;
        [SerializeField] private int skillCooldown;
        [SerializeField] private int skillDuration;
        
        public SkillGeneType SkillGeneType => skillGeneType;
         
    }
}
