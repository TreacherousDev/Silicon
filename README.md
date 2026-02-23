## What is Silicon?  
Silicon is an advanced camera mod for Cubic Castles that enhances the in-game camera system with powerful features for both gameplay and cinematography.

![SiliconGIF-ezgif com-video-to-gif-converter(1)](https://github.com/user-attachments/assets/78887645-bd76-4817-a8bc-8cbbd22076a9)

<img width="523" height="432" alt="image" src="https://github.com/user-attachments/assets/f712c2af-9a8f-4e9a-bddb-f5bf7135083a" />

*last updated: v2.10

## Table of Contents
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Features](#features)
- [Creating Cinematics](#creating-cinematics)
  - [Keyframe Animation](#keyframe-animation)
  - [Interpolation Types](#interpolation-types)
  - [Saving & Loading Animations](#saving--loading-animations)
- [Hotkeys](#hotkeys)
- [Troubleshooting](#troubleshooting)
- [Frequently Asked Questions](#frequently-asked-questions)
- [Contact](#contact)


---
## Installation

1. Download the latest version from the [releases page](https://github.com/TreacherousDev/Silicon/releases)
2. Run the setup wizard to install

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
  Roll the camera sideways with `Q` and `E`
  Rotate the camera with `RMB` + mouse drag or `Arrow Keys`    
5. Fine-tune your view using the sliders:
   **Camera Sensitivity:** Camera pan speed when right click dragging   
   **Field of View:** Vanilla zoom method, expanded to a wider range of 10° to 135°
   **Distance to Focal Point:** Set to the lowest value to emulate 1st Person FOV   
   **Sight Range:** Game fog visibility   
7. For clean screenshots or cinematics, use:
   **Hide Player:** to make your character invisible (Recommended for 1st Person POV)  
   **Hide UI:** to remove interface elements  
   **Hide Nametags:** to disable floating names above players (Recommended for 1st Person POV)  

---
## Features
Silicon provides several improvements to the default camera system that are active immediately upon connection:
- Unlocked vertical rotation allowing you to see blocks from below
- Improved camera height to center on the qbee's head instead of below its feet
- Overwrites in-game scroll and right click drag keybinds for smoothness and adjustability

---
### Field of View

![2026-02-2405-22-03-ezgif com-video-to-gif-converter](https://github.com/user-attachments/assets/b762d61a-3d5c-498b-9d47-7d46d4e6b7f9)

Controls the camera's field of view between 10° and 135°:
- Default setting is 33°
- Lower values create a telephoto effect with less peripheral vision
- Higher values create a wide-angle effect with more peripheral vision
---
### Distance to Focal Point

![2026-02-2405-23-21-ezgif com-video-to-gif-converter](https://github.com/user-attachments/assets/0579da2f-c97c-4071-b8c8-ff2357ea3bf4)

Controls how far the camera is positioned from the focal point:
- Default setting is 22 units
- Setting to very low values (e.g, 1) creates a first-person-like perspective
- Higher values (30+) provide a more distant, overhead view
---
### Sight Range

![2026-02-2404-40-47-ezgif com-video-to-gif-converter](https://github.com/user-attachments/assets/385b8356-f2dc-4f55-a074-e9f69ea745f7)

Controls the visibility of the in game fog:
- Default setting is 110 blocks
- Lower values bring the fog closer, limiting how much of the world is visible
- Higher values push the fog farther away, revealing more of the environment

---
### Hide Player

![2026-02-2404-47-31-ezgif com-video-to-gif-converter](https://github.com/user-attachments/assets/06f1d70f-8c54-4692-91df-2f13af7cdb60)

Toggle player character visibility:
- Useful when using first-person-perspective
- Prevents the player model from blocking the camera view

---
### Hide UI

![2026-02-2404-48-30-ezgif com-video-to-gif-converter](https://github.com/user-attachments/assets/22bea016-c76d-4c21-8e18-5d09a09e7d75)

Toggle the visibility of on-screen interface elements:
- Hides HUD elements: health, chat, level, cubits and inventory
- Useful for taking clean screenshots or recording cinematic scenes

---
### Hide Nametags

![2026-02-2404-49-13-ezgif com-video-to-gif-converter](https://github.com/user-attachments/assets/625f0c66-aaf3-4205-8633-b97ad1e72ab7)


Toggle the visibility of player and entity nametags:
- Removes floating names above players
- Recommended to have on when using 1st person POV

---
### Freecam Mode

![2026-02-2404-53-40-ezgif com-optimize](https://github.com/user-attachments/assets/9f63d4d6-cbe7-4ebb-a4c9-0a60dc58ddd6)

Freecam detaches the camera from your player character, allowing independent camera movement:
- **Toggle**: Use the Freecam switch or press F5
- **Movement**: WASD for horizontal movement, Spacebar/Ctrl for vertical movement
- **Rotation**: Arrow Keys or Right Click to look around, Q and E to roll sideways
- **Speed Sliders**: Controls how fast the camera moves or rotates while in Freecam mode
  
When Freecam is enabled, player character movement is disabled to prevent conflicts between camera and character control.

---
### Default Presets

![2026-02-2404-56-55-ezgif com-video-to-gif-converter](https://github.com/user-attachments/assets/0a7f5f53-837b-4825-81b6-c76718d7726d)

Shortcut keybinds to reset the camera layout to useful configurations:
- **1**: Standard Realm View
- **2**: Standard Overworld View
- **3**: First Person POV
- **4**: /Skycam
  
---
### Cinematics System

![2026-02-2405-02-58-ezgif com-optimize(1)](https://github.com/user-attachments/assets/e5ae1cc4-38ad-49f8-a867-286f3f8066fa)

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
6. Check and update the movement speed, FOV and sight range as needed
7. Click "Add Frame" again to create another keyframe
8. Repeat steps 5–7 to continue building your animation
9. Once finished, click "Play" to preview the animation
10. Use the Animation Frame List to select, reorder, or remove keyframes

Toggling the reverse button will play the animation in reverse, from the currently selected keyframe going up to the first.  
You can reorder keyframes by selecting a row and dragging it into its preferred position. Double clicking a row also allows you to manually edit its content.

<img width="975" height="586" alt="image" src="https://github.com/user-attachments/assets/f7b5c62e-46a7-4d09-8adf-8e5514d871a0" />


Each keyframe stores:
- Camera position (X, Y, Z)
- Camera rotation (Pitch, Yaw. Roll)
- Field of View
- Sight Range
- Movement speed

### Interpolation Types

Silicon provides several interpolation methods to define how the camera transitions between keyframes. These affect the speed curve of the movement:  

| **Interpolation Method** | **Description**                                                               |
| ------------------------ | ----------------------------------------------------------------------------- |
| **Linear**               | Moves at a constant speed between keyframes.                                  |
| **Ease**                 | Applies a subtle, smooth curve — a basic ease that feels more natural.        |
| **Ease In**              | Starts slowly and accelerates toward the next keyframe.                       |
| **Ease Out**             | Starts quickly and slows down near the end.                                   |
| **Ease In Out**          | Smooth acceleration at the beginning and deceleration at the end.             |
| **Exponential In**       | Begins extremely slow, then rapidly speeds up.                                |
| **Exponential Out**      | Begins fast and quickly eases into a stop.                                    |
| **Exponential In Out**   | Starts very slowly, accelerates rapidly in the middle, and slows down at end. |
| **Catmull-Rom**          | Creates a smooth curved path along all keyframes. Use this for cinematics.    |

Interpolation type applies to the entire animation and is not stored as file data.
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

## Hotkeys
The Hotkeys tab allows you to change or set keybinds for most of the tools present. 

To change a hotkey, click on the corresponding text box and enter your new input key. Do note that hotkeys work only when Cubic Castles is the focused window, preventing unwanted changes when tabbed out and working on something else in the foreground. Preferred keybinds are automatically saved as a file located in Silicon's installation directory, allowing it to remember preferences in future sessions.

**Default Hotkeys Confuguration:**   
- Camera Move Forward: `W`  
- Camera Move Backward: `S`  
- Camera Move Left: `A`  
- Camera Move Right: `D`  
- Camera Move Down: `Ctrl`  
- Camera Move Up: `Shift`  
- Camera Pitch Up: `↑`  
- Camera Pitch Down: `↓`  
- Camera Yaw Left: `←`  
- Camera Yaw Right: `→`  
- Camera Roll Left: `E`  
- Camera Rill Right: `Q`  
- Default Preset 1: `F1`  
- Default Preset 2: `F2`  
- Default Preset 3: `F3`  
- Default Preset 4: `F4`  
- Toggle Freecam: `F5`  
- Toggle Hide Nametags: `F6`  
- Toggle Hide UI: `F7`  
- Add New Frame: `F8`  
- Go To Previous Frame: `F9`  
- Go To Next Frame: `F10`  
- Play / Stop Animation: `F11`  

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
**Q: My computer says this program is a Virus!**  
A: Silicon uses memory injection to alter the behaviour of a program (Cubic Castles in this case). Malicious software use the same method to do bad things, which is why most antivirus will flag it as dangerous. 

**Q: Is Silicon detectable by Cubic Castles anti-cheat systems?**  
A: Silicon only modifies client side camera-related memory and doesn't send additional packets to the server that can be used to detect modified gameplay. However, use any third-party tool at your own risk.

**Q: Why does my character stop moving when Freecam is enabled?**  
A: By design, Silicon disables character movement when Freecam is active to prevent control conflicts. This is normal behavior.

**Q: I closed Silicon and now I can't move my character in-game!**  
A: Silicon overwrites the game's movement and camera inputs for smoothness and customizabiity. This also means that when Silicon is closed, these inputs no longer work. Close the game and open another instance to revert back to normal.

**Q: Will Silicon work with future updates of Cubic Castles?**  
A: Game updates require Silicon updates if they change the byte addresses of the camera functions (which is almost always). Check for new Silicon releases after major game updates, or contact the developer if none are available.

**Q: Will Silicon be released on Android / iOS?**  
A: No.

---
## Contact
- @treacherousdev on Discord
- GitHub issues page for any bugs or suggestions

---

