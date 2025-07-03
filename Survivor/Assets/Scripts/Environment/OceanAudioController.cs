using UnityEngine;
using Survivor.Characters;
using Crest;
using System.Collections;

namespace Survivor.Environment
{
    /// <summary>
    /// Controls ocean audio based on player proximity to water.
    /// Uses the same water detection system as CharacterSwimming.
    /// </summary>
    public class OceanAudioController : MonoBehaviour
    {
        [Header("Audio Settings")]
        [SerializeField] private AudioSource oceanAudioSource;
        [SerializeField] private AudioClip oceanWavesClip;
        [SerializeField] [UnityEngine.Range(0f, 1f)] private float oceanVolume = 0.6f;
        [SerializeField] private bool loopOceanAudio = true;
        
        [Header("Water Detection")]
        [Tooltip("Layer mask for water objects (fallback if Crest not available)")]
        public LayerMask waterLayer = 16; // Layer 4 (1 << 4 = 16)
        [Tooltip("Distance threshold to start playing ocean audio")]
        [SerializeField] private float waterProximityThreshold = 0.1f;
        [Tooltip("Show debug information")]
        [SerializeField] private bool showDebug = false;
        [Tooltip("Temporary debug: Force disable Crest water detection")]
        [SerializeField] private bool forceDisableCrest = false;
        
        [Header("Fade Settings")]
        [SerializeField] private bool useFadeInOut = true;
        [SerializeField] private float fadeDuration = 1f;
        
        // Water detection
        private bool isNearWater = false;
        private float distanceToWater = float.MaxValue;
        private SampleHeightHelper sampleHeightHelper;
        private ObjectWaterInteractionAdaptor waterAdaptor;
        
        // Audio state
        private bool isOceanPlaying = false;
        private Coroutine fadeCoroutine;
        
        // Player reference (auto-detected)
        private Transform playerTransform;
        
        private void Start()
        {
            // Find player automatically
            FindPlayer();
            
            // Setup audio source if not assigned
            if (oceanAudioSource == null)
            {
                oceanAudioSource = GetComponent<AudioSource>();
                if (oceanAudioSource == null)
                {
                    oceanAudioSource = gameObject.AddComponent<AudioSource>();
                }
            }
            
            // Check if audio clip is assigned
            if (oceanWavesClip == null)
            {
                Debug.LogError("[OceanAudioController] No ocean waves clip assigned! Please assign the waves.mp3 file in the inspector.");
                return;
            }
            
            // Configure audio source
            oceanAudioSource.clip = oceanWavesClip;
            oceanAudioSource.loop = loopOceanAudio;
            oceanAudioSource.volume = 0f; // Start silent
            oceanAudioSource.spatialBlend = 0f; // 2D audio (no 3D positioning)
            oceanAudioSource.playOnAwake = false;
            
            // Start playing the clip (but silent)
            oceanAudioSource.Play();
            
            Debug.Log($"[OceanAudioController] Audio source configured - Clip: {oceanWavesClip.name}, Volume: {oceanAudioSource.volume}, Playing: {oceanAudioSource.isPlaying}");
            
            // Initialize Crest water detection
            sampleHeightHelper = new SampleHeightHelper();
            
            // Setup Crest water interaction if available
            SetupCrestWaterInteraction();
        }
        
        private void FindPlayer()
        {
            // Try to find player with CharacterSwimming component first
            var playerSwimming = FindObjectOfType<CharacterSwimming>();
            if (playerSwimming != null)
            {
                playerTransform = playerSwimming.transform;
                Debug.Log($"[OceanAudioController] Found player: {playerTransform.name}");
                return;
            }
            
            // Fallback: try to find player by tag
            var playerByTag = GameObject.FindGameObjectWithTag("Player");
            if (playerByTag != null)
            {
                playerTransform = playerByTag.transform;
                Debug.Log($"[OceanAudioController] Found player by tag: {playerTransform.name}");
                return;
            }
            
            // Last resort: try to find any GameObject with "Player" in the name
            var allObjects = FindObjectsOfType<GameObject>();
            foreach (var obj in allObjects)
            {
                if (obj.name.ToLower().Contains("player"))
                {
                    playerTransform = obj.transform;
                    Debug.Log($"[OceanAudioController] Found player by name: {playerTransform.name}");
                    return;
                }
            }
            
            Debug.LogWarning("[OceanAudioController] No player found in scene! Ocean audio will not work.");
        }
        
