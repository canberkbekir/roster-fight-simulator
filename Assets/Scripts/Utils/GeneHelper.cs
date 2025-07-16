using System;
using System.Collections.Generic;
using System.Linq;
using Creatures.Genes;
using Creatures.Genes.Base;
using Managers;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Utils
{
    public static class GeneHelper
    {
        public static Gene[] GetCrossGenes(Gene[] motherGene, Gene[] fatherGene)
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

        public static Gene[] GeneSyncToGene(GeneSync[] geneSyncs)
        {
            var geneContainer = GameManager.Instance.ContainerService.GeneDataContainer;
            if (geneSyncs == null || geneSyncs.Length == 0)
            {
                return Array.Empty<Gene>();
            }
            
            var geneList = new List<Gene>();
            foreach (var geneSync in geneSyncs)
            {
                var geneData = geneContainer.GetGeneById(geneSync.id);
                if (geneData != null)
                {
                    
                    geneList.Add(new Gene(geneData));
                }
                else
                {
                    Debug.LogWarning($"Gene with ID {geneSync.id} not found in GeneDataContainer.");
                }
            }

            return geneList.ToArray();
        }
    }
    
    public static class GeneExtensions
    {
        public static Gene ApplyChance(this Gene g, float chance)
        {
            g.OverridePassingChance(chance);
            return g;
        }
    }
}