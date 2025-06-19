using UnityEngine;
using StarterAssets;
using Survivor.Environment;

namespace Survivor.Characters
{
    /// <summary>
    /// Automatically sets up swimming functionality for the player.
    /// Add this script to your player GameObject or call SetupSwimming() manually.
    /// </summary>
    public class PlayerSwimmingSetup : MonoBehaviour
    {
        [Header("Auto Setup")]
        public bool setupOnStart = true;
        public bool addSwimmingComponent = true;
        
        [Header("Swimming Configuration")]
        public float swimSpeed = 4f;
        public float swimUpSpeed = 3f;
        public float swimDownSpeed = 2f;
        public float swimThreshold = 0.67f;
        public float swimGravity = -2f;
        public float buoyancyForce = 2f;

        private void Start()
        {
            if (setupOnStart)
            {
                SetupSwimming();
            }
        }

        /// <summary>
        /// Sets up swimming functionality for the player
        /// </summary>
        public void SetupSwimming()
        {
            // Find the player if this script is not on the player
            GameObject player = gameObject;
            if (!HasPlayerComponents(gameObject))
            {
                player = FindPlayer();
                if (player == null)
                {
                    Debug.LogError("[PlayerSwimmingSetup] No player found with required components!");
                    return;
                }
            }

            // Add swimming component if requested
            if (addSwimmingComponent)
            {
                CharacterSwimming swimming = player.GetComponent<CharacterSwimming>();
                if (swimming == null)
                {
                    swimming = player.AddComponent<CharacterSwimming>();
                    Debug.Log("[PlayerSwimmingSetup] Added CharacterSwimming component to player");
                }

                // Configure swimming settings
                swimming.swimSpeed = swimSpeed;
                swimming.swimUpSpeed = swimUpSpeed;
                swimming.swimDownSpeed = swimDownSpeed;
                swimming.swimThreshold = swimThreshold;
                swimming.swimGravity = swimGravity;
                swimming.buoyancyForce = buoyancyForce;
            }

            // Ensure player has required components
            EnsureRequiredComponents(player);

            Debug.Log("[PlayerSwimmingSetup] Swimming setup complete!");
        }

        /// <summary>
        /// Checks if the GameObject has the required player components
        /// </summary>
        private bool HasPlayerComponents(GameObject obj)
        {
            return obj.GetComponent<StarterAssets.ThirdPersonController>() != null &&
                   obj.GetComponent<CharacterController>() != null &&
                   obj.GetComponent<StarterAssets.StarterAssetsInputs>() != null;
        }

        /// <summary>
        /// Finds the player GameObject in the scene
        /// </summary>
        private GameObject FindPlayer()
        {
            // Try to find by tag first
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null && HasPlayerComponents(player))
            {
                return player;
            }

            // Try to find by name
            player = GameObject.Find("Player");
            if (player != null && HasPlayerComponents(player))
            {
                return player;
            }

            // Try to find by component
            StarterAssets.ThirdPersonController controller = FindObjectOfType<StarterAssets.ThirdPersonController>();
            if (controller != null)
            {
                return controller.gameObject;
            }

            return null;
        }

        /// <summary>
        /// Ensures the player has all required components for swimming
        /// </summary>
        private void EnsureRequiredComponents(GameObject player)
        {
            // Check for ThirdPersonController
            if (player.GetComponent<StarterAssets.ThirdPersonController>() == null)
            {
                Debug.LogWarning("[PlayerSwimmingSetup] Player missing ThirdPersonController component!");
            }

            // Check for CharacterController
            if (player.GetComponent<CharacterController>() == null)
            {
                Debug.LogWarning("[PlayerSwimmingSetup] Player missing CharacterController component!");
            }

            // Check for StarterAssetsInputs
            if (player.GetComponent<StarterAssets.StarterAssetsInputs>() == null)
            {
                Debug.LogWarning("[PlayerSwimmingSetup] Player missing StarterAssetsInputs component!");
            }

            // Check for Animator (optional but recommended)
            if (player.GetComponent<Animator>() == null)
            {
                Debug.LogWarning("[PlayerSwimmingSetup] Player missing Animator component - swimming animations won't work!");
            }
        }

        /// <summary>
        /// Creates a water volume at the specified position
        /// </summary>
        public static GameObject CreateWaterVolume(Vector3 position, Vector3 size, float waterHeight)
        {
            GameObject waterVolume = new GameObject("WaterVolume");
            waterVolume.transform.position = position;

            // Add required components
            BoxCollider collider = waterVolume.AddComponent<BoxCollider>();
            WaterVolume waterVolumeComponent = waterVolume.AddComponent<WaterVolume>();

            // Configure the water volume
            waterVolumeComponent.volumeSize = size;
            waterVolumeComponent.waterHeight = waterHeight;

            Debug.Log($"[PlayerSwimmingSetup] Created water volume at {position}");
            return waterVolume;
        }

        /// <summary>
        /// Creates a simple water volume for testing
        /// </summary>
        [ContextMenu("Create Test Water Volume")]
        public void CreateTestWaterVolume()
        {
            Vector3 position = transform.position + Vector3.forward * 10f;
            CreateWaterVolume(position, new Vector3(20f, 5f, 20f), 0f);
        }

        /// <summary>
        /// Removes swimming component from player
        /// </summary>
        [ContextMenu("Remove Swimming")]
        public void RemoveSwimming()
        {
            GameObject player = FindPlayer();
            if (player != null)
            {
                CharacterSwimming swimming = player.GetComponent<CharacterSwimming>();
                if (swimming != null)
                {
                    DestroyImmediate(swimming);
                    Debug.Log("[PlayerSwimmingSetup] Removed CharacterSwimming component from player");
                }
            }
        }
    }
} 