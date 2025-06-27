using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Survivor.Characters;
using Survivor.Shared;
using StarterAssets;

namespace Survivor.Challenges
{
    /// <summary>
    /// Manages the setup and teardown of challenge scenes
    /// </summary>
    public class ChallengeSceneManager : MonoBehaviour
    {
        [Header("Spawn Points")]
        [SerializeField] private Transform[] team1SpawnPoints;
        [SerializeField] private Transform[] team2SpawnPoints;
        [SerializeField] private Transform[] sitOutSpawnPoints;
        
        [Header("Prefabs")]
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private GameObject npcPrefab;
        
        [Header("Scene Management")]
        [SerializeField] private string returnSceneName = "TropicalIsland";
        [SerializeField] private float returnDelay = 3f;
        
        private ChallengeSession currentSession;
        private List<GameObject> spawnedPlayers = new List<GameObject>();
        private List<GameObject> spawnedNPCs = new List<GameObject>();
        
        private void Start()
        {
            // Get the current challenge session from the ChallengeManager using reflection
            var challengeManagerType = System.Type.GetType("Survivor.Core.ChallengeManager");
            if (challengeManagerType != null)
            {
                var instanceProperty = challengeManagerType.GetProperty("Instance");
                if (instanceProperty != null)
                {
                    var challengeManager = instanceProperty.GetValue(null);
                    if (challengeManager != null)
                    {
                        var getCurrentSessionMethod = challengeManagerType.GetMethod("GetCurrentSession");
                        if (getCurrentSessionMethod != null)
                        {
                            currentSession = getCurrentSessionMethod.Invoke(challengeManager, null) as ChallengeSession;
                        }
                    }
                }
            }
            
            if (currentSession != null)
            {
                SetupChallengeScene();
            }
            else
            {
                Debug.LogError("[ChallengeSceneManager] No challenge session found!");
            }
        }
        
        private void SetupChallengeScene()
        {
            Debug.Log("[ChallengeSceneManager] Setting up challenge scene");
            
            // Spawn participants
            SpawnParticipants();
            
            // Set up camera and UI
            SetupCameraAndUI();
            
            // Initialize challenge-specific elements
            InitializeChallengeElements();
        }
        
        private void SpawnParticipants()
        {
            if (currentSession == null) return;
            
            // Group participants by tribe
            var tribes = new Dictionary<string, List<ChallengeParticipant>>();
            foreach (var participant in currentSession.participants)
            {
                if (!tribes.ContainsKey(participant.tribeId))
                {
                    tribes[participant.tribeId] = new List<ChallengeParticipant>();
                }
                tribes[participant.tribeId].Add(participant);
            }
            
            // Spawn each tribe
            int tribeIndex = 0;
            foreach (var tribe in tribes)
            {
                SpawnTribe(tribe.Key, tribe.Value, tribeIndex);
                tribeIndex++;
            }
            
            // Spawn sit-out players (hidden)
            SpawnSitOutPlayers();
            
            Debug.Log($"[ChallengeSceneManager] Spawned {spawnedPlayers.Count} players and {spawnedNPCs.Count} NPCs");
        }
        
        private void SpawnTribe(string tribeId, List<ChallengeParticipant> participants, int tribeIndex)
        {
            Transform[] spawnPoints = tribeIndex == 0 ? team1SpawnPoints : team2SpawnPoints;
            
            for (int i = 0; i < participants.Count && i < spawnPoints.Length; i++)
            {
                var participant = participants[i];
                var spawnPoint = spawnPoints[i];
                
                if (participant.isMainPlayer)
                {
                    SpawnMainPlayer(participant, spawnPoint);
                }
                else
                {
                    SpawnNPC(participant, spawnPoint);
                }
            }
        }
        
