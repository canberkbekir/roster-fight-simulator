using Genes.Base;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Breeder
{
    public class BreederGenesUI : BaseUI
    {
        [Header("Prefab & Container")] 
        [SerializeField] private BreederGeneUI geneUIPrefab; 
        [SerializeField] private Transform geneUIContainer; 
        
        private readonly List<BreederGeneUI> _instantiatedUIs = new List<BreederGeneUI>();
 
        public void SetGenes(Gene[] genes)
        {
            ClearAll();

            if (genes == null || genes.Length == 0)
            { 
                var placeholder = CreateGeneUI();
                placeholder.SetGene(null);
                return;
            }

            foreach (var gene in genes)
            {
                var ui = CreateGeneUI();
                if (gene != null)
                {
                    ui.SetGene(gene);
                }
                else
                {
                    ui.SetGene(null);
                }
            }
 
            LayoutRebuilder.ForceRebuildLayoutImmediate(geneUIContainer.GetComponent<RectTransform>());
        } 
        
        private void ClearAll()
        {
            foreach (var ui in _instantiatedUIs.Where(ui => ui != null))
            {
                Destroy(ui.gameObject);
            }

            _instantiatedUIs.Clear();
        }
 
        private BreederGeneUI CreateGeneUI()
        {
            var ui = Instantiate(geneUIPrefab, geneUIContainer);
            _instantiatedUIs.Add(ui);
            return ui;
        }
    }
}
