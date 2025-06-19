using UnityEngine;
using System.Collections.Generic;
using StarterAssets;

namespace Survivor.Environment
{
    public class WaterPhysicsSystem : MonoBehaviour
    {
        [Header("Water Properties")]
        public float waterHeight = 0f;
        public float buoyancyForce = 3f;
        public float dragForce = 1f;
        public float swimThreshold = 0.67f; // 2/3 submerged threshold

        [Header("Underwater Visual Effects")]
        public bool enableUnderwaterEffects = true;
        public bool useAdvancedEffects = false; // Use URP effects if true, simple effects if false
        public Color underwaterTint = new Color(0.2f, 0.4f, 0.8f, 0.3f);
        public float underwaterBlur = 0.5f;
        public float underwaterDistortion = 0.1f;
        public float effectTransitionSpeed = 2f;
        public float effectIntensity = 1f;

        [Header("Fish System")]
        public GameObject fishPrefab;
        public int numberOfFish = 20;
        public float fishSpawnRadius = 50f;
        public float fishMinDepth = -5f;
        public float fishMaxDepth = -1f;
        public float fishMinSpeed = 2f;
        public float fishMaxSpeed = 4f;

        [Header("Water Settings")]
        public float height = 0f;
        public float waveHeight = 0.5f;
        public float waveSpeed = 1f;
        public float waveScale = 1f;

        [Header("Swimming Settings")]
        public float swimSpeed = 3f;
        public float swimGravity = -2f;
        public float normalGravity = -15f;

        private List<Fish> activeFish = new List<Fish>();
        private List<StarterAssets.ThirdPersonController> playersInWater = new List<StarterAssets.ThirdPersonController>();
        private List<Rigidbody> rigidbodiesInWater = new List<Rigidbody>();
        private IUnderwaterEffect underwaterEffect;

        private void Start()
        {
            if (fishPrefab != null)
            {
                SpawnFish();
            }
            
            // Initialize water system
            transform.position = new Vector3(0, height, 0);
            
            // Set up water volume collider
            SetupWaterVolume();
            
            // Set up underwater effects
            SetupUnderwaterEffects();
        }

        private void SetupWaterVolume()
        {
            // Remove existing collider if any
            Collider existingCollider = GetComponent<Collider>();
            if (existingCollider != null)
            {
                DestroyImmediate(existingCollider);
            }

            // Add a box collider that covers the water volume
            BoxCollider waterCollider = gameObject.AddComponent<BoxCollider>();
            waterCollider.isTrigger = true;
            
            // Set the size to cover the water volume (adjust based on your needs)
            float waterDepth = 10f; // How deep the water goes
            waterCollider.size = new Vector3(100f, waterDepth, 100f); // Large volume
            waterCollider.center = new Vector3(0, -waterDepth/2f, 0); // Center at water surface
        }

        private void SetupUnderwaterEffects()
        {
            if (!enableUnderwaterEffects) return;
            
            if (useAdvancedEffects)
            {
                // Use advanced effects (now also works with Built-in RP)
                underwaterEffect = FindObjectOfType<UnderwaterEffect>();
                if (underwaterEffect == null)
                {
                    GameObject effectObject = new GameObject("UnderwaterEffect");
                    underwaterEffect = effectObject.AddComponent<UnderwaterEffect>();
                    DontDestroyOnLoad(effectObject);
                }
            }
            else
            {
                // Use simple effects (works with Built-in Render Pipeline)
                underwaterEffect = FindObjectOfType<SimpleUnderwaterEffect>();
                if (underwaterEffect == null)
                {
                    GameObject effectObject = new GameObject("SimpleUnderwaterEffect");
                    underwaterEffect = effectObject.AddComponent<SimpleUnderwaterEffect>();
                    DontDestroyOnLoad(effectObject);
                }
            }
            
            // Configure the effect
            underwaterEffect.underwaterTint = underwaterTint;
            underwaterEffect.underwaterBlur = underwaterBlur;
            underwaterEffect.underwaterDistortion = underwaterDistortion;
            underwaterEffect.transitionSpeed = effectTransitionSpeed;
            underwaterEffect.effectIntensity = effectIntensity;
        }

