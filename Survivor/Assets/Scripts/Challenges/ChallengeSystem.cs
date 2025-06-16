using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;
using Survivor.Tribes;
using Survivor.Characters;

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
        public UnityEvent<Character> onChallengeWin;
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
            npcManager = FindObjectOfType<TribeManager>();
            if (npcManager == null)
            {
                Debug.LogError("No TribeManager found in the scene! Make sure it exists and implements INPCManager.");
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
                    Character member = npcManager.GetTribeMember(memberName);
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
            // Add some default challenges
            var raceChallenge = ScriptableObject.CreateInstance<RaceChallenge>();
            raceChallenge.data = new ChallengeData
            {
                id = "race_1",
                title = "Survival Race",
                description = "Race to the finish line while avoiding obstacles",
                type = ChallengeType.Race,
                difficulty = ChallengeDifficulty.Medium,
                duration = 120f,
                isTeamChallenge = false
            };
            availableChallenges.Add(raceChallenge);

            var puzzleChallenge = ScriptableObject.CreateInstance<SlidingPuzzleChallenge>();
            puzzleChallenge.data = new ChallengeData
            {
                id = "puzzle_1",
                title = "Puzzle Master",
                description = "Solve puzzles to win immunity",
                type = ChallengeType.Puzzle,
                difficulty = ChallengeDifficulty.Hard,
                duration = 180f,
                isTeamChallenge = false
            };
            availableChallenges.Add(puzzleChallenge);

            var enduranceChallenge = ScriptableObject.CreateInstance<EnduranceChallenge>();
            enduranceChallenge.data = new ChallengeData
            {
                id = "endurance_1",
                title = "Swimming Challenge",
                description = "Swim to the buoy and back",
                type = ChallengeType.Endurance,
                difficulty = ChallengeDifficulty.Medium,
                duration = 90f,
                isTeamChallenge = false
            };
            availableChallenges.Add(enduranceChallenge);
        }

        public void StartRandomChallenge()
        {
            if (isChallengeActive || isOnCooldown)
            {
                Debug.Log("Cannot start a new challenge while one is active or on cooldown");
                return;
            }

            // Select a random challenge
            currentChallenge = availableChallenges[UnityEngine.Random.Range(0, availableChallenges.Count)];
            currentChallenge.StartChallenge();

            // Get all active NPCs
            var activeNPCs = npcManager.GetActiveNPCs();
            if (activeNPCs.Count < 2)
            {
                Debug.Log("Not enough active NPCs to start a challenge");
                return;
            }

            // Add all tribe members as participants
            foreach (string npcName in activeNPCs)
            {
                Character member = npcManager.GetTribeMember(npcName);
                if (member != null)
                {
                    currentChallenge.AddParticipant(member.CharacterName);
                }
            }

            isChallengeActive = true;
            onChallengeStarted?.Invoke(currentChallenge);

            // Start the challenge timer
            StartCoroutine(ChallengeTimer());
        }

        private IEnumerator ChallengeTimer()
        {
            float challengeTime = UnityEngine.Random.Range(minChallengeDuration, maxChallengeDuration);
            yield return new WaitForSeconds(challengeTime);

            if (isChallengeActive)
            {
                CompleteChallenge();
            }
        }

        private void CompleteChallenge()
        {
            if (!isChallengeActive || currentChallenge == null) return;

            // Determine winner
            string winnerName = currentChallenge.ParticipantScores
                .OrderByDescending(x => x.Value)
                .FirstOrDefault().Key;

            if (!string.IsNullOrEmpty(winnerName))
            {
                Character winner = npcManager.GetTribeMember(winnerName);
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

        private float CalculateScore(Character member, ChallengeType type)
        {
            if (member == null || member.Stats == null) return 0f;

            switch (type)
            {
                case ChallengeType.Race:
                    return member.Stats.speed;
                case ChallengeType.Puzzle:
                    return member.Stats.puzzleSolving;
                case ChallengeType.Endurance:
                    return member.Stats.swimming;
                default:
                    return 0f;
            }
        }

        // IChallengeManager implementation
        public void StartChallenge(Challenge challenge)
        {
            if (isChallengeActive || isOnCooldown)
            {
                Debug.Log("Cannot start a new challenge while one is active or on cooldown");
                return;
            }

            currentChallenge = challenge;
            currentChallenge.StartChallenge();

            // Get all active NPCs
            var activeNPCs = npcManager.GetActiveNPCs();
            if (activeNPCs.Count < 2)
            {
                Debug.Log("Not enough active NPCs to start a challenge");
                return;
            }

            // Add all tribe members as participants
            foreach (string npcName in activeNPCs)
            {
                Character member = npcManager.GetTribeMember(npcName);
                if (member != null)
                {
                    currentChallenge.AddParticipant(member.CharacterName);
                }
            }

            isChallengeActive = true;
            onChallengeStarted?.Invoke(currentChallenge);

            // Start the challenge timer
            StartCoroutine(ChallengeTimer());
        }

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