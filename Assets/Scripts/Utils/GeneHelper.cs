using System;
using System.Collections.Generic;
using System.Linq;
using Creatures.Genes;
using Creatures.Genes.Base;
using Random = UnityEngine.Random;

namespace Utils
{
    public class GeneHelper
    {
        public static Gene[] CrossGenes(Gene[] motherGene, Gene[] fatherGene)
        {
            if (motherGene == null || fatherGene == null)
            {
                throw new System.ArgumentNullException("Gene cannot be null");
            }

            // // Create a new Gene instance by averaging the values of the two genes
            // var newGene = new Gene
            // {
            //     Name = $"{motherGene.Name}-{fatherGene.Name}",
            //     Value = (motherGene.Value + fatherGene.Value) / 2f,
            //     Dominant = motherGene.Dominant && fatherGene.Dominant // Example logic for dominance
            // };

            return null;
        }

        public static Gene[] GetPassedGene(Gene[] parentGenes)
        {
            var passedGenes = new List<Gene>();

            foreach (var gene in parentGenes)
            { 
                var roll = Random.Range(0f, 100f);

                if (roll < gene.GenePassingChance)
                    passedGenes.Add(gene);
            }

            return passedGenes.ToArray();
        }

        public static GeneSync[] GeneToGeneSync(Gene[] genes)
        {
            if (genes == null || genes.Length == 0)
            {
                return Array.Empty<GeneSync>();
            }

            return genes.Select(g => new GeneSync(g)).ToArray();
        }
    }
}