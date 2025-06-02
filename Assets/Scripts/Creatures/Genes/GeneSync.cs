using Creatures.Genes.Base;

namespace Creatures.Genes
{
    [System.Serializable]
    public class 
        GeneSync
    {
        public int id;  
        public float currentPassingChance;

        public GeneSync()
        {
            
        }

        public GeneSync(Gene newGene)
        {
            id = newGene.GeneId;
            currentPassingChance = newGene.GenePassingChance; 
        } 
         
        public void UpdatePassingChance(float factor)
        {
            currentPassingChance *= factor;
        }
    }
}
