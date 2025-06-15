using UnityEngine;

namespace Survivor.Player
{
    public class PlayerSwimming : MonoBehaviour
    {
        private bool isInWater = false;
        private float waterHeight = 0f;
        private CharacterController characterController;
        private float originalGravity;
        private float swimSpeed = 3f;
        private float swimUpSpeed = 2f;

        private void Start()
        {
            characterController = GetComponent<CharacterController>();
            if (characterController == null)
            {
                Debug.LogError("PlayerSwimming requires a CharacterController component!");
            }
        }

        public void EnterWater(float height)
        {
            isInWater = true;
            waterHeight = height;
            
            // Disable normal gravity
            if (characterController != null)
            {
                characterController.enabled = false;
            }
        }

        public void ExitWater()
        {
            isInWater = false;
            
            // Re-enable normal movement
            if (characterController != null)
            {
                characterController.enabled = true;
            }
        }

        private void Update()
        {
            if (isInWater)
            {
                HandleSwimming();
            }
        }

        private void HandleSwimming()
        {
            // Get input
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            bool jumpPressed = Input.GetButton("Jump");

            // Calculate movement
            Vector3 moveDirection = new Vector3(horizontal, 0, vertical);
            if (moveDirection.magnitude > 1f)
            {
                moveDirection.Normalize();
            }

            // Apply swimming movement
            Vector3 movement = moveDirection * swimSpeed * Time.deltaTime;
            
            // Handle vertical movement
            if (jumpPressed)
            {
                movement.y = swimUpSpeed * Time.deltaTime;
            }
            else if (transform.position.y > waterHeight)
            {
                // Sink if not swimming up
                movement.y = -swimSpeed * 0.5f * Time.deltaTime;
            }

            // Move the player
            transform.position += movement;
        }
    }
} 