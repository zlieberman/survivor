# Ambient Noise System

A flexible and easy-to-use ambient noise system for your Tropical Island scene. This system allows you to add realistic ambient sounds to any object in your scene with minimal setup.

## Components Overview

### 1. AmbientNoiseEmitter
The core component that handles ambient sound playback. Can be attached to any GameObject to create localized ambient sounds.

### 2. AmbientSoundPresets
A helper component that provides preset configurations for common ambient sounds (water, wind, fire, etc.).

## Quick Start Guide

### Basic Setup

1. **Select an object** in your scene (tree, rock, water source, etc.)
2. **Add the AmbientNoiseEmitter component**:
   - Right-click on the object → Add Component → Search "AmbientNoiseEmitter"
3. **Add audio clips**:
   - Drag your audio files into the "Ambient Clips" array
   - Set the volume, timing, and other settings as needed
4. **Play!** The component will automatically start playing ambient sounds

### Using Presets (Recommended for beginners)

1. **Select an object** in your scene
2. **Add the AmbientSoundPresets component**:
   - Right-click on the object → Add Component → Search "AmbientSoundPresets"
3. **Choose a preset** from the dropdown (Water, Wind, Fire, etc.)
4. **Add appropriate audio clips** for your chosen preset
5. **The component will automatically configure** the AmbientNoiseEmitter with optimal settings

## Component Settings

### AmbientNoiseEmitter Settings

#### Audio Clips
- **Ambient Clips**: Array of audio clips that will play randomly at intervals
- **Random Clips**: Separate array for one-off random sounds

#### Playback Settings
- **Play On Start**: Automatically start playing when the scene loads
- **Loop**: Whether to continuously play ambient sounds
- **Volume**: Base volume level (0-1)
- **Pitch**: Base pitch level (0.1-3)
- **Randomize Pitch**: Add slight pitch variation for more natural sound
- **Pitch Variation**: Amount of pitch randomization

#### Timing Settings
- **Min/Max Interval**: Random time between sounds (in seconds)
- **Use Random Intervals**: Toggle between random and fixed intervals
- **Fixed Interval**: Time between sounds when not using random intervals

#### Spatial Settings
- **Use Spatial Audio**: Enable 3D positional audio
- **Max Distance**: How far the sound can be heard
- **Min Distance**: Distance at which sound is at full volume
- **Rolloff Mode**: How sound volume decreases with distance

#### Trigger Settings
- **Play On Player Enter**: Start playing when player enters trigger area
- **Stop On Player Exit**: Stop playing when player leaves trigger area
- **Player Tag**: Tag to identify the player object

#### Advanced Settings
- **Fade In/Out**: Smooth volume transitions
- **Fade Duration**: How long fade transitions take
- **Respect Audio Service Volume**: Automatically adjust to global SFX volume

### AmbientSoundPresets Settings

- **Preset Type**: Choose from predefined ambient sound types
- **Custom Clips**: Audio clips to use with custom preset

## Common Use Cases

### 1. Water Sources (Rivers, Streams, Waterfalls)

```
Setup:
- Add AmbientSoundPresets component
- Choose "Water" preset
- Add water audio clips (flowing, dripping, splashing)
- Set spatial audio enabled
- Max distance: 15-25 units
- Volume: 0.4-0.6
```

### 2. Trees and Vegetation

```
Setup:
- Add AmbientSoundPresets component
- Choose "Forest" or "Wind" preset
- Add leaf rustling, branch creaking sounds
- Set spatial audio enabled
- Max distance: 10-20 units
- Volume: 0.3-0.5
```

### 3. Campfires

```
Setup:
- Add AmbientSoundPresets component
- Choose "Fire" preset
- Add fire crackling, wood burning sounds
- Set spatial audio enabled
- Max distance: 8-15 units
- Volume: 0.5-0.7
```

### 4. Ocean and Beach

```
Setup:
- Add AmbientSoundPresets component
- Choose "Ocean" preset
- Add wave sounds, seagull calls
- Set spatial audio enabled
- Max distance: 30-50 units
- Volume: 0.6-0.8
```

### 5. Bird and Wildlife Areas

```
Setup:
- Add AmbientSoundPresets component
- Choose "Birds" or "Insects" preset
- Add bird chirps, insect sounds
- Set spatial audio enabled
- Max distance: 12-25 units
- Volume: 0.3-0.5
```

## Code Examples

### Basic Usage

