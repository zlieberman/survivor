using UnityEngine;
using Survivor.Characters;
using UnityEngine.AI;

namespace Survivor.Characters
{
    /// <summary>
    /// Test script to help verify NPC wandering behavior
    /// </summary>
    public class NPCWanderingTest : MonoBehaviour
    {
        [Header("Test Controls")]
        public bool enableTestControls = true;
        public KeyCode testWanderingKey = KeyCode.W;
        public KeyCode stopWanderingKey = KeyCode.S;
        public KeyCode returnToCampKey = KeyCode.R;
        public KeyCode spawnTestNPCKey = KeyCode.T;
        public KeyCode checkConflictsKey = KeyCode.C;

        [Header("Test Settings")]
        public GameObject testNPCPrefab;
        public Vector3 testSpawnPosition = Vector3.zero;

        private NPCBehavior[] npcBehaviors;

        private void Start()
        {
            if (enableTestControls)
            {
                Debug.Log("[NPCWanderingTest] Test controls enabled. Use W/S/R/T/C keys to test NPC behavior:");
                Debug.Log("[NPCWanderingTest] W - Test wandering");
                Debug.Log("[NPCWanderingTest] S - Stop wandering");
                Debug.Log("[NPCWanderingTest] R - Return to camp");
                Debug.Log("[NPCWanderingTest] T - Spawn test NPC");
                Debug.Log("[NPCWanderingTest] C - Check for conflicting components");
            }
        }

        private void Update()
        {
            if (!enableTestControls) return;

            // Test wandering
            if (Input.GetKeyDown(testWanderingKey))
            {
                TestAllNPCsWandering();
            }

            // Test stopping
            if (Input.GetKeyDown(stopWanderingKey))
            {
                StopAllNPCsWandering();
            }

            // Test returning to camp
            if (Input.GetKeyDown(returnToCampKey))
            {
                ReturnAllNPCsToCamp();
            }

            // Spawn test NPC
            if (Input.GetKeyDown(spawnTestNPCKey))
            {
                SpawnTestNPC();
            }

            // Check for conflicting components on existing NPCs
            if (Input.GetKeyDown(checkConflictsKey))
            {
                CheckAllNPCsForConflicts();
            }
        }

        private void TestAllNPCsWandering()
        {
            Debug.Log("[NPCWanderingTest] Testing all NPCs wandering behavior");
            npcBehaviors = FindObjectsOfType<NPCBehavior>();
            
            if (npcBehaviors.Length == 0)
            {
                Debug.LogWarning("[NPCWanderingTest] No NPCBehavior components found in scene!");
                return;
            }

            Debug.Log($"[NPCWanderingTest] Found {npcBehaviors.Length} NPCs");
            
            foreach (var npcBehavior in npcBehaviors)
            {
                if (npcBehavior != null)
                {
                    Debug.Log($"[NPCWanderingTest] Testing wandering for {npcBehavior.gameObject.name}");
                    
                    // Check if NavMeshAgent is properly configured
                    NavMeshAgent agent = npcBehavior.GetComponent<NavMeshAgent>();
                    if (agent != null)
                    {
                        Debug.Log($"[NPCWanderingTest] {npcBehavior.gameObject.name} - NavMeshAgent enabled: {agent.enabled}, isOnNavMesh: {agent.isOnNavMesh}");
                        
                        // Force a test destination
                        Vector3 testDestination = npcBehavior.transform.position + Vector3.forward * 5f;
                        agent.SetDestination(testDestination);
                        Debug.Log($"[NPCWanderingTest] Set test destination for {npcBehavior.gameObject.name}: {testDestination}");
                    }
                    else
                    {
                        Debug.LogError($"[NPCWanderingTest] {npcBehavior.gameObject.name} has no NavMeshAgent!");
                    }
                    
                    npcBehavior.TestWandering();
                }
            }
        }

