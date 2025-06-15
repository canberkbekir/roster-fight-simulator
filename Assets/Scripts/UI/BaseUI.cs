using UnityEngine; 
using DG.Tweening;

namespace UI
{
    public abstract class BaseUI : MonoBehaviour
    {
        public enum TransitionMode
        {
            Instant,
            Fade
        }

        public enum FadeSpeed
        {
            Fast,
            Normal,
            Slow
        }
 
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Settings")]
        [SerializeField] private bool hideOnStart = false;

        [Header("Transitions")]
        [SerializeField] private TransitionMode showMode = TransitionMode.Fade;
        [SerializeField] private TransitionMode hideMode = TransitionMode.Fade;
        [SerializeField] private FadeSpeed fadeSpeed = FadeSpeed.Normal;

        private float Duration
        {
            get
            {
                switch (fadeSpeed)
                {
                    case FadeSpeed.Fast:   return 0.2f;
                    case FadeSpeed.Slow:   return 1.2f;
                    case FadeSpeed.Normal:
                    default:               return 0.5f;
                }
            }
        }

        protected virtual void Awake()
        {
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                Debug.LogError("CanvasGroup missing.", this);
                return;
            }

            if (hideOnStart)
                Hide(immediate: true);
        }

        /// <summary>
        /// Show with either instant or fade, depending on 'showMode'.
        /// </summary>
        public virtual void Show(bool immediate = false)
        {
            if (immediate || showMode == TransitionMode.Instant)
            {
                SetVisibleInstant();
            }
            else
            {
                canvasGroup.blocksRaycasts = true;
                canvasGroup.DOFade(1f, Duration)
                           .OnStart(() => canvasGroup.interactable = false)
                           .OnComplete(() => canvasGroup.interactable = true)
                           .SetUpdate(true);
            }
        }

        /// <summary>
        /// Hide with either instant or fade, depending on 'hideMode'.
        /// </summary>
        public virtual void Hide(bool immediate = false)
        {
            if (immediate || hideMode == TransitionMode.Instant)
            {
                SetHiddenInstant();
            }
            else
            {
                canvasGroup.interactable = false;
                canvasGroup.DOFade(0f, Duration)
                           .OnComplete(() => canvasGroup.blocksRaycasts = false)
                           .SetUpdate(true);
            }
        }
        
        public virtual void Hide()
        {
            Hide(immediate: false);
        }
 
        public virtual void Show()
        {
            Show(immediate: false);
        }

        private void SetVisibleInstant()
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        private void SetHiddenInstant()
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }
}
