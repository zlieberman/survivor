using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Survivor.Challenges;
using Survivor.Characters;
using Survivor.UI;

namespace Survivor.Core
{
    public class UIManager : MonoBehaviour, IUIManager
    {
        public static UIManager Instance { get; private set; }

        [Header("Phase Display")]
        public TextMeshProUGUI phaseText;
        public TextMeshProUGUI timerText;
        public Image phaseProgressBar;

        [Header("Challenge UI")]
        public GameObject challengePanel;
        public TextMeshProUGUI challengeNameText;
        public TextMeshProUGUI challengeDescriptionText;
        public Image challengeProgressBar;

        [Header("Voting UI")]
        public GameObject votingPanel;
        public TextMeshProUGUI votingStatusText;
        public Transform voteResultsContainer;
        public GameObject voteResultPrefab;

        [Header("NPC Info")]
        public GameObject npcInfoPanel;
        public TextMeshProUGUI selectedNpcNameText;
        public Transform statsContainer;
        public Transform relationshipsContainer;
        public GameObject statPrefab;
        public GameObject relationshipPrefab;

        [Header("Player Status")]
        public PlayerStatusBar playerStatusBar;

        private GameObject mainMenuPanel;
        private GameObject gameHudPanel;
        private GameObject dialoguePanel;
        private GameObject pauseMenuPanel;

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
            // Subscribe to game events
            if (GameDayManager.Instance != null)
            {
                GameDayManager.Instance.onPhaseChange.AddListener((phase) => UpdatePhaseDisplay(phase));
                GameDayManager.Instance.onDayStart.AddListener(() => UpdatePhaseDisplay(GameDayManager.Instance.currentPhase));
            }

            if (ChallengeSystem.Instance != null)
            {
                ChallengeSystem.Instance.onChallengeStarted.AddListener((challenge) => ShowChallengeUI(challenge));
                ChallengeSystem.Instance.onChallengeCompleted.AddListener((challenge) => HideChallengeUI());
            }

            if (VotingSystem.Instance != null)
            {
                VotingSystem.Instance.onVotingStart.AddListener(ShowVotingUI);
                VotingSystem.Instance.onVotingEnd.AddListener(() => {
                    UpdateVoteCounts(VotingSystem.Instance.GetVoteResults());
                    Invoke(nameof(HideVotingUI), 5f); // Hide after 5 seconds
                });
                VotingSystem.Instance.onPlayerEliminated.AddListener((eliminatedPlayer) => {
                    Debug.Log($"{eliminatedPlayer} has been eliminated!");
                });
            }
        }

        private void Update()
        {
            UpdateTimers();
        }

        private void UpdateTimers()
        {
            if (GameDayManager.Instance != null)
            {
                float timeRemaining = GameDayManager.Instance.GetPhaseTimeRemaining();
                timerText.text = $"Time: {timeRemaining:F1}s";
                if (phaseProgressBar != null)
                {
                    phaseProgressBar.fillAmount = GameDayManager.Instance.GetPhaseProgress();
                }
            }

            if (ChallengeSystem.Instance != null && ChallengeSystem.Instance.IsInChallenge())
            {
                if (challengeProgressBar != null)
                {
                    challengeProgressBar.fillAmount = ChallengeSystem.Instance.GetChallengeProgress();
                }
            }

            if (VotingSystem.Instance != null && VotingSystem.Instance.IsVotingActive)
            {
                if (votingPanel != null)
                {
                    votingStatusText.text = $"Voting Progress: {VotingSystem.Instance.GetCurrentVotingProgress() * 100:F0}%";
                }
            }
        }

        private void UpdatePhaseDisplay(GamePhase phase)
        {
            if (GameDayManager.Instance != null)
            {
                phaseText.text = $"Day {GameDayManager.Instance.currentDay} - {phase}";
            }
        }

        public void ShowChallengeUI(Challenge challenge)
        {
            challengePanel.SetActive(true);
            challengeNameText.text = challenge.data.title;
            challengeDescriptionText.text = challenge.data.description;
        }

        public void HideChallengeUI()
        {
            challengePanel.SetActive(false);
        }

        public void UpdateChallengeProgress(float progress)
        {
            if (challengeProgressBar != null)
            {
                challengeProgressBar.fillAmount = progress;
            }
        }

        public void ShowVotingUI()
        {
            votingPanel.SetActive(true);
            ClearVoteResults();
        }

        private void OnVotingComplete(string eliminatedPlayer)
        {
            UpdateVoteCounts(VotingSystem.Instance.GetVoteResults());
            Invoke(nameof(HideVotingUI), 5f); // Hide after 5 seconds
        }

        public void HideVotingUI()
        {
            votingPanel.SetActive(false);
        }

        public void UpdateVoteCounts(Dictionary<string, int> voteCounts)
        {
            ClearVoteResults();
            foreach (var vote in voteCounts)
            {
                var npcData = NPCManager.Instance.GetNPCData(vote.Key);
                if (npcData != null)
                {
                    var resultObj = Instantiate(voteResultPrefab, voteResultsContainer);
                    var resultText = resultObj.GetComponent<TextMeshProUGUI>();
                    if (resultText != null)
                    {
                        resultText.text = $"{npcData.name}: {vote.Value} votes";
                    }
                }
            }
        }

