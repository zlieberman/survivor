# Underwater Visual Effects Setup Guide

This system provides realistic underwater visual effects that activate when the player is submerged in water.

## Features

- **Blue Tint**: Adds a blue underwater color filter
- **Blur Effect**: Creates a blurred underwater vision
- **Distortion**: Adds subtle camera shake and chromatic aberration
- **Smooth Transitions**: Effects fade in/out smoothly based on submersion level
- **Two Render Pipeline Support**: Works with both Built-in and Universal Render Pipeline

## Quick Setup

### Method 1: Using WaterVolume Component (Recommended)

1. **Create a WaterVolume:**
   - Create an empty GameObject
   - Add the `WaterVolume` component
   - Configure the water volume settings

2. **Configure Underwater Effects:**
   - In the "Underwater Visual Effects" section:
   - ✅ Check "Enable Underwater Effects"
   - Choose effect type:
     - **Simple Effects** (Built-in Render Pipeline): Check "Use Advanced Effects" = false
     - **Advanced Effects** (Universal Render Pipeline): Check "Use Advanced Effects" = true

3. **Customize Effects:**
   - `Underwater Tint`: Blue color for underwater filter (default: 0.2, 0.4, 0.8, 0.3)
   - `Underwater Blur`: Amount of blur effect (default: 0.5)
   - `Underwater Distortion`: Amount of distortion/shake (default: 0.1)
   - `Effect Transition Speed`: How fast effects fade in/out (default: 2)
   - `Effect Intensity`: Overall strength of effects (default: 1)

### Method 2: Manual Setup

1. **Add Effect Component to Camera:**
   - Select your main camera
   - Add Component → Scripts → Environment → SimpleUnderwaterEffect (for Built-in RP)
   - OR Add Component → Scripts → Environment → UnderwaterEffect (for URP)

2. **Configure Settings:**
   - Adjust the visual settings in the Inspector
   - Assign the player camera if not automatically detected

## Effect Types

### Simple Effects (Built-in Render Pipeline)
- ✅ Works with Built-in Render Pipeline
- ✅ No additional packages required
- ✅ Screen overlay with blue tint
- ✅ Subtle camera shake
- ✅ Color grading effects

### Advanced Effects (Universal Render Pipeline)
- ✅ Full post-processing effects
- ✅ Bloom and chromatic aberration
- ✅ Color adjustments and saturation
- ❌ Requires Universal Render Pipeline package
- ❌ More complex setup

## How It Works

1. **Detection**: The water system detects when the player is submerged
2. **Submersion Level**: Calculates how much of the player is underwater (0-100%)
3. **Effect Application**: Applies visual effects based on submersion level
4. **Smooth Transitions**: Effects fade in/out smoothly as the player enters/exits water

## Customization

### Runtime Configuration
```csharp
// Get the underwater effect
SimpleUnderwaterEffect effect = FindObjectOfType<SimpleUnderwaterEffect>();

// Change tint color
effect.SetUnderwaterTint(new Color(0.1f, 0.3f, 0.7f, 0.4f));

// Adjust blur amount
effect.SetUnderwaterBlur(0.8f);

// Change distortion
effect.SetUnderwaterDistortion(0.2f);

// Modify transition speed
effect.SetTransitionSpeed(3f);

// Set overall intensity
effect.SetEffectIntensity(1.5f);
```

### Multiple Water Volumes
- Each water volume can have different effect settings
- The system will use the settings from the water volume the player is currently in
- Effects transition smoothly between different water volumes

## Troubleshooting

### Effects Not Working
1. **Check Enable Underwater Effects**: Make sure it's enabled on the WaterVolume
2. **Verify Camera**: Ensure the camera is properly assigned
3. **Check Render Pipeline**: Use SimpleUnderwaterEffect for Built-in RP, UnderwaterEffect for URP
4. **Console Errors**: Check for any error messages in the console

### Performance Issues
1. **Reduce Effect Intensity**: Lower the effect intensity value
2. **Disable Advanced Effects**: Use simple effects instead of advanced ones
3. **Adjust Transition Speed**: Lower values for smoother but more expensive transitions

### Visual Quality
1. **Adjust Tint Color**: Modify the underwater tint for different water types
2. **Fine-tune Blur**: Increase/decrease blur for different water clarity
3. **Modify Distortion**: Adjust distortion for different water movement

## Example Configurations

### Clear Tropical Water
- Tint: (0.1, 0.4, 0.8, 0.2)
- Blur: 0.3
- Distortion: 0.05
- Intensity: 0.8

### Murky Swamp Water
- Tint: (0.1, 0.2, 0.3, 0.5)
- Blur: 0.7
- Distortion: 0.15
- Intensity: 1.2

### Deep Ocean Water
- Tint: (0.05, 0.1, 0.3, 0.6)
- Blur: 0.8
- Distortion: 0.1
- Intensity: 1.5

## Integration with Other Systems

The underwater effects work seamlessly with:
- **Swimming System**: Effects activate when swimming
- **Water Physics**: Buoyancy and drag systems
- **Character Controller**: Starter Assets integration
- **Post-processing**: Compatible with existing post-processing effects 