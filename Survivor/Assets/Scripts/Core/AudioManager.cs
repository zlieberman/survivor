using UnityEngine;
using UnityEngine.Audio;
using Survivor.Shared.Interfaces;
using Survivor.Shared;

namespace Survivor.Core
{
    public class AudioManager : MonoBehaviour, IAudioService
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("Audio Clips")]
        [SerializeField] private AudioClip tropicalIslandMusic;
        [SerializeField] private AudioClip[] ambientSounds;

        [Header("Audio Settings")]
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] [Range(0f, 1f)] private float musicVolume = 0.7f;
        [SerializeField] [Range(0f, 1f)] private float sfxVolume = 1f;
        [SerializeField] private bool playMusicOnStart = true;
        [SerializeField] private bool loopMusic = true;

        [Header("Fade Settings")]
        [SerializeField] private float fadeInDuration = 2f;
        [SerializeField] private float fadeOutDuration = 1f;

        private bool isMusicPlaying = false;
        private Coroutine fadeCoroutine;

        // IAudioService implementation
        public bool IsMusicPlaying => isMusicPlaying;
        public float MusicVolume => musicVolume;
        public float SFXVolume => sfxVolume;
        public AudioClip CurrentMusicClip => musicSource?.clip;

        private void Awake()
        {
            // Singleton pattern
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeAudioSources();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (playMusicOnStart && tropicalIslandMusic != null)
            {
                PlayTropicalIslandMusic();
            }
        }

        private void OnDestroy()
        {
            // Clear the service locator cache when this instance is destroyed
            if (Instance == this)
            {
                AudioServiceLocator.ClearCache();
            }
        }

        private void InitializeAudioSources()
        {
            // Create music source if it doesn't exist
            if (musicSource == null)
            {
                GameObject musicObject = new GameObject("MusicSource");
                musicObject.transform.SetParent(transform);
                musicSource = musicObject.AddComponent<AudioSource>();
                musicSource.playOnAwake = false;
                musicSource.loop = loopMusic;
                musicSource.volume = musicVolume;
            }

            // Create SFX source if it doesn't exist
            if (sfxSource == null)
            {
                GameObject sfxObject = new GameObject("SFXSource");
                sfxObject.transform.SetParent(transform);
                sfxSource = sfxObject.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
                sfxSource.volume = sfxVolume;
            }
        }

        public void PlayTropicalIslandMusic()
        {
            if (tropicalIslandMusic == null)
            {
                Debug.LogWarning("Tropical Island music clip is not assigned!");
                return;
            }

            if (isMusicPlaying)
            {
                Debug.Log("Music is already playing");
                return;
            }

            musicSource.clip = tropicalIslandMusic;
            musicSource.loop = loopMusic;
            
            if (fadeInDuration > 0f)
            {
                StartFadeIn();
            }
            else
            {
                musicSource.volume = musicVolume;
                musicSource.Play();
            }

            isMusicPlaying = true;
            Debug.Log("Started playing Tropical Island music");
        }

        public void StopMusic()
        {
            if (!isMusicPlaying) return;

            if (fadeOutDuration > 0f)
            {
                StartFadeOut();
            }
            else
            {
                musicSource.Stop();
                isMusicPlaying = false;
            }
        }

        public void PauseMusic()
        {
            if (isMusicPlaying)
            {
                musicSource.Pause();
            }
        }

        public void ResumeMusic()
        {
            if (isMusicPlaying)
            {
                musicSource.UnPause();
            }
        }

        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            if (musicSource != null)
            {
                musicSource.volume = musicVolume;
            }
        }

        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            if (sfxSource != null)
            {
                sfxSource.volume = sfxVolume;
            }
        }

        public void PlaySFX(AudioClip clip, float volume = 1f)
        {
            if (clip == null || sfxSource == null) return;

            sfxSource.PlayOneShot(clip, volume * sfxVolume);
        }

        public void PlayRandomAmbientSound()
        {
            if (ambientSounds == null || ambientSounds.Length == 0) return;

            AudioClip randomClip = ambientSounds[Random.Range(0, ambientSounds.Length)];
            PlaySFX(randomClip, 0.5f);
        }

        private void StartFadeIn()
        {
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }
            fadeCoroutine = StartCoroutine(FadeIn());
        }

        private void StartFadeOut()
        {
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }
            fadeCoroutine = StartCoroutine(FadeOut());
        }

        private System.Collections.IEnumerator FadeIn()
        {
            musicSource.volume = 0f;
            musicSource.Play();

            float elapsedTime = 0f;
            while (elapsedTime < fadeInDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / fadeInDuration;
                musicSource.volume = Mathf.Lerp(0f, musicVolume, progress);
                yield return null;
            }

            musicSource.volume = musicVolume;
        }

        private System.Collections.IEnumerator FadeOut()
        {
            float startVolume = musicSource.volume;
            float elapsedTime = 0f;

            while (elapsedTime < fadeOutDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / fadeOutDuration;
                musicSource.volume = Mathf.Lerp(startVolume, 0f, progress);
                yield return null;
            }

            musicSource.Stop();
            isMusicPlaying = false;
        }
    }
} 