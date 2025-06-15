using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using StarterAssets;
using System.Linq;
using Survivor.Generation;

namespace Survivor.Tribes
{
    public class TribeManager : MonoBehaviour, ITribeManager, INPCManager
    {
        public static TribeManager Instance { get; private set; }

        [Header("Spawn Settings")]
        public float spawnRadius = 10f; // Increased spawn radius
        public float minSpawnDistance = 2.5f; // Increased minimum distance between NPCs
        public int maxSpawnAttempts = 30; // Maximum attempts to find valid spawn position

        [Header("Tribe Settings")]
        public string tribeAName = "TribeA";
        public string tribeBName = "TribeB";

        [Header("Prefabs")]
        public GameObject npcPrefab; // Reference to the NPC prefab to spawn

        private Dictionary<string, List<TribeMember>> tribes = new Dictionary<string, List<TribeMember>>();
        private List<TribeMember> tribeMembers = new List<TribeMember>();
        private bool isInitialized = false;
        private CampGenerator campGenerator;

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

            // Find the camp generator
            campGenerator = FindObjectOfType<CampGenerator>();
            if (campGenerator == null)
            {
                Debug.LogError("CampGenerator not found in scene!");
            }
        }

        public void Initialize()
        {
            if (isInitialized) return;

            tribes.Clear();
            tribes[tribeAName] = new List<TribeMember>();
            tribes[tribeBName] = new List<TribeMember>();

            isInitialized = true;
            Debug.Log("TribeManager initialized");
        }

        public IEnumerator CreateTribes()
        {
            if (!isInitialized)
            {
                Debug.LogError("TribeManager not initialized!");
                yield break;
            }

            // Wait for camp to be placed
            while (campGenerator == null || !campGenerator.CampPlaced)
            {
                Debug.Log("Waiting for camp to be placed...");
                yield return new WaitForSeconds(0.5f);
            }

            // Create Tribe A (Player + 8 NPCs)
            yield return StartCoroutine(CreateTribe(tribeAName, true));

            // Create Tribe B (9 NPCs) but don't spawn them yet
            yield return StartCoroutine(CreateTribe(tribeBName, false, false));

            Debug.Log("Tribes created successfully");
        }

        private IEnumerator CreateTribe(string tribeName, bool includePlayer, bool spawnNPCs = true)
        {
            Debug.Log($"Starting to create tribe: {tribeName}");
            List<TribeMember> tribeMembers = new List<TribeMember>();
            
            // Create NPCs for the tribe
            int npcCount = includePlayer ? 8 : 9; // 8 NPCs + player for tribe A, 9 NPCs for tribe B
            Debug.Log($"Creating {npcCount} members for tribe {tribeName}");
            
            for (int i = 0; i < npcCount; i++)
            {
                // Find a valid spawn position
                Vector3 spawnPosition = FindValidSpawnPosition(tribeMembers);
                
                // Create NPC GameObject
                GameObject npcObject = Instantiate(npcPrefab, spawnPosition, Quaternion.identity);
                TribeMember tribeMember = npcObject.GetComponent<TribeMember>();
                
                if (tribeMember != null)
                {
                    // Initialize tribe member
                    string memberName = $"NPC_{tribeName}_{i + 1}";
                    tribeMember.Initialize(memberName, tribeName, includePlayer && i == 0, i);
                    
                    // Add to tribe
                    tribes[tribeName].Add(tribeMember);
                    tribeMembers.Add(tribeMember);

                    // If we shouldn't spawn NPCs for this tribe, disable the GameObject
                    if (!spawnNPCs && !tribeMember.IsPlayer)
                    {
                        npcObject.SetActive(false);
                    }
                    
                    Debug.Log($"Created tribe member: {memberName} for tribe {tribeName} at position {spawnPosition}");
                }
                else
                {
                    Debug.LogError($"Failed to get TribeMember component for NPC in tribe {tribeName}");
                }
                
                yield return null; // Wait one frame between each NPC creation
            }
            
            Debug.Log($"Finished creating tribe {tribeName} with {tribeMembers.Count} members");
            yield return null;
        }

        private Vector3 FindValidSpawnPosition(List<TribeMember> existingMembers)
        {
            if (campGenerator == null || !campGenerator.CampPlaced)
            {
                Debug.LogError("Camp not placed yet!");
                return Vector3.zero;
            }

            Vector3 campCenter = campGenerator.TentPosition;
            Vector3 position = Vector3.zero;
            int attempts = 0;
            bool validPosition = false;

            while (!validPosition && attempts < maxSpawnAttempts)
            {
                // Generate random position within spawn radius
                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                float distance = Random.Range(minSpawnDistance, spawnRadius);
                position = campCenter + new Vector3(
                    Mathf.Cos(angle) * distance,
                    0,
                    Mathf.Sin(angle) * distance
                );

                // Get the terrain height at this position
                Terrain terrain = Terrain.activeTerrain;
                if (terrain != null)
                {
                    position.y = terrain.SampleHeight(position);
                }

                // Check if position is valid (not too close to other NPCs)
                validPosition = true;
                foreach (TribeMember member in existingMembers)
                {
                    if (Vector3.Distance(position, member.transform.position) < minSpawnDistance)
                    {
                        validPosition = false;
                        break;
                    }
                }

                attempts++;
            }

            // If no valid position found, return a position at the edge of the spawn radius
            if (!validPosition)
            {
                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                position = campCenter + new Vector3(
                    Mathf.Cos(angle) * spawnRadius,
                    0,
                    Mathf.Sin(angle) * spawnRadius
                );

                // Get the terrain height at this position
                Terrain terrain = Terrain.activeTerrain;
                if (terrain != null)
                {
                    position.y = terrain.SampleHeight(position);
                }
            }

            return position;
        }

