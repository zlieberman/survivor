# Crest Water Interaction Setup Guide

This guide explains how to set up the swimming system to work with Crest's built-in water interaction system for proper foam and wake generation.

## Overview

The updated swimming system now integrates with Crest's water interaction system to:
- Generate foam when moving through water
- Create wakes behind the character
- Use Crest's accurate water height detection
- Fall back to collision detection if Crest is not available

## Prerequisites

1. **Crest Ocean System** must be set up in your scene
2. **Ocean Renderer** component must be present
3. **Dynamic Waves** simulation must be enabled

## Step-by-Step Setup

### Step 1: Verify Crest Ocean Setup

1. **Check Ocean Renderer**:
   - Ensure you have an Ocean Renderer in your scene
   - Verify it's properly configured with waves

2. **Enable Dynamic Waves**:
   - Select your Ocean Renderer
   - In the **Animated Waves Settings**:
     - Set **Collision Source** to **ComputeShaderQueries**
     - Enable **Dynamic Waves** if not already enabled

3. **Check Foam Settings** (optional):
   - In **Foam Settings**:
     - Enable **Foam** if you want foam effects
     - Adjust **Wave Foam Strength** and **Foam Fade Rate** as desired

### Step 2: Configure Character Swimming

1. **Select your character** in the Hierarchy

2. **Add/Configure CharacterSwimming script**:
   - Add the **CharacterSwimming** component if not already present
   - Configure the settings:

   **Swimming Settings**:
   - **Water Speed Multiplier**: 0.7 (70% speed when touching water)
   - **Swim Speed Multiplier**: 0.5 (50% speed when swimming)
   - **Swim Gravity**: -2 (reduced gravity when swimming)
   - **Swim Depth Threshold**: 0.9 (start swimming when 0.9 units deep)

   **Crest Water Interaction**:
   - **Enable Crest Interaction**: ✅ Checked (enables foam/wakes)
   - **Interaction Strength**: 1.0 (adjust for foam intensity)
   - **Interaction Radius**: 1.0 (size of interaction area)

   **Water Detection**:
   - **Water Layer**: Set to your Water layer (fallback only)
   - **Show Debug**: ✅ Checked for testing

### Step 3: Test the System

1. **Enter Play mode**

2. **Move your character into the ocean**:
   - You should see foam generated around the character
   - Wakes should appear behind the character when moving
   - Movement should slow when touching water
   - Swimming mode should activate when fully submerged

3. **Check the Console** for setup messages:
   - Should see "Added ObjectWaterInteraction component"
   - Should see "Added ObjectWaterInteractionAdaptor component"

## How It Works

### Crest Integration
- **ObjectWaterInteraction**: Generates foam and wakes based on character movement
- **ObjectWaterInteractionAdaptor**: Provides accurate water detection using Crest's system
- **SampleHeightHelper**: Gets precise water height from Crest's wave simulation

### Water Detection Priority
1. **Crest Detection** (if OceanRenderer available): Uses Crest's accurate wave height
2. **Collision Detection** (fallback): Uses Physics.OverlapSphere with water colliders

### Foam and Wake Generation
- Foam appears around the character when in water
- Wake trails appear behind the character when moving
- Intensity controlled by **Interaction Strength** setting
- Area of effect controlled by **Interaction Radius** setting

## Troubleshooting

### No Foam/Wakes Appearing
1. **Check Crest Setup**:
   - Ensure Ocean Renderer is present and configured
   - Verify Dynamic Waves are enabled
   - Check that Collision Source is set to ComputeShaderQueries

2. **Check Character Setup**:
   - Ensure **Enable Crest Interaction** is checked
   - Verify **Interaction Strength** is > 0
   - Check that character is actually moving through water

3. **Check Foam Settings**:
   - Ensure Foam is enabled in Ocean Renderer settings
   - Adjust **Wave Foam Strength** if foam is too weak/strong

### Character Not Detecting Water
1. **Check Crest Components**:
   - Look for "Added ObjectWaterInteraction component" message in Console
   - Verify ObjectWaterInteractionAdaptor component is present

2. **Fallback to Collision Detection**:
   - If Crest is not working, the system will use collision detection
   - Ensure water objects have colliders on the Water layer

### Performance Issues
1. **Reduce Interaction Radius**: Smaller radius = better performance
2. **Adjust Foam Settings**: Lower foam fade rate = less computation
3. **Check Dynamic Wave Settings**: Higher simulation frequency = more computation

## Advanced Configuration

### Foam Intensity
- **Interaction Strength**: Controls how much foam/wake is generated
- **Wave Foam Strength**: Controls overall foam intensity in Crest settings
- **Foam Fade Rate**: Controls how quickly foam disappears

### Wake Effects
- Wakes are automatically generated based on character velocity
- Larger **Interaction Radius** = wider wake trail
- Higher **Interaction Strength** = more visible wake

### Water Detection Accuracy
- Crest detection is more accurate than collision detection
- Automatically accounts for wave height variations
- Works with any Crest ocean setup (no manual collider needed)

## Integration with Other Systems

The script provides the same public properties as before:
- `IsSwimming`: Whether character is currently swimming
- `IsInWater`: Whether character is touching water
- `SubmersionDepth`: How deep the character is in water
- `WaterHeight`: Height of the water surface

You can access these from other scripts to add additional water effects while benefiting from Crest's accurate water interaction. 