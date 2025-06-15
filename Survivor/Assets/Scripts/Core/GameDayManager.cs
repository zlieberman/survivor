using UnityEngine;
using System;
using UnityEngine.Events;
using System.Collections;

namespace Survivor.Core
{
    public enum GamePhase
    {
        Exploration,
        Challenge,
        SocialTime,
        TribalCouncil,
        Elimination
    }

    /// <summary>
    /// Manages the game's day/night cycle and phase transitions
    /// </summary>
    public class GameDayManager : MonoBehaviour
    {
        public static GameDayManager Instance { get; private set; }

        [Header("Game State")]
        public int currentDay = 1;
        public GamePhase currentPhase = GamePhase.Exploration;
        public float phaseDuration = 300f; // 5 minutes per phase by default
        private float currentPhaseTime;
        private int humanPlayerId = -1; // Track the human player's ID

        [Header("Events")]
        public UnityEvent onDayStart;
        public UnityEvent onDayEnd;
        public UnityEvent<GamePhase> onPhaseChange;
        public UnityEvent<int> onPlayerRegistered;

        // Phase-specific events
        public UnityEvent onExplorationStart;
        public UnityEvent onChallengeStart;
        public UnityEvent onSocialTimeStart;
        public UnityEvent onTribalCouncilStart;
        public UnityEvent onEliminationStart;

        private NPCManager npcManager;
        private ChallengeSystem challengeSystem;
        private VotingSystem votingSystem;
        private bool isGameActive = false;

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
            StartGame();
        }

        private void Update()
        {
            if (NPCManager.Instance.GetActiveNPCs().Count <= 1)
            {
                // Game Over - Only one player remains
                Debug.Log("Game Over! Winner: " + NPCManager.Instance.GetActiveNPCs()[0]);
                enabled = false;
                return;
            }

            currentPhaseTime += Time.deltaTime;
            if (currentPhaseTime >= phaseDuration)
            {
                AdvancePhase();
            }
        }

        private void AdvancePhase()
        {
            // Determine next phase
            GamePhase nextPhase = currentPhase switch
            {
                GamePhase.Exploration => GamePhase.Challenge,
                GamePhase.Challenge => GamePhase.SocialTime,
                GamePhase.SocialTime => GamePhase.TribalCouncil,
                GamePhase.TribalCouncil => GamePhase.Elimination,
                GamePhase.Elimination => GamePhase.Exploration,
                _ => GamePhase.Exploration
            };

            // Set the new phase
            SetPhase(nextPhase);
        }

        /// <summary>
        /// Registers a player in the game system
        /// </summary>
        /// <param name="playerId">The ID to register for the player</param>
        public void RegisterPlayer(int playerId)
        {
            if (humanPlayerId != -1)
            {
                Debug.LogWarning("Attempting to register player when one is already registered!");
                return;
            }

            humanPlayerId = playerId;
            onPlayerRegistered?.Invoke(playerId);
            Debug.Log($"Player registered with ID: {playerId}");
        }

        /// <summary>
        /// Gets the registered human player's ID
        /// </summary>
        public int GetHumanPlayerId()
        {
            return humanPlayerId;
        }

        /// <summary>
        /// Checks if the given ID belongs to the human player
        /// </summary>
        public bool IsHumanPlayer(int playerId)
        {
            return playerId == humanPlayerId;
        }

        public void Initialize(NPCManager npcManager, ChallengeSystem challengeSystem, VotingSystem votingSystem)
        {
            this.npcManager = npcManager;
            this.challengeSystem = challengeSystem;
            this.votingSystem = votingSystem;
        }

        public void StartGame()
        {
            if (!isGameActive)
            {
                isGameActive = true;
                StartCoroutine(GameLoop());
            }
        }

        private IEnumerator GameLoop()
        {
            while (isGameActive)
            {
                // Start new day
                onDayStart?.Invoke();
                Debug.Log($"Day {currentDay} started");

                // Exploration Phase
                SetPhase(GamePhase.Exploration);
                yield return new WaitForSeconds(phaseDuration);

                // Challenge Phase
                SetPhase(GamePhase.Challenge);
                yield return StartCoroutine(RunChallenge());

                // Social Time Phase
                SetPhase(GamePhase.SocialTime);
                yield return new WaitForSeconds(phaseDuration);

                // Tribal Council Phase
                SetPhase(GamePhase.TribalCouncil);
                yield return StartCoroutine(RunTribalCouncil());

                // Elimination Phase
                SetPhase(GamePhase.Elimination);
                yield return StartCoroutine(ProcessElimination());

                // End day
                onDayEnd?.Invoke();
                currentDay++;

                // Check game end condition
                if (npcManager.GetActiveNPCs().Count <= 2)
                {
                    EndGame();
                    break;
                }
            }
        }

        private IEnumerator RunChallenge()
        {
            challengeSystem.StartChallenge();
            while (challengeSystem.IsRunning)
            {
                yield return null;
            }
        }

        private IEnumerator RunTribalCouncil()
        {
            votingSystem.StartVoting();
            while (votingSystem.IsVotingActive)
            {
                yield return null;
            }
        }

        private IEnumerator ProcessElimination()
        {
            string eliminatedNPC = votingSystem.GetEliminatedPlayer();
            if (!string.IsNullOrEmpty(eliminatedNPC))
            {
                npcManager.EliminateNPC(eliminatedNPC);
                Debug.Log($"{eliminatedNPC} has been eliminated");
            }
            yield return new WaitForSeconds(3f); // Time for elimination animation/notification
        }

        private void SetPhase(GamePhase newPhase)
        {
            currentPhase = newPhase;
            currentPhaseTime = 0f;
            onPhaseChange?.Invoke(newPhase);
            Debug.Log($"Entering {newPhase} phase");
            TriggerPhaseEvent(newPhase);
        }

        private void TriggerPhaseEvent(GamePhase phase)
        {
            switch (phase)
            {
                case GamePhase.Exploration:
                    onExplorationStart?.Invoke();
                    break;
                case GamePhase.Challenge:
                    onChallengeStart?.Invoke();
                    break;
                case GamePhase.SocialTime:
                    onSocialTimeStart?.Invoke();
                    break;
                case GamePhase.TribalCouncil:
                    onTribalCouncilStart?.Invoke();
                    break;
                case GamePhase.Elimination:
                    onEliminationStart?.Invoke();
                    break;
            }
        }

        public void EndGame()
        {
            isGameActive = false;
            Debug.Log("Game has ended");
            // Implement winner determination and end game ceremony
        }

        public void PauseGame()
        {
            isGameActive = false;
        }

        public void ResumeGame()
        {
            if (!isGameActive)
            {
                isGameActive = true;
                StartCoroutine(GameLoop());
            }
        }

        public float GetPhaseTimeRemaining()
        {
            return Mathf.Max(0, phaseDuration - currentPhaseTime);
        }

        public float GetPhaseProgress()
        {
            return currentPhaseTime / phaseDuration;
        }
    }
} 