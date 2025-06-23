using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using StarterAssets;
using System.Linq;
using Survivor.UI;
using StarterAssets;
using Survivor.Characters;
using Survivor.Generation;
using Survivor.Shared;

namespace Survivor.Core
{
    [DefaultExecutionOrder(-50)]
    public class TribeManager : MonoBehaviour, ITribeManager, INPCManager
    {
        public static TribeManager Instance { get; private set; }

        [Header("Spawn Settings")]
        public float spawnRadius = 10f;
        public float minSpawnDistance = 2.5f;
        public int maxSpawnAttempts = 30;

        [Header("Tribe Settings")]
        public string tribeAName = "Tribe A";
        public string tribeBName = "Tribe B";

        [Header("Prefabs")]
        public GameObject npcPrefab;
        
        [Header("UI")]
        [SerializeField] private TribeInfoMenuController tribeInfoMenuController;
        
        private Dictionary<string, List<Character>> tribes = new Dictionary<string, List<Character>>();
        private List<Character> tribeMembers = new List<Character>();
        private bool isInitialized = false;
        private ProceduralIslandGenerator islandGenerator;

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

            Debug.Log($"[TribeManager] Awake - tribeAName: {tribeAName}, tribeBName: {tribeBName}");
        }

        private void Start()
        {
            // Find the island generator
            islandGenerator = FindObjectOfType<ProceduralIslandGenerator>();
            if (islandGenerator == null)
            {
                Debug.LogError("[TribeManager] Could not find ProceduralIslandGenerator in scene!");
            }
            else
            {
                Debug.Log("[TribeManager] Found ProceduralIslandGenerator");
            }

            // Try to find the UI controller if not assigned
            if (tribeInfoMenuController == null)
            {
                tribeInfoMenuController = FindObjectOfType<TribeInfoMenuController>();
                if (tribeInfoMenuController != null)
                {
                    Debug.Log("[TribeManager] Found TribeInfoMenuController automatically");
                }
                else
                {
                    Debug.LogWarning("[TribeManager] TribeInfoMenuController not found! UI updates will not work.");
                }
            }
        }

        private void Update()
        {
            // Handle T key press to show tribe info menu
            if (InputBlocker.GetKeyDown(KeyCode.T))
            {
                Debug.Log("[TribeManager] T key pressed - showing tribe info menu");
                ShowTribeInfoMenu();
            }
        }

        public void Initialize()
        {
            if (isInitialized) return;

            Debug.Log($"[TribeManager] Initializing with tribe names - A: {tribeAName}, B: {tribeBName}");
            tribes.Clear();
            tribes[tribeAName] = new List<Character>();
            tribes[tribeBName] = new List<Character>();

            isInitialized = true;
            Debug.Log("[TribeManager] Initialization complete");
        }

        public IEnumerator CreateTribes()
        {
            if (!isInitialized)
            {
                Debug.LogError("TribeManager not initialized!");
                yield break;
            }

            // Wait for island generation and camp placement
            while (islandGenerator == null || !islandGenerator.IsGenerationComplete())
            {
                Debug.Log("Waiting for island generation to complete...");
                yield return new WaitForSeconds(0.5f);
            }

            // Wait for camp to be placed
            while (islandGenerator.GetCampPosition() == null)
            {
                Debug.Log("Waiting for camp to be placed...");
                yield return new WaitForSeconds(0.5f);
            }

            yield return StartCoroutine(CreateTribe(tribeAName, true));
            yield return StartCoroutine(CreateTribe(tribeBName, false, false));

            Debug.Log("Tribes created successfully");
        }

