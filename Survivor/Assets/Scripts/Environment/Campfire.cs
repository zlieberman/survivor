using UnityEngine;
using UnityEngine.Events;

namespace Survivor.Environment
{
    public class Campfire : MonoBehaviour
    {
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
            if (fireLight != null)
                originalLightIntensity = fireLight.intensity;

            if (fireParticles != null)
                originalParticleEmission = fireParticles.emission.rateOverTime.constant;

            UpdateFireEffects();
        }

        private void Update()
        {
            if (isLit)
            {
                // Burn fuel
                fuelLevel -= fuelBurnRate * Time.deltaTime;
                
                // Check if fire should go out
                if (fuelLevel <= 0)
                {
                    fuelLevel = 0;
                    ExtinguishFire();
                    onFuelEmpty?.Invoke();
                }

                // Update effects based on fuel level
                UpdateFireEffects();
            }
        }

        public void LightFire()
        {
            if (!isLit && fuelLevel > 0)
            {
                isLit = true;
                UpdateFireEffects();
                onFireStart?.Invoke();
            }
        }

        public void ExtinguishFire()
        {
            if (isLit)
            {
                isLit = false;
                UpdateFireEffects();
                onFireExtinguish?.Invoke();
            }
        }

        public void AddFuel(float amount)
        {
            fuelLevel = Mathf.Min(fuelLevel + amount, 100f);
            UpdateFireEffects();
        }

        private void UpdateFireEffects()
        {
            float intensityMultiplier = isLit ? (fuelLevel / 100f) : 0f;

            // Update light
            if (fireLight != null)
            {
                fireLight.intensity = originalLightIntensity * intensityMultiplier * lightIntensityMultiplier;
            }

            // Update particles
            if (fireParticles != null)
            {
                var emission = fireParticles.emission;
                emission.rateOverTime = originalParticleEmission * intensityMultiplier;
                fireParticles.gameObject.SetActive(isLit);
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

                fireAudioSource.volume = intensityMultiplier;
            }
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