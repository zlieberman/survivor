using UnityEngine;
using StarterAssets;
using Crest;
using System.Collections;

namespace Survivor.Characters
{
    [RequireComponent(typeof(CharacterController))]
    public class CharacterSwimming : FloatingObjectBase
    {
        [Header("Swimming Settings")]
        [Tooltip("Gravity when swimming")]
        public float swimGravity = -2f;
        [Tooltip("Depth in units to start swimming")]
        public float swimDepthThreshold = 0.5f;

        [Header("Crest Buoyancy")]
        [Tooltip("Strength of buoyancy force per meter of submersion")]
        public float buoyancyCoeff = 3f;
        [Tooltip("Maximum buoyancy force (Infinity = no limit)")]
        public float maximumBuoyancyForce = Mathf.Infinity;
        [Tooltip("Object width for physics calculations")]
        public float objectWidth = 1.8f;

        [Header("Animation Integration")]
        [Tooltip("Enable floating behavior when swimming (no movement input)")]
        public bool enableFloating = true;
        [Tooltip("Speed threshold for floating (below this = floating)")]
        public float floatingSpeedThreshold = 0.1f;

        [Header("Crest Water Interaction")]
        [Tooltip("Enable Crest water interaction for foam and wakes")]
        public bool enableCrestInteraction = true;
        [Tooltip("Strength of water interaction (affects foam/wake intensity)")]
        [UnityEngine.Range(0f, 2f)]
        public float interactionStrength = 1f;
        [Tooltip("Size of the interaction area")]
        public float interactionRadius = 0.5f;

        [Header("Water Detection")]
        [Tooltip("Layer mask for water objects (fallback if Crest not available)")]
        public LayerMask waterLayer = 16; // Layer 4 (1 << 4 = 16)
        [Tooltip("Show debug information")]
        public bool showDebug = false;
        [Tooltip("Temporary debug: Force disable Crest water detection")]
        public bool forceDisableCrest = false;

        private CharacterController characterController;
        private MonoBehaviour thirdPersonController;
        private Animator animator;
        
        // Original values to restore when leaving water
        private float originalGravity;
        
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
        private int speedHash;
        private int motionSpeedHash;

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

            // Setup animation hashes
            isSwimmingHash = Animator.StringToHash("IsSwimming");
            speedHash = Animator.StringToHash("Speed");
            motionSpeedHash = Animator.StringToHash("MotionSpeed");

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
            UpdateAnimation();
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
            if (Crest.OceanRenderer.Instance != null && waterAdaptor != null && !forceDisableCrest)
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
            // Use Crest's SampleHeightHelper for proper depth-based detection
            bool wasInWater = isInWater;
            
            // Sample water height at character position
            sampleHeightHelper.Init(transform.position, interactionRadius, true);
            bool hasWaterData = sampleHeightHelper.Sample(out float waterHeight);
            
            if (hasWaterData)
            {
                this.waterHeight = waterHeight;
                submersionDepth = CalculateSubmersionDepth();
                
                // Only consider "in water" if submerged enough to matter
                // This prevents swimming from starting when just feet are wet
                // Use a larger threshold to ensure the character is actually in water
                isInWater = submersionDepth > 0.3f; // Increased threshold for more accurate detection
            }
            else
            {
                isInWater = false;
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
                Debug.Log($"[CharacterSwimming] {gameObject.name} - Has Water Data: {hasWaterData}, In Water: {isInWater}, Swimming: {isSwimming}, Depth: {submersionDepth:F2}, Water Height: {waterHeight:F2}, Character Y: {transform.position.y:F2}, Character Feet Y: {transform.position.y - characterController.height * 0.5f:F2}");
            }
        }

