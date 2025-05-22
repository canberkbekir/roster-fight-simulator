using Genes.Base;
using Genes.Base.ScriptableObjects;

namespace Genes
{
    [System.Serializable]
    public class GeneInstance
    {
        public Gene gene;  
        public float currentPassingChance;   

        public GeneInstance(Gene newGene)
        {
            gene = newGene;
            currentPassingChance = gene.GenePassingChance;
        } 
         
        public void UpdatePassingChance(float factor)
        {
            currentPassingChance *= factor;
        }
    }
}