        private IEnumerator CreateTribe(string tribeName, bool includePlayer, bool spawnNPCs = true)
        {
            Debug.Log($"Starting to create tribe: {tribeName}");
            List<Character> tribeMembers = new List<Character>();
            
            int npcCount = includePlayer ? 8 : 9;
            Debug.Log($"Creating {npcCount} members for tribe {tribeName}");
            
            for (int i = 0; i < npcCount; i++)
            {
                Vector3 spawnPosition = FindValidSpawnPosition(tribeMembers);
                
                GameObject npcObject = Instantiate(npcPrefab, spawnPosition, Quaternion.identity);
                Character character = npcObject.GetComponent<Character>();
                
                if (character != null)
                {
                    // Assign gender first
                    Gender gender = NPCGenerator.Instance.AssignGender(tribeName);
                    character.Gender = gender;
                    string memberName = NPCGenerator.Instance.GenerateRandomName(gender);
                    character.Initialize(memberName, tribeName, includePlayer && i == 0, i);
                    
                    // Add NavMeshAgent if not present
                    NavMeshAgent navAgent = npcObject.GetComponent<NavMeshAgent>();
                    if (navAgent == null)
                    {
                        navAgent = npcObject.AddComponent<NavMeshAgent>();
                        navAgent.radius = 0.5f;
                        navAgent.height = 2f;
                        navAgent.baseOffset = 0f;
                        navAgent.speed = 2f;
                        navAgent.angularSpeed = 120f;
                        navAgent.acceleration = 8f;
                        navAgent.stoppingDistance = 0.5f;
                        Debug.Log($"Added NavMeshAgent to {memberName}");
                    }

                    // Add NPCBehavior component if not present
                    Survivor.Characters.NPCBehavior npcBehavior = npcObject.GetComponent<Survivor.Characters.NPCBehavior>();
                    if (npcBehavior == null)
                    {
                        npcBehavior = npcObject.AddComponent<Survivor.Characters.NPCBehavior>();
                        Debug.Log($"Added NPCBehavior to {memberName}");
                    }

                    // Set up animator for NPCBehavior
                    Animator animator = npcObject.GetComponent<Animator>();
                    if (animator != null)
                    {
                        npcBehavior.animator = animator;
                        Debug.Log($"Assigned animator to NPCBehavior for {memberName}");
                    }
                    else
                    {
                        Debug.LogWarning($"No Animator component found on {memberName}, NPCBehavior will not animate");
                    }

                    // Add CharacterController if not present (required for NPCBehavior)
                    UnityEngine.CharacterController characterController = npcObject.GetComponent<UnityEngine.CharacterController>();
                    if (characterController == null)
                    {
                        characterController = npcObject.AddComponent<UnityEngine.CharacterController>();
                        characterController.height = 2f;
                        characterController.radius = 0.5f;
                        characterController.stepOffset = 0.3f;
                        Debug.Log($"Added CharacterController to {memberName}");
                    }

                    // Add collider if not present
                    Collider collider = npcObject.GetComponent<Collider>();
                    if (collider == null)
                    {
                        CapsuleCollider capsuleCollider = npcObject.AddComponent<CapsuleCollider>();
                        capsuleCollider.height = 2f;
                        capsuleCollider.radius = 0.5f;
                        capsuleCollider.center = new Vector3(0, 1f, 0);
                        Debug.Log($"Added CapsuleCollider to {memberName}");
                    }

                    // Configure NPC for NavMeshAgent movement (disable conflicting components)
                    ConfigureNPCForNavMeshMovement(npcObject, memberName);
                    
                    tribes[tribeName].Add(character);
                    tribeMembers.Add(character);

                    if (!spawnNPCs || (tribeName != tribeAName && !character.IsPlayer))
                    {
                        npcObject.SetActive(false);
                    }
                    
                    Debug.Log($"Created tribe member: {memberName} for tribe {tribeName} at position {spawnPosition}");
                }
                else
                {
                    Debug.LogError($"Failed to get Character component for NPC in tribe {tribeName}");
                }
                
                yield return null;
            }
            
            Debug.Log($"Finished creating tribe {tribeName} with {tribeMembers.Count} members");
            yield return null;
        }

