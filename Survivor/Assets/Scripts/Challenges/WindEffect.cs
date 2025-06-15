using UnityEngine;

namespace Survivor.Challenges
{
    public class WindEffect : MonoBehaviour
    {
        [SerializeField] private float windSpeed = 1f;
        [SerializeField] private float windStrength = 0.5f;
        [SerializeField] private float particleSpeed = 2f;
        [SerializeField] private ParticleSystem windParticles;
        [SerializeField] private AudioSource windAudio;

        private float currentWindDirection;
        private float targetWindDirection;

        private void Start()
        {
            if (windParticles != null)
            {
                var main = windParticles.main;
                main.startSpeed = particleSpeed;
            }

            if (windAudio != null)
            {
                windAudio.Play();
            }
        }

        public void SetWindDirection(float direction)
        {
            targetWindDirection = direction;
        }

        private void Update()
        {
            // Smoothly interpolate current wind direction
            currentWindDirection = Mathf.Lerp(currentWindDirection, targetWindDirection, Time.deltaTime * windSpeed);

            // Update particle system
            if (windParticles != null)
            {
                var main = windParticles.main;
                main.startRotation = currentWindDirection * Mathf.PI;
                
                var emission = windParticles.emission;
                emission.rateOverTime = Mathf.Abs(currentWindDirection) * windStrength * 10f;
            }

            // Update audio
            if (windAudio != null)
            {
                windAudio.volume = Mathf.Abs(currentWindDirection) * windStrength;
                windAudio.pitch = 1f + (currentWindDirection * 0.2f);
            }
        }
    }
} 