using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;
using Survivor.Shared;
using Survivor.Shared.Interfaces;
using Survivor.Characters.Dialogue;

namespace Survivor.UI
{
    public class DialogueUI : MonoBehaviour, IDialogueUI
    {
        [Header("UI References")]
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private TextMeshProUGUI npcNameText;
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private TMP_InputField playerInput;
        [SerializeField] private Button sendButton;
        [SerializeField] private Button closeButton;

        private IDialogueSystem dialogueSystem;
        private bool isProcessingResponse = false;

        private void Start()
        {
            // Find the DialogueManager through the interface
            dialogueSystem = FindObjectOfType<DialogueManager>() as IDialogueSystem;
            if (dialogueSystem == null)
            {
                Debug.LogError("[DialogueUI] Could not find DialogueManager implementing IDialogueSystem");
                return;
            }

            // Subscribe to dialogue events
            dialogueSystem.OnDialogueStateChanged += HandleDialogueStateChanged;
            dialogueSystem.OnDialogueLine += HandleDialogueLine;

            // Set up UI event listeners
            if (sendButton != null)
            {
                sendButton.onClick.AddListener(SendMessage);
            }
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(CloseDialogue);
            }
            if (playerInput != null)
            {
                playerInput.onSubmit.AddListener(_ => SendMessage());
            }

            // Hide dialogue panel initially
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (dialogueSystem != null)
            {
                dialogueSystem.OnDialogueStateChanged -= HandleDialogueStateChanged;
                dialogueSystem.OnDialogueLine -= HandleDialogueLine;
            }
        }

        private void HandleDialogueStateChanged(bool isInDialogue)
        {
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(isInDialogue);
            }

            if (isInDialogue)
            {
                // Focus the input field when dialogue starts
                if (playerInput != null)
                {
                    playerInput.ActivateInputField();
                }
            }
            else
            {
                // Clear input when dialogue ends
                if (playerInput != null)
                {
                    playerInput.text = string.Empty;
                }
            }
        }

        private void HandleDialogueLine(string message)
        {
            if (dialogueText != null)
            {
                dialogueText.text = message;
            }
        }

        public void ShowDialogue(string npcName, string initialMessage)
        {
            if (npcNameText != null)
            {
                npcNameText.text = npcName;
            }
            if (dialogueText != null)
            {
                dialogueText.text = initialMessage;
            }
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(true);
            }
        }

        public void CloseDialogue()
        {
            if (dialogueSystem != null)
            {
                dialogueSystem.EndDialogue();
            }
        }

        private async void SendMessage()
        {
            if (isProcessingResponse || playerInput == null || string.IsNullOrWhiteSpace(playerInput.text))
            {
                return;
            }

            string message = playerInput.text;
            playerInput.text = string.Empty;
            isProcessingResponse = true;

            try
            {
                if (dialogueSystem != null)
                {
                    string response = await dialogueSystem.GenerateResponse(message);
                    if (dialogueText != null)
                    {
                        dialogueText.text = response;
                    }
                }
            }
            finally
            {
                isProcessingResponse = false;
                if (playerInput != null)
                {
                    playerInput.ActivateInputField();
                }
            }
        }
    }
} 