        public List<TribeMember> GetTribeMembers(string tribeName)
        {
            return tribes.ContainsKey(tribeName) ? tribes[tribeName] : new List<TribeMember>();
        }

        public TribeMember GetPlayer()
        {
            return tribeMembers.FirstOrDefault(member => member.IsPlayer);
        }

        public void EliminateMember(TribeMember member)
        {
            if (member != null)
            {
                member.gameObject.SetActive(false);
                tribeMembers.Remove(member);
                foreach (var tribe in tribes.Values)
                {
                    tribe.Remove(member);
                }
            }
        }

        public TribeMember GetNPCData(string npcName)
        {
            return tribeMembers.FirstOrDefault(member => member.name == npcName);
        }

        public void StartChallenge(List<TribeMember> participants)
        {
            foreach (var participant in participants)
            {
                if (participant != null)
                {
                    participant.IsInChallenge = true;
                }
            }
        }

        public void EliminateParticipant(TribeMember member)
        {
            if (member != null)
            {
                member.IsInChallenge = false;
                EliminateMember(member);
            }
        }

        public bool IsParticipantInChallenge(TribeMember member)
        {
            return member != null && member.IsInChallenge;
        }

        public List<TribeMember> GetActiveParticipants()
        {
            return tribeMembers.Where(member => member != null && member.IsInChallenge).ToList();
        }

        // INPCManager implementation
        public TribeMember GetTribeMember(string memberName)
        {
            return tribeMembers.FirstOrDefault(member => member.name == memberName);
        }

        public List<TribeMember> GetAllTribeMembers()
        {
            return new List<TribeMember>(tribeMembers);
        }

        public void AddTribeMember(TribeMember member)
        {
            if (member != null && !tribeMembers.Contains(member))
            {
                tribeMembers.Add(member);
            }
        }

        public void RemoveTribeMember(string memberName)
        {
            var member = GetTribeMember(memberName);
            if (member != null)
            {
                tribeMembers.Remove(member);
            }
        }

        // INPCManager implementation
        List<string> INPCManager.GetActiveNPCs()
        {
            return tribeMembers
                .Where(member => member != null && member.gameObject.activeInHierarchy)
                .Select(member => member.name)
                .ToList();
        }

        // ITribeManager implementation
        IEnumerable<string> ITribeManager.GetActiveNPCs()
        {
            return tribeMembers
                .Where(member => member != null && member.gameObject.activeInHierarchy)
                .Select(member => member.name);
        }

        public void UpdateRelationship(string npc1Name, string npc2Name, float delta)
        {
            var npc1 = GetTribeMember(npc1Name);
            var npc2 = GetTribeMember(npc2Name);

            if (npc1 != null && npc2 != null)
            {
                // Update relationship in both directions
                npc1.Relationships[npc2Name] = Mathf.Clamp((npc1.Relationships.ContainsKey(npc2Name) ? npc1.Relationships[npc2Name] : 0f) + delta, -1f, 1f);
                npc2.Relationships[npc1Name] = Mathf.Clamp((npc2.Relationships.ContainsKey(npc1Name) ? npc2.Relationships[npc1Name] : 0f) + delta, -1f, 1f);
            }
        }

        public bool CanStartChallenge(Vector3 position)
        {
            // Check if there are enough active NPCs nearby to start a challenge
            var nearbyNPCs = tribeMembers
                .Where(member => member != null && 
                               member.gameObject.activeInHierarchy && 
                               Vector3.Distance(member.transform.position, position) <= spawnRadius)
                .ToList();

            // Require at least 2 NPCs to start a challenge
            return nearbyNPCs.Count >= 2;
        }

        // New method to spawn NPCs from a specific tribe
        public void SpawnTribeNPCs(string tribeName)
        {
            if (!tribes.ContainsKey(tribeName))
            {
                Debug.LogError($"Tribe {tribeName} does not exist!");
                return;
            }

            foreach (TribeMember member in tribes[tribeName])
            {
                if (!member.IsPlayer && !member.gameObject.activeInHierarchy)
                {
                    // Find a valid spawn position
                    Vector3 spawnPosition = FindValidSpawnPosition(tribes[tribeName]);
                    member.transform.position = spawnPosition;
                    member.gameObject.SetActive(true);
                    Debug.Log($"Spawned tribe member {member.memberName} at position {spawnPosition}");
                }
            }
        }
    }
} 