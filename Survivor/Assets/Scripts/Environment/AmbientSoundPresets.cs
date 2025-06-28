using UnityEngine;
using Survivor.Environment;

namespace Survivor.Environment
{
    /// <summary>
    /// Helper component that provides preset configurations for common ambient sounds.
    /// Attach this to any GameObject to quickly set up ambient noise with predefined settings.
    /// </summary>
    public class AmbientSoundPresets : MonoBehaviour
    {
        [Header("Preset Selection")]
        [SerializeField] private AmbientPresetType presetType = AmbientPresetType.Custom;
        
        [Header("Audio Clips")]
        [SerializeField] private AudioClip[] customClips;
        
        private AmbientNoiseEmitter noiseEmitter;
        
        public enum AmbientPresetType
        {
            Custom,
            Water,
            Wind,
            Fire,
            Forest,
            Ocean,
            Birds,
            Insects,
            Rain,
            Thunder
        }
        
        private void Awake()
        {
            // Get or add the AmbientNoiseEmitter component
            noiseEmitter = GetComponent<AmbientNoiseEmitter>();
            if (noiseEmitter == null)
            {
                noiseEmitter = gameObject.AddComponent<AmbientNoiseEmitter>();
            }
            
            // Apply the selected preset
            ApplyPreset(presetType);
        }
        
        private void OnValidate()
        {
            // Apply preset when changed in inspector
            if (Application.isPlaying && noiseEmitter != null)
            {
                ApplyPreset(presetType);
            }
        }
        
        /// <summary>
        /// Applies a preset configuration to the ambient noise emitter
        /// </summary>
        public void ApplyPreset(AmbientPresetType preset)
        {
            if (noiseEmitter == null) return;
            
            switch (preset)
            {
                case AmbientPresetType.Water:
                    ApplyWaterPreset();
                    break;
                case AmbientPresetType.Wind:
                    ApplyWindPreset();
                    break;
                case AmbientPresetType.Fire:
                    ApplyFirePreset();
                    break;
                case AmbientPresetType.Forest:
                    ApplyForestPreset();
                    break;
                case AmbientPresetType.Ocean:
                    ApplyOceanPreset();
                    break;
                case AmbientPresetType.Birds:
                    ApplyBirdsPreset();
                    break;
                case AmbientPresetType.Insects:
                    ApplyInsectsPreset();
                    break;
                case AmbientPresetType.Rain:
                    ApplyRainPreset();
                    break;
                case AmbientPresetType.Thunder:
                    ApplyThunderPreset();
                    break;
                case AmbientPresetType.Custom:
                    ApplyCustomPreset();
                    break;
            }
        }
        
        private void ApplyWaterPreset()
        {
            // Water sounds: gentle, continuous, medium volume
            noiseEmitter.SetVolume(0.4f);
            // Note: You'll need to assign water audio clips in the inspector
            // Recommended: water flowing, dripping, splashing sounds
        }
        
        private void ApplyWindPreset()
        {
            // Wind sounds: soft, continuous, variable volume
            noiseEmitter.SetVolume(0.3f);
            // Note: You'll need to assign wind audio clips in the inspector
            // Recommended: wind through trees, gentle breeze, wind gusts
        }
        
        private void ApplyFirePreset()
        {
            // Fire sounds: crackling, continuous, medium-high volume
            noiseEmitter.SetVolume(0.6f);
            // Note: You'll need to assign fire audio clips in the inspector
            // Recommended: wood crackling, fire burning, ember sounds
        }
        
        private void ApplyForestPreset()
        {
            // Forest sounds: various nature sounds, medium volume
            noiseEmitter.SetVolume(0.5f);
            // Note: You'll need to assign forest audio clips in the inspector
            // Recommended: leaves rustling, branches creaking, forest ambience
        }
        
        private void ApplyOceanPreset()
        {
            // Ocean sounds: waves, seagulls, medium-high volume
            noiseEmitter.SetVolume(0.7f);
            // Note: You'll need to assign ocean audio clips in the inspector
            // Recommended: wave sounds, seagull calls, ocean ambience
        }
        
        private void ApplyBirdsPreset()
        {
            // Bird sounds: chirping, calls, low-medium volume
            noiseEmitter.SetVolume(0.4f);
            // Note: You'll need to assign bird audio clips in the inspector
            // Recommended: bird chirps, calls, wing flaps
        }
        
        private void ApplyInsectsPreset()
        {
            // Insect sounds: buzzing, chirping, low volume
            noiseEmitter.SetVolume(0.3f);
            // Note: You'll need to assign insect audio clips in the inspector
            // Recommended: cricket chirps, bee buzzing, cicada sounds
        }
        
        private void ApplyRainPreset()
        {
            // Rain sounds: continuous, medium volume
            noiseEmitter.SetVolume(0.5f);
            // Note: You'll need to assign rain audio clips in the inspector
            // Recommended: rain on leaves, rain on ground, rain ambience
        }
        
        private void ApplyThunderPreset()
        {
            // Thunder sounds: occasional, loud, dramatic
            noiseEmitter.SetVolume(0.8f);
            // Note: You'll need to assign thunder audio clips in the inspector
            // Recommended: thunder claps, distant thunder, storm sounds
        }
        
        private void ApplyCustomPreset()
        {
            // Use the custom clips assigned in the inspector
            if (customClips != null && customClips.Length > 0)
            {
                noiseEmitter.SetAmbientClips(customClips);
            }
        }
        
        /// <summary>
        /// Sets the preset type and applies it
        /// </summary>
        public void SetPresetType(AmbientPresetType newPreset)
        {
            presetType = newPreset;
            ApplyPreset(presetType);
        }
        
        /// <summary>
        /// Gets the current preset type
        /// </summary>
        public AmbientPresetType GetPresetType()
        {
            return presetType;
        }
        
        /// <summary>
        /// Sets custom audio clips
        /// </summary>
        public void SetCustomClips(AudioClip[] clips)
        {
            customClips = clips;
            if (presetType == AmbientPresetType.Custom)
            {
                ApplyCustomPreset();
            }
        }
    }
} 