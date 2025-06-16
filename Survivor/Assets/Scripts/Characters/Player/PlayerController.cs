using UnityEngine;
using UnityEngine.InputSystem;
using Survivor.Characters;
using Survivor.Shared;
using Survivor.Characters.Dialogue;
using Survivor.Characters.UI;
using TMPro;
using UnityEngine.UI;

namespace Survivor.Characters
{
    [RequireComponent(typeof(UnityEngine.CharacterController))]
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

        [Header("Dialogue UI")]
        public GameObject dialoguePanel;
        public TMP_InputField playerInputField;
        public TextMeshProUGUI npcResponseText;
        public Button sendButton;
        public Button closeDialogueButton;

        private IDialogueInteractable selectedNPC;
        private bool inDialogue;
        private DialogueManager dialogueManager;
        private PlayerManager playerManager;
        private UnityEngine.CharacterController controller;
        private Vector3 velocity;
        private Camera mainCamera;

        protected virtual void Start()
        {
            // Get components
            controller = GetComponent<UnityEngine.CharacterController>();
            mainCamera = Camera.main;

            // Get managers
            playerManager = FindObjectOfType<PlayerManager>();
            if (playerManager != null)
            {
                playerManager.RegisterPlayer(this);
            }

            // Setup dialogue UI
            if (sendButton != null)
                sendButton.onClick.AddListener(SendDialogue);
            if (closeDialogueButton != null)
                closeDialogueButton.onClick.AddListener(CloseDialogue);
            if (dialoguePanel != null)
                dialoguePanel.SetActive(false);

            // Get dialogue manager
            dialogueManager = FindObjectOfType<DialogueManager>();
            if (dialogueManager != null)
            {
                dialogueManager.OnDialogueLine += OnDialogueReceived;
            }
        }

        protected virtual void Update()
        {
            if (!inDialogue)
            {
                HandleMovement();
                HandleInteraction();
            }
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

            // Handle interaction input
            if (Input.GetKeyDown(KeyCode.E) && selectedNPC != null)
            {
                StartDialogue(selectedNPC);
            }
        }

        private void StartDialogue(IDialogueInteractable interactable)
        {
            if (dialogueManager == null) return;

            inDialogue = true;
            interactable.OnDialogueStart();
            
            // Optional: Lock cursor, disable movement, etc.
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private async void SendDialogue()
        {
            if (!inDialogue || selectedNPC == null || string.IsNullOrEmpty(playerInputField.text))
                return;

            string playerMessage = playerInputField.text;
            playerInputField.text = "";

            // Send message to NPC through dialogue system
            if (dialogueManager != null)
            {
                string response = await dialogueManager.GenerateResponse(playerMessage);
                OnDialogueReceived(response);
            }
        }

        private void OnDialogueReceived(string response)
        {
            // Handle dialogue response
            Debug.Log($"Received dialogue: {response}");
        }

        private void CloseDialogue()
        {
            if (!inDialogue) return;

            inDialogue = false;
            if (selectedNPC != null)
            {
                selectedNPC.OnDialogueEnd();
            }
            selectedNPC = null;
            CharacterUIManager.Instance.HideNPCInfo();

            // Optional: Reset cursor state
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (dialogueManager != null)
            {
                dialogueManager.OnDialogueLine -= OnDialogueReceived;
            }

            // Unregister player
            if (playerManager != null)
            {
                playerManager.UnregisterPlayer(this);
            }
        }

        protected virtual void OnDrawGizmosSelected()
        {
            // Draw interaction range in editor
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionRange);
        }
    }
} 