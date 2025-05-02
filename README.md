# Silicon
## An Advanced Camera Mod for Cubic Castles

![Silicon Logo](https://github.com/user-attachments/assets/626ca9a7-2412-4e7e-9b4b-70ffc2c212ec)

## Table of Contents
- [Overview](#overview)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Features](#features)
  - [Default Camera Enhancements](#default-camera-enhancements)
  - [Freecam Mode](#freecam-mode)
  - [Field of View (FOV)](#field-of-view-fov)
  - [Camera Distance](#camera-distance)
  - [Hide Player Model](#hide-player-model)
  - [Cinematics System](#cinematics-system)
- [Controls](#controls)
- [User Interface Guide](#user-interface-guide)
- [Creating Cinematics](#creating-cinematics)
  - [Keyframe Animation](#keyframe-animation)
  - [Interpolation Types](#interpolation-types)
  - [Saving & Loading Animations](#saving--loading-animations)
- [Troubleshooting](#troubleshooting)
- [Advanced Settings](#advanced-settings)
- [Frequently Asked Questions](#frequently-asked-questions)
- [Contact](#contact)

## What is Silicon?
Silicon is a camera modification tool for Cubic Castles that enhances the in-game camera system with advanced features for both gameplay and cinematography.

## Installation

1. Download the latest version from the [releases page](https://github.com/TreacherousDev/Silicon/releases)
2. Run the setup wizard to install
3. Launch Cubic Castles
4. Start Silicon.exe
5. The status indicator will show "CONNECTED" in green when Silicon successfully connects to the game

**System Requirements:**
- Windows 7/8/10/11 with administrator privileges
- Cubic Castles installed and updated to the latest version

## Quick Start

1. Launch Cubic Castles and log in
2. Launch Silicon
3. Check "CONNECTED" status to verify that the program is working successfully
4. Basic camera enhancements are active by default (unlocked rotations RMB & Arrow keys)
5. Toggle "Freecam" to detach the camera from your character
6. Use WASD + Shift/Ctrl for movement and IJKL for rotation
7. Adjust FOV and Camera Distance sliders as desired

## Features

### Default Camera Enhancements

Silicon provides several improvements to the default camera system that are active immediately upon connection:

- Unlocked vertical rotation allowing full 360° viewing
- Improved camera height to center on the qbee's head instead of below its feet
- FOV and camera distance adjustment capabilities

### Freecam Mode

Freecam detaches the camera from your player character, allowing independent camera movement:

- **Toggle**: Use the Freecam switch or press F1
- **Movement**: WASD for horizontal movement, Shift/Ctrl for vertical movement
- **Rotation**: IJKL keys for pitch and yaw
- **Speed Controls**: Adjustable movement and rotation speed sliders

When Freecam is enabled, player character movement is disabled to prevent conflicts between camera and character control.

### Field of View (FOV)

Adjust the camera's field of view between 10° and 135°:

- Default setting is 33°
- Lower values create a telephoto effect with less peripheral vision
- Higher values create a wide-angle effect with more peripheral vision

### Camera Distance

Controls how far the camera is positioned from the focal point:

- Default setting is 22 units
- Setting to very low values (e.g, 1) creates a first-person-like perspective
- Higher values (30+) provide a more distant, overhead view


### Hide Player Model

Toggle player character visibility:

- Useful when using first-person-perspective
- Prevents the player model from blocking the camera view


### Cinematics System

Silicon includes a robust cinematics system for creating smooth camera animations:

- Keyframe-based animation system
- Multiple interpolation methods for different animation styles
- Save and load animation data

## Controls

### Freecam Navigation
- **W/A/S/D**: Move camera horizontally (relative to current view)
- **Shift**: Move camera down
- **Ctrl**: Move camera up
- **I/K**: Rotate camera up/down (pitch)
- **J/L**: Rotate camera left/right (yaw)

### Function Keys
- **F1**: Toggle Freecam mode

### Cinematic Animation Hotkeys
- **C**: Go to Frame 1
- **V**: Go to Frame 2
- **B**: Go to Frame 3
- **N**: Go to Frame 4
- **M**: Go to Frame 5

## User Interface Guide

Silicon's interface is divided into several sections:

### Status Bar
- **Process ID**: Shows the Cubic Castles process ID when connected
- **Status**: Shows connection status (CONNECTED or DISCONNECTED)

### Camera Controls
- **Freecam Switch**: Toggles camera detachment from player character
- **Hide Player Model Switch**: Toggles player character visibility
- **FOV Slider**: Adjusts field of view (10° - 135°)
- **Camera Distance Slider**: Adjusts camera distance (1 - 100 blocks)
- **Movement Speed Slider**: Controls camera movement speed in Freecam mode
- **Rotation Speed Slider**: Controls camera rotation speed in Freecam mode

### Cinematics Panel
- **Animation Frame List**: Displays all keyframes with position and rotation data
- **Add Frame Button**: Adds current camera position as a new keyframe
- **Delete Frame Button**: Removes selected keyframe(s)
- **Play Button**: Starts animation playback
- **Go To Frame Button**: Moves camera to selected keyframe
- **Save Animation Button**: Saves animation data to a JSON file
- **Load Animation Button**: Loads animation data from a JSON file
- **Interpolation Method**: Dropdown to select animation curve type

### Information Display
- **Camera Position**: Shows current X, Y, Z coordinates
- **Camera Rotation**: Shows current Pitch and Yaw values
- **Camera Settings**: Shows current FOV and Distance values

## Creating Cinematics

### Keyframe Animation

Silicon's cinematic system uses keyframes to create smooth camera animations:

1. Enable Freecam mode
2. Position the camera at the desired starting point
3. Click "Add Frame" to create your first keyframe
4. Move the camera to the next position
5. Click "Add Frame" again to create another keyframe
6. Continue adding keyframes as needed
7. Click "Play" to preview the animation
8. Use the Animation Frame List to select, reorder, or remove keyframes

Each keyframe stores:
- Camera position (X, Y, Z)
- Camera rotation (Pitch, Yaw)
- Movement speed

### Interpolation Types

Silicon offers different interpolation methods to control how the camera moves between keyframes:

- **Linear**: Constant speed between keyframes, no acceleration or deceleration
- **Ease In**: Starts slow and accelerates toward the end
- **Ease Out**: Starts fast and decelerates toward the end
- **Ease In/Out**: Smooth acceleration at the start and deceleration at the end
- **Cubic**: More pronounced curve than Ease In/Out

Select the interpolation method from the dropdown menu before playing the animation.

### Saving & Loading Animations

Save your animations to reuse them later:

1. Create your animation by adding keyframes
2. Click "Save Animation"
3. Choose a filename and location
4. Animation will be saved as a JSON file

To load a previously saved animation:

1. Click "Load Animation"
2. Select the JSON file
3. The keyframes will be loaded into the Animation Frame List


## Troubleshooting

### Common Issues and Solutions

**Silicon shows "DISCONNECTED" status:**
- Make sure Cubic Castles is running
- Verify you're running the latest version of both Silicon and Cubic Castles
- Disable your Windows Antivirus and / or Firewall
- Restart Silicon if the issue persists

**Controls not responding in Freecam mode:**
- Toggle Freecam off and on
- Check that no other program is intercepting your keypresses
- Restart Silicon if the issue persists

**Animation playback issues:**
- Check the info panel for any "NaN" types
- If there are, restart Silicon

**Game crashes when using Silicon:**
- Ensure you're using the latest version of Silicon
- Silicon may be out of date after the game updates. Contact the developer for a new patch


## Frequently Asked Questions

**Q: Is Silicon detectable by Cubic Castles anti-cheat systems?**
A: Silicon only modifies client side camera-related memory and doesn't send additional packets to the server that can be used to detect modified gameplay. However, use any third-party tool at your own risk.

**Q: Why does my character stop moving when Freecam is enabled?**
A: By design, Silicon disables character movement when Freecam is active to prevent control conflicts. This is normal behavior.

**Q: Can I use Silicon for streaming or recording videos?**
A: Yes! Silicon is designed with content creation in mind and works perfectly with OBS and other recording software.

**Q: Will Silicon work with future updates of Cubic Castles?**
A: Game updates might require Silicon updates if they change how the camera system works. Check for new Silicon releases after major game updates, or contact the developer if none are available.

## Contact
- @treacherousdev on Discord
- GitHub issues page for any bugs or suggestions

---