        private void ClearVoteResults()
        {
            foreach (Transform child in voteResultsContainer)
            {
                Destroy(child.gameObject);
            }
        }

        public void ShowNPCInfo(Character character)
        {
            if (character == null) return;

            npcInfoPanel.SetActive(true);
            selectedNpcNameText.text = character.CharacterName;

            // Clear existing stats
            foreach (Transform child in statsContainer)
            {
                Destroy(child.gameObject);
            }

            // Display stats
            if (character.Stats != null)
            {
                DisplayStat("Perception", character.Stats.perception);
                DisplayStat("Deception", character.Stats.deception);
                DisplayStat("Persuasion", character.Stats.persuasion);
                DisplayStat("Puzzle Solving", character.Stats.puzzleSolving);
                DisplayStat("Swimming", character.Stats.swimming);
                DisplayStat("Speed", character.Stats.speed);
                DisplayStat("Strength", character.Stats.strength);
                DisplayStat("Charisma", character.Stats.charisma);
                DisplayStat("Honesty", character.Stats.honesty);
                DisplayStat("Trust", character.Stats.trust);
                DisplayStat("Honor", character.Stats.honor);

                // Display game-impacted stats
                DisplayStat("Energy", character.Stats.energy);
                DisplayStat("Hunger", character.Stats.hunger);
                DisplayStat("Thirst", character.Stats.thirst);
            }

            // Clear existing relationships
            foreach (Transform child in relationshipsContainer)
            {
                Destroy(child.gameObject);
            }

            // Display tribe information
            DisplayRelationship("Tribe", character.TribeName);
        }

        private void DisplayStat(string statName, float value)
        {
            var statObj = Instantiate(statPrefab, statsContainer);
            var statText = statObj.GetComponent<TextMeshProUGUI>();
            if (statText != null)
            {
                statText.text = $"{statName}: {value}";
            }
        }

        private void DisplayRelationship(string npcName, string trustLevel)
        {
            var relationshipObj = Instantiate(relationshipPrefab, relationshipsContainer);
            var relationshipText = relationshipObj.GetComponent<TextMeshProUGUI>();
            if (relationshipText != null)
            {
                relationshipText.text = $"{npcName}: {trustLevel}";
            }
        }

        private void DisplayRelationship(string npcName, float trust)
        {
            var relationshipObj = Instantiate(relationshipPrefab, relationshipsContainer);
            var relationshipText = relationshipObj.GetComponent<TextMeshProUGUI>();
            if (relationshipText != null)
            {
                string trustLevel = trust < 0.8f ? "Low" : trust > 1.2f ? "High" : "Neutral";
                relationshipText.text = $"{npcName}: {trustLevel} Trust";
            }
        }

        public void HideNPCInfo()
        {
            npcInfoPanel.SetActive(false);
        }

        public void Initialize(
            GameObject mainMenuPanel,
            GameObject gameHudPanel,
            GameObject dialoguePanel,
            GameObject challengePanel,
            GameObject pauseMenuPanel)
        {
            this.mainMenuPanel = mainMenuPanel;
            this.gameHudPanel = gameHudPanel;
            this.dialoguePanel = dialoguePanel;
            this.challengePanel = challengePanel;
            this.pauseMenuPanel = pauseMenuPanel;

            // Hide all panels initially
            HideAllPanels();

            // Ensure player status bar is visible when game HUD is shown
            if (playerStatusBar != null)
            {
                playerStatusBar.gameObject.SetActive(true);
            }
        }

        public void ShowMainMenu()
        {
            HideAllPanels();
            if (mainMenuPanel != null)
                mainMenuPanel.SetActive(true);
        }

        public void ShowGameHUD()
        {
            HideAllPanels();
            if (gameHudPanel != null)
                gameHudPanel.SetActive(true);
        }

        public void ShowDialogue()
        {
            if (dialoguePanel != null)
                dialoguePanel.SetActive(true);
        }

        public void HideDialogue()
        {
            if (dialoguePanel != null)
                dialoguePanel.SetActive(false);
        }

        public void ShowChallenge()
        {
            if (challengePanel != null)
                challengePanel.SetActive(true);
        }

        public void HideChallenge()
        {
            if (challengePanel != null)
                challengePanel.SetActive(false);
        }

        public void ShowPauseMenu()
        {
            if (pauseMenuPanel != null)
                pauseMenuPanel.SetActive(true);
        }

        public void HidePauseMenu()
        {
            if (pauseMenuPanel != null)
                pauseMenuPanel.SetActive(false);
        }

        private void HideAllPanels()
        {
            if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
            if (gameHudPanel != null) gameHudPanel.SetActive(false);
            if (dialoguePanel != null) dialoguePanel.SetActive(false);
            if (challengePanel != null) challengePanel.SetActive(false);
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        }
    }
} 