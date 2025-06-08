using System;
using Sirenix.OdinInspector;
using UnityEngine;
using Managers;

namespace Utils
{
    [Serializable]
    [InlineProperty] 
    public class Cooldown
    { 
        [Title("⚙️ Cooldown Settings")]
        
        [LabelText("Duration (s)")]
        [OnValueChanged(nameof(OnDurationChanged))]
        public float duration;

        [ShowInInspector, ReadOnly]
        [LabelText("Remaining (s)")]
        [ProgressBar(0, "$duration")]
        public float Remaining { get; private set; }

        [ShowInInspector, ReadOnly]
        [LabelText("Running?")]
        public bool IsRunning { get; private set; }

        [FoldoutGroup("Events"), HideLabel]
        [Tooltip("Called when Remaining reaches zero.")]
        public Action OnFinished;

        // ───────────────────────────────────────────────────────────────────────

        public Cooldown(float duration)
        {
            this.duration  = duration;
            Remaining = duration;
            SubscribeToGameManager();
        }

        private void OnDurationChanged()
        {
            Remaining = duration;
        }

        private void SubscribeToGameManager()
        {
            GameManager.OnGamePaused  += Pause;
            GameManager.OnGameResumed += Resume;
        }


        [ButtonGroup("Actions", 2)]
        [Button(ButtonSizes.Small)]
        private void StartButton()  => Start();

        [ButtonGroup("Actions", 2)]
        [Button(ButtonSizes.Small)]
        private void PauseButton()  => Pause();

        [ButtonGroup("Actions", 2)]
        [Button(ButtonSizes.Small)]
        private void ResumeButton() => Resume();

        [ButtonGroup("Actions", 2)]
        [Button(ButtonSizes.Small)]
        private void StopButton()   => Stop();


        public void Start()
        {
            Remaining = duration;
            IsRunning = true;
        }

        public void Stop()
        {
            IsRunning = false;
            Remaining = duration;
        }

        public void Pause()
        {
            IsRunning = false;
        }

        public void Resume()
        {
            if (Remaining > 0f)
                IsRunning = true;
        }

        public void Tick(float deltaTime)
        {
            if (!IsRunning) return;

            Remaining -= deltaTime;
            if (Remaining <= 0f)
            {
                IsRunning = false;
                OnFinished?.Invoke();
            }
        }
        
        public void SetDuration(float newDuration)
        {
            duration = newDuration;
            Remaining = newDuration;
        }

        public bool IsReady => Remaining <= 0f;
    }
}
