// UI/InventorySlotUI.cs

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class InventorySlotUI : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI  countText;

        /// <summary>Sets this slot to display the given icon and count.</summary>
        public void SetItem(Sprite icon, int? count)
        {
            iconImage.sprite  = icon;
            iconImage.enabled = icon != null;
            if(count == null)
                countText.text = "";
            else
                countText.text    = count > 1 ? count.ToString() : "";
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