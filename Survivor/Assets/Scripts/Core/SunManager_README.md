# SunManager Setup Guide

## Overview
The SunManager creates a dynamic day/night cycle with sunrise at 5 AM and sunset at 9 PM. It automatically adjusts lighting, sun position, and skybox colors based on the game time.

## Features
- **Dynamic Sun Movement**: Sun rises at 5 AM, sets at 9 PM
- **Automatic Lighting**: Smooth transitions between day and night lighting
- **Skybox Integration**: Changes sky colors based on time of day
- **Performance Optimized**: Only updates when hour changes
- **Easy Configuration**: All settings exposed in inspector

## Setup Instructions

### 1. Create Sun GameObject
1. Create an empty GameObject named "Sun"
2. Add a Light component (set Type to "Directional")
3. Add the `SunManager` script to the same GameObject
4. Position the sun at a reasonable height (e.g., Y = 100)

### 2. Configure SunManager
1. **Sun Settings**:
   - Assign the Light component to "Sun Light"
   - The sun's Transform will be auto-assigned
   - Optionally assign a skybox material

2. **Time Settings**:
   - Sunrise Hour: 5 (5 AM)
   - Sunset Hour: 21 (9 PM)
   - Adjust transition durations as needed

3. **Lighting Settings**:
   - Day Light Color: Warm daylight (default: 1, 0.95, 0.8)
   - Night Light Color: Cool moonlight (default: 0.1, 0.1, 0.3)
   - Day/Night Intensity: Adjust brightness levels
   - Temperature: Color temperature in Kelvin

4. **Skybox Settings**:
   - Day/Night Sky Colors: Main sky colors
   - Day/Night Horizon Colors: Horizon line colors

### 3. Skybox Setup (Optional)
For best results, use a procedural skybox material:
1. Create a new material
2. Set shader to "Skybox/Procedural"
3. Assign to RenderSettings.skybox
4. The SunManager will automatically update skybox colors

### 4. Integration with TimeManager
The SunManager automatically integrates with your existing TimeManager:
- Subscribes to time provider events
- Updates sun position every hour
- Works with your current 2-minute-per-hour time scale

## Configuration Examples

### Tropical Island Setting
- Sunrise: 6 AM, Sunset: 6 PM
- Day Light: Warm yellow (1, 0.9, 0.7)
- Night Light: Deep blue (0.05, 0.1, 0.3)
- Day Sky: Bright blue (0.4, 0.6, 1.0)

### Forest Setting
- Sunrise: 5 AM, Sunset: 8 PM
- Day Light: Filtered green (0.8, 0.9, 0.7)
- Night Light: Dark green (0.05, 0.1, 0.15)
- Day Sky: Muted blue (0.3, 0.5, 0.8)

## Public Methods
- `IsDay()`: Returns true if it's currently daytime
- `IsNight()`: Returns true if it's currently nighttime
- `GetDayProgress()`: Returns 0-1 progress through the day
- `GetSunriseTime()`: Returns sunrise time as (hour, minute)
- `GetSunsetTime()`: Returns sunset time as (hour, minute)

## Performance Notes
- Updates only when the hour changes (not every frame)
- Minimal impact on performance
- Can be disabled by setting enabled = false

## Troubleshooting
1. **No lighting changes**: Check that the Light component is assigned
2. **No skybox changes**: Ensure skybox material is assigned and has the correct properties
3. **Sun not moving**: Verify the sun Transform is assigned
4. **Time not syncing**: Check that TimeManager is active and registered

## Testing
Use the context menu options in the inspector:
- "Test Day Lighting": Set lighting to full day
- "Test Night Lighting": Set lighting to full night

## EnhancedSunManager Features
The EnhancedSunManager includes additional features:
- Moon lighting for night scenes
- Weather effects (clouds, fog)
- Sunrise/sunset color transitions
- Smooth frame-by-frame updates option
- More realistic lighting calculations 