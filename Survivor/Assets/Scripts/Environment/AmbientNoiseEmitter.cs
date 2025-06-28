using UnityEngine;
using System.Collections;
using Survivor.Shared;

namespace Survivor.Environment
{
    /// <summary>
    /// Component that emits ambient noise from a specific location in the scene.
    /// Can be attached to any GameObject to create localized ambient sounds.
    /// </summary>
    public class AmbientNoiseEmitter : MonoBehaviour
    {
        [Header("Audio Clips")]
        [SerializeField] private AudioClip[] ambientClips;
        [SerializeField] private AudioClip[] randomClips;
        
        [Header("Playback Settings")]
        [SerializeField] private bool playOnStart = true;
        [SerializeField] private bool loop = true;
        [SerializeField] [Range(0f, 1f)] private float volume = 0.5f;
        [SerializeField] [Range(0.1f, 3f)] private float pitch = 1f;
        [SerializeField] private bool randomizePitch = true;
        [SerializeField] [Range(0.8f, 1.2f)] private float pitchVariation = 0.1f;
        
        [Header("Timing Settings")]
        [SerializeField] private float minInterval = 5f;
        [SerializeField] private float maxInterval = 15f;
        [SerializeField] private bool useRandomIntervals = true;
        [SerializeField] private float fixedInterval = 10f;
        
        [Header("Spatial Settings")]
        [SerializeField] private bool useSpatialAudio = true;
        [SerializeField] [Range(0f, 50f)] private float maxDistance = 20f;
        [SerializeField] [Range(0f, 5f)] private float minDistance = 1f;
        [SerializeField] private AudioRolloffMode rolloffMode = AudioRolloffMode.Linear;
        
        [Header("Trigger Settings")]
        [SerializeField] private bool playOnPlayerEnter = false;
        [SerializeField] private bool stopOnPlayerExit = false;
        [SerializeField] private string playerTag = "Player";
        
        [Header("Advanced Settings")]
        [SerializeField] private bool fadeInOut = true;
        [SerializeField] private float fadeDuration = 1f;
        [SerializeField] private bool respectAudioServiceVolume = true;
        
        private AudioSource audioSource;
        private IAudioService audioService;
        private Coroutine playCoroutine;
        private bool isPlaying = false;
        private bool playerInRange = false;
        
        // Events
        public System.Action<AudioClip> OnAmbientSoundPlayed;
        public System.Action OnAmbientSoundStopped;
        
        private void Awake()
        {
            SetupAudioSource();
        }
        
