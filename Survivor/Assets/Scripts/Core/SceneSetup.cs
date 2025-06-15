using UnityEngine;
using Survivor.Common;
using Survivor.Core;
using Survivor.Dialogue;
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

            // Initialize Dialogue System
            if (dialogueSystem != null && tribeManager != null)
            {
                dialogueSystem.Initialize(tribeManager);
                Debug.Log("Dialogue System initialized");
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
                    // Get the player's tribe member component
                    TribeMember playerTribeMember = player.GetComponent<TribeMember>();
                    if (playerTribeMember != null)
                    {
                        Debug.Log($"Player spawned in tribe: {playerTribeMember.tribeName}");
                        List<TribeMember> tribeMembers = tribeManager.GetTribeMembers(playerTribeMember.tribeName);
                        Debug.Log($"Found {tribeMembers.Count} members in player's tribe");
                        foreach (TribeMember member in tribeMembers)
                        {
                            if (!member.IsPlayer)
                            {
                                Vector3 spawnPos = campPos.Value + Random.insideUnitSphere * 5f;
                                spawnPos.y = campPos.Value.y;
                                member.transform.position = spawnPos;
                                Debug.Log($"Spawned tribe member {member.memberName} at position {spawnPos}");
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError("Player prefab is missing TribeMember component!");
                    }
                }
                else
                {
                    Debug.LogError("Camp position is not available from ProceduralIslandGenerator!");
                }
            }
            else
            {
                Debug.LogError("Player prefab or ProceduralIslandGenerator reference is missing!");
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