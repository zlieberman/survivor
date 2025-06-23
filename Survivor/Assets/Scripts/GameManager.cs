using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Survivor.Generation;
using UnityEngine.InputSystem;
using UnityEngine.AI;
using System.Collections;
using Survivor.Characters;
using Survivor.Characters.UI;
using Survivor.Shared.Interfaces;
using Survivor.Shared;
using Survivor.UI;
using Survivor.Interactables;
using Survivor.Core.Interaction;
using Survivor.Core;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Player Settings")]
    public GameObject playerPrefab;
    public GameObject npcPrefab;

    [Header("Prefabs")]
    [SerializeField] private GameObject dialogueUIPrefab;

    [Header("Managers")]
    [SerializeField] private DialogueManager dialogueManager;

    private CharacterStats mainPlayer;
    private ProceduralIslandGenerator islandGenerator;
    private CampGenerator campGenerator;
    private bool isGameInitialized = false;
    private bool isInitializing = false;

    private void Awake()
    {
        Debug.Log("GameManager Awake called");
        
        // Check if this is the first instance
        if (Instance == null)
        {
            Debug.Log("First GameManager instance - setting up singleton");
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Initialize immediately if we're starting in the TropicalIsland scene
            if (SceneManager.GetActiveScene().name == "TropicalIsland")
            {
                Debug.Log("Starting in TropicalIsland scene - initializing immediately");
                InitializeGame();
            }
            InitializeDialogueSystem();
        }
        else
        {
            Debug.Log("GameManager instance already exists - destroying duplicate");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Debug.Log("GameManager Start called");
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        Debug.Log("GameManager OnDestroy called");
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void StartGame()
    {
        Debug.Log("StartGame called");
        
        if (isInitializing)
        {
            Debug.LogWarning("Game initialization already in progress!");
            return;
        }

        if (isGameInitialized)
        {
            Debug.LogWarning("Game already initialized!");
            return;
        }

        isInitializing = true;
        Debug.Log("Starting game initialization");

        // Initialize game
        InitializeGame();
        
        // Load the TropicalIsland scene
        Debug.Log("Loading TropicalIsland scene");
        SceneManager.LoadScene("TropicalIsland");
    }

    private void InitializeGame()
    {
        Debug.Log("Initializing game");
        
        // Create main player stats
        mainPlayer = new CharacterStats();
        mainPlayer.perception = Random.Range(50, 101);
        mainPlayer.deception = Random.Range(50, 101);
        mainPlayer.persuasion = Random.Range(50, 101);
        mainPlayer.puzzleSolving = Random.Range(50, 101);
        mainPlayer.swimming = Random.Range(50, 101);
        mainPlayer.speed = Random.Range(50, 101);
        mainPlayer.strength = Random.Range(50, 101);
        mainPlayer.agility = Random.Range(50, 101);
        mainPlayer.intelligence = Random.Range(50, 101);
        mainPlayer.stamina = Random.Range(50, 101);
        mainPlayer.charisma = Random.Range(50, 101);
        mainPlayer.honesty = Random.Range(50, 101);
        mainPlayer.trust = Random.Range(50, 101);
        mainPlayer.honor = Random.Range(50, 101);
        Debug.Log("Main player stats created");

        isGameInitialized = true;
        isInitializing = false;
        Debug.Log("Game initialization complete");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Scene loaded: {scene.name}");
        if (scene.name == "TropicalIsland")
        {
            Debug.Log("TropicalIsland scene loaded, starting island generation wait");
            
            // Ensure game is initialized
            if (!isGameInitialized)
            {
                Debug.LogWarning("Game not initialized - initializing now");
                InitializeGame();
            }
            
            StartCoroutine(WaitForIslandGeneration());
        }
    }

    private IEnumerator WaitForIslandGeneration()
    {
        Debug.Log("Starting WaitForIslandGeneration coroutine");

        // Find the island generator
        ProceduralIslandGenerator islandGenerator = FindObjectOfType<ProceduralIslandGenerator>();
        if (islandGenerator == null)
        {
            Debug.LogError("Could not find ProceduralIslandGenerator in scene!");
            yield break;
        }
        Debug.Log("Found ProceduralIslandGenerator");

        // Wait for island generation to complete
        while (!islandGenerator.IsGenerationComplete())
        {
            Debug.Log("Waiting for island generation to complete");
            yield return new WaitForSeconds(0.5f);
        }
        Debug.Log("Island generation complete");

        // Find the camp generator
        CampGenerator campGenerator = FindObjectOfType<CampGenerator>();
        if (campGenerator == null)
        {
            Debug.LogError("Could not find CampGenerator in scene!");
            yield break;
        }
        Debug.Log("Found CampGenerator");

        // Wait for camp placement
        while (!campGenerator.CampPlaced)
        {
            Debug.Log("Waiting for camp placement");
            yield return new WaitForSeconds(0.5f);
        }
        Debug.Log("Camp placement complete");

        // Spawn player
        SpawnPlayer();
    }

    private Transform FindCampSpawnPointRecursive(Transform parent)
    {
        foreach (Transform child in parent)
        {
            if (child.name == "CampSpawnPoint")
            {
                return child;
            }

            Transform result = FindCampSpawnPointRecursive(child);
            if (result != null)
            {
                return result;
            }
        }
        return null;
    }

    private void SpawnPlayer()
    {
        if (!isGameInitialized)
        {
            Debug.LogError("Cannot spawn player - game not initialized!");
            return;
        }

        if (campGenerator == null)
        {
            Debug.LogError("Cannot spawn player - camp generator not found!");
            return;
        }

        Debug.Log($"Camp position: {campGenerator.transform.position}");
        Transform spawnPoint = campGenerator.transform.Find("CampSpawnPoint");
        if (spawnPoint == null)
        {
            // Try to find it recursively
            spawnPoint = FindCampSpawnPointRecursive(campGenerator.transform);
        }

        if (spawnPoint == null)
        {
            Debug.LogError("Camp spawn point not found!");
            return;
        }

        Debug.Log($"Found spawn point at: {spawnPoint.position}");

        // Spawn the main player
        if (playerPrefab != null)
        {
            Vector3 spawnPosition = spawnPoint.position;
            Debug.Log($"Attempting to spawn player at: {spawnPosition}");

            // Ensure the spawn position is above ground
            RaycastHit hit;
            if (Physics.Raycast(spawnPosition + Vector3.up * 10f, Vector3.down, out hit, 20f, LayerMask.GetMask("Default")))
            {
                spawnPosition.y = hit.point.y + 1f;
                Debug.Log($"Adjusted spawn position to: {spawnPosition}");
            }

            GameObject playerObject = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
            if (playerObject != null)
            {
                Debug.Log($"Player spawned successfully at: {playerObject.transform.position}");

                // Ensure the player has all required components
                UnityEngine.CharacterController controller = playerObject.GetComponent<UnityEngine.CharacterController>();
                if (controller == null)
                {
                    controller = playerObject.AddComponent<UnityEngine.CharacterController>();
                    controller.height = 2f;
                    controller.radius = 0.5f;
                    controller.stepOffset = 0.3f;
                    Debug.Log("Added CharacterController component to player");
                }

                ThirdPersonController thirdPersonController = playerObject.GetComponent<ThirdPersonController>();
                if (thirdPersonController == null)
                {
                    thirdPersonController = playerObject.AddComponent<ThirdPersonController>();
                    Debug.Log("Added ThirdPersonController component to player");
                }

                // Set up the player's camera
                GameObject mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
                if (mainCamera != null)
                {
                    CameraController cameraController = mainCamera.GetComponent<CameraController>();
                    if (cameraController != null)
                    {
                        cameraController.SetTarget(playerObject.transform);
                        Debug.Log("Set up camera to follow player");
                    }
                }

                // Set up the player's input
                PlayerInput playerInput = playerObject.GetComponent<PlayerInput>();
                if (playerInput == null)
                {
                    playerInput = playerObject.AddComponent<PlayerInput>();
                    playerInput.actions = Resources.Load<InputActionAsset>("PlayerInputActions");
                    Debug.Log("Added PlayerInput component to player");
                }
            }
            else
            {
                Debug.LogError("Failed to instantiate player prefab!");
            }
        }
        else
        {
            Debug.LogError("Player prefab not assigned!");
        }
    }

    private void InitializeDialogueSystem()
    {
        // Create dialogue UI if it doesn't exist
        if (dialogueUIPrefab != null && FindObjectOfType<DialogueUI>() == null)
        {
            GameObject dialogueUI = Instantiate(dialogueUIPrefab);
            dialogueUI.name = "DialogueUI";
            DontDestroyOnLoad(dialogueUI);
        }

        // Create dialogue manager if it doesn't exist
        if (dialogueManager == null)
        {
            GameObject managerObj = new GameObject("DialogueManager");
            dialogueManager = managerObj.AddComponent<DialogueManager>();
            DontDestroyOnLoad(managerObj);
        }
        
        // Note: InteractableManager should be created manually in the scene
        // Use the InteractableManagerCreator component if needed
    }
} 