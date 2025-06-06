using UnityEngine;
using Inputs;
using Managers;

namespace Players
{
    public class PlayerCharacterLookInput : MonoBehaviour
    {
        [Space(15.0f)]
        public bool invertLook = true;
        public float mouseSensitivity = 0.1f;

        [Space(15.0f)]
        public float minPitch = -80.0f;
        public float maxPitch = 80.0f;

        private PlayerCharacter _character;
        private InputReader _inputReader;

        private void Awake()
        {
            _character = GetComponent<PlayerCharacter>();
            _inputReader = GameManager.Instance.InputReader;
        }

        private void OnEnable()
        {
            if (_inputReader)
                _inputReader.LookEvent += HandleLook;
        }

        private void OnDisable()
        {
            if (_inputReader)
                _inputReader.LookEvent -= HandleLook;
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void HandleLook(Vector2 lookInputRaw)
        {
            var lookInput = lookInputRaw * mouseSensitivity;
            _character.AddControlYawInput(lookInput.x);
            _character.AddControlPitchInput(
                invertLook ? -lookInput.y : lookInput.y,
                minPitch,
                maxPitch
            );
        }
    }
}