        private void SpawnMainPlayer(ChallengeParticipant participant, Transform spawnPoint)
        {
            if (playerPrefab == null)
            {
                Debug.LogError("[ChallengeSceneManager] Player prefab not assigned!");
                return;
            }
            
            var playerObject = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
            spawnedPlayers.Add(playerObject);
            
            // Set up player components
            var thirdPersonController = playerObject.GetComponent<ThirdPersonController>();
            if (thirdPersonController == null)
            {
                thirdPersonController = playerObject.AddComponent<ThirdPersonController>();
            }
            
            // Set up camera to follow this player
            var mainCamera = Camera.main;
            if (mainCamera != null)
            {
                var cameraController = mainCamera.GetComponent<CameraController>();
                if (cameraController != null)
                {
                    cameraController.SetTarget(playerObject.transform);
                }
            }
            
            Debug.Log($"[ChallengeSceneManager] Spawned main player at {spawnPoint.position}");
        }
        
        private void SpawnNPC(ChallengeParticipant participant, Transform spawnPoint)
        {
            if (npcPrefab == null)
            {
                Debug.LogError("[ChallengeSceneManager] NPC prefab not assigned!");
                return;
            }
            
            var npcObject = Instantiate(npcPrefab, spawnPoint.position, spawnPoint.rotation);
            spawnedNPCs.Add(npcObject);
            
            // Set up NPC components
            var npcCharacter = npcObject.GetComponent<NPCCharacter>();
            if (npcCharacter == null)
            {
                npcCharacter = npcObject.AddComponent<NPCCharacter>();
            }
            
            // Set NPC name
            npcObject.name = participant.playerName;
            
            Debug.Log($"[ChallengeSceneManager] Spawned NPC {participant.playerName} at {spawnPoint.position}");
        }
        
        private void SpawnSitOutPlayers()
        {
            if (currentSession.sitOutPlayers == null) return;
            
            for (int i = 0; i < currentSession.sitOutPlayers.Count && i < sitOutSpawnPoints.Length; i++)
            {
                var participant = currentSession.sitOutPlayers[i];
                var spawnPoint = sitOutSpawnPoints[i];
                
                // Spawn but disable rendering (they're "sitting out")
                GameObject sitOutObject;
                if (participant.isMainPlayer)
                {
                    sitOutObject = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
                    spawnedPlayers.Add(sitOutObject);
                }
                else
                {
                    sitOutObject = Instantiate(npcPrefab, spawnPoint.position, spawnPoint.rotation);
                    spawnedNPCs.Add(sitOutObject);
                }
                
                // Disable rendering and interaction
                var renderers = sitOutObject.GetComponentsInChildren<Renderer>();
                foreach (var renderer in renderers)
                {
                    renderer.enabled = false;
                }
                
                var colliders = sitOutObject.GetComponentsInChildren<Collider>();
                foreach (var collider in colliders)
                {
                    collider.enabled = false;
                }
                
                Debug.Log($"[ChallengeSceneManager] Spawned sit-out player {participant.playerName} (hidden)");
            }
        }
        
        private void SetupCameraAndUI()
        {
            // Set up challenge-specific UI
            var challengeUI = FindObjectOfType<ChallengeUI>();
            if (challengeUI != null)
            {
                challengeUI.Initialize(currentSession);
            }
            
            // Set up challenge-specific camera settings
            var mainCamera = Camera.main;
            if (mainCamera != null)
            {
                // Adjust camera for challenge scene if needed
                var cameraController = mainCamera.GetComponent<CameraController>();
                if (cameraController != null)
                {
                    // Could add challenge-specific camera settings here
                    Debug.Log("[ChallengeSceneManager] Camera controller found and configured");
                }
            }
        }
        
        private void InitializeChallengeElements()
        {
            // Initialize any challenge-specific game objects
            var challengeElements = FindObjectsOfType<ChallengeElement>();
            foreach (var element in challengeElements)
            {
                element.Initialize(currentSession);
            }
        }
        
        public void EndChallenge(string winningTribeId)
        {
            Debug.Log($"[ChallengeSceneManager] Challenge ended. Winner: {winningTribeId}");
            
            // Show results
            ShowChallengeResults(winningTribeId);
            
            // Clean up challenge scene
            CleanupChallengeScene();
            
            // Return to main island after delay
            StartCoroutine(ReturnToMainIsland());
        }
        
