using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;

namespace Survivor.Core
{
    /// <summary>
    /// Manages the voting process during tribal council
    /// </summary>
    public class VotingSystem : MonoBehaviour
    {
        public static VotingSystem Instance { get; private set; }

        [Header("Voting Configuration")]
        public float votingDuration = 60f;
        public float minVotingThreshold = 0.5f; // Minimum percentage of votes needed

        [Header("Events")]
        public UnityEvent onVotingStart;
        public UnityEvent onVotingEnd;
        public UnityEvent<string> onPlayerEliminated;

        private NPCManager npcManager;
        private Dictionary<string, Dictionary<string, int>> votes; // voter -> (votee -> count)
        public bool IsVotingActive { get; private set; }
        private string eliminatedPlayer;
        private float votingStartTime;

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
            // Subscribe to game phase changes
            if (GameDayManager.Instance != null)
            {
                GameDayManager.Instance.onTribalCouncilStart.AddListener(StartVoting);
            }
        }

        private void Update()
        {
            if (IsVotingActive)
            {
                // Voting process is handled by StartVoting coroutine
            }
        }

        public void Initialize(NPCManager npcManager)
        {
            this.npcManager = npcManager;
            votes = new Dictionary<string, Dictionary<string, int>>();
        }

        public void StartVoting()
        {
            if (IsVotingActive) return;

            votes.Clear();
            eliminatedPlayer = null;
            IsVotingActive = true;
            votingStartTime = Time.time;
            onVotingStart?.Invoke();

            // Initialize voting tracking
            foreach (string npcName in npcManager.GetActiveNPCs())
            {
                votes[npcName] = new Dictionary<string, int>();
            }

            // Start voting process
            StartCoroutine(VotingProcess());
        }

        private System.Collections.IEnumerator VotingProcess()
        {
            float elapsedTime = 0f;

            while (elapsedTime < votingDuration)
            {
                // NPCs cast their votes based on relationships and alliances
                foreach (string voterName in npcManager.GetActiveNPCs())
                {
                    if (!HasVoted(voterName))
                    {
                        CastNPCVote(voterName);
                    }
                }

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            EndVoting();
        }

        private void CastNPCVote(string voterName)
        {
            NPCManager.NPCData voter = npcManager.GetNPCData(voterName);
            if (voter == null) return;

            // Get all possible voting targets
            List<string> possibleTargets = npcManager.GetActiveNPCs()
                .Where(name => name != voterName)
                .ToList();

            if (possibleTargets.Count == 0) return;

            // Calculate voting weights based on relationships and alliances
            Dictionary<string, float> votingWeights = new Dictionary<string, float>();
            foreach (string targetName in possibleTargets)
            {
                float weight = CalculateVotingWeight(voter, targetName);
                votingWeights[targetName] = weight;
            }

            // Select target based on weights
            string selectedTarget = SelectTargetFromWeights(votingWeights);
            RegisterVote(voterName, selectedTarget);
        }

        private float CalculateVotingWeight(NPCManager.NPCData voter, string targetName)
        {
            float weight = 100f;

            // Reduce weight based on relationship
            float relationship = npcManager.GetRelationship(voter.name, targetName);
            weight *= (100f - relationship) / 100f;

            // Reduce weight if in alliance
            if (npcManager.AreAllied(voter.name, targetName))
            {
                weight *= (100f - voter.loyalty) / 100f;
            }

            // Add randomness based on sneakiness
            float randomFactor = Random.Range(0f, voter.sneakiness / 100f);
            weight *= (1f + randomFactor);

            return weight;
        }

        private string SelectTargetFromWeights(Dictionary<string, float> weights)
        {
            float totalWeight = weights.Values.Sum();
            float randomPoint = Random.Range(0f, totalWeight);
            float currentSum = 0f;

            foreach (var kvp in weights)
            {
                currentSum += kvp.Value;
                if (randomPoint <= currentSum)
                {
                    return kvp.Key;
                }
            }

            return weights.First().Key; // Fallback
        }

        public void RegisterVote(string voterName, string targetName)
        {
            if (!IsVotingActive || !votes.ContainsKey(voterName)) return;

            var voterVotes = votes[voterName];
            if (!voterVotes.ContainsKey(targetName))
            {
                voterVotes[targetName] = 0;
            }
            voterVotes[targetName]++;

            Debug.Log($"{voterName} voted for {targetName}");
        }

        private void EndVoting()
        {
            if (!IsVotingActive) return;

            // Count total votes for each player
            Dictionary<string, int> voteCounts = new Dictionary<string, int>();
            foreach (var voterVotes in votes.Values)
            {
                foreach (var kvp in voterVotes)
                {
                    if (!voteCounts.ContainsKey(kvp.Key))
                    {
                        voteCounts[kvp.Key] = 0;
                    }
                    voteCounts[kvp.Key] += kvp.Value;
                }
            }

            // Determine eliminated player
            if (voteCounts.Count > 0)
            {
                eliminatedPlayer = voteCounts.OrderByDescending(kvp => kvp.Value).First().Key;
                onPlayerEliminated?.Invoke(eliminatedPlayer);
            }

            IsVotingActive = false;
            onVotingEnd?.Invoke();
        }

        public bool HasVoted(string voterName)
        {
            return votes.ContainsKey(voterName) && votes[voterName].Count > 0;
        }

        public string GetEliminatedPlayer()
        {
            return eliminatedPlayer;
        }

        public Dictionary<string, int> GetVoteResults()
        {
            Dictionary<string, int> voteCounts = new Dictionary<string, int>();
            foreach (var voterVotes in votes.Values)
            {
                foreach (var kvp in voterVotes)
                {
                    if (!voteCounts.ContainsKey(kvp.Key))
                    {
                        voteCounts[kvp.Key] = 0;
                    }
                    voteCounts[kvp.Key] += kvp.Value;
                }
            }
            return voteCounts;
        }

        /// <summary>
        /// Gets the current voting progress as a value between 0 and 1
        /// </summary>
        public float GetCurrentVotingProgress()
        {
            if (!IsVotingActive) return 0f;
            float elapsedTime = Time.time - votingStartTime;
            return Mathf.Clamp01(elapsedTime / votingDuration);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
} 