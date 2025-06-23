using UnityEngine;
using System;
using System.Threading.Tasks;
using Survivor.Shared;
using Survivor.Shared.Interfaces;
using Survivor.Characters.UI;
using Survivor.UI;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using System.Threading;
using System.Net.Http.Headers;
using System.Collections.Generic;
using Survivor.Characters;
using TMPro;

namespace Survivor.Core.Interaction
{
    [Serializable]
    public class ChatMessage
    {
        public string role { get; set; }   // "user", "system", or "assistant"
        public string content { get; set; }
    }

    [Serializable]
    public class ChatCompletionRequest
    {
        public string model { get; set; }
        public float temperature { get; set; }
        public int max_tokens { get; set; }
        public ChatMessage[] messages { get; set; }
    }

    [Serializable]
    public class ChatCompletionResponse
    {
        public List<Choice> choices { get; set; }
    }

    [Serializable]
    public class Choice
    {
        public ChatMessage message { get; set; }
    }

    public class DialogueManager : MonoBehaviour, IDialogueSystem
    {
        private static DialogueManager instance;
        public static DialogueManager Instance => instance;

        [Header("Dialogue Settings")]
        [SerializeField] private float interactionDistance = 3f;
        [SerializeField] private KeyCode interactKey = KeyCode.E;
        public float dialogueTimeout = 30f;

        [Header("OpenAI Settings")]
        [SerializeField] private string openAiApiKey = "";
        private string OpenAiApiKey
        {
            get
            {
                string key = string.IsNullOrEmpty(openAiApiKey) ? System.Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? "" : openAiApiKey;
                if (string.IsNullOrEmpty(key))
                {
                    Debug.LogWarning("[DialogueManager] OpenAI API key is not set! Dialogue will use fallback responses. Please set OPENAI_API_KEY environment variable or configure in Inspector.");
                }
                else
                {
                    Debug.Log("[DialogueManager] Using OpenAI API key from " + (string.IsNullOrEmpty(openAiApiKey) ? "environment variable" : "Inspector"));
                }
                return key;
            }
        }
        [SerializeField] private string openAiModel = "gpt-3.5-turbo";
        [SerializeField] private float temperature = 0.7f;
        [SerializeField] private int maxTokens = 150;

        public event Action<string> OnDialogueLine;
        public event Action<bool> OnDialogueStateChanged;

        private IDialogueInteractable currentInteractable;
        private bool isInRange = false;
        private bool isInDialogue = false;
        private CursorLockMode previousCursorLockState;
        private bool wasCursorVisible;
        private ChatController chatController;
        private HttpClient httpClient;

        private CancellationTokenSource currentDialogueCts;

        // Store chat history per NPC
        private Dictionary<string, List<ChatMessage>> npcChatHistories = new Dictionary<string, List<ChatMessage>>();

        public bool IsInDialogue => isInDialogue;
        public bool IsDialogueValid => isInDialogue && currentInteractable != null;

        private static readonly System.Random threadSafeRandom = new System.Random();

        public bool IsInteractable { get; private set; }
        public float DistanceToPlayer { get; private set; }

        [Header("UI")]
        [SerializeField] private DialogueUI dialogueUI;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                httpClient = new HttpClient();
                
                string apiKey = OpenAiApiKey;
                if (string.IsNullOrEmpty(apiKey))
                {
                    Debug.LogWarning("[DialogueManager] OpenAI API key is not set! Dialogue will use fallback responses. Please set OPENAI_API_KEY environment variable or configure in Inspector.");
                    return;
                }
                
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("UnityGameClient/1.0");
                Debug.Log("[DialogueManager] Initialized as singleton with OpenAI API key");
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

