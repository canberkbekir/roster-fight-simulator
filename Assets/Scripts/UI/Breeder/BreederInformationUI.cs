using Creatures.Roosters;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Breeder
{
    public class BreederInformationUI : BaseUI
    {
        [Header("UI Elements")] 
        [SerializeField] private Image roosterImage;
        [SerializeField] private TMPro.TextMeshProUGUI roosterNameText;
        
        [Header("Settings")]
        [SerializeField] private Sprite defaultRoosterImage;

        public void SetRooster(Rooster rooster)
        {
            roosterImage.sprite = defaultRoosterImage;
            if (rooster != null) return;
            roosterImage.sprite = defaultRoosterImage;
            roosterNameText.text = "No Rooster Selected"; 
        }
    }
}