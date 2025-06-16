using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Survivor.Tribes;
using Survivor.Characters;

namespace Survivor.Core
{
    /// <summary>
    /// Manages all NPCs in the game, handling their creation, relationships, and game state
    /// </summary>
    public class NPCManager : MonoBehaviour, INPCManager
    {
        public static NPCManager Instance { get; private set; }

        [System.Serializable]
        public class NPCData
        {
            public string name;
            public int loyalty;      // 0-100: Likelihood to stay loyal to alliances
            public int sneakiness;   // 0-100: Ability to deceive and plot
            public int charisma;     // 0-100: Influence on other NPCs
            public int aggression;   // 0-100: Tendency to take aggressive actions
            public Dictionary<string, int> relationships = new Dictionary<string, int>();
            public List<string> alliance = new List<string>();
        }

        [System.Serializable]
        private class NameData
        {
            public string[] firstNames;
            public string[] lastNames;
        }

        [Header("NPC Configuration")]
        public GameObject npcPrefab;
        public int startingNPCCount = 8;
        
        [Header("Spawn Settings")]
        public Vector3 spawnCenter = Vector3.zero;
        public float spawnRadius = 10f;

        [Header("NPC Settings")]
        public int initialNPCCount = 8;
        public int minStatValue = 20;
        public int maxStatValue = 100;

        private Dictionary<string, NPCData> npcs = new Dictionary<string, NPCData>();
        private List<string> eliminatedNPCs = new List<string>();
        private List<string> availableFirstNames = new List<string>();
        private List<string> availableLastNames = new List<string>();
        private HashSet<string> usedNames = new HashSet<string>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                LoadNames();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void LoadNames()
        {
            TextAsset configFile = Resources.Load<TextAsset>("names");
            if (configFile != null)
            {
                NameData nameData = JsonUtility.FromJson<NameData>(configFile.text);
                availableFirstNames = new List<string>(nameData.firstNames);
                availableLastNames = new List<string>(nameData.lastNames);
                ShuffleNames();
            }
            else
            {
                Debug.LogError("Failed to load names.json from Resources folder!");
            }
        }

        private void ShuffleNames()
        {
            // Fisher-Yates shuffle for first names
            for (int i = availableFirstNames.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                string temp = availableFirstNames[i];
                availableFirstNames[i] = availableFirstNames[j];
                availableFirstNames[j] = temp;
            }

            // Fisher-Yates shuffle for last names
            for (int i = availableLastNames.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                string temp = availableLastNames[i];
                availableLastNames[i] = availableLastNames[j];
                availableLastNames[j] = temp;
            }
        }

        private string GetRandomName()
        {
            if (availableFirstNames.Count == 0 || availableLastNames.Count == 0)
            {
                // If we run out of names, reset the pools
                LoadNames();
            }

            string firstName = availableFirstNames[0];
            string lastName = availableLastNames[0];
            string fullName = $"{firstName} {lastName}";

            availableFirstNames.RemoveAt(0);
            availableLastNames.RemoveAt(0);
            usedNames.Add(fullName);

            return fullName;
        }

        private void ReleaseName(string name)
        {
            if (usedNames.Contains(name))
            {
                string[] nameParts = name.Split(' ');
                if (nameParts.Length == 2)
                {
                    availableFirstNames.Add(nameParts[0]);
                    availableLastNames.Add(nameParts[1]);
                }
                usedNames.Remove(name);
            }
        }

        private void Start()
        {
            GenerateInitialNPCs();
        }

        private void GenerateInitialNPCs()
        {
            for (int i = 0; i < initialNPCCount; i++)
            {
                CreateNPC(GetRandomName());
            }

            // Initialize relationships between NPCs
            InitializeRelationships();
        }

        /// <summary>
        /// Generates a random number from a normal distribution using the Box-Muller transform
        /// </summary>
        /// <param name="mean">The mean of the distribution</param>
        /// <param name="stdDev">The standard deviation of the distribution</param>
        /// <returns>A random number from the normal distribution</returns>
        private int GenerateNormalRandom(int mean, int stdDev)
        {
            // Box-Muller transform
            float u1 = Random.value;
            float u2 = Random.value;
            float randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) * Mathf.Cos(2.0f * Mathf.PI * u2);
            float randNormal = mean + stdDev * randStdNormal;
            
            // Clamp to 0-100 range
            return Mathf.Clamp(Mathf.RoundToInt(randNormal), 0, 100);
        }

        public NPCData CreateNPC(string name = null)
        {
            // If no name provided, get a random one
            if (string.IsNullOrEmpty(name))
            {
                name = GetRandomName();
            }

            if (npcs.ContainsKey(name))
                return null;

            NPCData npc = new NPCData
            {
                name = name,
                loyalty = GenerateNormalRandom(50, 30),
                sneakiness = GenerateNormalRandom(50, 30),
                charisma = GenerateNormalRandom(50, 30),
                aggression = GenerateNormalRandom(50, 30)
            };

            npcs.Add(name, npc);
            return npc;
        }

        /// <summary>
        /// Initializes starting relationships between all NPCs
        /// </summary>
        private void InitializeRelationships()
        {
            foreach (var npc1 in npcs.Values)
            {
                foreach (var npc2 in npcs.Values)
                {
                    if (npc1.name != npc2.name)
                    {
                        // Initialize relationship value using normal distribution
                        int relationshipValue = GenerateNormalRandom(50, 30);
                        npc1.relationships[npc2.name] = relationshipValue;
                    }
                }
            }
        }