        private void ShowChallengeResults(string winningTribeId)
        {
            // Show challenge results UI
            var resultsUI = FindObjectOfType<ChallengeResultsUI>();
            if (resultsUI != null)
            {
                resultsUI.ShowResults(winningTribeId, currentSession);
            }
            else
            {
                Debug.LogWarning("[ChallengeSceneManager] No ChallengeResultsUI found!");
            }
        }
        
        private void CleanupChallengeScene()
        {
            // Disable player input
            var players = FindObjectsOfType<ThirdPersonController>();
            foreach (var player in players)
            {
                player.enabled = false;
            }
            
            // Disable NPC AI
            var npcs = FindObjectsOfType<NPCCharacter>();
            foreach (var npc in npcs)
            {
                npc.enabled = false;
            }
            
            Debug.Log("[ChallengeSceneManager] Challenge scene cleaned up");
        }
        
        private System.Collections.IEnumerator ReturnToMainIsland()
        {
            yield return new WaitForSeconds(returnDelay);
            
            Debug.Log("[ChallengeSceneManager] Returning to main island");
            SceneManager.LoadScene(returnSceneName);
        }
        
        // Public methods for external access
        public List<GameObject> GetSpawnedPlayers()
        {
            return new List<GameObject>(spawnedPlayers);
        }
        
        public List<GameObject> GetSpawnedNPCs()
        {
            return new List<GameObject>(spawnedNPCs);
        }
        
        public ChallengeSession GetCurrentSession()
        {
            return currentSession;
        }
        
        // Debug methods
        [ContextMenu("Test Spawn Participants")]
        public void TestSpawnParticipants()
        {
            if (currentSession != null)
            {
                SpawnParticipants();
            }
            else
            {
                Debug.LogWarning("[ChallengeSceneManager] No current session for test spawn!");
            }
        }
        
        [ContextMenu("Show Spawn Status")]
        public void ShowSpawnStatus()
        {
            Debug.Log("=== Challenge Scene Manager Status ===");
            Debug.Log($"[ChallengeSceneManager] Spawned players: {spawnedPlayers.Count}");
            Debug.Log($"[ChallengeSceneManager] Spawned NPCs: {spawnedNPCs.Count}");
            Debug.Log($"[ChallengeSceneManager] Team 1 spawn points: {team1SpawnPoints?.Length ?? 0}");
            Debug.Log($"[ChallengeSceneManager] Team 2 spawn points: {team2SpawnPoints?.Length ?? 0}");
            Debug.Log($"[ChallengeSceneManager] Sit-out spawn points: {sitOutSpawnPoints?.Length ?? 0}");
            
            if (currentSession != null)
            {
                Debug.Log($"[ChallengeSceneManager] Current session: {currentSession.challenge.challengeName}");
                Debug.Log($"[ChallengeSceneManager] Participants: {currentSession.participants.Count}");
                Debug.Log($"[ChallengeSceneManager] Sit-out players: {currentSession.sitOutPlayers.Count}");
            }
            
            Debug.Log("=== End Status ===");
        }
    }
    
    /// <summary>
    /// Base class for challenge-specific UI elements
    /// </summary>
    public abstract class ChallengeUI : MonoBehaviour
    {
        public abstract void Initialize(ChallengeSession session);
        public abstract void UpdateProgress(float progress);
        public abstract void UpdateScores(Dictionary<string, int> scores);
    }
    
    /// <summary>
    /// Base class for challenge results UI
    /// </summary>
    public abstract class ChallengeResultsUI : MonoBehaviour
    {
        public abstract void ShowResults(string winningTribeId, ChallengeSession session);
    }
    
    /// <summary>
    /// Base class for challenge-specific game elements
    /// </summary>
    public abstract class ChallengeElement : MonoBehaviour
    {
        public abstract void Initialize(ChallengeSession session);
        public abstract void OnChallengeStart();
        public abstract void OnChallengeEnd();
    }
} 