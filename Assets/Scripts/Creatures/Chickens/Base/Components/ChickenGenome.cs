using System;
using System.Linq;
using Creatures.Genes.Base;
using Mirror;
using UnityEngine;

namespace Creatures.Chickens.Base.Components
{
    public class ChickenGenome : ChickenComponentBase
    {
        [Header("Genome Configuration")]
        [SerializeField] private int maxGenes = 10;
        [SerializeField] private bool allowDuplicateGenes = false;
        
        private Gene[] _genes;
        public Gene[] Genes => _genes ?? Array.Empty<Gene>();

        /// <summary>
        /// Initializes the genome with the provided genes.
        /// </summary>
        /// <param name="owner">The chicken entity that owns this genome.</param>
        /// <param name="genes">The genes to initialize with. Can be null or empty.</param>
        public void Init(ChickenEntity owner, Gene[] genes)
        {
            if (Owner != null)
            {
                Debug.LogWarning($"[ChickenGenome:{owner?.name}] Genome already initialized. Skipping re-initialization.");
                return;
            }
            
            base.Init(owner);
            ValidateGenomeConfiguration();
            
            if (genes == null || genes.Length == 0)
            {
                Debug.Log($"[ChickenGenome:{owner?.name}] Initializing with empty gene set.");
                SetGeneInstances(Array.Empty<Gene>());
                return;
            }
            
            SetGeneInstances(genes);
        }
        
        /// <summary>
        /// Sets the gene instances with validation.
        /// </summary>
        /// <param name="newGenes">The new genes to set. Must not be null.</param>
        /// <exception cref="ArgumentNullException">Thrown when newGenes is null.</exception>
        public void SetGeneInstances(Gene[] newGenes)
        {
            if (newGenes == null)
            {
                throw new ArgumentNullException(nameof(newGenes), "Gene array cannot be null. Use empty array for no genes.");
            }

            var validatedGenes = ValidateAndFilterGenes(newGenes);
            _genes = validatedGenes;
            
            Debug.Log($"[ChickenGenome:{Owner?.name}] Set {_genes.Length} valid genes out of {newGenes.Length} provided genes.");
            
            Owner?.EventBus?.RaiseGeneInstancesUpdated(_genes);
        }

        /// <summary>
        /// Validates the genome configuration for valid settings.
        /// </summary>
        private void ValidateGenomeConfiguration()
        {
            if (maxGenes <= 0)
            {
                Debug.LogError($"[ChickenGenome:{Owner?.name}] Invalid maxGenes configuration: {maxGenes}. Must be greater than 0. Using default value of 10.");
                maxGenes = 10;
            }
            
            if (maxGenes > 100)
            {
                Debug.LogWarning($"[ChickenGenome:{Owner?.name}] Very high maxGenes value: {maxGenes}. This might impact performance.");
            }
        }

        /// <summary>
        /// Validates and filters the provided genes.
        /// </summary>
        /// <param name="genes">The genes to validate and filter.</param>
        /// <returns>An array of valid genes.</returns>
        private Gene[] ValidateAndFilterGenes(Gene[] genes)
        {
            if (genes.Length > maxGenes)
            {
                Debug.LogWarning($"[ChickenGenome:{Owner?.name}] Too many genes provided ({genes.Length}). Limiting to {maxGenes} genes.");
                genes = genes.Take(maxGenes).ToArray();
            }

            var validGenes = new System.Collections.Generic.List<Gene>();
            var seenGeneIds = new System.Collections.Generic.HashSet<int>();

            for (int i = 0; i < genes.Length; i++)
            {
                var gene = genes[i];
                
                if (gene == null)
                {
                    Debug.LogWarning($"[ChickenGenome:{Owner?.name}] Gene at index {i} is null. Skipping.");
                    continue;
                }

                if (!ValidateGene(gene, i))
                {
                    continue;
                }

                // Check for duplicates if not allowed
                if (!allowDuplicateGenes && seenGeneIds.Contains(gene.GeneId))
                {
                    Debug.LogWarning($"[ChickenGenome:{Owner?.name}] Duplicate gene found: {gene.GeneName} (ID: {gene.GeneId}). Skipping duplicate.");
                    continue;
                }

                validGenes.Add(gene);
                seenGeneIds.Add(gene.GeneId);
            }

            return validGenes.ToArray();
        }

