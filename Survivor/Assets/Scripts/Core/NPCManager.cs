using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Survivor.Core
{
    /// <summary>
    /// Manages all NPCs in the game, handling their creation, relationships, and game state
    /// </summary>
    public class NPCManager : MonoBehaviour
    {
        public static NPCManager Instance { get; private set; }

        [System.Serializable]
        public class NPCData
        {
            public string name;
            public float loyalty;      // 0-100: Likelihood to stay loyal to alliances
            public float sneakiness;   // 0-100: Ability to deceive and plot
            public float charisma;     // 0-100: Influence on other NPCs
            public float aggression;   // 0-100: Tendency to take aggressive actions
            public Dictionary<string, float> relationships = new Dictionary<string, float>();
            public List<string> alliance = new List<string>();
        }

        [Header("NPC Configuration")]
        public GameObject npcPrefab;
        public int startingNPCCount = 8;
        
        [Header("Spawn Settings")]
        public Vector3 spawnCenter = Vector3.zero;
        public float spawnRadius = 10f;

        [Header("NPC Settings")]
        public int initialNPCCount = 8;
        public float minStatValue = 20f;
        public float maxStatValue = 100f;

        private Dictionary<string, NPCData> npcs = new Dictionary<string, NPCData>();
        private List<string> eliminatedNPCs = new List<string>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            GenerateInitialNPCs();
        }

        private void GenerateInitialNPCs()
        {
            string[] defaultNames = {
                "Alex", "Jordan", "Morgan", "Taylor", "Sam", 
                "Casey", "Riley", "Quinn", "Jamie", "Avery"
            };

            for (int i = 0; i < initialNPCCount && i < defaultNames.Length; i++)
            {
                CreateNPC(defaultNames[i]);
            }

            // Initialize relationships between NPCs
            InitializeRelationships();
        }

        public NPCData CreateNPC(string name)
        {
            if (npcs.ContainsKey(name))
                return null;

            NPCData npc = new NPCData
            {
                name = name,
                loyalty = Random.Range(minStatValue, maxStatValue),
                sneakiness = Random.Range(minStatValue, maxStatValue),
                charisma = Random.Range(minStatValue, maxStatValue),
                aggression = Random.Range(minStatValue, maxStatValue)
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
                        // Initialize random relationship value (30-70 for balanced start)
                        float relationshipValue = Random.Range(30f, 70f);
                        npc1.relationships[npc2.name] = relationshipValue;
                    }
                }
            }
        }

        /// <summary>
        /// Updates relationships between NPCs based on game events
        /// </summary>
        public void UpdateRelationship(string npc1Name, string npc2Name, float delta)
        {
            if (!npcs.ContainsKey(npc1Name) || !npcs.ContainsKey(npc2Name))
                return;

            NPCData npc1 = npcs[npc1Name];
            if (!npc1.relationships.ContainsKey(npc2Name))
                npc1.relationships[npc2Name] = 50f;

            npc1.relationships[npc2Name] = Mathf.Clamp(npc1.relationships[npc2Name] + delta, 0f, 100f);
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
        /// Gets all eliminated NPCs in the game
        /// </summary>
        public List<string> GetEliminatedNPCs()
        {
            return new List<string>(eliminatedNPCs);
        }

        /// <summary>
        /// Gets an NPC by their player ID
        /// </summary>
        public NPCData GetNPCData(string npcName)
        {
            return npcs.ContainsKey(npcName) ? npcs[npcName] : null;
        }

        /// <summary>
        /// Gets the relationship between two NPCs
        /// </summary>
        public float GetRelationship(string npc1Name, string npc2Name)
        {
            if (!npcs.ContainsKey(npc1Name) || !npcs.ContainsKey(npc2Name))
                return 0f;

            NPCData npc1 = npcs[npc1Name];
            return npc1.relationships.ContainsKey(npc2Name) ? npc1.relationships[npc2Name] : 50f;
        }

        /// <summary>
        /// Checks if two NPCs are allied
        /// </summary>
        public bool AreAllied(string npc1Name, string npc2Name)
        {
            if (!npcs.ContainsKey(npc1Name) || !npcs.ContainsKey(npc2Name))
                return false;

            return npcs[npc1Name].alliance.Contains(npc2Name);
        }

        // Temporary name generation - replace with proper name list later
        private string GenerateRandomName()
        {
            string[] names = {
                "Alex", "Jordan", "Morgan", "Taylor", "Sam", "Casey", "Riley", "Quinn",
                "Avery", "Parker", "Blake", "Charlie", "Jamie", "Phoenix", "River", "Sage"
            };
            return names[Random.Range(0, names.Length)];
        }
    }
} 