using Genes.Base;
using UnityEngine;

namespace UI.Breeder
{
    public class BreederGeneUI : BaseUI
    {
        [SerializeField] private UnityEngine.UI.Image geneImage;
        [SerializeField] private TMPro.TextMeshProUGUI geneNameText;
        [SerializeField] private TMPro.TextMeshProUGUI genePercentageText;

        [SerializeField] private Sprite defaultGeneImage;

        public void SetGene(Gene gene)
        {
            if (gene == null)
            {
                geneImage.sprite = defaultGeneImage;
                geneNameText.text = "No Gene Selected";
                genePercentageText.text = "";
                return;
            }

            geneImage.sprite = gene.GeneIcon;
            geneNameText.text = gene.GeneName;
            genePercentageText.text = $"{gene.GenePassingChance * 100}%";

            if (geneImage.sprite == null)
            {
                geneImage.sprite = defaultGeneImage;
            }
            
        }
    }
}