# Simple Swimming System Setup Guide

This guide explains how to set up the simplified swimming system that uses Unity's built-in collision detection.

## Overview

The new swimming system is much simpler and uses:
- Unity's built-in collision detection (Physics.OverlapSphere)
- Standard layer-based water detection
- Simple speed multipliers for water resistance
- Automatic animation control via the "IsSwimming" parameter

## Unity Setup Steps

### 1. Create a Water Layer
1. Go to **Edit > Project Settings > Tags and Layers**
2. Add a new layer called "Water" (e.g., layer 8)
3. This layer will be used for all water objects

### 2. Set Up Water Objects
1. Create water objects (oceans, lakes, pools, etc.)
2. Add **Collider** components to water objects (Box Collider, Mesh Collider, etc.)
3. Set the water objects to use the **Water** layer
4. Make sure colliders are **NOT** set as triggers (they need to be solid for collision detection)

### 3. Configure the Character
1. Select your player character GameObject
2. Add the **CharacterSwimming** script component
3. Configure the settings:
   - **Water Layer**: Set to the Water layer you created (layer 8)
   - **Water Speed Multiplier**: 0.7 (70% speed when in water)
   - **Swim Speed Multiplier**: 0.5 (50% speed when swimming)
   - **Swim Gravity**: -2 (reduced gravity when swimming)
   - **Swim Depth Threshold**: 0.9 (start swimming when 0.9 units deep)
   - **Show Debug**: Enable for testing

### 4. Set Up Animation (Optional)
1. Open your character's Animator Controller
2. Add a **Bool** parameter called "IsSwimming"
3. Create swimming animations and transitions based on this parameter
4. The script will automatically set this parameter to true/false

### 5. Test the System
1. Enter Play mode
2. Move your character into water objects
3. You should see:
   - Movement slows when touching water
   - Movement slows further when fully submerged
   - Gravity reduces when swimming
   - "IsSwimming" animation parameter changes

## How It Works

### Water Detection
- Uses `Physics.OverlapSphere()` to detect water colliders
- Checks a sphere around the character's center
- Automatically detects when entering/leaving water

### Swimming States
1. **Normal**: Full speed, normal gravity
2. **In Water**: Reduced speed (waterSpeedMultiplier), normal gravity
3. **Swimming**: Further reduced speed (swimSpeedMultiplier), reduced gravity (swimGravity)

### Depth Calculation
- Calculates submersion depth based on water surface height
- Switches to swimming mode when depth exceeds swimDepthThreshold
- Uses character feet position for consistent calculation

## Troubleshooting

### Character Not Detecting Water
- Ensure water objects are on the correct layer
- Check that water colliders are not set as triggers
- Verify the Water Layer mask is set correctly in the script

### Movement Not Slowing in Water
- Check that the character has a ThirdPersonController component
- Verify the speed multipliers are set correctly
- Enable debug mode to see detection status

### Animation Not Working
- Ensure the Animator Controller has an "IsSwimming" bool parameter
- Check that the character has an Animator component
- Verify animation transitions are set up correctly

## Customization

### Speed Multipliers
- **waterSpeedMultiplier**: How much to slow down when touching water (0.5 = half speed)
- **swimSpeedMultiplier**: How much to slow down when swimming (0.3 = 30% speed)

### Swimming Threshold
- **swimDepthThreshold**: How deep the character needs to be to start swimming
- Adjust based on your character's height and water depth

### Gravity
- **swimGravity**: Gravity when swimming (negative values = downward force)
- Use -2 for realistic swimming, -1 for more buoyant swimming

## Integration with Other Systems

The script provides public properties for other systems:
- `IsSwimming`: Whether character is currently swimming
- `IsInWater`: Whether character is touching water
- `SubmersionDepth`: How deep the character is in water
- `WaterHeight`: Height of the water surface

You can access these from other scripts to add additional water effects. 