        private void SpawnFish()
        {
            for (int i = 0; i < numberOfFish; i++)
            {
                Vector3 randomPosition = transform.position + new Vector3(
                    Random.Range(-fishSpawnRadius, fishSpawnRadius),
                    Random.Range(fishMinDepth, fishMaxDepth),
                    Random.Range(-fishSpawnRadius, fishSpawnRadius)
                );

                GameObject fishObject = Instantiate(fishPrefab, randomPosition, Random.rotation);
                Fish fish = fishObject.GetComponent<Fish>();
                if (fish != null)
                {
                    fish.Initialize(
                        Random.Range(fishMinSpeed, fishMaxSpeed),
                        fishSpawnRadius,
                        fishMinDepth,
                        fishMaxDepth
                    );
                    activeFish.Add(fish);
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // Handle CharacterController-based players
            StarterAssets.ThirdPersonController player = other.GetComponent<StarterAssets.ThirdPersonController>();
            if (player != null && !playersInWater.Contains(player))
            {
                playersInWater.Add(player);
                OnPlayerEnterWater(player);
                return;
            }

            // Handle Rigidbody objects
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null && !rigidbodiesInWater.Contains(rb))
            {
                rigidbodiesInWater.Add(rb);
                OnRigidbodyEnterWater(rb);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            // Handle CharacterController-based players
            StarterAssets.ThirdPersonController player = other.GetComponent<StarterAssets.ThirdPersonController>();
            if (player != null && playersInWater.Contains(player))
            {
                playersInWater.Remove(player);
                OnPlayerExitWater(player);
                return;
            }

            // Handle Rigidbody objects
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null && rigidbodiesInWater.Contains(rb))
            {
                rigidbodiesInWater.Remove(rb);
                OnRigidbodyExitWater(rb);
            }
        }

        private void OnPlayerEnterWater(StarterAssets.ThirdPersonController player)
        {
            Debug.Log("[WaterPhysicsSystem] Player entered water");
            
            // Store original gravity
            if (player.GetType().GetField("Gravity", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(player) is float originalGravity)
            {
                player.GetType().GetField("_originalGravity", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(player, originalGravity);
            }
        }

        private void OnPlayerExitWater(StarterAssets.ThirdPersonController player)
        {
            Debug.Log("[WaterPhysicsSystem] Player exited water");
            
            // Restore original gravity
            var originalGravityField = player.GetType().GetField("_originalGravity", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (originalGravityField?.GetValue(player) is float originalGravity)
            {
                player.GetType().GetField("Gravity", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(player, originalGravity);
            }
        }

        private void OnRigidbodyEnterWater(Rigidbody rb)
        {
            // Store original drag values
            rb.gameObject.AddComponent<WaterPhysicsData>().StoreOriginalValues(rb);
        }

        private void OnRigidbodyExitWater(Rigidbody rb)
        {
            // Restore original drag values
            WaterPhysicsData waterData = rb.GetComponent<WaterPhysicsData>();
            if (waterData != null)
            {
                waterData.RestoreOriginalValues(rb);
                Destroy(waterData);
            }
        }

        private void Update()
        {
            // Update water animation
            UpdateWaterAnimation();
            
            // Handle players in water
            foreach (StarterAssets.ThirdPersonController player in playersInWater)
            {
                if (player != null)
                {
                    HandlePlayerInWater(player);
                }
            }
            
            // Handle rigidbodies in water
            foreach (Rigidbody rb in rigidbodiesInWater)
            {
                if (rb != null)
                {
                    HandleRigidbodyInWater(rb);
                }
            }
        }

        private void HandlePlayerInWater(StarterAssets.ThirdPersonController player)
        {
            if (player == null) return;

            // Get the water volume bounds
            BoxCollider waterCollider = GetComponent<BoxCollider>();
            if (waterCollider == null) return;

            // Calculate player's position relative to the water volume
            Vector3 playerPos = player.transform.position;
            Vector3 waterVolumePos = transform.position;
            Vector3 waterVolumeSize = waterCollider.size;
            Vector3 waterVolumeCenter = waterCollider.center;
            
            // Calculate the actual water volume bounds in world space
            Vector3 waterVolumeMin = waterVolumePos + waterVolumeCenter - (waterVolumeSize * 0.5f);
            Vector3 waterVolumeMax = waterVolumePos + waterVolumeCenter + (waterVolumeSize * 0.5f);

            // Check if player is within the water volume bounds
            bool isInWaterVolume = playerPos.x >= waterVolumeMin.x && playerPos.x <= waterVolumeMax.x &&
                                  playerPos.y >= waterVolumeMin.y && playerPos.y <= waterVolumeMax.y &&
                                  playerPos.z >= waterVolumeMin.z && playerPos.z <= waterVolumeMax.z;

            if (!isInWaterVolume) return;

            // Calculate submersion based on distance from water surface
            float playerHeight = player.GetComponent<CharacterController>().height;
            float playerBottom = player.transform.position.y - playerHeight / 2f;
            float playerTop = player.transform.position.y + playerHeight / 2f;
            float waterSurface = waterHeight;

            // Calculate submersion percentage
            float submergedHeight = Mathf.Max(0, waterSurface - playerBottom);
            float totalHeight = playerTop - playerBottom;
            float submersionPercentage = Mathf.Clamp01(submergedHeight / totalHeight);

            // For deep water, if player is below water surface, consider them fully submerged
            if (playerPos.y < waterSurface)
            {
                submersionPercentage = Mathf.Max(submersionPercentage, 0.8f); // At least 80% submerged when below surface
            }

            // Update underwater effects
            if (enableUnderwaterEffects && underwaterEffect != null)
            {
                underwaterEffect.SetSubmersionLevel(submersionPercentage);
                
                // Debug information
                if (Debug.isDebugBuild)
                {
                    Debug.Log($"[WaterSystem] Player at Y: {playerPos.y}, Water Surface: {waterSurface}, Submersion: {submersionPercentage:P0}, Effects: {(submersionPercentage > 0.1f ? "Active" : "Inactive")}");
                }
            }

            // Check if player should be swimming (more than 2/3 submerged)
            bool shouldSwim = submersionPercentage >= swimThreshold;

            // Apply swimming physics
            if (shouldSwim)
            {
                ApplySwimmingPhysics(player, submersionPercentage);
            }
            else
            {
                ApplyWadingPhysics(player, submersionPercentage);
            }
        }

        private void ApplySwimmingPhysics(StarterAssets.ThirdPersonController player, float submersionPercentage)
        {
            // Set swimming gravity
            var gravityField = player.GetType().GetField("Gravity", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (gravityField != null)
            {
                gravityField.SetValue(player, swimGravity);
            }

            // Get input for swimming
            StarterAssets.StarterAssetsInputs input = player.GetComponent<StarterAssets.StarterAssetsInputs>();
            if (input != null)
            {
                // Apply swimming movement
                Vector3 swimDirection = new Vector3(input.move.x, 0, input.move.y).normalized;
                if (swimDirection.magnitude > 0.1f)
                {
                    // Apply swimming force to the character controller
                    CharacterController controller = player.GetComponent<CharacterController>();
                    Vector3 swimVelocity = swimDirection * swimSpeed;
                    
                    // Add some upward force when swimming
                    if (input.jump)
                    {
                        swimVelocity.y = swimSpeed * 0.5f;
                    }
                    
                    controller.Move(swimVelocity * Time.deltaTime);
                }
            }
        }

        private void ApplyWadingPhysics(StarterAssets.ThirdPersonController player, float submersionPercentage)
        {
            // Apply reduced gravity based on submersion
            var gravityField = player.GetType().GetField("Gravity", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (gravityField != null)
            {
                float reducedGravity = Mathf.Lerp(normalGravity, swimGravity, submersionPercentage);
                gravityField.SetValue(player, reducedGravity);
            }

            // Apply water resistance to movement
            StarterAssets.StarterAssetsInputs input = player.GetComponent<StarterAssets.StarterAssetsInputs>();
            if (input != null)
            {
                // Reduce movement speed based on submersion
                float waterResistance = 1f - (submersionPercentage * 0.5f);
                
                // Apply resistance to the player's movement speed
                var moveSpeedField = player.GetType().GetField("MoveSpeed", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                var sprintSpeedField = player.GetType().GetField("SprintSpeed", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                
                if (moveSpeedField != null)
                {
                    float originalMoveSpeed = (float)moveSpeedField.GetValue(player);
                    moveSpeedField.SetValue(player, originalMoveSpeed * waterResistance);
                }
                
                if (sprintSpeedField != null)
                {
                    float originalSprintSpeed = (float)sprintSpeedField.GetValue(player);
                    sprintSpeedField.SetValue(player, originalSprintSpeed * waterResistance);
                }
            }
        }

        private void HandleRigidbodyInWater(Rigidbody rb)
        {
            if (rb == null) return;

            // Get bounds from the collider, not the rigidbody
            Collider collider = rb.GetComponent<Collider>();
            if (collider == null) return;

            float submergedDepth = Mathf.Clamp01(
                (waterHeight - rb.transform.position.y) / collider.bounds.size.y
            );

            // Apply buoyancy force
            Vector3 buoyancy = Vector3.up * buoyancyForce * submergedDepth;
            rb.AddForce(buoyancy, ForceMode.Acceleration);

            // Apply drag
            rb.drag = dragForce * submergedDepth;
            rb.angularDrag = dragForce * submergedDepth;
        }

        private void UpdateWaterAnimation()
        {
            // Simple wave animation for the water surface
            if (waveHeight > 0)
            {
                Vector3 position = transform.position;
                position.y = height + Mathf.Sin(Time.time * waveSpeed) * waveHeight * 0.5f;
                transform.position = position;
            }
        }

        // Public method to get water height at a specific position
        public float GetWaterHeightAtPosition(Vector3 position)
        {
            return waterHeight + Mathf.Sin((position.x + position.z) * waveScale + Time.time * waveSpeed) * waveHeight * 0.5f;
        }

        // Public method to check if a position is underwater
        public bool IsUnderwater(Vector3 position)
        {
            return position.y < GetWaterHeightAtPosition(position);
        }

        // Public method to get submersion percentage for any object
        public float GetSubmersionPercentage(Vector3 position, float objectHeight)
        {
            float waterSurface = GetWaterHeightAtPosition(position);
            float objectBottom = position.y - objectHeight / 2f;
            float objectTop = position.y + objectHeight / 2f;
            
            float submergedHeight = Mathf.Max(0, waterSurface - objectBottom);
            float totalHeight = objectTop - objectBottom;
            
            return Mathf.Clamp01(submergedHeight / totalHeight);
        }

        private void OnDrawGizmos()
        {
            // Draw water volume bounds for debugging
            BoxCollider waterCollider = GetComponent<BoxCollider>();
            if (waterCollider != null)
            {
                Gizmos.color = new Color(0.2f, 0.5f, 0.8f, 0.3f);
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawCube(waterCollider.center, waterCollider.size);
                
                // Draw water surface
                Gizmos.color = new Color(0.2f, 0.5f, 0.8f, 0.8f);
                Gizmos.DrawWireCube(new Vector3(0, waterHeight - transform.position.y, 0), new Vector3(waterCollider.size.x, 0.1f, waterCollider.size.z));
            }
        }
    }

    // Helper class to store original physics values
    public class WaterPhysicsData : MonoBehaviour
    {
        private float originalDrag;
        private float originalAngularDrag;

        public void StoreOriginalValues(Rigidbody rb)
        {
            originalDrag = rb.drag;
            originalAngularDrag = rb.angularDrag;
        }

        public void RestoreOriginalValues(Rigidbody rb)
        {
            rb.drag = originalDrag;
            rb.angularDrag = originalAngularDrag;
        }
    }

    public class Fish : MonoBehaviour
    {
        private float speed;
        private float boundaryRadius;
        private float minDepth;
        private float maxDepth;
        private Vector3 targetPosition;
        private float nextDirectionChange;

        public void Initialize(float speed, float boundaryRadius, float minDepth, float maxDepth)
        {
            this.speed = speed;
            this.boundaryRadius = boundaryRadius;
            this.minDepth = minDepth;
            this.maxDepth = maxDepth;
            SetNewTarget();
        }

        private void Update()
        {
            if (Time.time >= nextDirectionChange)
            {
                SetNewTarget();
            }

            // Move towards target
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                speed * Time.deltaTime
            );

            // Look at movement direction
            if ((targetPosition - transform.position).sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(targetPosition - transform.position),
                    Time.deltaTime * 4f
                );
            }
        }

        private void SetNewTarget()
        {
            targetPosition = new Vector3(
                Random.Range(-boundaryRadius, boundaryRadius),
                Random.Range(minDepth, maxDepth),
                Random.Range(-boundaryRadius, boundaryRadius)
            );
            nextDirectionChange = Time.time + Random.Range(3f, 8f);
        }
    }
} 