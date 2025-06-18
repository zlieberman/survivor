# Campfire Wood Burning Issue - Fix Documentation

## Problem Description
The campfire's wood count was not decreasing every hour when the campfire was in the lit state. The `OnGameHourPassed()` method in `CampfireInteractable` was properly implemented, but it was never being called because the campfire was not properly registered with the `InteractableManager`.

## Root Cause
The `InteractableManager` instance was never being created in the scene. The `CampfireInteractable` was trying to register with `InteractableManager.Instance`, but since no instance existed, the registration was failing silently.

## Solution Implemented

### 1. Manual InteractableManager Creation
- **Approach**: InteractableManager should be created manually in the scene
- **Tool**: Use the `InteractableManagerCreator` component for easy creation
- **Location**: Create as a GameObject in the scene with the `InteractableManager` component

### 2. Fixed Assembly Definition References
- **File**: `Survivor/Assets/Scripts/Core/Core.asmdef`
- **Change**: Added `Survivor.Interactables` reference to fix namespace compilation errors
- **File**: `Survivor/Assets/Scripts/GameManager.asmdef`
- **Change**: Added `Survivor.Interactables` reference to fix namespace compilation errors

### 3. Enhanced Debug Logging
- **File**: `Survivor/Assets/Scripts/Interactables/CampfireInteractable.cs`
- **Changes**:
  - Added detailed logging during registration process
  - Added debug methods for testing (`TestGameHourPassed`, `ShowStatus`)
  - Added context menu items for easy testing

- **File**: `Survivor/Assets/Scripts/Core/TimeManager.cs`
- **Changes**:
  - Enhanced debug logging in `UpdateAllSystems()` method
  - Added warnings when no listeners are registered
  - Better error messages explaining the impact

### 4. Created Test Scripts
- **File**: `Survivor/Assets/Scripts/Editor/CampfireSystemTest.cs`
- **Purpose**: Provides a comprehensive test script to verify the system is working
- **File**: `Survivor/Assets/Scripts/Editor/InteractableManagerCreator.cs`
- **Purpose**: Provides a simple way to create InteractableManager in scene

## Setup Instructions

### Step 1: Create InteractableManager in Scene
1. In your scene, create a new empty GameObject
2. Name it "InteractableManager"
3. Add the `InteractableManager` component to it
4. The component will automatically set up the singleton instance

### Step 2: Alternative - Use InteractableManagerCreator
1. Add the `InteractableManagerCreator` component to any GameObject in the scene
2. Right-click on the component and select "Create InteractableManager"
3. This will automatically create the InteractableManager GameObject with the correct component

## How the System Works

1. **Manual Setup**: Create an `InteractableManager` instance in the scene
2. **Registration**: Each `CampfireInteractable` registers itself with the `InteractableManager` during its `Start()` method
3. **Hour Updates**: Every game hour, `TimeManager` calls `UpdateAllSystems()` which:
   - Finds the `InteractableManager` instance
   - Gets all registered `IGameHourListener` objects
   - Calls `OnGameHourPassed()` on each listener
4. **Wood Burning**: The campfire's `OnGameHourPassed()` method:
   - Checks if the campfire is lit
   - Decreases wood count by `woodBurnedPerHour` (default: 1)
   - Extinguishes the campfire if wood reaches 0
   - Updates the visual display

## Testing the Fix

### Method 1: Using the Test Script
1. Add the `CampfireSystemTest` component to any GameObject in the scene
2. Use the following keys:
   - `T` - Test game hour passed (manually trigger wood burning)
   - `W` - Add 5 wood to player inventory
   - `L` - Check if campfire can be lit
   - `S` - Show current status

### Method 2: Using Context Menus
1. Select a `CampfireInteractable` in the scene
2. Right-click and use:
   - "Test Game Hour Passed" - Manually trigger wood burning
   - "Show Campfire Status" - View current state

### Method 3: Using InteractableManagerCreator
1. Add the `InteractableManagerCreator` component to any GameObject in the scene
2. Right-click and use:
   - "Create InteractableManager" - Create InteractableManager if missing
   - "Check InteractableManager Status" - Verify InteractableManager exists

### Method 4: Console Commands
1. Open the Console window in Unity
2. Look for debug messages from:
   - `[CampfireInteractable]` - Registration and wood burning
   - `[TimeManager]` - Hour updates and listener management
   - `[InteractableManager]` - Registration confirmations

## Expected Behavior After Fix

1. **Registration**: You should see `[CampfireInteractable] Successfully registered with InteractableManager` in the console
2. **Hour Updates**: Every 2 minutes (120 seconds), you should see `[TimeManager] ===== GAME HOUR X PASSED =====`
3. **Wood Burning**: If the campfire is lit, you should see `[CampfireInteractable] Campfire is lit - burning 1 wood`
4. **Visual Updates**: The wood count display should update to show decreasing wood amounts

## Troubleshooting

### If wood still doesn't burn:
1. Check console for `[CampfireInteractable] InteractableManager.Instance is null` - this means InteractableManager wasn't created
2. Check console for `[TimeManager] No game hour listeners registered` - this means campfires aren't registering
3. Use the `CampfireSystemTest` script to diagnose the issue

### If you see registration errors:
1. Ensure the `InteractableManager` GameObject exists in the scene with the `InteractableManager` component
2. Use the `InteractableManagerCreator` component to create it if missing
3. Check that the `CampfireInteractable` component is properly attached to campfire objects
4. Verify that the campfire is on the "Interactable" layer

### If you see compilation errors:
1. Ensure the assembly definition files have been updated with `Survivor.Interactables` references
2. Check that all using statements are correct in the scripts
3. Rebuild the project to ensure all references are properly resolved

### If you see prefab errors:
1. The InteractableManager prefab was removed to avoid conflicts
2. Create the InteractableManager manually in the scene as described above
3. Use the `InteractableManagerCreator` component for easy creation

## Files Modified
- `Survivor/Assets/Scripts/Interactables/CampfireInteractable.cs`
- `Survivor/Assets/Scripts/Core/TimeManager.cs`
- `Survivor/Assets/Scripts/Core/Core.asmdef`
- `Survivor/Assets/Scripts/GameManager.asmdef`
- `Survivor/Assets/Scripts/Editor/CampfireSystemTest.cs` (new)
- `Survivor/Assets/Scripts/Editor/InteractableManagerCreator.cs` (new) 