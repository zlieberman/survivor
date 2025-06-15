using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using StarterAssets;
using System.Linq;

namespace Survivor.Tribes
{
    public class TribeManager : MonoBehaviour, ITribeManager, INPCManager
    {
        public static TribeManager Instance { get; private set; }

        [Header("Spawn Settings")]
        public float spawnRadius = 5f;
        public float minSpawnDistance = 2f;

        [Header("Tribe Settings")]
        public string tribeAName = "TribeA";
        public string tribeBName = "TribeB";

        private Dictionary<string, List<TribeMember>> tribes = new Dictionary<string, List<TribeMember>>();
        private List<TribeMember> tribeMembers = new List<TribeMember>();
        private bool isInitialized = false;

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

        public void Initialize()
        {
            if (isInitialized) return;

            tribes.Clear();
            tribes[tribeAName] = new List<TribeMember>();
            tribes[tribeBName] = new List<TribeMember>();

            isInitialized = true;
            Debug.Log("TribeManager initialized");
        }

        private IEnumerator CreateTribe(string tribeName, bool includePlayer)
        {
            List<TribeMember> tribeMembers = new List<TribeMember>();
            // Implementation here
            yield return null;
        }

        private Vector3 CalculateSpawnPosition(Vector3 center, List<TribeMember> existingMembers)
        {
            Vector3 position;
            int maxAttempts = 10;
            int attempts = 0;

            do
            {
                position = center + Random.insideUnitSphere * spawnRadius;
                position.y = center.y;
                attempts++;
            } while (attempts < maxAttempts && existingMembers.Any(m => Vector3.Distance(m.transform.position, position) < minSpawnDistance));

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
    }
} 