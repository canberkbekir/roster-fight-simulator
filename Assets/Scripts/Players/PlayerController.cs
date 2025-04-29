using Mirror;
using UnityEngine;

namespace Players
{
    public class PlayerController : NetworkBehaviour
    {
        private PlayerCharacter _playerCharacter;

        [Header("Camera Settings")]
        public float mouseSensitivity = 2.0f;

        private void Start()
        {
            _playerCharacter = GetComponent<PlayerCharacter>(); 

            if (isLocalPlayer)
            { 
                _playerCharacter.cameraParent.gameObject.SetActive(true);
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                _playerCharacter.cameraParent.gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            if (!isLocalPlayer) return; 
        }
    }
}