        private Vector3 FindValidSpawnPosition(List<Character> existingMembers)
        {
            Vector3? campPos = islandGenerator?.GetCampPosition();
            if (!campPos.HasValue)
            {
                Debug.LogError("Camp position not found!");
                return Vector3.zero;
            }

            for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
            {
                Vector3 randomOffset = Random.insideUnitSphere * spawnRadius;
                randomOffset.y = 0; // Keep on the same Y level
                Vector3 spawnPos = campPos.Value + randomOffset;

                // Check distance from existing members
                bool tooClose = false;
                foreach (var member in existingMembers)
                {
                    if (Vector3.Distance(spawnPos, member.transform.position) < minSpawnDistance)
                    {
                        tooClose = true;
                        break;
                    }
                }

                if (!tooClose)
                {
                    return spawnPos;
                }
            }

            Debug.LogWarning("Could not find valid spawn position, using camp position");
            return campPos.Value;
        }

        public List<Character> GetTribeMembers(string tribeName)
        {
            Debug.Log($"[TribeManager] Getting members for tribe: {tribeName}");
            if (tribes.ContainsKey(tribeName))
            {
                Debug.Log($"[TribeManager] Found {tribes[tribeName].Count} members in tribe {tribeName}");
                return tribes[tribeName];
            }
            Debug.LogWarning($"[TribeManager] No tribe found with name: {tribeName}");
            return new List<Character>();
        }

        public Character GetPlayer()
        {
            return tribeMembers.FirstOrDefault(member => member.IsPlayer);
        }

        public void EliminateMember(Character member)
        {
            if (member != null)
            {
                member.gameObject.SetActive(false);
                tribeMembers.Remove(member);
                foreach (var tribe in tribes.Values)
                {
                    tribe.Remove(member);
                }
                
                // Update UI if this affects the current tribe
                UpdateTribeInfoUI();
            }
        }

        public Character GetNPCData(string npcName)
        {
            return tribeMembers.FirstOrDefault(member => member.CharacterName == npcName);
        }

        public void StartChallenge(List<Character> participants)
        {
            foreach (var participant in participants)
            {
                if (participant != null)
                {
                    participant.IsInChallenge = true;
                }
            }
        }

        public void EliminateParticipant(Character member)
        {
            if (member != null)
            {
                member.IsInChallenge = false;
                EliminateMember(member);
            }
        }

        public bool IsParticipantInChallenge(Character member)
        {
            return member != null && member.IsInChallenge;
        }

        public List<Character> GetActiveParticipants()
        {
            return tribeMembers.Where(member => member != null && member.IsInChallenge).ToList();
        }

        public Character GetTribeMember(string memberName)
        {
            return tribeMembers.FirstOrDefault(member => member.CharacterName == memberName);
        }

        public List<Character> GetAllTribeMembers()
        {
            return new List<Character>(tribeMembers);
        }

        public void AddTribeMember(Character member)
        {
            if (member != null && !tribeMembers.Contains(member))
            {
                tribeMembers.Add(member);
                
                // Update UI if this affects the current tribe
                UpdateTribeInfoUI();
            }
        }

        public void RemoveTribeMember(string memberName)
        {
            var member = GetTribeMember(memberName);
            if (member != null)
            {
                tribeMembers.Remove(member);
                
                // Update UI if this affects the current tribe
                UpdateTribeInfoUI();
            }
        }

        List<string> INPCManager.GetActiveNPCs()
        {
            return tribeMembers
                .Where(member => member != null && member.gameObject.activeInHierarchy)
                .Select(member => member.CharacterName)
                .ToList();
        }

        IEnumerable<string> ITribeManager.GetActiveNPCs()
        {
            return tribeMembers
                .Where(member => member != null && member.gameObject.activeInHierarchy)
                .Select(member => member.CharacterName);
        }

        public void UpdateRelationship(string npc1Name, string npc2Name, float delta)
        {
            var npc1 = GetTribeMember(npc1Name);
            var npc2 = GetTribeMember(npc2Name);

            if (npc1 != null && npc2 != null)
            {
                npc1.UpdateRelationship(npc2Name, delta);
                npc2.UpdateRelationship(npc1Name, delta);
            }
        }

        public bool CanStartChallenge(Vector3 position)
        {
            var nearbyNPCs = tribeMembers
                .Where(member => member != null && 
                               member.gameObject.activeInHierarchy && 
                               Vector3.Distance(member.transform.position, position) <= spawnRadius)
                .ToList();

            return nearbyNPCs.Count >= 2;
        }

