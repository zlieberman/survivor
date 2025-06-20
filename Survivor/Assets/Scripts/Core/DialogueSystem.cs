using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.Events;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using Survivor.Characters;

namespace Survivor.Core
{
    [Serializable]
    public class DialogueContext
    {
        public string npcName;
        public Dictionary<string, float> stats;
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

        [Header("Dialogue Events")]
        public UnityEvent<string> onDialogueStart;
        public UnityEvent onDialogueEnd;
        public UnityEvent<string> onDialogueLine;

        private HttpClient httpClient;
        private bool isProcessingDialogue;
        private INPCManager npcManager;
        private bool isInDialogue = false;

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

        public void Initialize(INPCManager npcManager)
        {
            this.npcManager = npcManager;
        }

        public void StartDialogue(string dialogueId)
        {
            if (isInDialogue) return;

            isInDialogue = true;
            onDialogueStart?.Invoke(dialogueId);
            // TODO: Load and start dialogue sequence
        }

        public void EndDialogue()
        {
            if (!isInDialogue) return;

            isInDialogue = false;
            onDialogueEnd?.Invoke();
        }

        public void ShowDialogueLine(string line)
        {
            onDialogueLine?.Invoke(line);
        }

        public bool IsInDialogue()
        {
            return isInDialogue;
        }

        public async Task<string> GenerateDialogue(Character character, string playerPrompt)
        {
            if (character == null) return "Error: No NPC selected.";

            if (isProcessingDialogue) return null;
            isProcessingDialogue = true;

            try
            {
                var context = BuildDialogueContext(character, playerPrompt);
                
                // Convert context to JSON
                string jsonContext = JsonConvert.SerializeObject(context);

                // Create the prompt for the LLM
                string fullPrompt = $@"You are {context.npcName}, a contestant in a Survivor-style game. 
Respond to the player based on your personality and the current game context.
Your personality traits are: {FormatPersonality(context.stats)}
Game context: {FormatGameContext(context.gameContext)}
Recent history with player: {context.playerHistory}

Player says: {context.dialoguePrompt}

Respond in character, keeping your response concise (1-2 sentences). Consider your personality traits and current game situation.";

                // Send request to LLM
                var response = await SendToLLM(fullPrompt);
                
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

        private DialogueContext BuildDialogueContext(Character character, string playerPrompt)
        {
            var context = new DialogueContext
            {
                npcName = character.CharacterName,
                stats = new Dictionary<string, float>
                {
                    { "perception", character.Stats.perception },
                    { "deception", character.Stats.deception },
                    { "persuasion", character.Stats.persuasion },
                    { "puzzleSolving", character.Stats.puzzleSolving },
                    { "charisma", character.Stats.charisma },
                    { "honesty", character.Stats.honesty },
                    { "trust", character.Stats.trust },
                    { "honor", character.Stats.honor }
                },
                gameContext = new Dictionary<string, bool>
                {
                    { "isInChallenge", false }, // TODO: update when we implement challenges
                    { "isTribalCouncil", false }, // TODO: update when we implement tribal council
                    { "isPlayer", character.IsPlayer }
                },
                playerHistory = GetPlayerHistory(character),
                dialoguePrompt = playerPrompt
            };

            return context;
        }

        private string GetPlayerHistory(Character character)
        {
            // For now, return a simple history based on tribe membership
            return $"Member of {character.TribeName} tribe.";
        }

        private string FormatPersonality(Dictionary<string, float> personality)
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