        /// <summary>
        /// Validates a single gene for integrity and correctness.
        /// </summary>
        /// <param name="gene">The gene to validate.</param>
        /// <param name="index">The index of the gene in the original array (for error reporting).</param>
        /// <returns>True if the gene is valid, false otherwise.</returns>
        private bool ValidateGene(Gene gene, int index)
        {
            // Validate gene ID
            if (gene.GeneId <= 0)
            {
                Debug.LogWarning($"[ChickenGenome:{Owner?.name}] Gene at index {index} has invalid ID: {gene.GeneId}. Skipping.");
                return false;
            }

            // Validate gene name
            if (string.IsNullOrEmpty(gene.GeneName))
            {
                Debug.LogWarning($"[ChickenGenome:{Owner?.name}] Gene at index {index} (ID: {gene.GeneId}) has null or empty name. Skipping.");
                return false;
            }

            // Validate gene description
            if (string.IsNullOrEmpty(gene.GeneDescription))
            {
                Debug.LogWarning($"[ChickenGenome:{Owner?.name}] Gene {gene.GeneName} (ID: {gene.GeneId}) has null or empty description.");
            }

            // Validate passing chance
            if (gene.GenePassingChance < 0f || gene.GenePassingChance > 1f)
            {
                Debug.LogWarning($"[ChickenGenome:{Owner?.name}] Gene {gene.GeneName} (ID: {gene.GeneId}) has invalid passing chance: {gene.GenePassingChance}. Clamping to valid range [0-1].");
                gene.OverridePassingChance(Mathf.Clamp01(gene.GenePassingChance));
            }

            // Validate gene features
            if (gene.GeneFeatures == null)
            {
                Debug.LogWarning($"[ChickenGenome:{Owner?.name}] Gene {gene.GeneName} (ID: {gene.GeneId}) has null features array.");
            }
            else
            {
                for (int j = 0; j < gene.GeneFeatures.Length; j++)
                {
                    if (gene.GeneFeatures[j] == null)
                    {
                        Debug.LogWarning($"[ChickenGenome:{Owner?.name}] Gene {gene.GeneName} (ID: {gene.GeneId}) has null feature at index {j}.");
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Adds a gene to the genome with validation.
        /// </summary>
        /// <param name="gene">The gene to add. Must not be null.</param>
        /// <returns>True if the gene was added successfully, false otherwise.</returns>
        public bool AddGene(Gene gene)
        {
            if (gene == null)
            {
                Debug.LogError($"[ChickenGenome:{Owner?.name}] Cannot add null gene.");
                return false;
            }

            if (_genes.Length >= maxGenes)
            {
                Debug.LogWarning($"[ChickenGenome:{Owner?.name}] Cannot add gene {gene.GeneName}: genome is at maximum capacity ({maxGenes}).");
                return false;
            }

            if (!allowDuplicateGenes && _genes.Any(g => g?.GeneId == gene.GeneId))
            {
                Debug.LogWarning($"[ChickenGenome:{Owner?.name}] Cannot add gene {gene.GeneName}: duplicate gene ID {gene.GeneId} already exists.");
                return false;
            }

            if (!ValidateGene(gene, _genes.Length))
            {
                return false;
            }

            var newGenes = new Gene[_genes.Length + 1];
            Array.Copy(_genes, newGenes, _genes.Length);
            newGenes[_genes.Length] = gene;
            
            SetGeneInstances(newGenes);
            return true;
        }

        /// <summary>
        /// Removes a gene from the genome by ID.
        /// </summary>
        /// <param name="geneId">The ID of the gene to remove.</param>
        /// <returns>True if the gene was removed successfully, false otherwise.</returns>
        public bool RemoveGene(int geneId)
        {
            var geneIndex = Array.FindIndex(_genes, g => g?.GeneId == geneId);
            if (geneIndex == -1)
            {
                Debug.LogWarning($"[ChickenGenome:{Owner?.name}] Gene with ID {geneId} not found. Cannot remove.");
                return false;
            }

            var newGenes = new Gene[_genes.Length - 1];
            Array.Copy(_genes, 0, newGenes, 0, geneIndex);
            Array.Copy(_genes, geneIndex + 1, newGenes, geneIndex, _genes.Length - geneIndex - 1);
            
            SetGeneInstances(newGenes);
            return true;
        }

        /// <summary>
        /// Gets a gene by its ID.
        /// </summary>
        /// <param name="geneId">The ID of the gene to find.</param>
        /// <returns>The gene if found, null otherwise.</returns>
        public Gene GetGeneById(int geneId)
        {
            return _genes.FirstOrDefault(g => g?.GeneId == geneId);
        }

        /// <summary>
        /// Gets a gene by its name.
        /// </summary>
        /// <param name="geneName">The name of the gene to find.</param>
        /// <returns>The gene if found, null otherwise.</returns>
        public Gene GetGeneByName(string geneName)
        {
            if (string.IsNullOrEmpty(geneName))
            {
                Debug.LogWarning($"[ChickenGenome:{Owner?.name}] Cannot search for gene with null or empty name.");
                return null;
            }
            
            return _genes.FirstOrDefault(g => g?.GeneName == geneName);
        }

        /// <summary>
        /// Gets the number of genes in the genome.
        /// </summary>
        public int GeneCount => _genes?.Length ?? 0;

        /// <summary>
        /// Gets the maximum number of genes allowed in the genome.
        /// </summary>
        public int MaxGenes => maxGenes;

        /// <summary>
        /// Gets whether duplicate genes are allowed.
        /// </summary>
        public bool AllowDuplicateGenes => allowDuplicateGenes;
    }
}