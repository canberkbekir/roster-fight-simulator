using UnityEngine;
using UnityEngine.Serialization;

namespace UI
{
    public abstract class BaseUI : MonoBehaviour
    {
        [FormerlySerializedAs("_canvasGroup")] [SerializeField]
        private CanvasGroup canvasGroup;

        [Header("Settings")] [SerializeField] private bool hideOnStart = false;

        protected virtual void Awake()
        {
            if (canvasGroup == null)
            { 
                canvasGroup = GetComponent<CanvasGroup>();
            } 
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            } 

            if (canvasGroup == null)
            {
                Debug.LogError("CanvasGroup component is not assigned in the inspector.");
                return;
            }

            if (hideOnStart)
            {
                Hide();
            }
        }

        public virtual void Show()
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        public virtual void Hide()
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }
}