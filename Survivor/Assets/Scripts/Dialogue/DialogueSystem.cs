using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.Events;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using Survivor.Core;

namespace Survivor.Dialogue
{
    [Serializable]
    public class DialogueContext
    {
        public string npcName;
        public Dictionary<string, int> personality;
        public Dictionary<string, bool> gameContext;
        public string playerHistory;
        public string dialoguePrompt;
    }

    public class DialogueSystem : MonoBehaviour
    {
        public static DialogueSystem Instance { get; private set; }

        [Header("LLM Configuration")]
        public string llmEndpoint = "http://localhost:11434/api/generate"; // Default Ollama endpoint
        public string llmModel = "mistral"; // Default model
        public float responseTemperature = 0.7f;
        public int maxTokens = 100;

        [Header("Events")]
        public UnityEvent<string> onDialogueReceived;
        public UnityEvent onDialogueStart;
        public UnityEvent onDialogueEnd;

        private HttpClient httpClient;
        private bool isProcessingDialogue;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                httpClient = new HttpClient();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public async Task<string> GenerateDialogue(NPC npc, string playerPrompt)
        {
            if (isProcessingDialogue) return null;
            isProcessingDialogue = true;

            try
            {
                // Build dialogue context
                var context = BuildDialogueContext(npc, playerPrompt);
                
                // Convert context to JSON
                string jsonContext = JsonConvert.SerializeObject(context);

                // Create the prompt for the LLM
                string fullPrompt = $@"You are {context.npcName}, a contestant in a Survivor-style game. 
Respond to the player based on your personality and the current game context.
Your personality traits are: {FormatPersonality(context.personality)}
Game context: {FormatGameContext(context.gameContext)}
Recent history with player: {context.playerHistory}

Player says: {context.dialoguePrompt}

Respond in character, keeping your response concise (1-2 sentences). Consider your personality traits and current game situation.";

                // Send request to LLM
                var response = await SendToLLM(fullPrompt);
                
                onDialogueReceived?.Invoke(response);
                return response;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error generating dialogue: {e.Message}");
                return "Sorry, I'm having trouble responding right now.";
            }
            finally
            {
                isProcessingDialogue = false;
            }
        }

        private DialogueContext BuildDialogueContext(NPC npc, string playerPrompt)
        {
            var context = new DialogueContext
            {
                npcName = npc.npcName,
                personality = new Dictionary<string, int>
                {
                    { "loyalty", npc.loyalty },
                    { "sneakiness", npc.sneakiness },
                    { "charisma", npc.charisma },
                    { "aggression", npc.aggression }
                },
                gameContext = new Dictionary<string, bool>
                {
                    { "isInChallenge", ChallengeSystem.Instance.IsInChallenge() },
                    { "isVotingTime", VotingSystem.Instance.IsVotingActive },
                    { "hasAlliance", npc.currentAlliances.Contains(0) }, // 0 is player ID
                    { "trustsPlayer", npc.trustScores.TryGetValue(0, out float trust) && trust > 1.2f }
                },
                playerHistory = GetPlayerHistory(npc),
                dialoguePrompt = playerPrompt
            };

            return context;
        }

        private string GetPlayerHistory(NPC npc)
        {
            // Get recent memories involving the player
            var playerMemories = npc.recentMemories
                .FindAll(m => m.Contains("player") || m.Contains("Player"));

            if (playerMemories.Count == 0)
                return "No significant history with player.";

            return string.Join(" ", playerMemories);
        }

        private string FormatPersonality(Dictionary<string, int> personality)
        {
            var traits = new List<string>();
            foreach (var trait in personality)
            {
                string level = trait.Value <= 3 ? "low" : trait.Value >= 8 ? "high" : "moderate";
                traits.Add($"{trait.Key}: {level}");
            }
            return string.Join(", ", traits);
        }

        private string FormatGameContext(Dictionary<string, bool> context)
        {
            var situations = new List<string>();
            foreach (var item in context)
            {
                if (item.Value)
                {
                    situations.Add(item.Key);
                }
            }
            return situations.Count > 0 ? string.Join(", ", situations) : "normal game phase";
        }

        private async Task<string> SendToLLM(string prompt)
        {
            try
            {
                var requestBody = new
                {
                    model = llmModel,
                    prompt = prompt,
                    temperature = responseTemperature,
                    max_tokens = maxTokens
                };

                var content = new StringContent(
                    JsonConvert.SerializeObject(requestBody),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await httpClient.PostAsync(llmEndpoint, content);
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                // Parse response based on your LLM API's response format
                // This is a simplified example
                var responseObj = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonResponse);
                return responseObj["response"] ?? "I'm not sure how to respond to that.";
            }
            catch (Exception e)
            {
                Debug.LogError($"Error calling LLM API: {e.Message}");
                return "I'm having trouble thinking of a response.";
            }
        }

        private void OnDestroy()
        {
            httpClient?.Dispose();
        }
    }
} 