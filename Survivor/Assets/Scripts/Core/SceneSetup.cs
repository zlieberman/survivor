using UnityEngine;
using Survivor.Common;
using Survivor.Characters;
using Survivor.Environment;
using Survivor.Generation;
using Survivor.Interactables;
using Survivor.UI;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;

namespace Survivor.Core
{
    [DefaultExecutionOrder(-500)] // Ensure this runs before other scripts
    public class SceneSetup : MonoBehaviour
    {
        [Header("Core Systems")]
        [SerializeField] private TribeManager tribeManager;
        [SerializeField] private EnvironmentManager environmentManager;
        [SerializeField] private PlayerManager playerManager;

        [Header("UI Panels")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject gameHudPanel;
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private GameObject challengePanel;
        [SerializeField] private GameObject pauseMenuPanel;
        [SerializeField] private TribeInfoMenuController tribeInfoMenuController;

        [Header("Player")]
        [SerializeField] private GameObject playerPrefab;

        [Header("Island Generation")]
        [SerializeField] private Survivor.Generation.ProceduralIslandGenerator islandGenerator;

        private void Awake()
        {
            Debug.Log("[SceneSetup] Awake called");
            InitializeSystems();
        }

        private void InitializeSystems()
        {
            Debug.Log("[SceneSetup] Initializing game systems...");
            
            // Note: InteractableManager should be created manually in the scene
            // Use the InteractableManagerCreator component if needed

            // Initialize Environment Manager
            if (environmentManager != null)
            {
                environmentManager.Initialize();
                Debug.Log("[SceneSetup] Environment Manager initialized");
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
                
                // Wire up UI controller with TribeManager
                WireUpTribeInfoUI();
                
                StartCoroutine(CreateTribesAndSpawnPlayer());
            }
            else
            {
                Debug.LogError("[SceneSetup] TribeManager reference is missing!");
            }
        }

        private void WireUpTribeInfoUI()
        {
            // Try to find the UI controller if not assigned
            if (tribeInfoMenuController == null)
            {
                tribeInfoMenuController = FindObjectOfType<TribeInfoMenuController>();
                if (tribeInfoMenuController != null)
                {
                    Debug.Log("[SceneSetup] Found TribeInfoMenuController automatically");
                }
                else
                {
                    Debug.LogWarning("[SceneSetup] TribeInfoMenuController not found in scene!");
                    return;
                }
            }

            // Wire up the UI controller with TribeManager
            if (tribeManager != null && tribeInfoMenuController != null)
            {
                tribeManager.SetUIController(tribeInfoMenuController);
                Debug.Log("[SceneSetup] Successfully wired TribeInfoMenuController with TribeManager");
            }
        }

        private IEnumerator CreateTribesAndSpawnPlayer()
        {
            Debug.Log("[SceneSetup] Starting tribe creation process...");
            yield return StartCoroutine(tribeManager.CreateTribes());
            
            // Wait a frame to ensure tribes are created
            yield return null;
            
            // Create player and spawn player
            CreatePlayer();

            // Spawn tribe members
            SpawnTribeMembers(tribeManager.tribeAName);
            
            Debug.Log("[SceneSetup] Tribe creation and player spawning completed");
        }

        private void CreatePlayer()
        {
            if (playerPrefab == null || islandGenerator == null)
            {
                Debug.LogError("[SceneSetup] Player prefab or island generator is missing!");
                return;
            }
            
            Vector3? campPos = islandGenerator.GetCampPosition();
            if (!campPos.HasValue)
            {
                Debug.LogError("[SceneSetup] Camp position not found!");
                return;
            }

            Debug.Log("[SceneSetup] Spawning player at camp position..." );
            GameObject player = Instantiate(playerPrefab, campPos.Value, Quaternion.identity);
            
            // Configure animator settings immediately
            Animator animator = player.GetComponentInChildren<Animator>();
            if (animator != null)
            {
                // First disable and re-enable the animator to force a refresh
                animator.enabled = false;
                
                // Set the animator settings
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                animator.updateMode = AnimatorUpdateMode.Normal;
                
                // Force the animator to update its state
                animator.Rebind();
                animator.Update(0f);
                
                // Re-enable the animator
                animator.enabled = true;
                
                Debug.Log("[SceneSetup] Configured animator settings - CullingMode: AlwaysAnimate, UpdateMode: Normal");
                
                // Verify the animator controller is properly assigned
                if (animator.runtimeAnimatorController == null)
                {
                    Debug.LogError("[SceneSetup] Animator controller is null on player!");
                }
                else
                {
                    Debug.Log($"[SceneSetup] Animator controller assigned: {animator.runtimeAnimatorController.name}");
                }
            }
            else
            {
                Debug.LogError("[SceneSetup] Could not find Animator component on player!");
            }
            
            // Get the player's character component (either Character or PlayerCharacter)
            Character playerCharacter = player.GetComponent<PlayerCharacter>();
            if (playerCharacter == null)
            {
                playerCharacter = player.GetComponent<Character>();
            }
            
            if (playerCharacter == null)
            {
                Debug.LogError("[SceneSetup] Player prefab does not have a Character or PlayerCharacter component!");
                return;
            }

            // Find the virtual camera
            var vcam = FindObjectOfType<CinemachineVirtualCamera>();
            if (vcam != null)
            {
                // Find the PlayerCameraRoot in the player
                Transform cameraRoot = player.transform.Find("PlayerCameraRoot");
                if (cameraRoot != null)
                {
                    // Set the follow and look at targets
                    vcam.Follow = cameraRoot;
                    vcam.LookAt = cameraRoot;
                    Debug.Log($"Set up camera to follow player at position: {cameraRoot.position}");
                }
                else
                {
                    Debug.LogError("Could not find PlayerCameraRoot in player prefab!");
                }
            }
            else
            {
                Debug.LogError("Could not find CinemachineVirtualCamera in scene!");
            }

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
            
            // Register with PlayerManager if it's a Character
            Character character = player.GetComponent<Character>();
            if (character != null && playerManager != null)
            {
                playerManager.RegisterPlayer(character);
                Debug.Log("[SceneSetup] Registered player with PlayerManager");
            }
        }

        private void SpawnTribeMembers(string tribeName)
        {
            Vector3? campPos = islandGenerator.GetCampPosition();
            if (!campPos.HasValue)
            {
                Debug.LogError("[SceneSetup] Camp position not found!");
                return;
            }

            List<Character> tribeMembers = tribeManager.GetTribeMembers(tribeName);
            Debug.Log($"[SceneSetup] Found {tribeMembers.Count} members in tribe: {tribeName}");
            
            if (tribeMembers.Count == 0)
            {
                Debug.LogWarning($"[SceneSetup] No tribe members found for tribe: {tribeName}");
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
                    Vector3 spawnPos = campPos.Value + new Vector3(5f, 0f, 0f) + Random.insideUnitSphere * 5f;
                    spawnPos.y = campPos.Value.y;
                    member.transform.position = spawnPos;
                    Debug.Log($"[SceneSetup] Spawned tribe member {member.CharacterName} at position {spawnPos}");
                }
            }
        }

        private void OnDestroy()
        {
        }
    }
} 