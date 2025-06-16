using UnityEngine;
using System;
using System.Threading.Tasks;
using Survivor.Shared;
using Survivor.Shared.Interfaces;

namespace Survivor.Characters.Dialogue
{
    public class DialogueManager : MonoBehaviour, IDialogueSystem
    {
        private static DialogueManager instance;
        public static DialogueManager Instance => instance;

        [Header("Dialogue Settings")]
        [SerializeField] private float interactionDistance = 3f;
        [SerializeField] private KeyCode interactKey = KeyCode.E;
        public float dialogueTimeout = 30f;
        public int maxRetries = 3;

        public event Action<string> OnDialogueLine;
        public event Action<bool> OnDialogueStateChanged;

        private IDialogueInteractable currentInteractable;
        private bool isInRange = false;
        private bool isInDialogue = false;
        private CursorLockMode previousCursorLockState;
        private bool wasCursorVisible;

        public bool IsInDialogue => isInDialogue;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                Debug.Log("[DialogueManager] Initialized as singleton");
            }
            else
            {
                Debug.Log("[DialogueManager] Destroying duplicate instance");
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // Initialize cursor state
            previousCursorLockState = Cursor.lockState;
            wasCursorVisible = Cursor.visible;
        }

        private void Update()
        {
            if (isInRange && Input.GetKeyDown(interactKey) && currentInteractable != null && !isInDialogue)
            {
                Debug.Log($"[DialogueManager] Starting dialogue with {currentInteractable.GetDisplayName()}");
                StartDialogue(currentInteractable.GetDisplayName(), "Starting conversation...");
            }
        }

        public void SetInteractable(IDialogueInteractable interactable, bool inRange)
        {
            if (currentInteractable != interactable || isInRange != inRange)
            {
                Debug.Log($"[DialogueManager] Setting interactable: {(interactable != null ? interactable.GetDisplayName() : "null")}, inRange: {inRange}");
                currentInteractable = inRange ? interactable : null;
                isInRange = inRange;
            }
        }

        public async Task<string> GenerateResponse(string playerMessage)
        {
            if (currentInteractable == null)
            {
                Debug.LogError("[DialogueManager] GenerateResponse called with no current interactable");
                return "Error: No interactable specified.";
            }

            try
            {
                Debug.Log($"[DialogueManager] Generating response for {currentInteractable.GetDisplayName()}: {playerMessage}");
                // TODO: Implement actual dialogue generation logic
                // For now, return a simple response
                return $"{currentInteractable.GetDisplayName()}: I understand you said '{playerMessage}'. Let me think about that...";
            }
            catch (Exception e)
            {
                Debug.LogError($"[DialogueManager] Error generating response: {e.Message}");
                return "I'm having trouble understanding right now. Can you try again?";
            }
        }

        public void StartDialogue(string npcName, string initialMessage)
        {
            if (isInDialogue)
            {
                Debug.Log("[DialogueManager] Already in dialogue, ignoring StartDialogue call");
                return;
            }

            Debug.Log($"[DialogueManager] Starting dialogue with {npcName}");
            isInDialogue = true;

            // Save current cursor state
            previousCursorLockState = Cursor.lockState;
            wasCursorVisible = Cursor.visible;

            // Show cursor and unlock it
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // Notify the NPC that dialogue has started
            if (currentInteractable != null)
            {
                currentInteractable.OnDialogueStart();
            }

            // Notify listeners about dialogue state change
            OnDialogueStateChanged?.Invoke(true);
            OnDialogueLine?.Invoke(initialMessage);
        }

        public void EndDialogue()
        {
            if (!isInDialogue)
            {
                Debug.Log("[DialogueManager] Not in dialogue, ignoring EndDialogue call");
                return;
            }

            Debug.Log("[DialogueManager] Ending dialogue");
            isInDialogue = false;

            // Notify the NPC that dialogue has ended
            if (currentInteractable != null)
            {
                currentInteractable.OnDialogueEnd();
            }

            // Restore previous cursor state
            Cursor.lockState = previousCursorLockState;
            Cursor.visible = wasCursorVisible;

            // Notify listeners about dialogue state change
            OnDialogueStateChanged?.Invoke(false);
            OnDialogueLine?.Invoke("Dialogue ended");

            // Clear current interactable
            currentInteractable = null;
            isInRange = false;
        }

        private void OnDestroy()
        {
            // Ensure we clean up if destroyed while in dialogue
            if (isInDialogue)
            {
                EndDialogue();
            }
        }
    }
} 