            // Find ChatController
            chatController = FindObjectOfType<ChatController>(true); // Include inactive objects
            if (chatController == null)
            {
                Debug.LogError("[DialogueManager] Could not find ChatController in scene! Make sure it's attached to the ChatPanel GameObject.");
                // Try to find it by name as a fallback
                GameObject chatPanel = GameObject.Find("ChatPanel");
                if (chatPanel != null)
                {
                    chatController = chatPanel.GetComponent<ChatController>();
                    if (chatController == null)
                    {
                        Debug.LogError("[DialogueManager] Found ChatPanel but it doesn't have a ChatController component!");
                    }
                    else
                    {
                        Debug.Log("[DialogueManager] Found ChatController on ChatPanel GameObject");
                    }
                }
                else
                {
                    Debug.LogError("[DialogueManager] Could not find ChatPanel GameObject in scene!");
                }
            }
            else
            {
                Debug.Log("[DialogueManager] Found ChatController in scene");
            }

            // Ensure InputBlocker exists
            if (InputBlocker.Instance == null)
            {
                GameObject inputBlockerObj = new GameObject("InputBlocker");
                inputBlockerObj.AddComponent<InputBlocker>();
                Debug.Log("[DialogueManager] Created InputBlocker for chat input management");
            }

            // Find DialogueUI if not assigned
            if (dialogueUI == null)
            {
                dialogueUI = FindObjectOfType<DialogueUI>();
                if (dialogueUI != null)
                {
                    Debug.Log("[DialogueManager] Found DialogueUI automatically");
                }
                else
                {
                    Debug.LogWarning("[DialogueManager] DialogueUI not found! Dialogue will not be shown.");
                }
            }

            // Wire up send button to manager's handler
            if (dialogueUI != null && dialogueUI.SendButton != null)
            {
                // Remove the old wiring since we're now using events
                // dialogueUI.SendButton.onClick.RemoveAllListeners();
                // dialogueUI.SendButton.onClick.AddListener(OnSendButtonClicked);
            }

            // Subscribe to dialogue UI close event
            if (dialogueUI != null)
            {
                dialogueUI.OnDialogueClosed += EndDialogue;
                dialogueUI.OnMessageSent += OnDialogueUIMessageSent;
            }
        }

        private void Update()
        {
            // If we're already in dialogue, don't check for NPC proximity
            if (isInDialogue)
            {
                // Only handle input for ending dialogue (Escape key)
                if (InputBlocker.GetKeyDown(KeyCode.Escape))
                {
                    Debug.Log("[DialogueManager] Escape key pressed during dialogue, ending dialogue");
                    EndDialogue();
                }
                return; // Exit early to prevent proximity checks during active dialogue
            }

            // Find all NPCCharacter instances in the scene
            var npcs = GameObject.FindObjectsOfType<Survivor.Characters.NPCCharacter>();
            Survivor.Characters.NPCCharacter closestNPC = null;
            float closestDistance = float.MaxValue;

            foreach (var npc in npcs)
            {
                if (npc.IsInteractable && npc.DistanceToPlayer <= interactionDistance)
                {
                    if (npc.DistanceToPlayer < closestDistance)
                    {
                        closestDistance = npc.DistanceToPlayer;
                        closestNPC = npc;
                    }
                }
            }

            // Set the current interactable if found, otherwise null
            if (closestNPC != null)
            {
                if (currentInteractable != closestNPC)
                {
                    currentInteractable = closestNPC;
                    isInRange = true;
                    Debug.Log($"[DialogueManager] Found interactable NPC: {closestNPC.CharacterName}");
                }
            }
            else
            {
                if (currentInteractable != null)
                {
                    Debug.Log("[DialogueManager] No interactable NPC in range");
                }
                currentInteractable = null;
                isInRange = false;
            }

            // Handle input for starting dialogue
            if (isInRange && InputBlocker.GetKeyDown(interactKey) && currentInteractable != null && !isInDialogue)
            {
                Debug.Log($"[DialogueManager] Starting dialogue with {currentInteractable.GetDisplayName()}");
                StartDialogue(currentInteractable.GetDisplayName(), "Starting conversation...");
            }
            
            // Debug logging for E key presses
            if (InputBlocker.GetKeyDown(interactKey))
            {
                Debug.Log($"[DialogueManager] E key pressed - isInRange: {isInRange}, currentInteractable: {(currentInteractable != null ? currentInteractable.GetDisplayName() : "null")}, isInDialogue: {isInDialogue}");
            }
        }