        /// <summary>
        /// Updates relationships between NPCs based on game events
        /// </summary>
        public void UpdateRelationship(string npc1Name, string npc2Name, int delta)
        {
            if (!npcs.ContainsKey(npc1Name) || !npcs.ContainsKey(npc2Name))
                return;

            NPCData npc1 = npcs[npc1Name];
            if (!npc1.relationships.ContainsKey(npc2Name))
                npc1.relationships[npc2Name] = 50;

            npc1.relationships[npc2Name] = Mathf.Clamp(npc1.relationships[npc2Name] + delta, 0, 100);
        }

        public void FormAlliance(string npc1Name, string npc2Name)
        {
            if (!npcs.ContainsKey(npc1Name) || !npcs.ContainsKey(npc2Name))
                return;

            NPCData npc1 = npcs[npc1Name];
            NPCData npc2 = npcs[npc2Name];

            if (!npc1.alliance.Contains(npc2Name))
                npc1.alliance.Add(npc2Name);
            if (!npc2.alliance.Contains(npc1Name))
                npc2.alliance.Add(npc1Name);
        }

        public void BreakAlliance(string npc1Name, string npc2Name)
        {
            if (!npcs.ContainsKey(npc1Name) || !npcs.ContainsKey(npc2Name))
                return;

            NPCData npc1 = npcs[npc1Name];
            NPCData npc2 = npcs[npc2Name];

            npc1.alliance.Remove(npc2Name);
            npc2.alliance.Remove(npc1Name);
        }

        /// <summary>
        /// Eliminates an NPC from the game
        /// </summary>
        public void EliminateNPC(string npcName)
        {
            if (!npcs.ContainsKey(npcName))
                return;

            // Remove from alliances
            NPCData eliminatedNPC = npcs[npcName];
            foreach (string ally in eliminatedNPC.alliance.ToArray())
            {
                BreakAlliance(npcName, ally);
            }

            // Remove relationships
            foreach (var npc in npcs.Values)
            {
                npc.relationships.Remove(npcName);
                npc.alliance.Remove(npcName);
            }

            // Release the name back to the pool
            ReleaseName(npcName);

            // Move to eliminated list
            eliminatedNPCs.Add(npcName);
            npcs.Remove(npcName);
        }

        /// <summary>
        /// Gets all active NPCs in the game
        /// </summary>
        public List<string> GetActiveNPCs()
        {
            return new List<string>(npcs.Keys);
        }

        /// <summary>
        /// Gets all eliminated NPCs
        /// </summary>
        public List<string> GetEliminatedNPCs()
        {
            return new List<string>(eliminatedNPCs);
        }

        /// <summary>
        /// Gets data for a specific NPC
        /// </summary>
        public NPCData GetNPCData(string npcName)
        {
            return npcs.ContainsKey(npcName) ? npcs[npcName] : null;
        }

        /// <summary>
        /// Gets the relationship value between two NPCs
        /// </summary>
        public int GetRelationship(string npc1Name, string npc2Name)
        {
            if (!npcs.ContainsKey(npc1Name) || !npcs.ContainsKey(npc2Name))
                return 0;

            NPCData npc1 = npcs[npc1Name];
            return npc1.relationships.ContainsKey(npc2Name) ? npc1.relationships[npc2Name] : 0;
        }

        /// <summary>
        /// Checks if two NPCs are allied
        /// </summary>
        public bool AreAllied(string npc1Name, string npc2Name)
        {
            if (!npcs.ContainsKey(npc1Name) || !npcs.ContainsKey(npc2Name))
                return false;

            NPCData npc1 = npcs[npc1Name];
            return npc1.alliance.Contains(npc2Name);
        }

        public Character GetTribeMember(string memberName)
        {
            NPCData npcData = GetNPCData(memberName);
            if (npcData == null) return null;

            Character character = new Character();
            character.CharacterName = npcData.name;
            character.Stats = new CharacterStats
            {
                perception = npcData.loyalty,
                deception = npcData.sneakiness,
                persuasion = npcData.charisma,
                puzzleSolving = 50, // Default value
                swimming = 50, // Default value
                speed = 50, // Default value
                strength = 50, // Default value
                charisma = npcData.charisma,
                honesty = 100 - npcData.sneakiness,
                trust = npcData.loyalty,
                honor = 100 - npcData.aggression
            };
            return character;
        }

        public List<Character> GetAllTribeMembers()
        {
            return npcs.Keys.Select(name => GetTribeMember(name)).ToList();
        }

        public void AddTribeMember(Character member)
        {
            if (member == null) return;
            // If no name is provided, generate a random one
            if (string.IsNullOrEmpty(member.CharacterName))
            {
                member.CharacterName = GetRandomName();
            }
            CreateNPC(member.CharacterName);
        }

        public void RemoveTribeMember(string memberName)
        {
            EliminateNPC(memberName);
        }

        // Update NPC data
        public void UpdateNPCData(Character character)
        {
            if (character != null)
            {
                Debug.Log($"Updating NPC: {character.CharacterName}");
                if (character.Stats != null)
                {
                    // Update stats
                }
            }
        }
    }
} 