        private void StopAllNPCsWandering()
        {
            npcBehaviors = FindObjectsOfType<NPCBehavior>();
            Debug.Log($"[NPCWanderingTest] Stopping wandering for {npcBehaviors.Length} NPCs");

            foreach (var npcBehavior in npcBehaviors)
            {
                if (npcBehavior != null)
                {
                    npcBehavior.TestStopWandering();
                }
            }
        }

        private void ReturnAllNPCsToCamp()
        {
            npcBehaviors = FindObjectsOfType<NPCBehavior>();
            Debug.Log($"[NPCWanderingTest] Returning {npcBehaviors.Length} NPCs to camp");

            foreach (var npcBehavior in npcBehaviors)
            {
                if (npcBehavior != null)
                {
                    npcBehavior.TestReturnToCamp();
                }
            }
        }

        private void SpawnTestNPC()
        {
            if (testNPCPrefab == null)
            {
                Debug.LogWarning("[NPCWanderingTest] No test NPC prefab assigned!");
                return;
            }

            GameObject testNPC = Instantiate(testNPCPrefab, testSpawnPosition, Quaternion.identity);
            testNPC.name = "TestNPC";
            Debug.Log($"[NPCWanderingTest] Spawned test NPC at {testSpawnPosition}");
            
            // Configure NPC for NavMeshAgent movement
            ConfigureTestNPCForMovement(testNPC);
            
            // Check for conflicting components
            CheckForConflictingComponents(testNPC);
        }

        private void ConfigureTestNPCForMovement(GameObject npc)
        {
            Debug.Log($"[NPCWanderingTest] Configuring test NPC {npc.name} for NavMeshAgent movement");

            // Disable ThirdPersonController (conflicts with NavMeshAgent)
            MonoBehaviour thirdPersonController = npc.GetComponent<MonoBehaviour>();
            if (thirdPersonController != null && thirdPersonController.GetType().Name.Contains("ThirdPersonController"))
            {
                thirdPersonController.enabled = false;
                Debug.Log($"Disabled ThirdPersonController on {npc.name}");
            }

            // Disable PlayerInput component (not needed for NPCs)
            var playerInput = npc.GetComponent("UnityEngine.InputSystem.PlayerInput");
            if (playerInput != null)
            {
                var enabledProperty = playerInput.GetType().GetProperty("enabled");
                if (enabledProperty != null)
                {
                    enabledProperty.SetValue(playerInput, false);
                    Debug.Log($"Disabled PlayerInput on {npc.name}");
                }
            }

            // Disable StarterAssets.ThirdPersonController if it exists
            var starterAssetsController = npc.GetComponent("StarterAssets.ThirdPersonController");
            if (starterAssetsController != null)
            {
                var enabledProperty = starterAssetsController.GetType().GetProperty("enabled");
                if (enabledProperty != null)
                {
                    enabledProperty.SetValue(starterAssetsController, false);
                    Debug.Log($"Disabled StarterAssets.ThirdPersonController on {npc.name}");
                }
            }

            // Disable any other movement controllers that might conflict
            var movementControllers = npc.GetComponents<MonoBehaviour>();
            foreach (var controller in movementControllers)
            {
                string controllerName = controller.GetType().Name.ToLower();
                if (controllerName.Contains("controller") && 
                    !controllerName.Contains("npc") && 
                    !controllerName.Contains("behavior") &&
                    controller.enabled)
                {
                    controller.enabled = false;
                    Debug.Log($"Disabled conflicting controller {controller.GetType().Name} on {npc.name}");
                }
            }

            // Ensure NavMeshAgent is properly configured
            NavMeshAgent navAgent = npc.GetComponent<NavMeshAgent>();
            if (navAgent != null)
            {
                navAgent.enabled = true;
                navAgent.updatePosition = true;
                navAgent.updateRotation = true;
                navAgent.updateUpAxis = false;
                Debug.Log($"Configured NavMeshAgent on {npc.name}");
            }

            // Ensure CharacterController doesn't interfere with NavMeshAgent
            UnityEngine.CharacterController characterController = npc.GetComponent<UnityEngine.CharacterController>();
            if (characterController != null)
            {
                // Keep CharacterController for collision detection but don't use it for movement
                characterController.enabled = true;
                Debug.Log($"Kept CharacterController for collision on {npc.name}");
            }

            Debug.Log($"Finished configuring {npc.name} for NavMeshAgent movement");
        }

