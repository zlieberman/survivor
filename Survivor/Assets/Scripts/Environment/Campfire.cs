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

            UpdateFireEffects();
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

            // Update audio
            if (fireAudioSource != null)
            {
                if (isLit && !fireAudioSource.isPlaying)
                    fireAudioSource.Play();
                else if (!isLit && fireAudioSource.isPlaying)
                    fireAudioSource.Stop();

                fireAudioSource.volume = fuelAmount * 0.01f;
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
        }
    }
} 