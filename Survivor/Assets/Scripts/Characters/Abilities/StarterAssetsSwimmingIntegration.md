# Starter Assets Swimming Integration Guide

This guide explains how to integrate the swimming system with Unity's Starter Assets animation controller to create proper swimming animations and floating behavior.

## Overview

The goal is to:
1. Add a swimming animation state to the Starter Assets animator controller
2. Create smooth transitions between walking/running and swimming
3. Make the character float on the water surface when swimming
4. Use the existing `IsSwimming` parameter from the CharacterSwimming script

## Step 1: Add Swimming Parameter to Animator Controller

### 1.1 Open the Animator Controller
1. Find your character's **Animator** component
2. Click on the **Controller** field to open the Animator window
3. The controller should be `StarterAssetsThirdPerson`

### 1.2 Add IsSwimming Parameter
1. In the **Parameters** tab, click the **+** button
2. Select **Bool**
3. Name it exactly `IsSwimming`
4. Set default value to **false**

## Step 2: Create Swimming Animation State

### 2.1 Add Swimming Animation Clip
1. **Import a swimming animation** (or create one):
   - You can find swimming animations on the Asset Store
   - Or use a modified version of existing animations
   - Or create a simple floating animation

2. **Create the animation clip**:
   - Right-click in Project window → Create → Animation Clip
   - Name it "Swimming" or "Swim"
   - Set it to loop

### 2.2 Add Swimming State to Animator
1. In the **Animator** window, right-click in empty space
2. Select **Create State** → **Empty**
3. Name it "Swimming"
4. Drag your swimming animation clip into the **Motion** field

## Step 3: Create Animation Transitions

### 3.1 Transition from Ground Movement to Swimming
1. **Right-click on "Grounded" state** (the main movement state)
2. Select **Make Transition**
3. Click on the **"Swimming" state**
4. In the transition settings:
   - **Has Exit Time**: Unchecked
   - **Transition Duration**: 0.25
   - **Conditions**: Add condition
     - **Parameter**: IsSwimming
     - **Condition**: Greater than 0 (or just check the box)

### 3.2 Transition from Swimming to Ground Movement
1. **Right-click on "Swimming" state**
2. Select **Make Transition**
3. Click on the **"Grounded" state**
4. In the transition settings:
   - **Has Exit Time**: Unchecked
   - **Transition Duration**: 0.25
   - **Conditions**: Add condition
     - **Parameter**: IsSwimming
     - **Condition**: Less than 1 (or just uncheck the box)

### 3.3 Transition from Air States to Swimming
1. **From "InAir" state to "Swimming"**:
   - Same transition setup as above
   - Use IsSwimming parameter

2. **From "JumpStart" state to "Swimming"**:
   - Same transition setup
   - Use IsSwimming parameter

## Step 4: Configure Swimming Animation

### 4.1 Swimming State Settings
1. **Select the "Swimming" state**
2. In the Inspector:
   - **Speed**: 1.0 (or adjust as needed)
   - **Motion**: Your swimming animation clip
   - **Foot IK**: Disabled (since swimming doesn't need foot IK)

### 4.2 Animation Clip Settings
1. **Select your swimming animation clip**
2. In the Inspector:
   - **Loop Time**: Checked
   - **Loop Pose**: Checked
   - **Cycle Offset**: 0
   - **Root Transform Position (Y)**: Keep Original
   - **Root Transform Rotation (Y)**: Keep Original

## Step 5: Create Floating Swimming Animation (Optional)

If you want the character to float on the water surface:

### 5.1 Create Floating Animation
1. **Duplicate your swimming animation**
2. **Modify the root motion**:
   - Reduce or remove forward movement
   - Add gentle up/down floating motion
   - Add slight side-to-side sway

### 5.2 Alternative: Use Blend Tree
1. **Create a Blend Tree** in the Swimming state
2. **Add two motions**:
   - Motion 1: Swimming forward animation
   - Motion 2: Floating in place animation
3. **Blend Parameter**: Use Speed (0 = floating, higher = swimming)

## Step 6: Update CharacterSwimming Script

The script already sets the `IsSwimming` parameter, but you might want to add floating behavior:

```csharp
// In the StartSwimming() method, add:
if (animator != null)
{
    animator.SetBool(isSwimmingHash, true);
    // Optionally set speed to 0 for floating
    animator.SetFloat("Speed", 0f);
}

// In the StopSwimming() method, add:
if (animator != null)
{
    animator.SetBool(isSwimmingHash, false);
    // Restore normal speed control
    animator.SetFloat("Speed", _speed);
}
```

## Step 7: Test the Integration

### 7.1 Test Animation Transitions
1. **Enter Play mode**
2. **Move character into water**
3. **Verify transitions**:
   - Walking → Swimming (when submerged)
   - Swimming → Walking (when leaving water)
   - Jumping → Swimming (if jumping into water)

### 7.2 Test Floating Behavior
1. **Stop moving** while swimming
2. **Character should float** on water surface
3. **Movement should resume** when input is given

## Step 8: Fine-tune the Experience

### 8.1 Adjust Transition Timing
- **Faster transitions**: Reduce Transition Duration (0.1-0.2)
- **Smoother transitions**: Increase Transition Duration (0.3-0.5)

### 8.2 Adjust Swimming Speed
- **In CharacterSwimming script**: Modify `swimSpeedMultiplier`
- **In animation**: Adjust the Speed parameter of the Swimming state

### 8.3 Add Swimming Sound Effects
- **Create swimming audio clips**
- **Add AudioSource** to character
- **Play sounds** when `IsSwimming` becomes true

## Troubleshooting

### Animation Not Playing
1. **Check parameter name**: Must be exactly "IsSwimming"
2. **Check transition conditions**: Ensure they're set correctly
3. **Check animation clip**: Make sure it's assigned to the state

### Character Not Floating
1. **Check gravity settings**: `swimGravity` should be low (like -2)
2. **Check animation**: Ensure swimming animation doesn't have forward motion
3. **Check speed**: Set Speed to 0 when swimming for floating

### Transitions Too Abrupt
1. **Increase transition duration**: 0.3-0.5 seconds
2. **Add exit time**: Check "Has Exit Time" for smoother transitions
3. **Adjust transition offset**: Fine-tune when transitions occur

## Advanced Features

### 8.1 Swimming Speed Variations
- **Slow swimming**: Speed 0.5-1.0
- **Fast swimming**: Speed 1.5-2.0
- **Use input magnitude** to control swimming speed

### 8.2 Swimming Direction
- **Forward swimming**: Normal swimming animation
- **Backward swimming**: Reverse animation or separate clip
- **Side swimming**: Separate left/right swimming animations

### 8.3 Swimming with Items
- **Weapon swimming**: Different animation when holding weapons
- **Item swimming**: Different animation when carrying items
- **Use animation layers** for additive swimming animations

This integration will give you a complete swimming system that works seamlessly with the Starter Assets animation controller! 