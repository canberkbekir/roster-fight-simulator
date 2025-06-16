using DG.Tweening;
using Interactions.Base;
using UnityEngine;
using UnityEngine.Serialization;

namespace Interactions.Objects.Doors
{
    public class Door : InteractableBase
    { 
        [Header("Settings")]
        [SerializeField] private float openDelay = 2f;
        [SerializeField] private float openAngle = 90f;
        [SerializeField] private float closeAngle = 0f;
        [SerializeField] private bool isOpen = false;
        [SerializeField] private Ease easeType = Ease.OutSine;

        private Tween _currentTween;

        private void Start()
        {
            var targetY = isOpen ? openAngle : closeAngle;
            transform.localRotation = Quaternion.Euler(0, targetY, 0);
        }

        public override void OnInteract(GameObject interactor)
        {
            base.OnInteract(interactor); 
            ToggleDoor();
        }
        
        private void ToggleDoor()
        {
            isOpen = !isOpen; 
            var targetY = isOpen ? openAngle : closeAngle; 
            _currentTween?.Kill();

            _currentTween = transform.DOLocalRotate(new Vector3(0, targetY, 0), openDelay)
                .SetEase(easeType);
        }
    }
}