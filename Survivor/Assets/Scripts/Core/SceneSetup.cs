using UnityEngine;
using Survivor.Common;
using Survivor.Core;
using Survivor.Dialogue;
using Survivor.Environment;
using Survivor.Tribes;
using Survivor.Challenges;

namespace Survivor.Core
{
    public class SceneSetup : MonoBehaviour
    {
        [Header("Core Systems")]
        [SerializeField] private TribeManager tribeManager;
        [SerializeField] private ChallengeSystem challengeSystem;
        [SerializeField] private DialogueSystem dialogueSystem;
        [SerializeField] private UIManager uiManager;
        [SerializeField] private EnvironmentManager environmentManager;

        [Header("UI Panels")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject gameHudPanel;
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private GameObject challengePanel;
        [SerializeField] private GameObject pauseMenuPanel;

        [Header("Player")]
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private Transform playerSpawnPoint;

        private void Awake()
        {
            InitializeSystems();
            SpawnPlayer();
        }

        private void InitializeSystems()
        {
            // Initialize UI Manager
            if (uiManager != null)
            {
                uiManager.Initialize(
                    mainMenuPanel,
                    gameHudPanel,
                    dialoguePanel,
                    challengePanel,
                    pauseMenuPanel
                );
            }

            // Initialize Dialogue System
            if (dialogueSystem != null && tribeManager != null)
            {
                dialogueSystem.Initialize(tribeManager);
            }

            // Initialize Environment Manager
            if (environmentManager != null)
            {
                environmentManager.Initialize();
            }

            // Initialize Challenge System
            if (challengeSystem != null)
            {
                challengeSystem.onChallengeStarted.AddListener(OnChallengeStarted);
                challengeSystem.onChallengeCompleted.AddListener(OnChallengeCompleted);
                challengeSystem.onChallengeFailed.AddListener(OnChallengeFailed);
            }

            // Initialize Tribe Manager
            if (tribeManager != null)
            {
                tribeManager.Initialize();
            }
        }

        private void SpawnPlayer()
        {
            if (playerPrefab != null && playerSpawnPoint != null)
            {
                Instantiate(playerPrefab, playerSpawnPoint.position, playerSpawnPoint.rotation);
            }
        }

        private void OnChallengeStarted(Challenge challenge)
        {
            if (uiManager != null)
            {
                uiManager.ShowChallengeUI(challenge);
            }
        }

        private void OnChallengeCompleted(Challenge challenge)
        {
            if (uiManager != null)
            {
                uiManager.HideChallengeUI();
            }
        }

        private void OnChallengeFailed(Challenge challenge)
        {
            if (uiManager != null)
            {
                uiManager.HideChallengeUI();
            }
        }

        private void OnDestroy()
        {
            if (challengeSystem != null)
            {
                challengeSystem.onChallengeStarted.RemoveListener(OnChallengeStarted);
                challengeSystem.onChallengeCompleted.RemoveListener(OnChallengeCompleted);
                challengeSystem.onChallengeFailed.RemoveListener(OnChallengeFailed);
            }
        }
    }
} 