        public void SpawnTribeNPCs(string tribeName)
        {
            if (!tribes.ContainsKey(tribeName))
            {
                Debug.LogError($"Tribe {tribeName} does not exist!");
                return;
            }

            foreach (Character member in tribes[tribeName])
            {
                if (!member.IsPlayer && !member.gameObject.activeInHierarchy)
                {
                    Vector3 spawnPosition = FindValidSpawnPosition(tribes[tribeName]);
                    member.transform.position = spawnPosition;
                    
                    // Ensure NPC has all necessary components for wandering
                    EnsureNPCComponents(member.gameObject);
                    
                    member.gameObject.SetActive(true);
                    Debug.Log($"Spawned tribe member {member.CharacterName} at position {spawnPosition}");
                }
            }
        }

        /// <summary>
        /// Ensures that an NPC GameObject has all necessary components for wandering behavior
        /// </summary>
        private void EnsureNPCComponents(GameObject npcObject)
        {
            // Add NavMeshAgent if not present
            NavMeshAgent navAgent = npcObject.GetComponent<NavMeshAgent>();
            if (navAgent == null)
            {
                navAgent = npcObject.AddComponent<NavMeshAgent>();
                navAgent.radius = 0.5f;
                navAgent.height = 2f;
                navAgent.baseOffset = 0f;
                navAgent.speed = 2f;
                navAgent.angularSpeed = 120f;
                navAgent.acceleration = 8f;
                navAgent.stoppingDistance = 0.5f;
                Debug.Log($"Added NavMeshAgent to {npcObject.name}");
            }

            // Add NPCBehavior component if not present
            Survivor.Characters.NPCBehavior npcBehavior = npcObject.GetComponent<Survivor.Characters.NPCBehavior>();
            if (npcBehavior == null)
            {
                npcBehavior = npcObject.AddComponent<Survivor.Characters.NPCBehavior>();
                Debug.Log($"Added NPCBehavior to {npcObject.name}");
            }

            // Set up animator for NPCBehavior
            Animator animator = npcObject.GetComponent<Animator>();
            if (animator != null)
            {
                npcBehavior.animator = animator;
                Debug.Log($"Assigned animator to NPCBehavior for {npcObject.name}");
            }
            else
            {
                Debug.LogWarning($"No Animator component found on {npcObject.name}, NPCBehavior will not animate");
            }

            // Add CharacterController if not present (required for NPCBehavior)
            UnityEngine.CharacterController characterController = npcObject.GetComponent<UnityEngine.CharacterController>();
            if (characterController == null)
            {
                characterController = npcObject.AddComponent<UnityEngine.CharacterController>();
                characterController.height = 2f;
                characterController.radius = 0.5f;
                characterController.stepOffset = 0.3f;
                Debug.Log($"Added CharacterController to {npcObject.name}");
            }

            // Add collider if not present
            Collider collider = npcObject.GetComponent<Collider>();
            if (collider == null)
            {
                CapsuleCollider capsuleCollider = npcObject.AddComponent<CapsuleCollider>();
                capsuleCollider.height = 2f;
                capsuleCollider.radius = 0.5f;
                capsuleCollider.center = new Vector3(0, 1f, 0);
                Debug.Log($"Added CapsuleCollider to {npcObject.name}");
            }

            // Configure NPC for NavMeshAgent movement (disable conflicting components)
            ConfigureNPCForNavMeshMovement(npcObject, npcObject.name);
        }

        /// <summary>
        /// Updates the tribe info UI with current data
        /// </summary>
        public void UpdateTribeInfoUI()
        {
            if (tribeInfoMenuController == null)
            {
                Debug.LogWarning("[TribeManager] TribeInfoMenuController not available for UI update");
                return;
            }

            var player = GetPlayer();
            if (player == null)
            {
                Debug.LogWarning("[TribeManager] No player found for UI update");
                return;
            }

            var tribeMembers = GetTribeMembers(player.TribeName);
            tribeInfoMenuController.SetTribeInfo(player.TribeName, tribeMembers);
        }

