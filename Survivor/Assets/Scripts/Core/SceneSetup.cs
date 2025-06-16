using UnityEngine;
using Survivor.Common;
using Survivor.Core;
using Survivor.Characters;
using Survivor.Environment;
using Survivor.Challenges;
using System.Collections;
using System.Collections.Generic;
using Survivor.Generation;

namespace Survivor.Core
{
    [DefaultExecutionOrder(-100)] // Ensure this runs before other scripts
    public class SceneSetup : MonoBehaviour
    {
        [Header("Core Systems")]
        [SerializeField] private TribeManager tribeManager;
        [SerializeField] private ChallengeSystem challengeSystem;
        [SerializeField] private UIManager uiManager;
        [SerializeField] private EnvironmentManager environmentManager;
        [SerializeField] private PlayerManager playerManager;

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
            Debug.Log("[SceneSetup] Awake called");
            InitializeSystems();
        }

        private void InitializeSystems()
        {
            Debug.Log("[SceneSetup] Initializing game systems...");
            
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
                Debug.Log("[SceneSetup] UI Manager initialized");
            }

            // Initialize Environment Manager
            if (environmentManager != null)
            {
                environmentManager.Initialize();
                Debug.Log("[SceneSetup] Environment Manager initialized");
            }

            // Initialize Challenge System
            if (challengeSystem != null)
            {
                challengeSystem.onChallengeStarted.AddListener(OnChallengeStarted);
                challengeSystem.onChallengeCompleted.AddListener(OnChallengeCompleted);
                challengeSystem.onChallengeFailed.AddListener(OnChallengeFailed);
                Debug.Log("[SceneSetup] Challenge System initialized");
            }

            // Initialize Player Manager
            if (playerManager == null)
            {
                Debug.Log("[SceneSetup] Creating PlayerManager...");
                GameObject playerManagerObj = new GameObject("PlayerManager");
                playerManager = playerManagerObj.AddComponent<PlayerManager>();
                DontDestroyOnLoad(playerManagerObj);
            }
            Debug.Log("[SceneSetup] PlayerManager initialized");

            // Initialize Tribe Manager and create tribes
            if (tribeManager != null)
            {
                Debug.Log("[SceneSetup] Initializing Tribe Manager...");
                tribeManager.Initialize();
                StartCoroutine(CreateTribesAndSpawnPlayer());
            }
            else
            {
                Debug.LogError("[SceneSetup] TribeManager reference is missing!");
            }
        }

        private IEnumerator CreateTribesAndSpawnPlayer()
        {
            Debug.Log("[SceneSetup] Starting tribe creation process...");
            yield return StartCoroutine(tribeManager.CreateTribes());
            
            // Wait a frame to ensure tribes are created
            yield return null;
            
            // Spawn player after tribes are created
            SpawnPlayer();
            
            Debug.Log("[SceneSetup] Tribe creation and player spawning completed");
        }

        private void SpawnPlayer()
        {
            if (playerPrefab != null && islandGenerator != null)
            {
                Vector3? campPos = islandGenerator.GetCampPosition();
                if (campPos.HasValue)
                {
                    Debug.Log("[SceneSetup] Spawning player at camp position..." );
                    GameObject player = Instantiate(playerPrefab, campPos.Value, Quaternion.identity);
                    
                    // Get the player's character component (either Character or PlayerCharacter)
                    Character playerCharacter = player.GetComponent<PlayerCharacter>();
                    if (playerCharacter == null)
                    {
                        playerCharacter = player.GetComponent<Character>();
                    }
                    
                    if (playerCharacter != null)
                    {
                        Debug.Log($"[SceneSetup] Found Character component on player. Current tribe name: {playerCharacter.TribeName}");
                        
                        // Explicitly set the tribe name first
                        playerCharacter.TribeName = tribeManager.tribeAName;
                        Debug.Log($"[SceneSetup] Set player's tribe name to: {playerCharacter.TribeName}");
                        
                        // Then initialize with all properties
                        playerCharacter.Initialize(
                            "Player",  // Default name
                            tribeManager.tribeAName,  // Use Tribe A as the player's tribe
                            true,  // Is player
                            0  // ID
                        );
                        
                        Debug.Log($"[SceneSetup] After initialization - Player tribe name: {playerCharacter.TribeName}");
                        
                        // Register player with TribeManager
                        tribeManager.AddTribeMember(playerCharacter);
                        Debug.Log("[SceneSetup] Registered player with TribeManager");
                        
                        // Register with PlayerManager if it's a PlayerController
                        PlayerController playerController = player.GetComponent<PlayerController>();
                        if (playerController != null && playerManager != null)
                        {
                            playerManager.RegisterPlayer(playerController);
                            Debug.Log("[SceneSetup] Registered player with PlayerManager");
                        }
                        
                        List<Character> tribeMembers = tribeManager.GetTribeMembers(playerCharacter.TribeName);
                        Debug.Log($"[SceneSetup] Found {tribeMembers.Count} members in tribe: {playerCharacter.TribeName}");
                        
                        if (tribeMembers.Count == 0)
                        {
                            Debug.LogWarning($"[SceneSetup] No tribe members found for tribe: {playerCharacter.TribeName}");
                            // Debug all tribes
                            var allTribes = tribeManager.GetAllTribeMembers();
                            Debug.Log($"[SceneSetup] Total tribe members across all tribes: {allTribes.Count}");
                            foreach (var member in allTribes)
                            {
                                Debug.Log($"[SceneSetup] Tribe member: {member.CharacterName}, Tribe: {member.TribeName}, IsPlayer: {member.IsPlayer}");
                            }
                        }
                        
                        foreach (Character member in tribeMembers)
                        {
                            if (!member.IsPlayer)
                            {
                                Vector3 spawnPos = campPos.Value + Random.insideUnitSphere * 5f;
                                spawnPos.y = campPos.Value.y;
                                member.transform.position = spawnPos;
                                Debug.Log($"[SceneSetup] Spawned tribe member {member.CharacterName} at position {spawnPos}");
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError("[SceneSetup] Player prefab does not have a Character or PlayerCharacter component!");
                    }
                }
                else
                {
                    Debug.LogError("[SceneSetup] Camp position not found!");
                }
            }
            else
            {
                Debug.LogError("[SceneSetup] Player prefab or island generator is missing!");
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