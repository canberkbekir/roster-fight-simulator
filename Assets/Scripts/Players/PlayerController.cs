using ECM2;
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

            // // Fare hareketinden input al
            // float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            // float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
            //
            // // Kamerayı ve karakteri döndür
            // _playerCharacter.AddControlYawInput(mouseX);
            // _playerCharacter.AddControlPitchInput(-mouseY); // Y ekseni tersi
            //
            // // Hareket inputları (örnek)
            // float horizontal = Input.GetAxis("Horizontal");
            // float vertical = Input.GetAxis("Vertical");
            // Vector3 move = new Vector3(horizontal, 0, vertical);
            //
            // _playerCharacter.SetMovementDirection(transform.position + move);
        }
    }
}