## What is Silicon?  
Silicon is an advanced camera mod for Cubic Castles that enhances the in-game camera system with powerful features for both gameplay and cinematography.

![image](https://github.com/user-attachments/assets/b4971739-037d-423c-88f4-729547f5255b)


## Table of Contents
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Features](#features)
- [Creating Cinematics](#creating-cinematics)
  - [Keyframe Animation](#keyframe-animation)
  - [Interpolation Types](#interpolation-types)
  - [Saving & Loading Animations](#saving--loading-animations)
- [Troubleshooting](#troubleshooting)
- [Frequently Asked Questions](#frequently-asked-questions)
- [Contact](#contact)


---
## Installation

1. Download the latest version from the [releases page](https://github.com/TreacherousDev/Silicon/releases)
2. Run the setup wizard to install
3. Launch Cubic Castles
4. Start Silicon.exe
5. The status indicator will show "CONNECTED" in green when Silicon successfully connects to the game

**System Requirements:**
- Windows 7/8/10/11 with administrator privileges
- Cubic Castles installed and updated to the latest version
  
---
## Quick Start

Follow these steps to quickly set up and begin using Silicon with Cubic Castles:
1. Launch Cubic Castles and log into your account
2. Launch Silicon after Cubic Castles is open.
3. In the information tab, check that the "CONNECTED" indicator is active. This confirms Silicon is successfully linked to the game.
4. Toggle the "Freecam" button to detach the camera from your character.  
  Move with `W` `A` `S` `D` keys   
  Ascend with `Shift`, descend with `Ctrl`   
  Rotate the camera with `RMB` + mouse drag or `Arrow Keys`    
5. Fine-tune your view using the sliders:  
   **Distance to Focal Point:** Set to the lowest value to emulate 1st Person FOV    
   **Field of View:** Vanilla zoom method, expanded to a wider range of 10° to 135°    
   **Sight Range:** Game fog visibility   
6. For clean screenshots or cinematics, use:  
   **Hide UI:** to remove interface elements  
   **Hide Player:** to make your character invisible (Recommended for 1st Person POV)  
   **Hide Nametags:** to disable floating names above players (Recommended for 1st Person POV)  

---
## Features
Silicon provides several improvements to the default camera system that are active immediately upon connection:
- Unlocked vertical rotation allowing you to see blocks from below
- Improved camera height to center on the qbee's head instead of below its feet

---
### Distance to Focal Point
Controls how far the camera is positioned from the focal point:
- Default setting is 22 units
- Setting to very low values (e.g, 1) creates a first-person-like perspective
- Higher values (30+) provide a more distant, overhead view

---
### Field of View
Controls the camera's field of view between 10° and 135°:
- Default setting is 33°
- Lower values create a telephoto effect with less peripheral vision
- Higher values create a wide-angle effect with more peripheral vision

---
### Sight Range
Controls the visibility of the in game fog:
- Default setting is 110 blocks
- Lower values bring the fog closer, limiting how much of the world is visible
- Higher values push the fog farther away, revealing more of the environment

---
### Hide Player
Toggle player character visibility:
- Useful when using first-person-perspective
- Prevents the player model from blocking the camera view

---
### Hide UI
Toggle the visibility of on-screen interface elements:
- Hides HUD elements: health, chat, level, cubits and inventory
- Useful for taking clean screenshots or recording cinematic scenes

---
### Hide Nametags
Toggle the visibility of player and entity nametags:
- Removes floating names above players
- Recommended to have on when using 1st person POV

---
### Freecam Mode
Freecam detaches the camera from your player character, allowing independent camera movement:
- **Toggle**: Use the Freecam switch or press F4
- **Movement**: WASD for horizontal movement, Shift/Ctrl for vertical movement
- **Rotation**: Arrow Keys or Right Click to look around
- **Speed Sliders**: Controls how fast the camera moves or rotates while in Freecam mode
When Freecam is enabled, player character movement is disabled to prevent conflicts between camera and character control.

---
### Cinematics System
Silicon includes a robust cinematics system for creating smooth camera animations:
- Keyframe-based animation system
- Multiple interpolation methods
- Save and load animation data
  
---

## Creating Cinematics
### Keyframe Animation

Silicon's cinematic system uses keyframes to create smooth camera animations. Here's a basic guide to get started:
1. Enable Freecam mode
2. Position the camera at the desired starting point
3. Set the desired movement speed and interpolation method
4. Click "Add Frame" to create your first keyframe
5. Move the camera to the next position
6. Check and update the movement speed and interpolation method if needed
7. Click "Add Frame" again to create another keyframe
8. Repeat steps 5–7 to continue building your animation
9. Once finished, click "Play" to preview the animation
10. Use the Animation Frame List to select, reorder, or remove keyframes

Each keyframe stores:
- Camera position (X, Y, Z)
- Camera rotation (Pitch, Yaw)
- Movement speed
- Interpolation type

### Interpolation Types

Silicon provides several interpolation methods to define how the camera transitions between keyframes. These affect the speed curve of the movement:  

| **Interpolation Method** | **Code** | **Description**                                                               |
| ------------------------ | -------- | ----------------------------------------------------------------------------- |
| **Linear**               | 0        | Moves at a constant speed between keyframes.                                  |
| **Ease**                 | 1        | Applies a subtle, smooth curve — a basic ease that feels more natural.        |
| **Ease In**              | 2        | Starts slowly and accelerates toward the next keyframe.                       |
| **Ease Out**             | 3        | Starts quickly and slows down near the end.                                   |
| **Ease In Out**          | 4        | Smooth acceleration at the beginning and deceleration at the end.             |
| **Exponential In**       | 5        | Begins extremely slow, then rapidly speeds up.                                |
| **Exponential Out**      | 6        | Begins fast and quickly eases into a stop.                                    |
| **Exponential In Out**   | 7        | Starts very slowly, accelerates rapidly in the middle, and slows down at end. |


Each method is stored in the animation data as a numeric code from 0 to 7, matching the order above. You can assign different interpolation types to each segment for more cinematic and expressive camera paths.

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

---
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
  
---

## Frequently Asked Questions

**Q: Is Silicon detectable by Cubic Castles anti-cheat systems?**  
A: Silicon only modifies client side camera-related memory and doesn't send additional packets to the server that can be used to detect modified gameplay. However, use any third-party tool at your own risk.

**Q: Why does my character stop moving when Freecam is enabled?**  
A: By design, Silicon disables character movement when Freecam is active to prevent control conflicts. This is normal behavior.

**Q: Will Silicon work with future updates of Cubic Castles?**  
A: Game updates  require Silicon updates if they change the byte addresses of the camera functions (which is almost always). Check for new Silicon releases after major game updates, or contact the developer if none are available.

---
## Contact
- @treacherousdev on Discord
- GitHub issues page for any bugs or suggestions

---

