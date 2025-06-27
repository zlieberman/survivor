using UnityEngine;
using StarterAssets;
using Crest;

namespace Survivor.Characters
{
    [RequireComponent(typeof(CharacterController))]
    public class CharacterSwimming : MonoBehaviour
    {
        [Header("Swimming Settings")]
        [Tooltip("Speed multiplier when in water (0.5 = half speed)")]
        public float waterSpeedMultiplier = 0.7f;
        [Tooltip("Speed multiplier when swimming (fully submerged)")]
        public float swimSpeedMultiplier = 0.5f;
        [Tooltip("Gravity when swimming")]
        public float swimGravity = -2f;
        [Tooltip("Depth in units to start swimming")]
        public float swimDepthThreshold = 0.9f;

        [Header("Crest Water Interaction")]
        [Tooltip("Enable Crest water interaction for foam and wakes")]
        public bool enableCrestInteraction = true;
        [Tooltip("Strength of water interaction (affects foam/wake intensity)")]
        [UnityEngine.Range(0f, 2f)]
        public float interactionStrength = 1f;
        [Tooltip("Size of the interaction area")]
        public float interactionRadius = 1f;

        [Header("Water Detection")]
        [Tooltip("Layer mask for water objects (fallback if Crest not available)")]
        public LayerMask waterLayer = 1;
        [Tooltip("Show debug information")]
        public bool showDebug = false;

        private CharacterController characterController;
        private MonoBehaviour thirdPersonController;
        private Animator animator;
        
        // Original values to restore when leaving water
        private float originalGravity;
        private float originalMoveSpeed;
        private float originalSprintSpeed;
        
        // Water state
        private bool isInWater = false;
        private bool isSwimming = false;
        private float waterHeight = 0f;
        private float submersionDepth = 0f;

        // Crest water interaction
        private SphereWaterInteraction waterInteraction;
        private ObjectWaterInteractionAdaptor waterAdaptor;
        private SampleHeightHelper sampleHeightHelper;

        // Animation
        private int isSwimmingHash;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            animator = GetComponent<Animator>();
            
            // Find ThirdPersonController (try both types)
            thirdPersonController = GetComponent<StarterAssets.ThirdPersonController>();
            if (thirdPersonController == null)
            {
                thirdPersonController = GetComponent<ThirdPersonController>();
            }

            // Setup animation hash
            isSwimmingHash = Animator.StringToHash("IsSwimming");

            // Initialize Crest water detection
            sampleHeightHelper = new SampleHeightHelper();
        }

        private void Start()
        {
            StoreOriginalValues();
            SetupCrestWaterInteraction();
        }

        private void Update()
        {
            CheckWaterContact();
            UpdateSwimmingState();
        }

        private void SetupCrestWaterInteraction()
        {
            if (!enableCrestInteraction || Crest.OceanRenderer.Instance == null)
            {
                Debug.Log("[CharacterSwimming] Crest water interaction disabled or OceanRenderer not found");
                return;
            }

            // Add SphereWaterInteraction for foam/wake generation (modern approach)
            waterInteraction = GetComponent<SphereWaterInteraction>();
            if (waterInteraction == null)
            {
                waterInteraction = gameObject.AddComponent<SphereWaterInteraction>();
                waterInteraction._radius = interactionRadius;
                waterInteraction._weight = interactionStrength;
                Debug.Log("[CharacterSwimming] Added SphereWaterInteraction component");
            }

            // Add ObjectWaterInteractionAdaptor for water detection
            waterAdaptor = GetComponent<ObjectWaterInteractionAdaptor>();
            if (waterAdaptor == null)
            {
                waterAdaptor = gameObject.AddComponent<ObjectWaterInteractionAdaptor>();
                Debug.Log("[CharacterSwimming] Added ObjectWaterInteractionAdaptor component");
            }
        }

        private void CheckWaterContact()
        {
            // Use Crest water detection if available
            if (Crest.OceanRenderer.Instance != null && waterAdaptor != null)
            {
                CheckWaterContactCrest();
            }
            else
            {
                // Fallback to collision-based detection
                CheckWaterContactCollision();
            }
        }

        private void CheckWaterContactCrest()
        {
            // Use Crest's built-in water detection
            bool wasInWater = isInWater;
            isInWater = waterAdaptor.InWater;

            if (isInWater)
            {
                // Sample water height using Crest
                sampleHeightHelper.Init(transform.position, interactionRadius);
                if (sampleHeightHelper.Sample(out float waterHeight))
                {
                    this.waterHeight = waterHeight;
                    submersionDepth = CalculateSubmersionDepth();
                }
            }
            else
            {
                waterHeight = 0f;
                submersionDepth = 0f;
            }

            // Handle water state changes
            if (isInWater && !wasInWater)
            {
                OnEnterWater();
            }
            else if (!isInWater && wasInWater)
            {
                OnExitWater();
            }

            if (showDebug)
            {
                Debug.Log($"[CharacterSwimming] {gameObject.name} - In Water: {isInWater}, Swimming: {isSwimming}, Depth: {submersionDepth:F2}");
            }
        }