```csharp
// Get the ambient noise emitter
AmbientNoiseEmitter emitter = GetComponent<AmbientNoiseEmitter>();

// Start playing ambient sounds
emitter.StartAmbientSounds();

// Stop playing
emitter.StopAmbientSounds();

// Play a single sound immediately
emitter.PlaySingleAmbientSound();

// Check if it's playing
if (emitter.IsPlaying)
{
    Debug.Log("Ambient sounds are playing");
}
```

### Using Presets

```csharp
// Get the preset component
AmbientSoundPresets presets = GetComponent<AmbientSoundPresets>();

// Change to a different preset
presets.SetPresetType(AmbientSoundPresets.AmbientPresetType.Fire);

// Set custom audio clips
AudioClip[] customClips = new AudioClip[] { clip1, clip2, clip3 };
presets.SetCustomClips(customClips);
```

### Event Handling

```csharp
AmbientNoiseEmitter emitter = GetComponent<AmbientNoiseEmitter>();

// Listen for when sounds are played
emitter.OnAmbientSoundPlayed += (clip) => {
    Debug.Log($"Playing ambient sound: {clip.name}");
};

// Listen for when sounds stop
emitter.OnAmbientSoundStopped += () => {
    Debug.Log("Ambient sounds stopped");
};
```

## Best Practices

### 1. Audio File Selection
- **Format**: Use .ogg for ambient sounds (good compression, small file size)
- **Length**: 10-30 seconds for looping ambient sounds
- **Quality**: 44.1kHz, 16-bit is sufficient for ambient sounds
- **Compression**: Use "Compressed In Memory" for ambient sounds

### 2. Volume Levels
- **Background ambient**: 0.3-0.5 volume
- **Foreground ambient**: 0.5-0.7 volume
- **Dramatic ambient**: 0.7-0.9 volume
- **Always test with other audio** to ensure good balance

### 3. Spatial Audio
- **Enable for localized sounds** (water, fire, specific areas)
- **Disable for global ambient** (wind, distant thunder)
- **Set appropriate distances** based on your scene scale
- **Use trigger colliders** for player-based activation

### 4. Performance
- **Limit concurrent ambient emitters** (aim for 5-10 per scene)
- **Use appropriate audio compression**
- **Consider distance-based culling** for large scenes
- **Test on target platforms** for performance

### 5. Audio Variety
- **Use multiple clips** for each emitter to avoid repetition
- **Randomize pitch** slightly for more natural sound
- **Vary timing intervals** to avoid predictable patterns
- **Layer different ambient types** for rich soundscapes

## Troubleshooting

### Common Issues

**No sound playing:**
- Check that audio clips are assigned
- Verify the AudioManager is in the scene
- Check that "Play On Start" is enabled
- Ensure volume is not set to 0

**Sound too loud/quiet:**
- Adjust the volume setting
- Check if "Respect Audio Service Volume" is enabled
- Verify global SFX volume in AudioManager

**Spatial audio not working:**
- Ensure "Use Spatial Audio" is enabled
- Check that the object has a Collider for trigger detection
- Verify the player has the correct tag

**Performance issues:**
- Reduce the number of concurrent ambient emitters
- Use compressed audio formats
- Increase minimum intervals between sounds
- Consider distance-based culling

**Audio cutting out:**
- Check that the GameObject is not being destroyed
- Verify audio file integrity
- Ensure AudioSource component is properly configured

## Integration with Existing Systems

The ambient noise system integrates seamlessly with your existing audio infrastructure:

- **Works with AudioManager**: Respects global volume settings
- **Uses AudioServiceLocator**: No direct dependencies
- **Compatible with existing AudioSources**: Won't conflict with other audio
- **Supports UI controls**: Can be controlled via existing audio UI

## File Structure

```
Assets/
├── Scripts/
│   └── Environment/
│       ├── AmbientNoiseEmitter.cs      # Core ambient sound component
│       ├── AmbientSoundPresets.cs      # Preset helper component
│       └── README_AmbientNoiseSystem.md # This documentation
└── [Your Audio Files]                  # Add your ambient sound files here
```

## Recommended Audio Resources

For ambient sounds, consider these sources:
- **Free**: Freesound.org, Zapsplat (free tier)
- **Paid**: AudioJungle, Unity Asset Store
- **Generated**: Tools like Audacity for creating custom ambient loops

## Support

If you encounter issues or need help setting up ambient sounds:
1. Check this documentation first
2. Verify all components are properly configured
3. Test with simple audio clips first
4. Check the Unity Console for error messages
5. Ensure your audio files are properly imported

The ambient noise system is designed to be simple yet powerful, allowing you to create rich, immersive audio environments with minimal effort! 