using System.Collections.Generic;
using UnityEngine;

namespace Genes.Base
{
    [CreateAssetMenu(fileName = "GeneData", menuName = "Genes/Gene Data Container", order = 0)]
    public class GeneDataContainer : ScriptableObject
    {
        [Tooltip("All of the possible genes this container can hold")]
        public List<GeneData> possibleGenes = new List<GeneData>();

        public GeneData GetRandomGene()
        {
            if (possibleGenes.Count == 0)
            {
                Debug.LogError("No genes available in the container.");
                return null;
            }

            var randomIndex = Random.Range(0, possibleGenes.Count);
            return possibleGenes[randomIndex]; 
        }
    }
}