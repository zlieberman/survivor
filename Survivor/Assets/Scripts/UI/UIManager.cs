using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Survivor.Core;

namespace Survivor.UI
{
    public class UIManager : MonoBehaviour
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
                ChallengeSystem.Instance.onChallengeStart.AddListener(ShowChallengeUI);
                ChallengeSystem.Instance.onChallengeEnd.AddListener(HideChallengeUI);
            }

            if (VotingSystem.Instance != null)
            {
                VotingSystem.Instance.onVotingStart.AddListener(ShowVotingUI);
                VotingSystem.Instance.onVotingEnd.AddListener(() => {
                    DisplayVoteResults();
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

        private void ShowChallengeUI(Challenge challenge)
        {
            challengePanel.SetActive(true);
            challengeNameText.text = challenge.name;
            challengeDescriptionText.text = challenge.description;
        }

        private void HideChallengeUI()
        {
            challengePanel.SetActive(false);
        }

        private void ShowVotingUI()
        {
            votingPanel.SetActive(true);
            ClearVoteResults();
        }

        private void OnVotingComplete(string eliminatedPlayer)
        {
            DisplayVoteResults();
            Invoke(nameof(HideVotingUI), 5f); // Hide after 5 seconds
        }

        private void HideVotingUI()
        {
            votingPanel.SetActive(false);
        }

        private void ClearVoteResults()
        {
            foreach (Transform child in voteResultsContainer)
            {
                Destroy(child.gameObject);
            }
        }

        private void DisplayVoteResults()
        {
            var voteResults = VotingSystem.Instance.GetVoteResults();
            foreach (var result in voteResults)
            {
                var npcData = NPCManager.Instance.GetNPCData(result.Key);
                if (npcData != null)
                {
                    var resultObj = Instantiate(voteResultPrefab, voteResultsContainer);
                    var resultText = resultObj.GetComponent<TextMeshProUGUI>();
                    if (resultText != null)
                    {
                        resultText.text = $"{npcData.name}: {result.Value} votes";
                    }
                }
            }
        }

        public void ShowNPCInfo(NPC npc)
        {
            if (npc == null) return;

            npcInfoPanel.SetActive(true);
            selectedNpcNameText.text = npc.npcName;

            // Clear existing stats
            foreach (Transform child in statsContainer)
            {
                Destroy(child.gameObject);
            }

            // Display personality traits and stats
            DisplayStat("Loyalty", npc.loyalty);
            DisplayStat("Sneakiness", npc.sneakiness);
            DisplayStat("Charisma", npc.charisma);
            DisplayStat("Aggression", npc.aggression);
            DisplayStat("Strength", npc.strength);
            DisplayStat("Agility", npc.agility);
            DisplayStat("Puzzle Skill", npc.puzzleSkill);

            // Clear existing relationships
            foreach (Transform child in relationshipsContainer)
            {
                Destroy(child.gameObject);
            }

            // Display relationships
            foreach (var relationship in npc.trustScores)
            {
                int targetPlayerId = relationship.Key;
                float trustValue = relationship.Value;
                string targetName = targetPlayerId == 0 ? "Player" : NPCManager.Instance.GetNPCData($"NPC_{targetPlayerId}")?.name ?? $"NPC {targetPlayerId}";
                DisplayRelationship(targetName, trustValue);
            }
        }

        private void DisplayStat(string statName, int value)
        {
            var statObj = Instantiate(statPrefab, statsContainer);
            var statText = statObj.GetComponent<TextMeshProUGUI>();
            if (statText != null)
            {
                statText.text = $"{statName}: {value}";
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
    }
} 