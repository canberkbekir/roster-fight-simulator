using Creatures.Genes.Base;

namespace Creatures.Genes.Features.Base
{
    [System.Serializable]
    public class SkillGeneFeature : GeneFeature,ISkillGeneFeature
    {
        public string SkillName { get; set; }
        public string SkillDescription { get; set;}
        public int SkillLevel { get; set;}
        public int SkillCooldown { get; set;}
        public int SkillDuration { get;set; }
        public SkillGeneType SkillGeneType { get; set; }
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