        private void Update()
        {
            if (playerTransform == null) return;
            
            CheckWaterProximity();
            UpdateOceanAudio();
        }
        
        private void SetupCrestWaterInteraction()
        {
            if (Crest.OceanRenderer.Instance == null)
            {
                Debug.Log("[OceanAudioController] Crest OceanRenderer not found - using collision-based detection");
                return;
            }

            // Add ObjectWaterInteractionAdaptor for water detection
            waterAdaptor = GetComponent<ObjectWaterInteractionAdaptor>();
            if (waterAdaptor == null)
            {
                waterAdaptor = gameObject.AddComponent<ObjectWaterInteractionAdaptor>();
                Debug.Log("[OceanAudioController] Added ObjectWaterInteractionAdaptor component");
            }
        }
        
        private void CheckWaterProximity()
        {
            bool wasNearWater = isNearWater;
            
            // Use Crest water detection if available
            if (Crest.OceanRenderer.Instance != null && waterAdaptor != null && !forceDisableCrest)
            {
                CheckWaterProximityCrest();
            }
            else
            {
                // Fallback to collision-based detection
                CheckWaterProximityCollision();
            }
            
            // Handle water proximity state changes
            if (isNearWater && !wasNearWater)
            {
                OnEnterWaterProximity();
            }
            else if (!isNearWater && wasNearWater)
            {
                OnExitWaterProximity();
            }
            
            if (showDebug)
            {
                Debug.Log($"[OceanAudioController] Near Water: {isNearWater}, Distance: {distanceToWater:F2}, Playing: {isOceanPlaying}");
            }
        }
        
        private void CheckWaterProximityCrest()
        {
            // Use Crest's SampleHeightHelper for proper water height detection
            sampleHeightHelper.Init(playerTransform.position, waterProximityThreshold, true);
            bool hasWaterData = sampleHeightHelper.Sample(out float waterHeight);
            
            if (hasWaterData)
            {
                // Calculate distance to water surface
                float playerHeight = playerTransform.position.y;
                distanceToWater = Mathf.Abs(playerHeight - waterHeight);
                
                // Check if player is within proximity threshold
                isNearWater = distanceToWater <= waterProximityThreshold;
            }
            else
            {
                isNearWater = false;
                distanceToWater = float.MaxValue;
            }
        }
        
        private void CheckWaterProximityCollision()
        {
            // Check for water colliders near the player
            Vector3 playerPosition = playerTransform.position;
            Collider[] waterColliders = Physics.OverlapSphere(playerPosition, waterProximityThreshold, waterLayer);
            
            if (waterColliders.Length > 0)
            {
                // Find the closest water surface
                float closestDistance = float.MaxValue;
                
                foreach (var waterCollider in waterColliders)
                {
                    // Get the water surface height (top of the collider)
                    float waterHeight = waterCollider.bounds.max.y;
                    float distance = Mathf.Abs(playerPosition.y - waterHeight);
                    
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                    }
                }
                
                distanceToWater = closestDistance;
                isNearWater = distanceToWater <= waterProximityThreshold;
            }
            else
            {
                isNearWater = false;
                distanceToWater = float.MaxValue;
            }
        }
        
        private void UpdateOceanAudio()
        {
            // Handle audio state changes based on water proximity
            if (isNearWater && !isOceanPlaying)
            {
                StartOceanAudio();
            }
            else if (!isNearWater && isOceanPlaying)
            {
                StopOceanAudio();
            }
        }
        
        private void OnEnterWaterProximity()
        {
            Debug.Log($"[OceanAudioController] Player entered water proximity (distance: {distanceToWater:F2})");
        }
        
        private void OnExitWaterProximity()
        {
            Debug.Log($"[OceanAudioController] Player left water proximity (distance: {distanceToWater:F2})");
        }
        
        private void StartOceanAudio()
        {
            if (isOceanPlaying) return;
            
            isOceanPlaying = true;
            
            if (useFadeInOut)
            {
                if (fadeCoroutine != null)
                {
                    StopCoroutine(fadeCoroutine);
                }
                fadeCoroutine = StartCoroutine(FadeIn());
            }
            else
            {
                oceanAudioSource.volume = oceanVolume;
            }
            
            Debug.Log("[OceanAudioController] Started ocean audio - player near water");
        }
        
