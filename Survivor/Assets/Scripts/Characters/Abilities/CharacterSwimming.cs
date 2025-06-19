using UnityEngine;
using StarterAssets;
using Survivor.Shared;

namespace Survivor.Characters
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(ThirdPersonController))]
    public class CharacterSwimming : MonoBehaviour, IWaterInteractable
    {
        [Header("Swimming Settings")]
        public float swimSpeed = 4f;
        public float swimUpSpeed = 3f;
        public float swimDownSpeed = 2f;
        public float waterDrag = 0.8f;
        public float swimThreshold = 0.67f; // 2/3 submerged threshold

        [Header("Water Physics")]
        public float swimGravity = -2f;
        public float normalGravity = -15f;
        public float buoyancyForce = 2f;

        private CharacterController characterController;
        private ThirdPersonController thirdPersonController;
        private StarterAssetsInputs input;
        private Animator animator;

        private bool isInWater = false;
        private bool isSwimming = false;
        private float waterHeight = 0f;
        private float originalGravity;
        private float originalMoveSpeed;
        private float originalSprintSpeed;

        // Animation IDs
        private int animIDSpeed;
        private int animIDGrounded;
        private int animIDJump;
        private int animIDFreeFall;
        private int animIDSwimming;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            thirdPersonController = GetComponent<ThirdPersonController>();
            input = GetComponent<StarterAssetsInputs>();
            animator = GetComponent<Animator>();

            // Store original values
            originalGravity = thirdPersonController.Gravity;
            originalMoveSpeed = thirdPersonController.MoveSpeed;
            originalSprintSpeed = thirdPersonController.SprintSpeed;

            // Get animation IDs
            AssignAnimationIDs();
        }

        private void AssignAnimationIDs()
        {
            animIDSpeed = Animator.StringToHash("Speed");
            animIDGrounded = Animator.StringToHash("Grounded");
            animIDJump = Animator.StringToHash("Jump");
            animIDFreeFall = Animator.StringToHash("FreeFall");
            animIDSwimming = Animator.StringToHash("Swimming");
        }

        public void OnEnterWater(float waterHeight)
        {
            isInWater = true;
            this.waterHeight = waterHeight;
            Debug.Log("[CharacterSwimming] Entered water at height: " + waterHeight);
        }

        public void OnExitWater()
        {
            isInWater = false;
            isSwimming = false;
            
            // Restore original values
            thirdPersonController.Gravity = originalGravity;
            thirdPersonController.MoveSpeed = originalMoveSpeed;
            thirdPersonController.SprintSpeed = originalSprintSpeed;

            // Update animator
            if (animator != null)
            {
                animator.SetBool(animIDSwimming, false);
            }

            Debug.Log("[CharacterSwimming] Exited water");
        }

        public void OnStayInWater(float waterHeight, float buoyancyForce, float dragForce)
        {
            this.waterHeight = waterHeight;
            
            // Calculate submersion percentage
            float submersionPercentage = CalculateSubmersionPercentage();
            
            // Check if should be swimming
            bool shouldSwim = submersionPercentage >= swimThreshold;
            
            if (shouldSwim && !isSwimming)
            {
                StartSwimming();
            }
            else if (!shouldSwim && isSwimming)
            {
                StopSwimming();
            }

            // Apply water physics
            ApplyWaterPhysics(submersionPercentage);
        }

        private float CalculateSubmersionPercentage()
        {
            float playerHeight = characterController.height;
            float playerBottom = transform.position.y - playerHeight / 2f;
            float playerTop = transform.position.y + playerHeight / 2f;
            float waterSurface = waterHeight;

            float submergedHeight = Mathf.Max(0, waterSurface - playerBottom);
            float totalHeight = playerTop - playerBottom;
            
            return Mathf.Clamp01(submergedHeight / totalHeight);
        }

        private void StartSwimming()
        {
            isSwimming = true;
            
            // Set swimming physics
            thirdPersonController.Gravity = swimGravity;
            thirdPersonController.MoveSpeed = swimSpeed;
            thirdPersonController.SprintSpeed = swimSpeed * 1.2f;

            // Update animator
            if (animator != null)
            {
                animator.SetBool(animIDSwimming, true);
                animator.SetBool(animIDGrounded, false);
            }

            Debug.Log("[CharacterSwimming] Started swimming");
        }

        private void StopSwimming()
        {
            isSwimming = false;
            
            // Restore normal physics
            thirdPersonController.Gravity = normalGravity;
            thirdPersonController.MoveSpeed = originalMoveSpeed;
            thirdPersonController.SprintSpeed = originalSprintSpeed;

            // Update animator
            if (animator != null)
            {
                animator.SetBool(animIDSwimming, false);
            }

            Debug.Log("[CharacterSwimming] Stopped swimming");
        }

        private void ApplyWaterPhysics(float submersionPercentage)
        {
            if (!isInWater) return;

            // Apply buoyancy force
            if (submersionPercentage > 0)
            {
                float buoyancyForce = this.buoyancyForce * submersionPercentage;
                Vector3 buoyancyVelocity = Vector3.up * buoyancyForce * Time.deltaTime;
                characterController.Move(buoyancyVelocity);
            }

            // Handle swimming movement
            if (isSwimming)
            {
                HandleSwimmingMovement();
            }
            else
            {
                // Apply water resistance to normal movement
                ApplyWaterResistance(submersionPercentage);
            }
        }

        private void HandleSwimmingMovement()
        {
            if (input == null) return;

            Vector3 swimDirection = Vector3.zero;

            // Horizontal movement
            if (input.move.magnitude > 0.1f)
            {
                swimDirection += new Vector3(input.move.x, 0, input.move.y).normalized * swimSpeed;
            }

            // Vertical movement
            if (input.jump)
            {
                swimDirection.y = swimUpSpeed;
            }
            else if (input.sprint) // Use sprint for diving
            {
                swimDirection.y = -swimDownSpeed;
            }

            // Apply swimming movement
            if (swimDirection.magnitude > 0.1f)
            {
                characterController.Move(swimDirection * Time.deltaTime);
            }

            // Update animator speed for swimming
            if (animator != null)
            {
                float swimSpeedMagnitude = new Vector3(swimDirection.x, 0, swimDirection.z).magnitude;
                animator.SetFloat(animIDSpeed, swimSpeedMagnitude);
            }
        }

        private void ApplyWaterResistance(float submersionPercentage)
        {
            // Reduce movement speed based on submersion
            float waterResistance = 1f - (submersionPercentage * waterDrag);
            
            thirdPersonController.MoveSpeed = originalMoveSpeed * waterResistance;
            thirdPersonController.SprintSpeed = originalSprintSpeed * waterResistance;
        }

        private void Update()
        {
            // Update swimming state based on submersion
            if (isInWater)
            {
                float submersionPercentage = CalculateSubmersionPercentage();
                bool shouldSwim = submersionPercentage >= swimThreshold;
                
                if (shouldSwim && !isSwimming)
                {
                    StartSwimming();
                }
                else if (!shouldSwim && isSwimming)
                {
                    StopSwimming();
                }
            }
        }

        // Public methods for external access
        public bool IsSwimming => isSwimming;
        public bool IsInWater => isInWater;
        public float GetSubmersionPercentage() => CalculateSubmersionPercentage();

        // Method to force swimming state (useful for debugging or special cases)
        public void ForceSwimmingState(bool swimming)
        {
            if (swimming && !isSwimming)
            {
                StartSwimming();
            }
            else if (!swimming && isSwimming)
            {
                StopSwimming();
            }
        }
    }
} 