        private void Start()
        {
            // Get the audio service
            audioService = AudioServiceLocator.GetAudioService();
            
            if (playOnStart)
            {
                StartAmbientSounds();
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (playOnPlayerEnter && other.CompareTag(playerTag))
            {
                playerInRange = true;
                StartAmbientSounds();
            }
        }
        
        private void OnTriggerExit(Collider other)
        {
            if (stopOnPlayerExit && other.CompareTag(playerTag))
            {
                playerInRange = false;
                StopAmbientSounds();
            }
        }
        
        private void SetupAudioSource()
        {
            // Create or get AudioSource component
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            
            // Configure AudioSource
            audioSource.playOnAwake = false;
            audioSource.loop = false; // We handle looping ourselves
            audioSource.volume = volume;
            audioSource.pitch = pitch;
            
            // Spatial settings
            if (useSpatialAudio)
            {
                audioSource.spatialBlend = 1f; // Full 3D
                audioSource.maxDistance = maxDistance;
                audioSource.minDistance = minDistance;
                audioSource.rolloffMode = rolloffMode;
                audioSource.dopplerLevel = 0f; // Disable doppler for ambient sounds
            }
            else
            {
                audioSource.spatialBlend = 0f; // 2D
            }
        }
        
        /// <summary>
        /// Starts playing ambient sounds
        /// </summary>
        public void StartAmbientSounds()
        {
            if (isPlaying || ambientClips == null || ambientClips.Length == 0)
                return;
                
            isPlaying = true;
            playCoroutine = StartCoroutine(PlayAmbientSoundsCoroutine());
        }
        
        /// <summary>
        /// Stops playing ambient sounds
        /// </summary>
        public void StopAmbientSounds()
        {
            isPlaying = false;
            
            if (playCoroutine != null)
            {
                StopCoroutine(playCoroutine);
                playCoroutine = null;
            }
            
            if (audioSource.isPlaying)
            {
                if (fadeInOut)
                {
                    StartCoroutine(FadeOutCoroutine());
                }
                else
                {
                    audioSource.Stop();
                }
            }
            
            OnAmbientSoundStopped?.Invoke();
        }
        
        /// <summary>
        /// Plays a single ambient sound immediately
        /// </summary>
        public void PlaySingleAmbientSound()
        {
            if (ambientClips == null || ambientClips.Length == 0)
                return;
                
            AudioClip clip = GetRandomClip(ambientClips);
            PlayClip(clip);
        }
        
        /// <summary>
        /// Plays a random sound from the random clips array
        /// </summary>
        public void PlayRandomSound()
        {
            if (randomClips == null || randomClips.Length == 0)
                return;
                
            AudioClip clip = GetRandomClip(randomClips);
            PlayClip(clip);
        }
        
        private IEnumerator PlayAmbientSoundsCoroutine()
        {
            while (isPlaying)
            {
                // Check if we should stop (for player exit triggers)
                if (playOnPlayerEnter && !playerInRange)
                {
                    break;
                }
                
                // Play a random ambient clip
                if (ambientClips != null && ambientClips.Length > 0)
                {
                    AudioClip clip = GetRandomClip(ambientClips);
                    PlayClip(clip);
                }
                
                // Wait for the next interval
                float interval = useRandomIntervals ? 
                    Random.Range(minInterval, maxInterval) : fixedInterval;
                    
                yield return new WaitForSeconds(interval);
            }
        }
        
        private AudioClip GetRandomClip(AudioClip[] clips)
        {
            if (clips == null || clips.Length == 0)
                return null;
                
            return clips[Random.Range(0, clips.Length)];
        }
        
        private void PlayClip(AudioClip clip)
        {
            if (clip == null || audioSource == null)
                return;
                
            // Set pitch
            if (randomizePitch)
            {
                float pitchVariationAmount = Random.Range(-pitchVariation, pitchVariation);
                audioSource.pitch = pitch + pitchVariationAmount;
            }
            else
            {
                audioSource.pitch = pitch;
            }
            
            // Set volume (respect audio service if enabled)
            float finalVolume = volume;
            if (respectAudioServiceVolume && audioService != null)
            {
                finalVolume *= audioService.SFXVolume;
            }
            audioSource.volume = finalVolume;
            
            // Play the clip
            if (fadeInOut)
            {
                StartCoroutine(FadeInCoroutine(clip));
            }
            else
            {
                audioSource.clip = clip;
                audioSource.Play();
            }
            
            OnAmbientSoundPlayed?.Invoke(clip);
        }
        
        private IEnumerator FadeInCoroutine(AudioClip clip)
        {
            audioSource.clip = clip;
            audioSource.volume = 0f;
            audioSource.Play();
            
            float targetVolume = volume;
            if (respectAudioServiceVolume && audioService != null)
            {
                targetVolume *= audioService.SFXVolume;
            }
            
            float elapsedTime = 0f;
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / fadeDuration;
                audioSource.volume = Mathf.Lerp(0f, targetVolume, progress);
                yield return null;
            }
            
            audioSource.volume = targetVolume;
        }
        
        private IEnumerator FadeOutCoroutine()
        {
            float startVolume = audioSource.volume;
            float elapsedTime = 0f;
            
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / fadeDuration;
                audioSource.volume = Mathf.Lerp(startVolume, 0f, progress);
                yield return null;
            }
            
            audioSource.Stop();
        }
        
        /// <summary>
        /// Sets the ambient clips array
        /// </summary>
        public void SetAmbientClips(AudioClip[] clips)
        {
            ambientClips = clips;
        }
        
        /// <summary>
        /// Sets the random clips array
        /// </summary>
        public void SetRandomClips(AudioClip[] clips)
        {
            randomClips = clips;
        }
        
        /// <summary>
        /// Sets the volume
        /// </summary>
        public void SetVolume(float newVolume)
        {
            volume = Mathf.Clamp01(newVolume);
            if (audioSource != null)
            {
                audioSource.volume = volume;
            }
        }
        
        /// <summary>
        /// Gets whether the emitter is currently playing
        /// </summary>
        public bool IsPlaying => isPlaying;
        
        /// <summary>
        /// Gets the current volume
        /// </summary>
        public float Volume => volume;
        
        private void OnDestroy()
        {
            StopAmbientSounds();
        }
        
        private void OnValidate()
        {
            // Ensure min interval is not greater than max interval
            if (minInterval > maxInterval)
            {
                maxInterval = minInterval;
            }
            
            // Ensure pitch variation is reasonable
            pitchVariation = Mathf.Clamp(pitchVariation, 0.01f, 0.5f);
        }
    }
} 