        private void CheckWaterContactCollision()
        {
            // Check if character is touching water using Physics.OverlapSphere
            Vector3 characterCenter = transform.position + characterController.center;
            float checkRadius = characterController.radius * 0.8f; // Slightly smaller than character radius
            
            Collider[] waterColliders = Physics.OverlapSphere(characterCenter, checkRadius, waterLayer);
            
            bool wasInWater = isInWater;
            
            // Get water height from the first water collider
            if (waterColliders.Length > 0)
            {
                // For simple water detection, use the collider's bounds
                waterHeight = waterColliders[0].bounds.max.y;
                submersionDepth = CalculateSubmersionDepth();
                
                // Use the same threshold as Crest detection for consistency
                isInWater = submersionDepth > 0.3f;
            }
            else
            {
                isInWater = false;
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
                Debug.Log($"[CharacterSwimming] {gameObject.name} - In Water: {isInWater}, Swimming: {isSwimming}, Depth: {submersionDepth:F2}, Water Colliders: {waterColliders.Length}, Character Y: {transform.position.y:F2}, Character Feet Y: {transform.position.y - characterController.height * 0.5f:F2}, Water Height: {waterHeight:F2}");
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

        private void UpdateAnimation()
        {
            if (animator == null) return;

            // Update swimming parameter
            animator.SetBool(isSwimmingHash, isSwimming);

            // Handle floating behavior when swimming
            if (isSwimming && enableFloating)
            {
                // Get current horizontal speed
                float horizontalSpeed = new Vector3(characterController.velocity.x, 0f, characterController.velocity.z).magnitude;
                
                // If moving slowly, set speed to 0 for floating animation
                if (horizontalSpeed < floatingSpeedThreshold)
                {
                    animator.SetFloat(speedHash, 0f);
                    animator.SetFloat(motionSpeedHash, 0f);
                }
            }
        }

        private float CalculateSubmersionDepth()
        {
            // Calculate how deep the character is in water
            // float characterFeet = transform.position.y - characterController.height * 0.5f;
            float depth = waterHeight - transform.position.y;
            return Mathf.Max(0f, depth);
        }

        private void StoreOriginalValues()
        {
            // Store original gravity value
            originalGravity = GetControllerValue<float>("Gravity");
            Debug.Log($"[CharacterSwimming] Stored original gravity: {originalGravity}");
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
            // Set swimming state in the controller
            SetSwimmingState(true);
            
            // Apply swimming gravity
            SetControllerValue("Gravity", swimGravity);
            
            Debug.Log($"[CharacterSwimming] {gameObject.name} entered water");
        }

        private void StartSwimming()
        {
            isSwimming = true;
            
            // Set swimming state in the controller
            SetSwimmingState(true);
            
            // Apply swimming gravity
            SetControllerValue("Gravity", swimGravity);
            
            Debug.Log($"[CharacterSwimming] {gameObject.name} started swimming");
        }

        private void StopSwimming()
        {
            isSwimming = false;
            
            // Set swimming state in the controller
            SetSwimmingState(false);
            
            // Restore normal gravity
            SetControllerValue("Gravity", originalGravity);
            
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
            // Set swimming state in the controller
            SetSwimmingState(false);
            
            // Restore original gravity
            SetControllerValue("Gravity", originalGravity);
        }

        private void SetSwimmingState(bool swimming)
        {
            // Use the ThirdPersonController's SetSwimming method
            if (thirdPersonController is StarterAssets.ThirdPersonController starterController)
            {
                starterController.SetSwimming(swimming);
            }
            else if (thirdPersonController is ThirdPersonController customController)
            {
                customController.SetSwimming(swimming);
            }
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

        // FloatingObjectBase implementation
        public override float ObjectWidth => objectWidth;
        public override bool InWater => isInWater;
        public override Vector3 Velocity => characterController != null ? characterController.velocity : Vector3.zero;

        // Debug method to help identify water detection issues
        [ContextMenu("Debug Water Detection")]
        public void DebugWaterDetection()
        {
            Debug.Log($"[CharacterSwimming] Debug Info for {gameObject.name}:");
            Debug.Log($"  Position: {transform.position}");
            Debug.Log($"  Character Height: {characterController.height}");
            Debug.Log($"  Character Feet Y: {transform.position.y - characterController.height * 0.5f}");
            Debug.Log($"  Character Center: {transform.position + characterController.center}");
            Debug.Log($"  Character Radius: {characterController.radius}");
            Debug.Log($"  Is In Water: {isInWater}");
            Debug.Log($"  Is Swimming: {isSwimming}");
            Debug.Log($"  Water Height: {waterHeight}");
            Debug.Log($"  Submersion Depth: {submersionDepth}");
            Debug.Log($"  OceanRenderer Instance: {Crest.OceanRenderer.Instance != null}");
            Debug.Log($"  Water Adaptor: {waterAdaptor != null}");
            Debug.Log($"  Force Disable Crest: {forceDisableCrest}");
            
            if (Crest.OceanRenderer.Instance != null)
            {
                Debug.Log($"  Ocean Center: {Crest.OceanRenderer.Instance.transform.position}");
                Debug.Log($"  Ocean Sea Level: {Crest.OceanRenderer.Instance.SeaLevel}");
            }
        }
    }
} 