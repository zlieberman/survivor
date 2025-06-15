using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;


namespace Survivor.Core
{
    public enum ChallengeType
    {
        Physical,      // Strength-focused
        Agility,      // Speed and dexterity
        Puzzle,       // Mental challenges
        Hybrid,       // Mix of different skills
        Mental,       // Mental challenges
        Social,       // Social challenges
        Endurance    // Endurance challenges
    }

    [Serializable]
    public class Challenge
    {
        public string name;
        public string description;
        public ChallengeType type;
        public float strengthWeight;
        public float agilityWeight;
        public float puzzleWeight;
        public NPCManager.NPCData winner; // Track the challenge winner
        public float duration;
        public bool isTeamChallenge;
        public Dictionary<string, float> participantScores = new Dictionary<string, float>();
    }

    /// <summary>
    /// Manages challenge creation, execution, and winner determination
    /// </summary>
    public class ChallengeSystem : MonoBehaviour
    {
        public static ChallengeSystem Instance { get; private set; }

        [Header("Challenge Configuration")]
        public List<Challenge> availableChallenges = new List<Challenge>();
        public float minChallengeDuration = 60f;
        public float maxChallengeDuration = 180f;
        private Challenge currentChallenge;
        public bool IsRunning { get; private set; }

        [Header("Events")]
        public UnityEvent<Challenge> onChallengeStart;
        public UnityEvent<NPC> onChallengeWin;
        public UnityEvent onChallengeEnd;
        public UnityEvent<string> onWinnerDetermined;

        private NPCManager npcManager;

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

            // Initialize default challenges if none are set
            if (availableChallenges.Count == 0)
            {
                InitializeDefaultChallenges();
            }
        }

        private void Start()
        {
            // Subscribe to game phase changes
            if (GameDayManager.Instance != null)
            {
                GameDayManager.Instance.onChallengeStart.AddListener(StartRandomChallenge);
            }
        }

        private void Update()
        {
            if (IsRunning)
            {
                // Update participant scores based on their stats
                foreach (string npcName in currentChallenge.participantScores.Keys)
                {
                    NPCManager.NPCData npc = npcManager.GetNPCData(npcName);
                    if (npc != null)
                    {
                        float score = CalculateScore(npc, currentChallenge.type);
                        currentChallenge.participantScores[npcName] += score * Time.deltaTime;
                    }
                }
            }
        }

        private void InitializeDefaultChallenges()
        {
            availableChallenges.Add(new Challenge
            {
                name = "Obstacle Course",
                description = "Navigate through a complex obstacle course",
                type = ChallengeType.Agility,
                strengthWeight = 0.3f,
                agilityWeight = 0.6f,
                puzzleWeight = 0.1f
            });

            availableChallenges.Add(new Challenge
            {
                name = "Puzzle Box",
                description = "Solve a series of interconnected puzzles",
                type = ChallengeType.Puzzle,
                strengthWeight = 0.1f,
                agilityWeight = 0.2f,
                puzzleWeight = 0.7f
            });

            availableChallenges.Add(new Challenge
            {
                name = "Weight Challenge",
                description = "Hold heavy objects for as long as possible",
                type = ChallengeType.Physical,
                strengthWeight = 0.8f,
                agilityWeight = 0.1f,
                puzzleWeight = 0.1f
            });

            availableChallenges.Add(new Challenge
            {
                name = "Memory Maze",
                description = "Navigate a maze while solving memory puzzles",
                type = ChallengeType.Hybrid,
                strengthWeight = 0.2f,
                agilityWeight = 0.4f,
                puzzleWeight = 0.4f
            });
        }

        public void StartChallenge(Challenge challenge = null)
        {
            if (IsRunning) return;

            if (challenge == null)
            {
                // Start a random challenge
                currentChallenge = availableChallenges[UnityEngine.Random.Range(0, availableChallenges.Count)];
            }
            else
            {
                currentChallenge = challenge;
            }

            currentChallenge.winner = null; // Reset winner at start
            currentChallenge.duration = UnityEngine.Random.Range(minChallengeDuration, maxChallengeDuration);
            currentChallenge.isTeamChallenge = UnityEngine.Random.value > 0.5f;
            IsRunning = true;

            Debug.Log($"Starting Challenge: {currentChallenge.name}\n{currentChallenge.description}");
            onChallengeStart?.Invoke(currentChallenge);

            // Initialize participant scores
            foreach (string npcName in npcManager.GetActiveNPCs())
            {
                currentChallenge.participantScores[npcName] = 0f;
            }

            // Start challenge coroutine
            StartCoroutine(RunChallenge());
        }

        public void StartRandomChallenge()
        {
            StartChallenge();
        }

        private System.Collections.IEnumerator RunChallenge()
        {
            float elapsedTime = 0f;

            while (elapsedTime < currentChallenge.duration)
            {
                elapsedTime += Time.deltaTime;

                yield return null;
            }

            EndChallenge();
        }

        private float CalculateScore(NPCManager.NPCData npc, ChallengeType type)
        {
            // Base score calculation using NPC stats
            float score = 0f;

            switch (type)
            {
                case ChallengeType.Physical:
                    score = (npc.aggression * 0.7f + npc.loyalty * 0.3f) / 100f;
                    break;
                case ChallengeType.Mental:
                    score = (npc.sneakiness * 0.6f + npc.charisma * 0.4f) / 100f;
                    break;
                case ChallengeType.Social:
                    score = (npc.charisma * 0.8f + npc.loyalty * 0.2f) / 100f;
                    break;
                case ChallengeType.Endurance:
                    score = (npc.loyalty * 0.6f + npc.aggression * 0.4f) / 100f;
                    break;
                case ChallengeType.Puzzle:
                    score = (npc.sneakiness * 0.7f + npc.charisma * 0.3f) / 100f;
                    break;
            }

            // Add some randomness
            score *= UnityEngine.Random.Range(0.8f, 1.2f);
            return score;
        }

        private void EndChallenge()
        {
            if (!IsRunning) return;

            // Determine winner
            string winner = DetermineWinner();
            if (!string.IsNullOrEmpty(winner))
            {
                currentChallenge.winner = npcManager.GetNPCData(winner);
                Debug.Log($"Challenge winner: {winner}");

                // Update relationships based on challenge outcome
                UpdateRelationships(winner);
            }

            IsRunning = false;
            onChallengeEnd?.Invoke();
        }

        private string DetermineWinner()
        {
            string winner = null;
            float highestScore = float.MinValue;

            foreach (var kvp in currentChallenge.participantScores)
            {
                if (kvp.Value > highestScore)
                {
                    highestScore = kvp.Value;
                    winner = kvp.Key;
                }
            }

            return winner;
        }

        private void UpdateRelationships(string winner)
        {
            // Increase relationships with winner
            foreach (string npcName in npcManager.GetActiveNPCs())
            {
                if (npcName != winner)
                {
                    float relationshipDelta = UnityEngine.Random.Range(5f, 15f);
                    npcManager.UpdateRelationship(npcName, winner, relationshipDelta);
                }
            }
        }

        public bool IsInChallenge()
        {
            return IsRunning;
        }

        public Challenge GetCurrentChallenge()
        {
            return currentChallenge;
        }

        public float GetChallengeProgress()
        {
            return IsRunning ? UnityEngine.Random.value : 0f;
        }

        /// <summary>
        /// Gets the most recent challenge winner, if any
        /// </summary>
        public NPCManager.NPCData GetLastWinner()
        {
            return currentChallenge?.winner;
        }

        public void Initialize(NPCManager npcManager)
        {
            this.npcManager = npcManager;
        }
    }
} 