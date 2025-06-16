using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Survivor.Shared.Interfaces;
using Survivor.Characters.Dialogue;

namespace Survivor.Characters.UI
{
    public class ChatController : MonoBehaviour, IChatSystem
    {
        [Header("UI References")]
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private Transform messageContainer;
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private Button sendButton;
        [SerializeField] private GameObject messagePrefab;
        [SerializeField] private TextMeshProUGUI npcNameText;

        [Header("Settings")]
        [SerializeField] private float messageSpacing = 10f;
        [SerializeField] private int maxMessages = 100;
        [SerializeField] private Color playerMessageColor = Color.green;
        [SerializeField] private Color npcMessageColor = Color.cyan;

        private class ChatMessage
        {
            public string text;
            public Color color;
        }

        private Dictionary<string, List<ChatMessage>> npcChatHistories = new Dictionary<string, List<ChatMessage>>();
        private List<GameObject> currentMessageObjects = new List<GameObject>();
        private string currentNPCName;
        private DialogueManager dialogueManager;

        private void Start()
        {
            if (inputField != null)
            {
                inputField.onValueChanged.AddListener(OnInputValueChanged);
            }

            if (sendButton != null)
            {
                sendButton.onClick.AddListener(SendMessage);
            }

            dialogueManager = FindObjectOfType<DialogueManager>();
            if (dialogueManager == null)
            {
                Debug.LogError("[ChatController] Could not find DialogueManager in scene!");
            }

            // Ensure message container has proper layout
            if (messageContainer != null)
            {
                // Add or get VerticalLayoutGroup
                VerticalLayoutGroup layoutGroup = messageContainer.GetComponent<VerticalLayoutGroup>();
                if (layoutGroup == null)
                {
                    layoutGroup = messageContainer.gameObject.AddComponent<VerticalLayoutGroup>();
                }
                
                // Configure layout settings
                layoutGroup.spacing = messageSpacing;
                layoutGroup.childAlignment = TextAnchor.UpperLeft;
                layoutGroup.childForceExpandWidth = true;
                layoutGroup.childForceExpandHeight = false;
                layoutGroup.childControlWidth = true;
                layoutGroup.childControlHeight = true;
                layoutGroup.padding = new RectOffset(10, 10, 10, 10);

                // Add ContentSizeFitter to ensure container expands properly
                ContentSizeFitter sizeFitter = messageContainer.GetComponent<ContentSizeFitter>();
                if (sizeFitter == null)
                {
                    sizeFitter = messageContainer.gameObject.AddComponent<ContentSizeFitter>();
                }
                sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            }
        }

        private void OnInputValueChanged(string value)
        {
            if (sendButton != null)
                sendButton.interactable = !string.IsNullOrWhiteSpace(value);
        }

        public void SetCurrentNPC(string npcName)
        {
            // Store current chat history if we have one
            if (!string.IsNullOrEmpty(currentNPCName) && currentMessageObjects.Count > 0)
            {
                var history = new List<ChatMessage>();
                foreach (var messageObj in currentMessageObjects)
                {
                    var textComponent = messageObj.GetComponent<TextMeshProUGUI>();
                    if (textComponent != null)
                    {
                        history.Add(new ChatMessage 
                        { 
                            text = textComponent.text,
                            color = textComponent.color
                        });
                    }
                }
                npcChatHistories[currentNPCName] = history;
            }

            // Clear current messages
            foreach (var messageObj in currentMessageObjects)
            {
                Destroy(messageObj);
            }
            currentMessageObjects.Clear();

            // Set new NPC
            currentNPCName = npcName;
            if (npcNameText != null)
            {
                npcNameText.text = npcName;
            }

            // Load chat history for new NPC if it exists
            if (npcChatHistories.ContainsKey(npcName))
            {
                foreach (var message in npcChatHistories[npcName])
                {
                    CreateMessageObject(message.text, message.color);
                }
            }
        }

        private GameObject CreateMessageObject(string message, Color color)
        {
            if (messagePrefab == null || messageContainer == null)
            {
                Debug.LogError("[ChatController] Message prefab or container is null!");
                return null;
            }

            GameObject messageObj = Instantiate(messagePrefab, messageContainer);
            TextMeshProUGUI messageText = messageObj.GetComponent<TextMeshProUGUI>();
            
            if (messageText != null)
            {
                messageText.text = message;
                messageText.color = color;
                messageText.margin = new Vector4(10, 5, 10, 5); // Add padding around text
                Debug.Log($"[ChatController] Added message: {message}");
            }
            else
            {
                Debug.LogError("[ChatController] Message prefab is missing TextMeshProUGUI component!");
                Destroy(messageObj);
                return null;
            }

            currentMessageObjects.Add(messageObj);
            return messageObj;
        }

        public async void SendMessage()
        {
            if (inputField == null || string.IsNullOrWhiteSpace(inputField.text)) return;

            // Check if we're in a valid dialogue session
            if (dialogueManager == null || !dialogueManager.IsDialogueValid)
            {
                AddMessage("System: You need to be in a dialogue with an NPC to send messages.", Color.red);
                return;
            }

            string message = inputField.text;
            inputField.text = "";
            inputField.ActivateInputField();

            // Add message to chat with "Me:" prefix
            AddMessage($"Me: {message}", playerMessageColor);

            // Generate response from dialogue manager
            string response = await dialogueManager.GenerateResponse(message);
            AddNPCMessage(response);
        }

        public void AddNPCMessage(string message)
        {
            if (!string.IsNullOrEmpty(currentNPCName))
            {
                AddMessage($"{currentNPCName}: {message}", npcMessageColor);
            }
            else
            {
                AddMessage(message, npcMessageColor);
            }
        }

        public void AddMessage(string message, Color color)
        {
            GameObject messageObj = CreateMessageObject(message, color);
            if (messageObj == null) return;

            // Update NPC's chat history
            if (!string.IsNullOrEmpty(currentNPCName))
            {
                if (!npcChatHistories.ContainsKey(currentNPCName))
                {
                    npcChatHistories[currentNPCName] = new List<ChatMessage>();
                }
                npcChatHistories[currentNPCName].Add(new ChatMessage { text = message, color = color });
            }

            // Remove oldest if needed
            if (currentMessageObjects.Count > maxMessages)
            {
                Destroy(currentMessageObjects[0]);
                currentMessageObjects.RemoveAt(0);
            }

            // Scroll to bottom
            Canvas.ForceUpdateCanvases();
            if (scrollRect != null)
            {
                scrollRect.verticalNormalizedPosition = 0f;
            }
            else
            {
                Debug.LogError("[ChatController] ScrollRect is null!");
            }
        }

        public void ClearChat()
        {
            foreach (var messageObj in currentMessageObjects)
            {
                Destroy(messageObj);
            }
            currentMessageObjects.Clear();
            npcChatHistories.Clear();
        }

        public void FocusInputField()
        {
            if (inputField != null)
            {
                inputField.ActivateInputField();
            }
        }

        private void OnDestroy()
        {
            if (inputField != null)
            {
                inputField.onValueChanged.RemoveListener(OnInputValueChanged);
            }

            if (sendButton != null)
            {
                sendButton.onClick.RemoveListener(SendMessage);
            }

            ClearChat();
        }
    }
} 