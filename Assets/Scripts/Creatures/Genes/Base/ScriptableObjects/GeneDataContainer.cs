using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Creatures.Genes.Base.ScriptableObjects
{
    [CreateAssetMenu(fileName = "GeneDataContainer", menuName = "Genes/Gene Data Container", order = 0)]
    public class GeneDataContainer : ScriptableObject
    {
        [Title("Possible Genes")]
        [ListDrawerSettings(
        ShowFoldout = true,
        DraggableItems = false,
        ShowIndexLabels = true)]
        public List<GeneData> possibleGenes = new();

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
        
        public GeneData GetGeneById(int id)
        {
            if (possibleGenes.Count == 0)
            {
                Debug.LogError("No genes available in the container.");
                return null;
            }
            
            var gene = possibleGenes.FirstOrDefault(g => g.GeneId == id);
            if (gene != null) return gene;
            Debug.LogError($"Gene with ID {id} not found in the container.");
            
           
            return null; 
        }

#if UNITY_EDITOR
        private void OnValidate()
        { 
            for (var i = 0; i < possibleGenes.Count; i++)
            {
                var gene = possibleGenes[i];
                if (gene == null) continue;

                var so   = new SerializedObject(gene);
                var prop = so.FindProperty("geneId");
                var   newId = i + 1;

                if (prop.intValue == newId) continue;
                prop.intValue = newId;
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(gene);
            }

            // mark the container dirty so Unity knows to save it if needed
            EditorUtility.SetDirty(this);
        }
#endif
    }
}