        /// <summary>
        /// Shows the tribe info menu with current data
        /// </summary>
        public void ShowTribeInfoMenu()
        {
            if (tribeInfoMenuController == null)
            {
                Debug.LogWarning("[TribeManager] TribeInfoMenuController not available");
                return;
            }

            var player = GetPlayer();
            if (player == null)
            {
                Debug.LogWarning("[TribeManager] No player found for tribe info menu");
                return;
            }

            var tribeMembers = GetTribeMembers(player.TribeName);
            tribeInfoMenuController.SetTribeInfo(player.TribeName, tribeMembers);
            tribeInfoMenuController.ToggleMenu();
        }

        /// <summary>
        /// Sets the UI controller reference
        /// </summary>
        /// <param name="uiController">The TribeInfoMenuController to use</param>
        public void SetUIController(TribeInfoMenuController uiController)
        {
            tribeInfoMenuController = uiController;
            Debug.Log("[TribeManager] UI Controller set");
        }

        private void ConfigureNPCForNavMeshMovement(GameObject npcObject, string memberName)
        {
            Debug.Log($"Configuring NPC {memberName} for NavMeshAgent movement");

            // Disable ThirdPersonController (conflicts with NavMeshAgent)
            MonoBehaviour thirdPersonController = npcObject.GetComponent<MonoBehaviour>();
            if (thirdPersonController != null && thirdPersonController.GetType().Name.Contains("ThirdPersonController"))
            {
                thirdPersonController.enabled = false;
                Debug.Log($"Disabled ThirdPersonController on {memberName}");
            }

            // Disable PlayerInput component (not needed for NPCs)
            var playerInput = npcObject.GetComponent("UnityEngine.InputSystem.PlayerInput");
            if (playerInput != null)
            {
                var enabledProperty = playerInput.GetType().GetProperty("enabled");
                if (enabledProperty != null)
                {
                    enabledProperty.SetValue(playerInput, false);
                    Debug.Log($"Disabled PlayerInput on {memberName}");
                }
            }

            // Disable StarterAssets.ThirdPersonController if it exists
            var starterAssetsController = npcObject.GetComponent("StarterAssets.ThirdPersonController");
            if (starterAssetsController != null)
            {
                var enabledProperty = starterAssetsController.GetType().GetProperty("enabled");
                if (enabledProperty != null)
                {
                    enabledProperty.SetValue(starterAssetsController, false);
                    Debug.Log($"Disabled StarterAssets.ThirdPersonController on {memberName}");
                }
            }

            // Disable any other movement controllers that might conflict
            var movementControllers = npcObject.GetComponents<MonoBehaviour>();
            foreach (var controller in movementControllers)
            {
                string controllerName = controller.GetType().Name.ToLower();
                if (controllerName.Contains("controller") && 
                    !controllerName.Contains("npc") && 
                    !controllerName.Contains("behavior") &&
                    controller.enabled)
                {
                    controller.enabled = false;
                    Debug.Log($"Disabled conflicting controller {controller.GetType().Name} on {memberName}");
                }
            }

            // Ensure NavMeshAgent is properly configured
            NavMeshAgent navAgent = npcObject.GetComponent<NavMeshAgent>();
            if (navAgent != null)
            {
                navAgent.enabled = true;
                navAgent.updatePosition = true;
                navAgent.updateRotation = true;
                navAgent.updateUpAxis = false;
                Debug.Log($"Configured NavMeshAgent on {memberName}");
            }

            // Ensure CharacterController doesn't interfere with NavMeshAgent
            UnityEngine.CharacterController characterController = npcObject.GetComponent<UnityEngine.CharacterController>();
            if (characterController != null)
            {
                // Keep CharacterController for collision detection but don't use it for movement
                characterController.enabled = true;
                Debug.Log($"Kept CharacterController for collision on {memberName}");
            }

            Debug.Log($"Finished configuring {memberName} for NavMeshAgent movement");
        }
    }
} 