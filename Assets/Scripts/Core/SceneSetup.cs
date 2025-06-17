using UnityEngine;
using System.Collections.Generic;
using Survivor.Common;
using Survivor.Core;
using Survivor.Characters;
using Survivor.Environment;
using Survivor.Challenges;
using System.Collections;
using Survivor.Generation;
using Cinemachine;

namespace Survivor.Core
{
    [DefaultExecutionOrder(-100)] // Ensure this runs before other scripts
    public class SceneSetup : MonoBehaviour
    {
        public TribeManager tribeManager;
        public PlayerManager playerManager;
        public GameObject playerPrefab;
        public IslandGenerator islandGenerator;

        void Start()
        {
            // Call SpawnPlayer at the start
            SpawnPlayer();
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
                    
                    // Ensure the player has the correct tag
                    player.tag = "Player";
                    Debug.Log("[SceneSetup] Set player tag to 'Player'");
                    
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

                        // Set up Cinemachine camera to follow the player
                        SetupCamera(player);
                        
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

        private void SetupCamera(GameObject player)
        {
            // Find all virtual cameras in the scene
            var vcams = FindObjectsOfType<CinemachineVirtualCamera>();
            Debug.Log($"[SceneSetup] Found {vcams.Length} virtual cameras in scene");

            foreach (var vcam in vcams)
            {
                Debug.Log($"[SceneSetup] Processing virtual camera: {vcam.name}");
                
                // Find the PlayerCameraRoot in the player prefab
                Transform cameraRoot = player.transform.Find("PlayerCameraRoot");
                if (cameraRoot != null)
                {
                    Debug.Log($"[SceneSetup] Found PlayerCameraRoot at position: {cameraRoot.position}");
                    vcam.Follow = cameraRoot;
                    vcam.LookAt = cameraRoot;
                    Debug.Log($"[SceneSetup] Set up camera {vcam.name} to follow player");
                }
                else
                {
                    Debug.LogError("[SceneSetup] Could not find PlayerCameraRoot in player prefab!");
                    // Try to find any child transform that might be suitable
                    foreach (Transform child in player.transform)
                    {
                        Debug.Log($"[SceneSetup] Found child transform: {child.name}");
                    }
                }
            }

            if (vcams.Length == 0)
            {
                Debug.LogError("[SceneSetup] No virtual cameras found in scene!");
            }
        }
    }
} 