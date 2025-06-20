using UnityEngine;
using UnityEngine.InputSystem;
using Survivor.Characters;
using Survivor.Shared;
using Survivor.Characters.UI;
using TMPro;
using UnityEngine.UI;

namespace Survivor.Characters
{
    [RequireComponent(typeof(UnityEngine.CharacterController))]
    [RequireComponent(typeof(Inventory))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        public float moveSpeed = 5f;
        public float rotationSpeed = 120f;
        public float gravity = -9.81f;
        public float jumpForce = 5f;

        [Header("Interaction")]
        public float interactionRange = 3f;
        public LayerMask npcLayer;
        private IDialogueInteractable selectedNPC;
        private UnityEngine.CharacterController controller;
        private Inventory inventory;
        private Vector3 velocity;
        private Camera mainCamera;

        protected virtual void Awake()
        {
            // Ensure Inventory component exists
            inventory = GetComponent<Inventory>();
            if (inventory == null)
            {
                Debug.Log("[PlayerController] Adding missing Inventory component");
                inventory = gameObject.AddComponent<Inventory>();
            }
        }

        protected virtual void Start()
        {
            // Get components
            controller = GetComponent<UnityEngine.CharacterController>();
            mainCamera = Camera.main;
        }

        protected virtual void Update()
        {
            HandleMovement();
            HandleInteraction();
        }

        protected virtual void HandleMovement()
        {
            // Get input
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            // Calculate movement direction
            Vector3 movement = new Vector3(horizontal, 0f, vertical);
            
            // Make movement relative to camera
            if (mainCamera != null)
            {
                movement = Quaternion.Euler(0, mainCamera.transform.eulerAngles.y, 0) * movement;
            }

            // Normalize movement vector
            if (movement.magnitude > 1f)
            {
                movement.Normalize();
            }

            // Apply movement
            controller.Move(movement * moveSpeed * Time.deltaTime);

            // Rotate character to face movement direction
            if (movement != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(movement),
                    Time.deltaTime * rotationSpeed
                );
            }

            // Apply gravity
            if (controller.isGrounded && velocity.y < 0)
            {
                velocity.y += -2f * gravity * Time.deltaTime;
            }

            // Handle jumping
            if (Input.GetButtonDown("Jump") && controller.isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            }

            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }

        private void HandleInteraction()
        {
            // Check for nearby NPCs
            Collider[] nearbyNPCs = Physics.OverlapSphere(transform.position, interactionRange, npcLayer);
            IDialogueInteractable closestNPC = null;
            float closestDistance = float.MaxValue;

            foreach (var collider in nearbyNPCs)
            {
                IDialogueInteractable interactable = collider.GetComponent<IDialogueInteractable>();
                if (interactable != null)
                {
                    float distance = Vector3.Distance(transform.position, collider.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestNPC = interactable;
                    }
                }
            }

            // Update selected NPC
            if (closestNPC != selectedNPC)
            {
                selectedNPC = closestNPC;
                if (selectedNPC != null)
                {
                    CharacterUIManager.Instance.ShowNPCInfo(selectedNPC.GetDisplayName());
                }
                else
                {
                    CharacterUIManager.Instance.HideNPCInfo();
                }
            }
        }

        private void OnDestroy()
        {
        }

        protected virtual void OnDrawGizmosSelected()
        {
            // Draw interaction range in editor
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionRange);
        }
    }
} 