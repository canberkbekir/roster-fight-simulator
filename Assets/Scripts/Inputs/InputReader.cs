using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Inputs
{
    [CreateAssetMenu(fileName = "InputReader", menuName = "Input/InputReader", order = 1)]
    public class InputReader : ScriptableObject,PlayerInputActions.IGameplayActions,PlayerInputActions.IUIActions
    { 
        private PlayerInputActions _inputActions;

        private void OnEnable()
        {
            if (_inputActions == null)
            {
                _inputActions = new PlayerInputActions();
                _inputActions.Gameplay.SetCallbacks(this);
                _inputActions.UI.SetCallbacks(this); 
                SetGameplay();
            }
            _inputActions.Enable(); 
        }
        
        private void OnDisable()
        {
            _inputActions.Disable();
        }

        public void SetGameplay()
        {
            _inputActions.Gameplay.Enable();
            _inputActions.UI.Disable();
        }
        
        public void SetUI()
        {
            _inputActions.UI.Enable();
            _inputActions.Gameplay.Disable();
        }

        public event Action<Vector2> MoveEvent;  
        public event Action MoveCanceledEvent;
        public event Action<Vector2> LookEvent;
        
        public event Action JumpEvent;
        public event Action JumpCanceledEvent;

        public event Action SprintEvent;
        public event Action CrouchEvent;
        
        public event Action PauseEvent;

        public void OnMove(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                MoveEvent?.Invoke(context.ReadValue<Vector2>());
            }
            else if (context.canceled)
            {
                MoveCanceledEvent?.Invoke();
            } 
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                LookEvent?.Invoke(context.ReadValue<Vector2>());
            }
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                JumpEvent?.Invoke();
            }
            else if (context.canceled)
            {
                JumpCanceledEvent?.Invoke();
            }
        }

        public void OnSprint(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                SprintEvent?.Invoke();
            }
        }

        public void OnCrouch(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                CrouchEvent?.Invoke();
            }
        }
    }
}
