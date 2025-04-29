using Genes.Base;

namespace Genes
{
    [System.Serializable]
    public class GeneInstance
    {
        public GeneData geneData;  
        public float currentPassingChance;   

        public GeneInstance(GeneData geneData)
        {
            this.geneData = geneData;
            currentPassingChance = geneData.BaseGenePassingChance;  
        } 
         
        public void UpdatePassingChance(float factor)
        {
            currentPassingChance *= factor;
        }
    }
}