        private void CheckForConflictingComponents(GameObject npc)
        {
            Debug.Log($"[NPCWanderingTest] Checking for conflicting components on {npc.name}");
            
            // Check for ThirdPersonController
            var thirdPersonController = npc.GetComponent<MonoBehaviour>();
            if (thirdPersonController != null && thirdPersonController.GetType().Name.Contains("ThirdPersonController"))
            {
                Debug.LogWarning($"[NPCWanderingTest] {npc.name} has ThirdPersonController: {thirdPersonController.GetType().Name} (enabled: {thirdPersonController.enabled})");
            }

            // Check for PlayerInput
            var playerInput = npc.GetComponent("UnityEngine.InputSystem.PlayerInput");
            if (playerInput != null)
            {
                var enabledProperty = playerInput.GetType().GetProperty("enabled");
                bool isEnabled = enabledProperty != null ? (bool)enabledProperty.GetValue(playerInput) : false;
                Debug.LogWarning($"[NPCWanderingTest] {npc.name} has PlayerInput component (enabled: {isEnabled})");
            }

            // Check for StarterAssets components
            var allComponents = npc.GetComponents<MonoBehaviour>();
            foreach (var component in allComponents)
            {
                if (component.GetType().Namespace != null && component.GetType().Namespace.Contains("StarterAssets"))
                {
                    Debug.LogWarning($"[NPCWanderingTest] {npc.name} has StarterAssets component: {component.GetType().Name} (enabled: {component.enabled})");
                }
            }

            // Check NavMeshAgent
            var navAgent = npc.GetComponent<NavMeshAgent>();
            if (navAgent != null)
            {
                Debug.Log($"[NPCWanderingTest] {npc.name} has NavMeshAgent (enabled: {navAgent.enabled}, isOnNavMesh: {navAgent.isOnNavMesh})");
            }
            else
            {
                Debug.LogError($"[NPCWanderingTest] {npc.name} has no NavMeshAgent!");
            }

            // Check NPCBehavior
            var npcBehavior = npc.GetComponent<NPCBehavior>();
            if (npcBehavior != null)
            {
                Debug.Log($"[NPCWanderingTest] {npc.name} has NPCBehavior (enabled: {npcBehavior.enabled})");
            }
            else
            {
                Debug.LogError($"[NPCWanderingTest] {npc.name} has no NPCBehavior!");
            }
        }

        private void CheckAllNPCsForConflicts()
        {
            Debug.Log("[NPCWanderingTest] Checking all NPCs for conflicting components");
            
            NPCBehavior[] allNPCs = FindObjectsOfType<NPCBehavior>();
            if (allNPCs.Length == 0)
            {
                Debug.LogWarning("[NPCWanderingTest] No NPCs found in scene!");
                return;
            }

            foreach (var npc in allNPCs)
            {
                if (npc != null)
                {
                    CheckForConflictingComponents(npc.gameObject);
                }
            }
        }

        private void OnGUI()
        {
            if (!enableTestControls) return;

            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label("NPC Wandering Test Controls:");
            GUILayout.Label($"W - Test wandering for all NPCs");
            GUILayout.Label($"S - Stop wandering for all NPCs");
            GUILayout.Label($"R - Return all NPCs to camp");
            GUILayout.Label($"T - Spawn test NPC");

            npcBehaviors = FindObjectsOfType<NPCBehavior>();
            GUILayout.Label($"Active NPCs with behavior: {npcBehaviors.Length}");

            foreach (var npcBehavior in npcBehaviors)
            {
                if (npcBehavior != null)
                {
                    string state = npcBehavior.IsWandering ? "Wandering" : 
                                  npcBehavior.IsIdle ? "Idle" : "Unknown";
                    GUILayout.Label($"{npcBehavior.name}: {state}");
                }
            }
            GUILayout.EndArea();
        }
    }
} 