        private void CheckWaterContactCollision()
        {
            // Check if character is touching water using Physics.OverlapSphere
            Vector3 characterCenter = transform.position + characterController.center;
            float checkRadius = characterController.radius * 0.8f; // Slightly smaller than character radius
            
            Collider[] waterColliders = Physics.OverlapSphere(characterCenter, checkRadius, waterLayer);
            
            bool wasInWater = isInWater;
            isInWater = waterColliders.Length > 0;

            // Get water height from the first water collider
            if (isInWater && waterColliders.Length > 0)
            {
                // For simple water detection, use the collider's bounds
                waterHeight = waterColliders[0].bounds.max.y;
                submersionDepth = CalculateSubmersionDepth();
            }
            else
            {
                waterHeight = 0f;
                submersionDepth = 0f;
            }

            // Handle water state changes
            if (isInWater && !wasInWater)
            {
                OnEnterWater();
            }
            else if (!isInWater && wasInWater)
            {
                OnExitWater();
            }

            if (showDebug)
            {
                Debug.Log($"[CharacterSwimming] {gameObject.name} - In Water: {isInWater}, Swimming: {isSwimming}, Depth: {submersionDepth:F2}");
            }
        }

        private void UpdateSwimmingState()
        {
            if (!isInWater) return;

            bool shouldSwim = submersionDepth >= swimDepthThreshold;
            
            if (shouldSwim && !isSwimming)
            {
                StartSwimming();
            }
            else if (!shouldSwim && isSwimming)
            {
                StopSwimming();
            }
        }

        private float CalculateSubmersionDepth()
        {
            // Calculate how deep the character is in water
            float characterFeet = transform.position.y - characterController.height * 0.5f;
            float depth = waterHeight - characterFeet;
            return Mathf.Max(0f, depth);
        }

        private void StoreOriginalValues()
        {
            originalGravity = GetControllerValue<float>("Gravity");
            originalMoveSpeed = GetControllerValue<float>("MoveSpeed");
            originalSprintSpeed = GetControllerValue<float>("SprintSpeed");
        }

        private T GetControllerValue<T>(string fieldName)
        {
            if (thirdPersonController == null) return default(T);
            
            var field = thirdPersonController.GetType().GetField(fieldName);
            if (field != null && field.FieldType == typeof(T))
            {
                return (T)field.GetValue(thirdPersonController);
            }
            return default(T);
        }

        private void SetControllerValue<T>(string fieldName, T value)
        {
            if (thirdPersonController == null) return;
            
            var field = thirdPersonController.GetType().GetField(fieldName);
            if (field != null && field.FieldType == typeof(T))
            {
                field.SetValue(thirdPersonController, value);
            }
        }

        private void OnEnterWater()
        {
            // Apply water resistance (slower movement)
            float speedMultiplier = isSwimming ? swimSpeedMultiplier : waterSpeedMultiplier;
            SetControllerValue("MoveSpeed", originalMoveSpeed * speedMultiplier);
            SetControllerValue("SprintSpeed", originalSprintSpeed * speedMultiplier);
            
            Debug.Log($"[CharacterSwimming] {gameObject.name} entered water");
        }

        private void StartSwimming()
        {
            isSwimming = true;
            
            // Set swimming physics
            SetControllerValue("Gravity", swimGravity);
            SetControllerValue("MoveSpeed", originalMoveSpeed * swimSpeedMultiplier);
            SetControllerValue("SprintSpeed", originalSprintSpeed * swimSpeedMultiplier);
            
            // Trigger swimming animation
            if (animator != null)
            {
                animator.SetBool(isSwimmingHash, true);
            }
            
            Debug.Log($"[CharacterSwimming] {gameObject.name} started swimming");
        }

        private void StopSwimming()
        {
            isSwimming = false;
            
            // Restore normal gravity
            SetControllerValue("Gravity", originalGravity);
            
            // Apply water resistance instead of swimming speed
            SetControllerValue("MoveSpeed", originalMoveSpeed * waterSpeedMultiplier);
            SetControllerValue("SprintSpeed", originalSprintSpeed * waterSpeedMultiplier);
            
            // Stop swimming animation
            if (animator != null)
            {
                animator.SetBool(isSwimmingHash, false);
            }
            
            Debug.Log($"[CharacterSwimming] {gameObject.name} stopped swimming");
        }

        private void OnExitWater()
        {
            if (isSwimming)
            {
                StopSwimming();
            }
            
            // Restore normal physics
            RestoreNormalPhysics();
            Debug.Log($"[CharacterSwimming] {gameObject.name} exited water");
        }

        private void RestoreNormalPhysics()
        {
            // Restore original values
            SetControllerValue("Gravity", originalGravity);
            SetControllerValue("MoveSpeed", originalMoveSpeed);
            SetControllerValue("SprintSpeed", originalSprintSpeed);
        }

        private void OnDrawGizmos()
        {
            if (!showDebug) return;
            
            // Draw water detection area
            Gizmos.color = isInWater ? Color.blue : Color.cyan;
            Vector3 characterCenter = transform.position + characterController.center;
            float checkRadius = characterController.radius * 0.8f;
            Gizmos.DrawWireSphere(characterCenter, checkRadius);
            
            // Draw interaction radius if using Crest
            if (enableCrestInteraction && Crest.OceanRenderer.Instance != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, interactionRadius);
            }
            
            // Draw submersion depth
            if (isInWater)
            {
                Gizmos.color = isSwimming ? Color.red : Color.yellow;
                Vector3 waterSurface = new Vector3(transform.position.x, waterHeight, transform.position.z);
                Gizmos.DrawLine(transform.position, waterSurface);
                Gizmos.DrawWireSphere(waterSurface, 0.2f);
            }
        }

        // Public properties for external access
        public bool IsSwimming => isSwimming;
        public bool IsInWater => isInWater;
        public float SubmersionDepth => submersionDepth;
        public float WaterHeight => waterHeight;
    }
} 