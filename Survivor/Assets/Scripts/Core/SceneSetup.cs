using UnityEngine;
using Survivor.Common;
using Survivor.Core;
using Survivor.Characters;
using Survivor.Environment;
using Survivor.Tribes;
using Survivor.Challenges;
using System.Collections;
using System.Collections.Generic;
using Survivor.Generation;

namespace Survivor.Core
{
    public class SceneSetup : MonoBehaviour
    {
        [Header("Core Systems")]
        [SerializeField] private TribeManager tribeManager;
        [SerializeField] private ChallengeSystem challengeSystem;
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

        [Header("Island Generation")]
        [SerializeField] private ProceduralIslandGenerator islandGenerator;

        private void Awake()
        {
            InitializeSystems();
        }

        private void InitializeSystems()
        {
            Debug.Log("Initializing game systems...");
            
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
                Debug.Log("UI Manager initialized");
            }

            // Initialize Environment Manager
            if (environmentManager != null)
            {
                environmentManager.Initialize();
                Debug.Log("Environment Manager initialized");
            }

            // Initialize Challenge System
            if (challengeSystem != null)
            {
                challengeSystem.onChallengeStarted.AddListener(OnChallengeStarted);
                challengeSystem.onChallengeCompleted.AddListener(OnChallengeCompleted);
                challengeSystem.onChallengeFailed.AddListener(OnChallengeFailed);
                Debug.Log("Challenge System initialized");
            }

            // Initialize Tribe Manager and create tribes
            if (tribeManager != null)
            {
                Debug.Log("Initializing Tribe Manager...");
                tribeManager.Initialize();
                StartCoroutine(CreateTribesAndSpawnPlayer());
            }
            else
            {
                Debug.LogError("TribeManager reference is missing in SceneSetup!");
            }
        }

        private IEnumerator CreateTribesAndSpawnPlayer()
        {
            Debug.Log("Starting tribe creation process...");
            yield return StartCoroutine(tribeManager.CreateTribes());
            
            // Wait a frame to ensure tribes are created
            yield return null;
            
            // Spawn player after tribes are created
            SpawnPlayer();
            
            Debug.Log("Tribe creation and player spawning completed");
        }

        private void SpawnPlayer()
        {
            if (playerPrefab != null && islandGenerator != null)
            {
                Vector3? campPos = islandGenerator.GetCampPosition();
                if (campPos.HasValue)
                {
                    Debug.Log("Spawning player at camp position..." );
                    GameObject player = Instantiate(playerPrefab, campPos.Value, Quaternion.identity);
                    // Get the player's character component
                    PlayerCharacter playerCharacter = player.GetComponent<PlayerCharacter>();
                    if (playerCharacter != null)
                    {
                        Debug.Log($"Player spawned in tribe: {playerCharacter.TribeName}");
                        List<Character> tribeMembers = tribeManager.GetTribeMembers(playerCharacter.TribeName);
                        Debug.Log($"Found {tribeMembers.Count} members in player's tribe");
                        foreach (Character member in tribeMembers)
                        {
                            if (!member.IsPlayer)
                            {
                                Vector3 spawnPos = campPos.Value + Random.insideUnitSphere * 5f;
                                spawnPos.y = campPos.Value.y;
                                member.transform.position = spawnPos;
                                Debug.Log($"Spawned tribe member {member.CharacterName} at position {spawnPos}");
                            }
                        }

                        // Set tribe information
                        if (playerCharacter != null)
                        {
                            playerCharacter.Initialize(
                                playerCharacter.CharacterName,
                                playerCharacter.TribeName,
                                playerCharacter.IsPlayer,
                                playerCharacter.GetInstanceID()
                            );
                        }
                    }
                    else
                    {
                        Debug.LogError("Player prefab does not have a PlayerCharacter component!");
                    }
                }
                else
                {
                    Debug.LogError("Camp position not found!");
                }
            }
            else
            {
                Debug.LogError("Player prefab or island generator is missing!");
            }
        }

        private void OnChallengeStarted(Challenge challenge)
        {
            Debug.Log($"Challenge started: {challenge.data.title}");
            // Additional challenge start logic
        }

        private void OnChallengeCompleted(Challenge challenge)
        {
            Debug.Log($"Challenge completed: {challenge.data.title}");
            // Additional challenge completion logic
        }

        private void OnChallengeFailed(Challenge challenge)
        {
            Debug.Log($"Challenge failed: {challenge.data.title}");
            // Additional challenge failure logic
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