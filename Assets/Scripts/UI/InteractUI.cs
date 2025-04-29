using TMPro;
using UnityEngine;

namespace UI
{
    public class InteractUI : MonoBehaviour
    {
        [SerializeField] private GameObject promptRoot;
        [SerializeField] private TMP_Text promptText;

        void Awake()
        {
            promptRoot.SetActive(false);
        }

        public void Show(string message)
        {
            promptText.text = message;
            promptRoot.SetActive(true);
        }

        public void Hide()
        {
            promptRoot.SetActive(false);
        }
    }
}