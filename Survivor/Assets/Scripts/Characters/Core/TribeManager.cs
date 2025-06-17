using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using StarterAssets;
using System.Linq;
using Survivor.Generation;

namespace Survivor.Characters
{
    [DefaultExecutionOrder(-50)]
    public class TribeManager : MonoBehaviour, ITribeManager, INPCManager
    {
        public static TribeManager Instance { get; private set; }

        [Header("Spawn Settings")]
        public float spawnRadius = 10f;
        public float minSpawnDistance = 2.5f;
        public int maxSpawnAttempts = 30;

        [Header("Tribe Settings")]
        public string tribeAName = "Tribe A";
        public string tribeBName = "Tribe B";

        [Header("Prefabs")]
        public GameObject npcPrefab;

        private Dictionary<string, List<Character>> tribes = new Dictionary<string, List<Character>>();
        private List<Character> tribeMembers = new List<Character>();
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

            Debug.Log($"[TribeManager] Awake - tribeAName: {tribeAName}, tribeBName: {tribeBName}");
            campGenerator = FindObjectOfType<CampGenerator>();
            if (campGenerator == null)
            {
                Debug.LogError("CampGenerator not found in scene!");
            }
        }

        public void Initialize()
        {
            if (isInitialized) return;

            Debug.Log($"[TribeManager] Initializing with tribe names - A: {tribeAName}, B: {tribeBName}");
            tribes.Clear();
            tribes[tribeAName] = new List<Character>();
            tribes[tribeBName] = new List<Character>();

            isInitialized = true;
            Debug.Log("[TribeManager] Initialization complete");
        }

        public IEnumerator CreateTribes()
        {
            if (!isInitialized)
            {
                Debug.LogError("TribeManager not initialized!");
                yield break;
            }

            while (campGenerator == null || !campGenerator.CampPlaced)
            {
                Debug.Log("Waiting for camp to be placed...");
                yield return new WaitForSeconds(0.5f);
            }

            yield return StartCoroutine(CreateTribe(tribeAName, true));
            yield return StartCoroutine(CreateTribe(tribeBName, false, false));

            Debug.Log("Tribes created successfully");
        }

        private IEnumerator CreateTribe(string tribeName, bool includePlayer, bool spawnNPCs = true)
        {
            Debug.Log($"Starting to create tribe: {tribeName}");
            List<Character> tribeMembers = new List<Character>();
            
            int npcCount = includePlayer ? 8 : 9;
            Debug.Log($"Creating {npcCount} members for tribe {tribeName}");
            
            for (int i = 0; i < npcCount; i++)
            {
                Vector3 spawnPosition = FindValidSpawnPosition(tribeMembers);
                
                GameObject npcObject = Instantiate(npcPrefab, spawnPosition, Quaternion.identity);
                Character character = npcObject.GetComponent<Character>();
                
                if (character != null)
                {
                    // Assign gender first
                    Gender gender = NPCGenerator.Instance.AssignGender(tribeName);
                    character.Gender = gender;
                    string memberName = NPCGenerator.Instance.GenerateRandomName(gender);
                    character.Initialize(memberName, tribeName, includePlayer && i == 0, i);
                    
                    tribes[tribeName].Add(character);
                    tribeMembers.Add(character);

                    if (!spawnNPCs || (tribeName != tribeAName && !character.IsPlayer))
                    {
                        npcObject.SetActive(false);
                    }
                    
                    Debug.Log($"Created tribe member: {memberName} for tribe {tribeName} at position {spawnPosition}");
                }
                else
                {
                    Debug.LogError($"Failed to get Character component for NPC in tribe {tribeName}");
                }
                
                yield return null;
            }
            
            Debug.Log($"Finished creating tribe {tribeName} with {tribeMembers.Count} members");
            yield return null;
        }

        private Vector3 FindValidSpawnPosition(List<Character> existingMembers)
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
                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                float distance = Random.Range(minSpawnDistance, spawnRadius);
                position = campCenter + new Vector3(
                    Mathf.Cos(angle) * distance,
                    0,
                    Mathf.Sin(angle) * distance
                );

                Terrain terrain = Terrain.activeTerrain;
                if (terrain != null)
                {
                    position.y = terrain.SampleHeight(position);
                }

