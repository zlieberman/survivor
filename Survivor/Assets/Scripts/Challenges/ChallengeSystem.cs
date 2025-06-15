using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;
using Survivor.Tribes;

namespace Survivor.Challenges
{
    public class ChallengeSystem : MonoBehaviour, IChallengeManager
    {
        public static ChallengeSystem Instance { get; private set; }

        [Header("Challenge Configuration")]
        public List<Challenge> availableChallenges = new List<Challenge>();
        public float minChallengeDuration = 60f;
        public float maxChallengeDuration = 180f;
        public float challengeCooldown = 30f;

        [Header("Events")]
        public UnityEvent<Challenge> onChallengeStarted;
        public UnityEvent<Challenge> onChallengeCompleted;
        public UnityEvent<Challenge> onChallengeFailed;
        public UnityEvent<TribeMember> onChallengeWin;
        public UnityEvent<string> onWinnerDetermined;

        private Challenge currentChallenge;
        private bool isChallengeActive;
        private float cooldownTimer;
        private bool isOnCooldown;
        private INPCManager npcManager;

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
            npcManager = FindObjectOfType<MonoBehaviour>() as INPCManager;
            if (npcManager == null)
            {
                Debug.LogError("No INPCManager implementation found in the scene!");
            }
        }

        private void Update()
        {
            if (isChallengeActive && currentChallenge != null)
            {
                currentChallenge.UpdateProgress(Time.deltaTime);

                // Update participant scores based on their stats
                foreach (string memberName in currentChallenge.ParticipantScores.Keys)
                {
                    TribeMember member = npcManager.GetTribeMember(memberName);
                    if (member != null)
                    {
                        float score = CalculateScore(member, currentChallenge.data.type);
                        currentChallenge.UpdateScore(memberName, score * Time.deltaTime);
                    }
                }

                if (currentChallenge.IsCompleted())
                {
                    CompleteChallenge();
                }
            }
            else if (isOnCooldown)
            {
                cooldownTimer -= Time.deltaTime;
                if (cooldownTimer <= 0f)
                {
                    isOnCooldown = false;
                }
            }
        }

        private void InitializeDefaultChallenges()
        {
            // Add default challenges here
            // This is just an example, you should create proper challenge assets
            var memoryMaze = ScriptableObject.CreateInstance<Challenge>();
            memoryMaze.data = new ChallengeData
            {
                id = "memory_maze",
                title = "Memory Maze",
                description = "Navigate a maze while solving memory puzzles",
                type = ChallengeType.Hybrid,
                strengthWeight = 0.2f,
                agilityWeight = 0.4f,
                puzzleWeight = 0.4f,
                difficulty = ChallengeDifficulty.Hard
            };
            availableChallenges.Add(memoryMaze);
        }

        public void StartChallenge(Challenge challenge = null)
        {
            if (isChallengeActive || isOnCooldown) return;

            if (challenge == null)
            {
                // Start a random challenge
                currentChallenge = availableChallenges[UnityEngine.Random.Range(0, availableChallenges.Count)];
            }
            else
            {
                currentChallenge = challenge;
            }

            // Initialize challenge
            currentChallenge.Initialize();
            currentChallenge.data.duration = UnityEngine.Random.Range(minChallengeDuration, maxChallengeDuration);
            currentChallenge.data.isTeamChallenge = UnityEngine.Random.value > 0.5f;

            // Add all tribe members as participants
            foreach (TribeMember member in npcManager.GetAllTribeMembers())
            {
                currentChallenge.AddParticipant(member.memberName);
            }

            isChallengeActive = true;
            currentChallenge.StartChallenge();

            Debug.Log($"Starting Challenge: {currentChallenge.data.title}\n{currentChallenge.data.description}");
            onChallengeStarted?.Invoke(currentChallenge);
        }

        public void CompleteChallenge()
        {
            if (!isChallengeActive || currentChallenge == null) return;

            // Determine winner
            string winnerName = currentChallenge.ParticipantScores
                .OrderByDescending(x => x.Value)
                .FirstOrDefault().Key;

            if (!string.IsNullOrEmpty(winnerName))
            {
                TribeMember winner = npcManager.GetTribeMember(winnerName);
                if (winner != null)
                {
                    onChallengeWin?.Invoke(winner);
                }
                onWinnerDetermined?.Invoke(winnerName);
            }

            currentChallenge.EndChallenge();
            isChallengeActive = false;
            isOnCooldown = true;
            cooldownTimer = challengeCooldown;

            onChallengeCompleted?.Invoke(currentChallenge);
            currentChallenge = null;
        }

        public void FailChallenge()
        {
            if (!isChallengeActive || currentChallenge == null) return;

            currentChallenge.EndChallenge();
            isChallengeActive = false;
            isOnCooldown = true;
            cooldownTimer = challengeCooldown;

            onChallengeFailed?.Invoke(currentChallenge);
            currentChallenge = null;
        }

        private float CalculateScore(TribeMember member, ChallengeType type)
        {
            float score = 0f;
            switch (type)
            {
                case ChallengeType.Physical:
                    score = member.stats.strength;
                    break;
                case ChallengeType.Agility:
                    score = member.stats.agility;
                    break;
                case ChallengeType.Puzzle:
                case ChallengeType.Mental:
                    score = member.stats.intelligence;
                    break;
                case ChallengeType.Social:
                    score = member.stats.charisma;
                    break;
                case ChallengeType.Endurance:
                    score = member.stats.stamina;
                    break;
                case ChallengeType.Hybrid:
                    score = (member.stats.strength + member.stats.agility + member.stats.intelligence) / 3f;
                    break;
            }
            return score;
        }

        // IChallengeManager implementation
        public Challenge GetCurrentChallenge()
        {
            return currentChallenge;
        }

        public bool IsInChallenge()
        {
            return isChallengeActive;
        }

        public float GetChallengeProgress()
        {
            return currentChallenge?.GetProgress() ?? 0f;
        }

        public float GetRemainingTime()
        {
            return currentChallenge?.RemainingTime ?? 0f;
        }

        public float GetCooldownTime()
        {
            return isOnCooldown ? cooldownTimer : 0f;
        }
    }
} 