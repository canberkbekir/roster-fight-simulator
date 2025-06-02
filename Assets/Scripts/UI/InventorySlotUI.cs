// UI/InventorySlotUI.cs

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class InventorySlotUI : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private TextMeshProUGUI  countText;
        [SerializeField] private Sprite defaultIconImage;

        /// <summary>Sets this slot to display the given icon and count.</summary>
        public void SetItem(Sprite icon, int? count)
        {
            if (!icon)
            {
                iconImage.sprite  = defaultIconImage;
                iconImage.enabled = true;
                countText.text    = "";
                return;  
            }
            iconImage.sprite  = icon;
            iconImage.enabled = icon != null;
            if(count == null)
                countText.text = "";
            else
                countText.text    = count > 1 ? count.ToString() : "";
        } 
        public void Highlight()
        {
            backgroundImage.color = Color.yellow; 
        }
        
        public void Unhighlight()
        {
            backgroundImage.color = Color.white; 
        }

        /// <summary>Clears this slot (no item).</summary>
        public void Clear()
        {
            iconImage.sprite  = null;
            iconImage.enabled = false;
            countText.text    = "";
        }
    }
}