                validPosition = true;
                foreach (Character member in existingMembers)
                {
                    if (Vector3.Distance(position, member.transform.position) < minSpawnDistance)
                    {
                        validPosition = false;
                        break;
                    }
                }

                attempts++;
            }

            if (!validPosition)
            {
                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                position = campCenter + new Vector3(
                    Mathf.Cos(angle) * spawnRadius,
                    0,
                    Mathf.Sin(angle) * spawnRadius
                );

                Terrain terrain = Terrain.activeTerrain;
                if (terrain != null)
                {
                    position.y = terrain.SampleHeight(position);
                }
            }

            return position;
        }

        public List<Character> GetTribeMembers(string tribeName)
        {
            Debug.Log($"[TribeManager] Getting members for tribe: {tribeName}");
            if (tribes.ContainsKey(tribeName))
            {
                Debug.Log($"[TribeManager] Found {tribes[tribeName].Count} members in tribe {tribeName}");
                return tribes[tribeName];
            }
            Debug.LogWarning($"[TribeManager] No tribe found with name: {tribeName}");
            return new List<Character>();
        }

        public Character GetPlayer()
        {
            return tribeMembers.FirstOrDefault(member => member.IsPlayer);
        }

        public void EliminateMember(Character member)
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

        public Character GetNPCData(string npcName)
        {
            return tribeMembers.FirstOrDefault(member => member.CharacterName == npcName);
        }

        public void StartChallenge(List<Character> participants)
        {
            foreach (var participant in participants)
            {
                if (participant != null)
                {
                    participant.IsInChallenge = true;
                }
            }
        }

        public void EliminateParticipant(Character member)
        {
            if (member != null)
            {
                member.IsInChallenge = false;
                EliminateMember(member);
            }
        }

        public bool IsParticipantInChallenge(Character member)
        {
            return member != null && member.IsInChallenge;
        }

        public List<Character> GetActiveParticipants()
        {
            return tribeMembers.Where(member => member != null && member.IsInChallenge).ToList();
        }

        public Character GetTribeMember(string memberName)
        {
            return tribeMembers.FirstOrDefault(member => member.CharacterName == memberName);
        }

        public List<Character> GetAllTribeMembers()
        {
            return new List<Character>(tribeMembers);
        }

        public void AddTribeMember(Character member)
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

        List<string> INPCManager.GetActiveNPCs()
        {
            return tribeMembers
                .Where(member => member != null && member.gameObject.activeInHierarchy)
                .Select(member => member.CharacterName)
                .ToList();
        }

        IEnumerable<string> ITribeManager.GetActiveNPCs()
        {
            return tribeMembers
                .Where(member => member != null && member.gameObject.activeInHierarchy)
                .Select(member => member.CharacterName);
        }

        public void UpdateRelationship(string npc1Name, string npc2Name, float delta)
        {
            var npc1 = GetTribeMember(npc1Name);
            var npc2 = GetTribeMember(npc2Name);

            if (npc1 != null && npc2 != null)
            {
                npc1.UpdateRelationship(npc2Name, delta);
                npc2.UpdateRelationship(npc1Name, delta);
            }
        }

        public bool CanStartChallenge(Vector3 position)
        {
            var nearbyNPCs = tribeMembers
                .Where(member => member != null && 
                               member.gameObject.activeInHierarchy && 
                               Vector3.Distance(member.transform.position, position) <= spawnRadius)
                .ToList();

            return nearbyNPCs.Count >= 2;
        }

        public void SpawnTribeNPCs(string tribeName)
        {
            if (!tribes.ContainsKey(tribeName))
            {
                Debug.LogError($"Tribe {tribeName} does not exist!");
                return;
            }

            foreach (Character member in tribes[tribeName])
            {
                if (!member.IsPlayer && !member.gameObject.activeInHierarchy)
                {
                    Vector3 spawnPosition = FindValidSpawnPosition(tribes[tribeName]);
                    member.transform.position = spawnPosition;
                    member.gameObject.SetActive(true);
                    Debug.Log($"Spawned tribe member {member.CharacterName} at position {spawnPosition}");
                }
            }
        }
    }
} 