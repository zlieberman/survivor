# Swimming Animation Setup Guide

## Overview
This guide explains how to set up swimming animations from Mixamo to work with the swimming system.

## Step 1: Download Swimming Animation from Mixamo
1. Go to [Mixamo](https://www.mixamo.com/)
2. Search for "swimming" animations
3. Recommended animations:
   - "Swimming" (basic swimming motion)
   - "Swimming Backstroke" (for variety)
   - "Swimming Breaststroke" (for variety)
4. Download the animations in FBX format

## Step 2: Import into Unity
1. Drag the downloaded FBX files into your Assets folder
2. Select each animation file in the Project window
3. In the Inspector, make sure:
   - **Rig** tab: Set Animation Type to "Humanoid"
   - **Animation** tab: Set Loop Time to true
   - **Animation** tab: Set Loop Pose to true

## Step 3: Set Up Animator Controller
1. Open your player's Animator Controller
2. Add a new Bool parameter called "Swimming"
3. Create a new state called "Swimming"
4. Assign your swimming animation to this state
5. Create transitions:
   - From "Idle" to "Swimming" (when Swimming = true)
   - From "Walking" to "Swimming" (when Swimming = true)
   - From "Running" to "Swimming" (when Swimming = true)
   - From "Swimming" to "Idle" (when Swimming = false)
   - From "Swimming" to "Walking" (when Swimming = false)

## Step 4: Configure Transitions
For each transition TO swimming:
- Set condition: Swimming = true
- Set transition duration: 0.1 seconds
- Enable "Has Exit Time" = false

For each transition FROM swimming:
- Set condition: Swimming = false
- Set transition duration: 0.1 seconds
- Enable "Has Exit Time" = false

## Step 5: Test the Animation
1. Enter Play mode
2. Walk into water until you're 1 unit deep
3. The character should switch to swimming animation
4. Move around to see the swimming motion

## Troubleshooting
- **Animation not playing**: Check that the "Swimming" parameter is being set correctly
- **Sharp transitions**: Increase transition duration
- **Animation loops**: Make sure Loop Time is enabled in the animation import settings
- **Wrong character**: Ensure the animation is compatible with your character's rig

## Advanced Setup
For more realistic swimming, consider:
- Adding different swimming animations for different depths
- Creating blend trees for smooth transitions between swimming styles
- Adding particle effects for water splashes
- Using animation events to trigger sound effects 