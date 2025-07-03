using UnityEngine;
using UnityEngine.Events;

namespace Survivor.Environment
{
    public class Campfire : MonoBehaviour
    {
        [Header("Campfire Settings")]
        public float warmthRadius = 5f;
        public float warmthIntensity = 1f;
        public float fuelAmount = 100f;
        public float burnRate = 0.1f;

        [Header("Fire Properties")]
        public bool isLit = true;
        public float fuelLevel = 100f;
        public float fuelBurnRate = 1f; // Units per second
        public float lightIntensityMultiplier = 1f;
        public float heatRadius = 5f;

        [Header("Components")]
        public Light fireLight;
        public ParticleSystem fireParticles;
        public ParticleSystem smokeParticles;
        public AudioSource fireAudioSource;

        [Header("Audio Settings")]
        [SerializeField] private float audioMaxDistance = 5f;
        [SerializeField] private float audioMinDistance = 1f;
        [SerializeField] private float baseAudioVolume = 0.5f;

        [Header("Events")]
        public UnityEvent onFireStart;
        public UnityEvent onFireExtinguish;
        public UnityEvent onFuelEmpty;

        private float originalLightIntensity;
        private float originalParticleEmission;

        private void Start()
        {
            // Get or add required components
            fireParticles = GetComponentInChildren<ParticleSystem>();
            fireLight = GetComponentInChildren<Light>();

            if (fireParticles == null)
            {
                Debug.LogWarning("No particle system found for campfire");
            }

            if (fireLight == null)
            {
                Debug.LogWarning("No light component found for campfire");
            }

            if (fireLight != null)
                originalLightIntensity = fireLight.intensity;

            if (fireParticles != null)
                originalParticleEmission = fireParticles.emission.rateOverTime.constant;

            // Setup audio source for spatial audio
            SetupAudioSource();

            UpdateFireEffects();
        }

        private void SetupAudioSource()
        {
            // Get or create AudioSource
            if (fireAudioSource == null)
            {
                fireAudioSource = GetComponent<AudioSource>();
                if (fireAudioSource == null)
                {
                    fireAudioSource = gameObject.AddComponent<AudioSource>();
                    Debug.LogWarning("No AudioSource found on campfire, created new one");
                }
                else
                {
                    Debug.Log("Found existing AudioSource on campfire");
                }
            }
            else
            {
                Debug.Log("Using assigned AudioSource from inspector");
            }

            // Check for multiple AudioSources
            AudioSource[] allAudioSources = GetComponents<AudioSource>();
            if (allAudioSources.Length > 1)
            {
                Debug.LogWarning($"Multiple AudioSources found on campfire! Count: {allAudioSources.Length}");
                foreach (var audioSource in allAudioSources)
                {
                    Debug.Log($"AudioSource: {audioSource.name}, Position: {audioSource.transform.position}, Clip: {audioSource.clip}");
                }
            }

            // Configure spatial audio settings without overriding existing clip and playOnAwake
            fireAudioSource.spatialBlend = 1f; // Full 3D audio
            fireAudioSource.maxDistance = audioMaxDistance;
            fireAudioSource.minDistance = audioMinDistance;
            fireAudioSource.rolloffMode = AudioRolloffMode.Linear;
            fireAudioSource.dopplerLevel = 0f; // Disable doppler for fire sounds
            fireAudioSource.loop = true;
            
            // Don't override playOnAwake - let the prefab settings handle this
            // fireAudioSource.playOnAwake = false;
            
            // Set initial volume
            fireAudioSource.volume = baseAudioVolume;
            
            // Ensure AudioSource position matches campfire position
            if (fireAudioSource.transform.position != transform.position)
            {
                Debug.LogWarning($"AudioSource position ({fireAudioSource.transform.position}) doesn't match campfire position ({transform.position}). Fixing...");
                fireAudioSource.transform.position = transform.position;
            }
            
            Debug.Log($"Campfire audio configured - MaxDistance: {audioMaxDistance}, MinDistance: {audioMinDistance}, Volume: {baseAudioVolume}");
            Debug.Log($"Campfire position: {transform.position}, AudioSource position: {fireAudioSource.transform.position}");
        }

        private void Update()
        {
            if (fuelAmount > 0)
            {
                fuelAmount -= burnRate * Time.deltaTime;
                UpdateFireEffects();
            }
            else
            {
                ExtinguishFire();
            }
        }

        private void UpdateFireEffects()
        {
            if (fireParticles != null)
            {
                var emission = fireParticles.emission;
                emission.rateOverTime = fuelAmount * 0.1f;
            }

            if (fireLight != null)
            {
                fireLight.intensity = fuelAmount * 0.01f;
            }

            // Update smoke
            if (smokeParticles != null)
            {
                smokeParticles.gameObject.SetActive(isLit);
            }

            // Update audio - use a multiplier that doesn't interfere with spatial positioning
            if (fireAudioSource != null)
            {
                // Only control volume, let the AudioSource handle playback based on prefab settings
                if (isLit)
                {
                    // Calculate volume based on fuel amount, but maintain spatial audio properties
                    float fuelMultiplier = Mathf.Clamp01(fuelAmount / 100f);
                    float newVolume = baseAudioVolume * fuelMultiplier;
                    fireAudioSource.volume = newVolume;
                    
                    // Debug info (only log occasionally to avoid spam)
                    if (Time.frameCount % 60 == 0) // Log every 60 frames
                    {
                        Debug.Log($"Campfire audio - Fuel: {fuelAmount}, Volume: {newVolume}, Playing: {fireAudioSource.isPlaying}, Position: {transform.position}");
                    }
                }
                else
                {
                    // Mute the audio when fire is extinguished
                    fireAudioSource.volume = 0f;
                    if (Time.frameCount % 60 == 0)
                    {
                        Debug.Log("Campfire audio muted - fire extinguished");
                    }
                }
            }
            else
            {
                Debug.LogError("FireAudioSource is null in UpdateFireEffects!");
            }
        }

        private void ExtinguishFire()
        {
            if (fireParticles != null)
            {
                fireParticles.Stop();
            }

            if (fireLight != null)
            {
                fireLight.intensity = 0f;
            }

            isLit = false;
            onFireExtinguish?.Invoke();
        }

        public void AddFuel(float amount)
        {
            fuelAmount = Mathf.Min(fuelAmount + amount, 100f);
            if (fireParticles != null && !fireParticles.isPlaying)
            {
                fireParticles.Play();
            }
            UpdateFireEffects();
        }

        private void OnTriggerStay(Collider other)
        {
            // Apply heat effect to objects in range
            if (isLit)
            {
                float distance = Vector3.Distance(transform.position, other.transform.position);
                if (distance <= heatRadius)
                {
                    // You can add heating effects here
                    // For example, drying wet objects, cooking food, etc.
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Draw heat radius in editor
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, heatRadius);
            
            // Draw audio radius in editor
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, audioMaxDistance);
        }
    }
} 