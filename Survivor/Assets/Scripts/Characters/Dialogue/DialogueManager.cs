using UnityEngine;
using System;
using System.Threading.Tasks;
using Survivor.Shared;
using Survivor.Shared.Interfaces;
using Survivor.Characters.UI;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using System.Threading;
using System.Net.Http.Headers;
using System.Collections.Generic;

namespace Survivor.Characters.Dialogue
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
        [Header("Rate Limiting")]
        [SerializeField] private int maxRetries = 3;
        [SerializeField] private float initialRetryDelay = 1f;
        [SerializeField] private float maxRetryDelay = 10f;
        [SerializeField] private float requestsPerMinute = 10f; // Conservative default

        [Header("OpenAI Settings")]
        [SerializeField] private string openAiApiKey = "";
        private string OpenAiApiKey
        {
            get
            {
                string key = string.IsNullOrEmpty(openAiApiKey) ? Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? "" : openAiApiKey;
                if (string.IsNullOrEmpty(key))
                {
                    Debug.LogError("[DialogueManager] OpenAI API key is not set! Please set it in the Inspector or as OPENAI_API_KEY environment variable.");
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

        private DateTime lastRequestTime = DateTime.MinValue;
        private readonly SemaphoreSlim rateLimiter = new SemaphoreSlim(1, 1);

        private CancellationTokenSource currentDialogueCts;

        public bool IsInDialogue => isInDialogue;
        public bool IsDialogueValid => isInDialogue && currentInteractable != null;

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
                    Debug.LogError("[DialogueManager] Failed to initialize: OpenAI API key is not set!");
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

        private async Task WaitForRateLimit()
        {
            await rateLimiter.WaitAsync();
            try
            {
                var timeSinceLastRequest = DateTime.Now - lastRequestTime;
                var minTimeBetweenRequests = TimeSpan.FromSeconds(60f / requestsPerMinute);
                
                if (timeSinceLastRequest < minTimeBetweenRequests)
                {
                    var waitTime = minTimeBetweenRequests - timeSinceLastRequest;
                    Debug.Log($"[DialogueManager] Rate limiting: waiting {waitTime.TotalSeconds:F1} seconds");
                    await Task.Delay(waitTime);
                }
                lastRequestTime = DateTime.Now;
            }
            finally
            {
                rateLimiter.Release();
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

        public async Task<string> GenerateResponse(string playerMessage)
        {
            if (!IsDialogueValid)
            {
                Debug.LogError("[DialogueManager] GenerateResponse called with invalid dialogue state");
                return "Error: No active dialogue session.";
            }

            if (string.IsNullOrEmpty(OpenAiApiKey))
            {
                Debug.LogError("[DialogueManager] Cannot generate response: OpenAI API key is not set!");
                return "Error: API configuration is missing.";
            }

            // Create new cancellation token source for this dialogue
            currentDialogueCts?.Cancel();
            currentDialogueCts = new CancellationTokenSource();
            var ct = currentDialogueCts.Token;

            int retryCount = 0;
            float currentDelay = initialRetryDelay;

            while (retryCount <= maxRetries)
            {
                try
                {
                    ct.ThrowIfCancellationRequested();
                    await WaitForRateLimit().ConfigureAwait(false);
                    
                    if (!IsDialogueValid)
                    {
                        Debug.Log("[DialogueManager] Dialogue ended while waiting for rate limit");
                        return "Dialogue ended.";
                    }

                    Debug.Log($"[DialogueManager] Generating response for {currentInteractable.GetDisplayName()}: {playerMessage}");
                    
                    var messages = new[]
                    {
                        new ChatMessage { role = "system", content = $"You are {currentInteractable.GetDisplayName()}, a character in a game. Respond naturally and concisely to the player's messages." },
                        new ChatMessage { role = "user", content = playerMessage }
                    };

                    var requestBody = new ChatCompletionRequest
                    {
                        model = openAiModel,
                        messages = messages,
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

                    var response = await httpClient.SendAsync(request, ct).ConfigureAwait(false);
                    var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    LogResponseDetails(response, responseBody);
                    
                    if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        var retryAfter = response.Headers.RetryAfter?.Delta ?? TimeSpan.FromSeconds(currentDelay);
                        Debug.LogWarning($"[DialogueManager] Rate limited. Retrying after {retryAfter.TotalSeconds:F1} seconds");
                        await Task.Delay(retryAfter, ct).ConfigureAwait(false);
                        currentDelay = Math.Min(currentDelay * 2, maxRetryDelay);
                        retryCount++;
                        continue;
                    }

                    if (!response.IsSuccessStatusCode)
                    {
                        Debug.LogError($"[DialogueManager] API Error: {response.StatusCode}\nResponse: {responseBody}");
                        return "I'm having trouble connecting right now. Please try again.";
                    }

                    try
                    {
                        var responseObj = JsonConvert.DeserializeObject<ChatCompletionResponse>(responseBody);
                        
                        if (responseObj?.choices == null || responseObj.choices.Count == 0)
                        {
                            Debug.LogError($"[DialogueManager] Unexpected API response format: {responseBody}");
                            return "I received an unexpected response. Please try again.";
                        }

                        string aiResponse = responseObj.choices[0].message.content;
                        Debug.Log($"[DialogueManager] Successfully generated response: {aiResponse}");
                        return aiResponse;
                    }
                    catch (JsonException e)
                    {
                        Debug.LogError($"[DialogueManager] Failed to parse API response: {e.Message}\nResponse: {responseBody}");
                        return "I received an invalid response. Please try again.";
                    }
                }
                catch (OperationCanceledException)
                {
                    Debug.Log("[DialogueManager] Dialogue generation cancelled");
                    return "Dialogue ended.";
                }
                catch (HttpRequestException e) when (e.Message.Contains("429"))
                {
                    Debug.LogWarning($"[DialogueManager] Rate limited (attempt {retryCount + 1}/{maxRetries + 1}). Waiting {currentDelay:F1} seconds...");
                    await Task.Delay(TimeSpan.FromSeconds(currentDelay), ct).ConfigureAwait(false);
                    currentDelay = Math.Min(currentDelay * 2, maxRetryDelay);
                    retryCount++;
                }
                catch (Exception e)
                {
                    Debug.LogError($"[DialogueManager] Error generating response: {e.Message}\nStack trace: {e.StackTrace}");
                    return "I'm having trouble understanding right now. Can you try again?";
                }
            }

            return "I'm getting too many requests right now. Please try again in a moment.";
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

            // Set current NPC in chat controller
            if (chatController != null)
            {
                chatController.SetCurrentNPC(npcName);
                chatController.AddNPCMessage(initialMessage);
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

            // Notify listeners about dialogue state change
            OnDialogueStateChanged?.Invoke(false);
            OnDialogueLine?.Invoke("Dialogue ended");

            // Clear current interactable
            currentInteractable = null;
            isInRange = false;
        }

        private void OnDestroy()
        {
            if (isInDialogue)
            {
                EndDialogue();
            }
            currentDialogueCts?.Dispose();
            httpClient?.Dispose();
            rateLimiter?.Dispose();
        }
    }
} 