        private void StopOceanAudio()
        {
            if (!isOceanPlaying) return;
            
            isOceanPlaying = false;
            
            if (useFadeInOut)
            {
                if (fadeCoroutine != null)
                {
                    StopCoroutine(fadeCoroutine);
                }
                fadeCoroutine = StartCoroutine(FadeOut());
            }
            else
            {
                oceanAudioSource.volume = 0f;
            }
            
            Debug.Log("[OceanAudioController] Stopped ocean audio - player away from water");
        }
        
        private IEnumerator FadeIn()
        {
            float startVolume = oceanAudioSource.volume;
            float elapsedTime = 0f;
            
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / fadeDuration;
                oceanAudioSource.volume = Mathf.Lerp(startVolume, oceanVolume, progress);
                yield return null;
            }
            
            oceanAudioSource.volume = oceanVolume;
        }
        
        private IEnumerator FadeOut()
        {
            float startVolume = oceanAudioSource.volume;
            float elapsedTime = 0f;
            
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / fadeDuration;
                oceanAudioSource.volume = Mathf.Lerp(startVolume, 0f, progress);
                yield return null;
            }
            
            oceanAudioSource.volume = 0f;
        }
        
        // Public methods for external control
        public void SetOceanVolume(float volume)
        {
            oceanVolume = Mathf.Clamp01(volume);
            if (isOceanPlaying)
            {
                oceanAudioSource.volume = oceanVolume;
            }
        }
        
        public void SetWaterProximityThreshold(float threshold)
        {
            waterProximityThreshold = Mathf.Max(0f, threshold);
        }
        
        public bool IsOceanPlaying => isOceanPlaying;
        public bool IsNearWater => isNearWater;
        public float OceanVolume => oceanVolume;
        public float DistanceToWater => distanceToWater;
        
        private void OnDrawGizmos()
        {
            if (!showDebug || playerTransform == null) return;
            
            // Draw water proximity detection area
            Gizmos.color = isNearWater ? Color.blue : Color.cyan;
            Gizmos.DrawWireSphere(playerTransform.position, waterProximityThreshold);
            
            // Draw distance to water if near water
            if (isNearWater)
            {
                Gizmos.color = Color.yellow;
                Vector3 waterSurface = new Vector3(playerTransform.position.x, 
                    playerTransform.position.y - distanceToWater, 
                    playerTransform.position.z);
                Gizmos.DrawLine(playerTransform.position, waterSurface);
                Gizmos.DrawWireSphere(waterSurface, 0.2f);
            }
        }
        
        // Debug method to help identify water detection issues
        [ContextMenu("Debug Water Detection")]
        public void DebugWaterDetection()
        {
            Debug.Log($"[OceanAudioController] Debug Info:");
            Debug.Log($"- Player Transform: {(playerTransform != null ? playerTransform.name : "null")}");
            Debug.Log($"- Water Layer: {waterLayer.value}");
            Debug.Log($"- Proximity Threshold: {waterProximityThreshold}");
            Debug.Log($"- Is Near Water: {isNearWater}");
            Debug.Log($"- Distance to Water: {distanceToWater}");
            Debug.Log($"- Ocean Playing: {isOceanPlaying}");
            Debug.Log($"- Crest Available: {Crest.OceanRenderer.Instance != null}");
            Debug.Log($"- Water Adaptor: {(waterAdaptor != null ? "Found" : "Missing")}");
        }
        
        // Manual test method to force start audio
        [ContextMenu("Force Start Audio")]
        public void ForceStartAudio()
        {
            Debug.Log("[OceanAudioController] Force starting audio");
            isOceanPlaying = true;
            oceanAudioSource.volume = oceanVolume;
        }
        
        // Manual test method to test audio at full volume
        [ContextMenu("Test Audio Full Volume")]
        public void TestAudioFullVolume()
        {
            Debug.Log("[OceanAudioController] Testing audio at full volume");
            if (oceanAudioSource != null && oceanWavesClip != null)
            {
                oceanAudioSource.volume = 1f;
                Debug.Log($"[OceanAudioController] Audio test - Volume: {oceanAudioSource.volume}, Playing: {oceanAudioSource.isPlaying}, Clip: {oceanWavesClip.name}");
            }
            else
            {
                Debug.LogError("[OceanAudioController] Cannot test audio - AudioSource or Clip is null!");
            }
        }
    }
} 