        private void LogRequestDetails(HttpRequestMessage request, string requestBody)
        {
            Debug.Log($"[DialogueManager] Request Details:\n" +
                     $"URL: {request.RequestUri}\n" +
                     $"Method: {request.Method}\n" +
                     $"Headers: {string.Join(", ", request.Headers)}\n" +
                     $"Body: {requestBody}");
        }

        private void LogResponseDetails(HttpResponseMessage response, string responseBody)
        {
            Debug.Log($"[DialogueManager] Response Details:\n" +
                     $"Status: {response.StatusCode} ({(int)response.StatusCode})\n" +
                     $"Headers: {string.Join(", ", response.Headers)}\n" +
                     $"Body: {responseBody}");
        }

        private string GetFallbackResponse()
        {
            try
            {
                string[] fallbackResponses = { "What?", "Could you say that in the other ear?" };
                lock (threadSafeRandom)
                {
                    return fallbackResponses[threadSafeRandom.Next(fallbackResponses.Length)];
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[DialogueManager] Error in GetFallbackResponse: {ex.Message}. Using hardcoded fallback.");
                return "What?"; // Hardcoded fallback if random fails
            }
        }

        public async Task<string> GenerateResponse(string playerMessage)
        {
            Debug.Log("[DialogueManager] GenerateResponse started");
            
            if (!IsDialogueValid)
            {
                Debug.LogError($"[DialogueManager] GenerateResponse called with invalid dialogue state - isInDialogue: {isInDialogue}, currentInteractable: {(currentInteractable != null ? currentInteractable.GetDisplayName() : "null")}");
                return GetFallbackResponse();
            }

            if (string.IsNullOrEmpty(OpenAiApiKey))
            {
                Debug.LogWarning("[DialogueManager] Cannot generate AI response: OpenAI API key is not set! Using fallback response.");
                return GetFallbackResponse();
            }

            // Store the current interactable to ensure it doesn't change during the API call
            var originalInteractable = currentInteractable;
            var originalNpcName = originalInteractable?.GetDisplayName();

            // Create new cancellation token source for this dialogue
            currentDialogueCts?.Cancel();
            currentDialogueCts = new CancellationTokenSource();
            var ct = currentDialogueCts.Token;

            try
            {
                Debug.Log("[DialogueManager] Starting API call...");
                
                // Check if dialogue state is still valid
                if (!IsDialogueValid || currentInteractable != originalInteractable)
                {
                    Debug.LogWarning($"[DialogueManager] Dialogue state changed during API call - isInDialogue: {isInDialogue}, currentInteractable: {(currentInteractable != null ? currentInteractable.GetDisplayName() : "null")}, originalInteractable: {(originalInteractable != null ? originalInteractable.GetDisplayName() : "null")}");
                    return "Dialogue ended.";
                }

                string npcName = originalNpcName;
                Debug.Log($"[DialogueManager] Generating response for {npcName}: {playerMessage}");
                
                try
                {
                    // Get or create chat history for this NPC
                    if (!npcChatHistories.ContainsKey(npcName))
                    {
                        Debug.Log($"[DialogueManager] Creating new chat history for {npcName}");
                        npcChatHistories[npcName] = new List<ChatMessage>();
                        
                        // Get tribe information
                        string tribeName = originalInteractable.TribeName;
                        var tribeManager = FindObjectOfType<TribeManager>();
                        var player = tribeManager?.GetPlayer();
                        var tribeMembers = tribeManager?.GetTribeMembers(tribeName) ?? new List<Character>();
                        
                        // Build tribe context
                        string tribeContext = $"You are {npcName}, a character in a game. You are part of the {tribeName} tribe. ";
                        
                        // Add player context if available
                        if (player != null)
                        {
                            tribeContext += $"The player is {player.CharacterName}, also in your tribe. ";
                            tribeContext += $"The player's stats are: ";
                            tribeContext += $"Perception: {player.Stats.perception}, ";
                            tribeContext += $"Deception: {player.Stats.deception}, ";
                            tribeContext += $"Persuasion: {player.Stats.persuasion}, ";
                            tribeContext += $"Puzzle Solving: {player.Stats.puzzleSolving}, ";
                            tribeContext += $"Swimming: {player.Stats.swimming}, ";
                            tribeContext += $"Speed: {player.Stats.speed}, ";
                            tribeContext += $"Strength: {player.Stats.strength}, ";
                            tribeContext += $"Agility: {player.Stats.agility}, ";
                            tribeContext += $"Intelligence: {player.Stats.intelligence}, ";
                            tribeContext += $"Stamina: {player.Stats.stamina}, ";
                            tribeContext += $"Charisma: {player.Stats.charisma}, ";
                            tribeContext += $"Honesty: {player.Stats.honesty}, ";
                            tribeContext += $"Trust: {player.Stats.trust}, ";
                            tribeContext += $"Honor: {player.Stats.honor}. ";
                            tribeContext += $"Energy: {player.Stats.energy}, ";
                            tribeContext += $"Hunger: {player.Stats.hunger}, ";
                            tribeContext += $"Thirst: {player.Stats.thirst}. ";
                        }
                        
                        // Add other tribe members context
                        if (tribeMembers.Count > 0)
                        {
                            tribeContext += "Other members of your tribe are: ";
                            foreach (var member in tribeMembers)
                            {
                                if (member != player && member != originalInteractable)
                                {
                                    tribeContext += $"{member.CharacterName} (";
                                    tribeContext += $"Perception: {member.Stats.perception}, ";
                                    tribeContext += $"Deception: {member.Stats.deception}, ";
                                    tribeContext += $"Persuasion: {member.Stats.persuasion}, ";
                                    tribeContext += $"Puzzle Solving: {member.Stats.puzzleSolving}, ";
                                    tribeContext += $"Swimming: {member.Stats.swimming}, ";
                                    tribeContext += $"Speed: {member.Stats.speed}, ";
                                    tribeContext += $"Strength: {member.Stats.strength}, ";
                                    tribeContext += $"Agility: {member.Stats.agility}, ";
                                    tribeContext += $"Intelligence: {member.Stats.intelligence}, ";
                                    tribeContext += $"Stamina: {member.Stats.stamina}, ";
                                    tribeContext += $"Charisma: {member.Stats.charisma}, ";
                                    tribeContext += $"Honesty: {member.Stats.honesty}, ";
                                    tribeContext += $"Trust: {member.Stats.trust}, ";
                                    tribeContext += $"Honor: {member.Stats.honor}, ";
                                    tribeContext += $"Energy: {member.Stats.energy}, ";
                                    tribeContext += $"Hunger: {member.Stats.hunger}, ";
                                    tribeContext += $"Thirst: {member.Stats.thirst}), ";
                                }
                            }
                            tribeContext = tribeContext.TrimEnd(',', ' ') + ". ";
                        }
                        
                        // Add your own stats
                        tribeContext += $"Your stats are: ";
                        tribeContext += $"Perception: {originalInteractable.Stats.perception}, ";
                        tribeContext += $"Deception: {originalInteractable.Stats.deception}, ";
                        tribeContext += $"Persuasion: {originalInteractable.Stats.persuasion}, ";
                        tribeContext += $"Puzzle Solving: {originalInteractable.Stats.puzzleSolving}, ";
                        tribeContext += $"Swimming: {originalInteractable.Stats.swimming}, ";
                        tribeContext += $"Speed: {originalInteractable.Stats.speed}, ";
                        tribeContext += $"Strength: {originalInteractable.Stats.strength}, ";
                        tribeContext += $"Agility: {originalInteractable.Stats.agility}, ";
                        tribeContext += $"Intelligence: {originalInteractable.Stats.intelligence}, ";
                        tribeContext += $"Stamina: {originalInteractable.Stats.stamina}, ";
                        tribeContext += $"Charisma: {originalInteractable.Stats.charisma}, ";
                        tribeContext += $"Honesty: {originalInteractable.Stats.honesty}, ";
                        tribeContext += $"Trust: {originalInteractable.Stats.trust}, ";
                        tribeContext += $"Honor: {originalInteractable.Stats.honor}. ";
                        tribeContext += $"Energy: {originalInteractable.Stats.energy}, ";
                        tribeContext += $"Hunger: {originalInteractable.Stats.hunger}, ";
                        tribeContext += $"Thirst: {originalInteractable.Stats.thirst}. ";
                        
                        tribeContext += "Respond naturally and concisely to the player's messages, taking into account your tribe members' stats and your own stats.";
                        
                        // Add system message for new conversations
                        npcChatHistories[npcName].Add(new ChatMessage 
                        { 
                            role = "system", 
                            content = tribeContext
                        });
                    }
                    else
                    {
                        Debug.Log($"[DialogueManager] Using existing chat history for {npcName} with {npcChatHistories[npcName].Count} messages");
                    }

                    // Add user message to history
                    npcChatHistories[npcName].Add(new ChatMessage 
                    { 
                        role = "user", 
                        content = playerMessage 
                    });

                    Debug.Log($"[DialogueManager] Chat history now has {npcChatHistories[npcName].Count} messages");

                    var requestBody = new ChatCompletionRequest
                    {
                        model = openAiModel,
                        messages = npcChatHistories[npcName].ToArray(),
                        temperature = temperature,
                        max_tokens = maxTokens
                    };

                    var jsonRequest = JsonConvert.SerializeObject(requestBody);
                    var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                    // Create request and log details
                    var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions")
                    {
                        Content = content
                    };
                    LogRequestDetails(request, jsonRequest);

                    // Send API request with timeout
                    var timeoutTask = Task.Delay(20000, ct); // 20 seconds
                    var responseTask = httpClient.SendAsync(request, ct);
                    var completedTask = await Task.WhenAny(responseTask, timeoutTask);

                    if (completedTask == timeoutTask)
                    {
                        Debug.LogWarning("[DialogueManager] API call timed out after 20 seconds. Using fallback response.");
                        return GetFallbackResponse();
                    }

                    var response = await responseTask;
                    var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    LogResponseDetails(response, responseBody);

                    if (!response.IsSuccessStatusCode)
                    {
                        // Log API errors as warnings instead of errors to prevent game-breaking issues
                        string errorMessage = $"[DialogueManager] API Error: {response.StatusCode}";
                        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            errorMessage += " - Invalid API key. Please check your OpenAI API key configuration.";
                        }
                        else if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                        {
                            errorMessage += " - Rate limit exceeded. Please wait before trying again.";
                        }
                        else if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                        {
                            errorMessage += " - OpenAI service temporarily unavailable.";
                        }
                        else
                        {
                            errorMessage += $" - Unexpected error: {responseBody}";
                        }
                        
                        Debug.LogWarning(errorMessage);
                        return GetFallbackResponse();
                    }

                    try
                    {
                        var responseObj = JsonConvert.DeserializeObject<ChatCompletionResponse>(responseBody);
                        
                        if (responseObj?.choices == null || responseObj.choices.Count == 0)
                        {
                            Debug.LogWarning($"[DialogueManager] Unexpected API response format. Using fallback response.");
                            return GetFallbackResponse();
                        }

                        string aiResponse = responseObj.choices[0].message.content;
                        
                        // Add AI response to chat history
                        npcChatHistories[npcName].Add(new ChatMessage 
                        { 
                            role = "assistant", 
                            content = aiResponse 
                        });

                        Debug.Log($"[DialogueManager] Successfully generated response: {aiResponse}");
                        return aiResponse;
                    }
                    catch (JsonException e)
                    {
                        Debug.LogWarning($"[DialogueManager] Failed to parse API response: {e.Message}. Using fallback response.");
                        return GetFallbackResponse();
                    }
                }
                catch (System.Exception apiEx)
                {
                    Debug.LogWarning($"[DialogueManager] API call failed: {apiEx.Message}. Using fallback response.");
                    return GetFallbackResponse();
                }
            }
            catch (OperationCanceledException)
            {
                Debug.Log("[DialogueManager] Dialogue generation cancelled");
                return "Dialogue ended.";
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[DialogueManager] Unexpected error in GenerateResponse: {e.Message}. Using fallback response.");
                return GetFallbackResponse();
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

            // Block input
            if (InputBlocker.Instance != null)
            {
                InputBlocker.Instance.BlockInput();
            }

            // Set current NPC in chat controller
            if (chatController != null)
            {
                chatController.SetCurrentNPC(npcName);
                chatController.AddNPCMessage(initialMessage);
            }

            // Show dialogue UI
            if (dialogueUI != null)
            {
                dialogueUI.ShowDialogue(npcName, initialMessage);
            }

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

            // Unblock input
            if (InputBlocker.Instance != null)
            {
                InputBlocker.Instance.UnblockInput();
            }

            // Cancel any ongoing dialogue generation
            currentDialogueCts?.Cancel();
            currentDialogueCts?.Dispose();
            currentDialogueCts = null;

            // Notify the NPC that dialogue has ended
            if (currentInteractable != null)
            {
                currentInteractable.OnDialogueEnd();
            }

            // Restore previous cursor state
            Cursor.lockState = previousCursorLockState;
            Cursor.visible = wasCursorVisible;

            // Hide dialogue UI
            if (dialogueUI != null)
            {
                dialogueUI.CloseDialogue();
            }

            // Notify listeners about dialogue state change
            OnDialogueStateChanged?.Invoke(false);
            OnDialogueLine?.Invoke("Dialogue ended");

            // Note: Don't clear currentInteractable or isInRange here
            // The Update() method will handle this based on proximity
        }

        public void SetInteractable(IDialogueInteractable interactable, bool inRange)
        {
            currentInteractable = interactable;
            isInRange = inRange;
            
            if (inRange && interactable != null)
            {
                Debug.Log($"[DialogueManager] Set interactable: {interactable.GetDisplayName()}");
            }
            else if (!inRange)
            {
                Debug.Log("[DialogueManager] Cleared interactable");
            }
        }

        private void OnDialogueUIMessageSent(string message)
        {
            Debug.Log($"[DialogueManager] OnDialogueUIMessageSent called with message: {message}");
            
            // Handle the message by sending it through the ChatController
            if (chatController != null)
            {
                Debug.Log("[DialogueManager] ChatController found, forwarding message");
                
                // Check if ChatController is already waiting for a response
                var isWaitingField = chatController.GetType().GetField("isWaitingForResponse", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (isWaitingField != null)
                {
                    bool isWaiting = (bool)isWaitingField.GetValue(chatController);
                    if (isWaiting)
                    {
                        Debug.LogWarning("[DialogueManager] ChatController is already waiting for a response, ignoring this message");
                        return;
                    }
                }
                
                // Set the input field text in ChatController and trigger its SendMessage
                var chatInputField = chatController.GetType().GetField("inputField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(chatController) as TMP_InputField;
                if (chatInputField != null)
                {
                    Debug.Log("[DialogueManager] Setting ChatController input field text");
                    chatInputField.text = message;
                    // Call the ChatController's SendMessage method
                    Debug.Log("[DialogueManager] Calling ChatController.SendMessage()");
                    chatController.SendMessage();
                }
                else
                {
                    Debug.LogError("[DialogueManager] Could not access ChatController input field!");
                }
            }
            else
            {
                Debug.LogError("[DialogueManager] ChatController is null! Cannot send message.");
            }
        }

        private void OnDestroy()
        {
            if (isInDialogue)
            {
                EndDialogue();
            }
            
            // Unsubscribe from dialogue UI events
            if (dialogueUI != null)
            {
                dialogueUI.OnDialogueClosed -= EndDialogue;
                dialogueUI.OnMessageSent -= OnDialogueUIMessageSent;
            }
            
            currentDialogueCts?.Dispose();
            httpClient?.Dispose();
        }
    }
} 