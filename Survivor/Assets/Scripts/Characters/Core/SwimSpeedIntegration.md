# Swim Speed Integration Guide

## Overview
The swimming system has been updated to use a dedicated `SwimSpeed` field in the `ThirdPersonController` instead of speed multipliers. This allows the `CharacterStatModifier` to directly control swimming speed based on character stats, just like it does for movement and crawl speeds.

## How It Works

### 1. ThirdPersonController Changes
- Added `SwimSpeed` field (default: 3.0 m/s)
- Added `_isSwimming` state variable
- Added `SetSwimming(bool)` method
- Modified `Move()` method to use `SwimSpeed` when swimming

### 2. CharacterStatModifier Integration
- Added support for `"swimspeed"` controller field
- Swimming stat (0-100) can now directly control swim speed
- Uses the same linear mapping system as other speeds

### 3. CharacterSwimming Component Changes
- Removed speed multiplier approach
- Now simply sets swimming state in ThirdPersonController
- Lets CharacterStatModifier handle the actual speed value
- Only manages gravity changes and swimming state

## Configuration

### Setting Up Swim Speed Modifier
In the `CharacterStatModifier` component, add a new stat modifier:

```yaml
- statName: Swimming
  controllerField: SwimSpeed
  minValue: 1.0
  maxValue: 6.0
  multiplier: 1.0
  useNormalDistribution: true
  distributionCenter: 0.5
  distributionSpread: 0.3
  offset: 0
```

### Example Configurations

#### Fast Swimmer (High Swimming Stat)
```yaml
- statName: Swimming
  controllerField: SwimSpeed
  minValue: 2.0
  maxValue: 8.0
  multiplier: 1.0
```

#### Slow Swimmer (Low Swimming Stat)
```yaml
- statName: Swimming
  controllerField: SwimSpeed
  minValue: 0.5
  maxValue: 3.0
  multiplier: 1.0
```

#### Balanced Swimmer
```yaml
- statName: Swimming
  controllerField: SwimSpeed
  minValue: 1.0
  maxValue: 5.0
  multiplier: 1.0
```

## Usage

### For Players
1. Set the character's swimming stat (0-100)
2. The CharacterStatModifier will automatically calculate swim speed
3. When swimming, the character will use the calculated SwimSpeed value

### For NPCs
1. Set the NPC's swimming stat (0-100)
2. The system works the same way as for players
3. NPCs will swim at speeds based on their swimming stat

### Testing
Use the context menu options in CharacterStatModifier:
- **Show Current Values**: See all current stats and speeds
- **Test All Modifiers**: Manually trigger stat updates
- **Test Speed Values**: See expected speeds for different stat values

## Benefits

1. **Consistent System**: Swim speed now works exactly like move speed and crawl speed
2. **Direct Control**: Swimming stat directly controls swim speed without multipliers
3. **Easy Configuration**: Simple min/max value setup in CharacterStatModifier
4. **Stat-Based**: Characters with higher swimming stats swim faster
5. **No Conflicts**: No more fighting between swimming system and stat modifier

## Migration from Old System

If you were using the old multiplier-based system:

1. **Remove old multipliers**: The `waterSpeedMultiplier` and `swimSpeedMultiplier` are no longer used
2. **Add SwimSpeed modifier**: Configure the CharacterStatModifier with a swimming stat modifier
3. **Set swimming stats**: Ensure characters have appropriate swimming stat values (0-100)
4. **Test**: Use the context menu options to verify the system is working

## Example Character Setup

```csharp
// Set a character's swimming stat
character.SetStat("swimming", 75f); // 75% swimming ability

// This will result in a swim speed of approximately 4.5 m/s
// (assuming minValue=1, maxValue=6, multiplier=1)
```

The system is now fully integrated and ready to use! 