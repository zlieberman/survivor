using UnityEngine;
using Survivor.Core;
using Survivor.UI;
using Survivor.Dialogue;
using TMPro;
using UnityEngine.UI;

namespace Survivor.Player
{
    [RequireComponent(typeof(CharacterController))]
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

        private CharacterController controller;
        private Vector3 velocity;
        private NPC selectedNPC;
        private Camera mainCamera;
        private bool inDialogue;

        private void Start()
        {
            controller = GetComponent<CharacterController>();
            mainCamera = Camera.main;

            // Register as player with ID 0
            GameDayManager.Instance.RegisterPlayer(0);

            // Setup dialogue UI
            if (sendButton != null)
                sendButton.onClick.AddListener(SendDialogue);
            if (closeDialogueButton != null)
                closeDialogueButton.onClick.AddListener(CloseDialogue);
            if (dialoguePanel != null)
                dialoguePanel.SetActive(false);

            // Subscribe to dialogue events
            if (Survivor.Dialogue.DialogueSystem.Instance != null)
            {
                Survivor.Dialogue.DialogueSystem.Instance.onDialogueReceived.AddListener(OnDialogueReceived);
            }
        }

        private void Update()
        {
            if (!inDialogue)
            {
                HandleMovement();
                HandleInteraction();
            }
        }

        private void HandleMovement()
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

            // Rotate player to face movement direction
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
            NPC closestNPC = null;
            float closestDistance = float.MaxValue;

            foreach (var collider in nearbyNPCs)
            {
                NPC npc = collider.GetComponent<NPC>();
                if (npc != null)
                {
                    float distance = Vector3.Distance(transform.position, collider.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestNPC = npc;
                    }
                }
            }

            // Update selected NPC
            if (closestNPC != selectedNPC)
            {
                selectedNPC = closestNPC;
                if (selectedNPC != null)
                {
                    UIManager.Instance.ShowNPCInfo(selectedNPC);
                }
                else
                {
                    UIManager.Instance.HideNPCInfo();
                }
            }

            // Handle interaction input
            if (Input.GetKeyDown(KeyCode.E) && selectedNPC != null)
            {
                StartDialogue(selectedNPC);
            }
        }

        private void StartDialogue(NPC npc)
        {
            if (dialoguePanel == null) return;

            inDialogue = true;
            dialoguePanel.SetActive(true);
            playerInputField.text = "";
            npcResponseText.text = $"Talking to {npc.npcName}...";
            
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
            string response = await Survivor.Dialogue.DialogueSystem.Instance.GenerateDialogue(selectedNPC, playerMessage);
            OnDialogueReceived(response);
        }

        private void OnDialogueReceived(string response)
        {
            if (npcResponseText != null)
            {
                npcResponseText.text = response;
            }
        }

        private void CloseDialogue()
        {
            if (!inDialogue) return;

            inDialogue = false;
            dialoguePanel.SetActive(false);
            selectedNPC = null;
            UIManager.Instance.HideNPCInfo();

            // Optional: Reset cursor state
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (Survivor.Dialogue.DialogueSystem.Instance != null)
            {
                Survivor.Dialogue.DialogueSystem.Instance.onDialogueReceived.RemoveListener(OnDialogueReceived);
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Draw interaction range in editor
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionRange);
        }
    }
} 