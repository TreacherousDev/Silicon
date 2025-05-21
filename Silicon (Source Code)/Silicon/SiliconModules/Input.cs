using System;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;


namespace Silicon
{
    public partial class SiliconForm
    {

        private bool isRightDragging = false;
        private Point lastMousePos;


        private HashSet<Keys> pressedKeys = new HashSet<Keys>();      // currently held down

        private object keyMonitorLock = new object();

        private static readonly Keys[] ALL_MONITORED_KEYS = GetAllMonitorableKeys();

        private static Keys[] GetAllMonitorableKeys()
        {
            var allKeys = Enum.GetValues(typeof(Keys)).Cast<Keys>().ToList();
            return allKeys.ToArray();
        }

        private static readonly Action NoOp = () => { };

        private Dictionary<Keys, (Action onPress, Action onRelease)> keyBindings;          // for single-press actions

        public class MovementState
        {
            public bool Forward, Backward, Left, Right;
            public bool Up, Down;
            public bool YawLeft, YawRight, PitchUp, PitchDown;
            public bool RollLeft, RollRight;
        }
        private MovementState movementState = new MovementState();

        private void InitializeKeyBindings()
        {
            keyBindings = new Dictionary<Keys, (Action onPress, Action onRelease)>
            {
                [Keys.W] = (() => movementState.Forward = true, () => movementState.Forward = false),
                [Keys.S] = (() => movementState.Backward = true, () => movementState.Backward = false),
                [Keys.A] = (() => movementState.Left = true, () => movementState.Left = false),
                [Keys.D] = (() => movementState.Right = true, () => movementState.Right = false),
                [Keys.ShiftKey] = (() => movementState.Down = true, () => movementState.Down = false),
                [Keys.ControlKey] = (() => movementState.Up = true, () => movementState.Up = false),
                [Keys.Up] = (() => movementState.PitchUp = true, () => movementState.PitchUp = false),
                [Keys.Down] = (() => movementState.PitchDown = true, () => movementState.PitchDown = false),
                [Keys.Left] = (() => movementState.YawLeft = true, () => movementState.YawLeft = false),
                [Keys.Right] = (() => movementState.YawRight = true, () => movementState.YawRight = false),
                [Keys.E] = (() => movementState.RollLeft = true, () => movementState.RollLeft = false),
                [Keys.Q] = (() => movementState.RollRight = true, () => movementState.RollRight = false),

                // Instant/toggle actions – use NoOp for release
                [Keys.F1] = (() => Preset1Button_Click(null, EventArgs.Empty), NoOp),
                [Keys.F2] = (() => Preset2Button_Click(null, EventArgs.Empty), NoOp),
                [Keys.F3] = (() => Preset3Button_Click(null, EventArgs.Empty), NoOp),
                [Keys.F4] = (() => Preset4Button_Click(null, EventArgs.Empty), NoOp),
                [Keys.F5] = (() => FreecamSwitch.Switched = !FreecamSwitch.Switched, NoOp),
                [Keys.F6] = (() => HideNametagsSwitch.Switched = !HideNametagsSwitch.Switched, NoOp),
                [Keys.F7] = (() => HideUserInterfaceSwitch.Switched = !HideUserInterfaceSwitch.Switched, NoOp),
                [Keys.F8] = (() => AddAnimationFrameButton_Click(null, EventArgs.Empty), NoOp),
                [Keys.F9] = (GoToPreviousFrame, NoOp),
                [Keys.F10] = (GoToNextFrame, NoOp),
                [Keys.F11] = (() => PlayAnimationButton_Click(null, EventArgs.Empty), NoOp)   
            };
        }

        private void InitMouseDrag()
        {
            mouseHook.OnRightDown += (Point pos) =>
            {
                if (IsCubicWindowFocused())
                {
                    isRightDragging = true;
                    lastMousePos = pos;
                }
            };

            mouseHook.OnMouseMove += (Point pos) =>
            {
                if (isRightDragging)
                {
                    int dx = pos.X - lastMousePos.X;
                    int dy = pos.Y - lastMousePos.Y;

                    float sensitivity = (float)CameraRotateSpeedSlider.Value / 1000;
                    currentCameraYaw += dx * sensitivity;
                    currentCameraPitch += dy * sensitivity;
                    currentCameraPitch = Clamp(currentCameraPitch, -89f, 89f);
                    targetCameraYaw = currentCameraYaw;
                    targetCameraPitch = currentCameraPitch;

                    lastMousePos = pos;
                }
            };

            mouseHook.OnRightUp += (Point pos) =>
            {
                isRightDragging = false;
            };
        }

        private void InitMouseScroll()
        {
            mouseHook.OnScrollDown += () =>
            {
                if (!IsCubicWindowFocused())
                    return;

                if ((Control.ModifierKeys & Keys.Alt) == Keys.Alt)
                {
                    CameraDistanceSlider.Value = Math.Min(300, CameraDistanceSlider.Value + 1);
                }
                else
                {
                    CameraFOVSlider.Value = Math.Min(135, CameraFOVSlider.Value + 1);
                }
            };

            mouseHook.OnScrollUp += () =>
            {
                if (!IsCubicWindowFocused())
                    return;

                if ((Control.ModifierKeys & Keys.Alt) == Keys.Alt)
                {
                    CameraDistanceSlider.Value = Math.Max(2, CameraDistanceSlider.Value - 1);
                }
                else
                {
                    CameraFOVSlider.Value = Math.Max(10, CameraFOVSlider.Value - 1);
                }
            };
        }

        private void StartKeyPolling()
        {
            keyPollingThread = new Thread(() =>
            {
                while (isRunning)
                {
                    UpdateKeyStates();
                    Thread.Sleep(5);
                }
            })
            {
                IsBackground = true
            };
            keyPollingThread.Start();
        }

        private void UpdateKeyStates()
        {
            // Check if our window has focus before processing keys
            if (!IsCubicWindowFocused() && !IsSiliconWindowFocused())
                return;

            HashSet<Keys> currentPressedKeys = new HashSet<Keys>();

            foreach (var key in ALL_MONITORED_KEYS)
            {
                bool isPressed = (GetAsyncKeyState(key) & 0x8000) != 0;

                if (isPressed)
                {
                    currentPressedKeys.Add(key);

                    // Detect newly pressed key
                    if (!pressedKeys.Contains(key))
                    {
                        HandleKeyDown(key);
                    }
                }
            }

            // Detect key releases
            foreach (var key in pressedKeys)
            {
                if (!currentPressedKeys.Contains(key))
                {
                    HandleKeyUp(key);
                }
            }

            // Update pressedKeys to current state
            pressedKeys = currentPressedKeys;

            // Small delay to prevent high CPU usage if no keys pressed
            if (pressedKeys.Count == 0)
            {
                Thread.Sleep(10);
            }
        }

        private void HandleKeyDown(Keys key)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => HandleKeyDown(key)));
                return;
            }

            if (keyBindings.TryGetValue(key, out var action))
            {
                action.onPress();
            }
        }

        void HandleKeyUp(Keys key)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => HandleKeyUp(key)));
                return;
            }

            if (keyBindings.TryGetValue(key, out var action))
            {
                action.onRelease();
            }
        }


        // Helper class to format key names nicely
        private class KeyItem
        {
            public Keys Key { get; }

            public KeyItem(Keys key)
            {
                Key = key;
            }

            public override string ToString()
            {
                string keyName = Key.ToString();

                // Remove "D" prefix from number keys (D0-D9) to show as 0-9
                if (keyName.Length == 2 && keyName[0] == 'D' && char.IsDigit(keyName[1]))
                {
                    return keyName.Substring(1);
                }

                // Remove "OEM" prefix from keys
                if (keyName.StartsWith("Oem"))
                {
                    // For OemPeriod, OemComma, etc., convert to friendly names
                    switch (keyName)
                    {
                        case "OemPeriod": return ".";
                        case "OemComma": return ",";
                        case "OemQuestion": return "?";
                        case "OemSemicolon": return ";";
                        case "OemQuotes": return "'";
                        case "OemOpenBrackets": return "[";
                        case "OemCloseBrackets": return "]";
                        case "OemPipe": return "\\";
                        case "OemMinus": return "-";
                        case "OemPlus": return "=";
                        case "Oemtilde": return "`";
                        default:
                            // For other OEM keys, remove the "Oem" prefix
                            return keyName.Substring(3);
                    }
                }

                return keyName;
            }
        }

        private void PopulateHotkeyPanel()
        {
            HotkeyPanel.Controls.Clear();
            foreach (var kvp in keyBindings)
            {
                Keys currentKey = kvp.Key;
                var (onPress, onRelease) = kvp.Value;
                string actionName = GetActionNameFromDelegate(onPress);

                // Hotkey Label UI
                Label lbl = new Label
                {
                    Text = actionName,
                    AutoSize = true,
                    Width = 150,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Margin = new Padding(3, 6, 3, 3)
                };

                // Hotkey TextBox UI
                TextBox txtKey = new TextBox
                {
                    Width = 100,
                    Text = currentKey.ToString(),
                    ReadOnly = true,
                    BackColor = SystemColors.Window,
                    Margin = new Padding(3, 3, 20, 3),
                    Tag = actionName
                };

                // Placeholder
                txtKey.Tag = new KeyCaptureInfo
                {
                    ActionName = actionName,
                    CurrentKey = currentKey
                };

                // Clear Hotkey Button
                Button btnClear = new Button
                {
                    Text = "×",
                    Width = 24,
                    Height = 24,
                    Margin = new Padding(0, 0, 3, 0),
                    Font = new Font(FontFamily.GenericSansSerif, 10, FontStyle.Bold),
                    FlatStyle = FlatStyle.Flat,
                    TextAlign = ContentAlignment.TopCenter
                };

                // Select Hotkey Textbox
                txtKey.Enter += (sender, e) =>
                {
                    if (sender is TextBox tb)
                    {
                        tb.BackColor = Color.LightYellow;
                        tb.Text = "Press any key...";
                    }
                };

                txtKey.Leave += (sender, e) =>
                {
                    if (sender is TextBox tb && tb.Tag is KeyCaptureInfo info)
                    {
                        tb.BackColor = SystemColors.Window;
                        tb.Text = info.CurrentKey.ToString();
                    }
                };

                // Change Hotkey
                txtKey.KeyDown += (sender, e) =>
                {
                    e.SuppressKeyPress = true; // Prevent beep sound

                    if (sender is TextBox tb && tb.Tag is KeyCaptureInfo info)
                    {
                        Keys newKey = e.KeyCode;

                        // Prevent duplicate hotkey assignment
                        if (keyBindings.ContainsKey(newKey))
                        {   
                        
                                MessageBox.Show($"The key '{newKey}' is already assigned to '{GetNameForKey(newKey)}'.",
                                                "Duplicate Hotkey",
                                                MessageBoxButtons.OK,
                                                MessageBoxIcon.Warning);
                                //return;
                        }

                        info.CurrentKey = newKey;
                        tb.Text = newKey.ToString();
                        UpdateHotkey(info.ActionName, info.CurrentKey);
                        HotkeyPanel.Focus();
                    }
                };

                // Clear Hotkey
                btnClear.Click += (sender, e) =>
                {
                    if (txtKey.Tag is KeyCaptureInfo info)
                    {
                        info.CurrentKey = Keys.None;
                        txtKey.Text = "None";
                        UpdateHotkey(info.ActionName, Keys.None);
                    }
                };

                // Panel to hold controls horizontally
                Panel panel = new Panel
                {
                    Width = HotkeyPanel.ClientSize.Width - 25,
                    Height = 30,
                    Margin = new Padding(0)
                };

                lbl.Location = new Point(0, 5);
                txtKey.Location = new Point(160, 2);
                btnClear.Location = new Point(txtKey.Right + 5, 2);

                panel.Controls.Add(lbl);
                panel.Controls.Add(txtKey);
                panel.Controls.Add(btnClear);
                HotkeyPanel.Controls.Add(panel);
            }
        }

        // Helper class to store information with the controls
        private class KeyCaptureInfo
        {
            public string ActionName { get; set; }
            public Keys CurrentKey { get; set; }
        }

        private void UpdateHotkey(string actionName, Keys newKey)
        {
            // Find the tuple (onPress, onRelease) in the dictionary by name
            (Action onPress, Action onRelease) actionToUpdate = default;
            Keys oldKey = Keys.None;

            foreach (var kvp in keyBindings)
            {
                string currentActionName = GetActionNameFromDelegate(kvp.Value.onPress);
                if (currentActionName == actionName)
                {
                    actionToUpdate = kvp.Value;
                    oldKey = kvp.Key;
                    break;
                }
            }

            if (actionToUpdate.onPress != null)
            {
                // Only update if key is valid and not already bound
                if (newKey != Keys.None && !keyBindings.ContainsKey(newKey))
                {
                    keyBindings.Remove(oldKey);
                    keyBindings[newKey] = actionToUpdate;
                }

                SaveKeyBindings();
            }
        }



        private void SaveKeyBindings()
        {
            //save to json, todo
        }

        private string GetActionNameFromDelegate(Action action)
        {
            if (action == null) return "Unknown Action";

            foreach (var kvp in keyBindings)
            {
                if (kvp.Value.onPress == action || kvp.Value.onRelease == action)
                    return GetNameForKey(kvp.Key); // or a dictionary from Keys -> string
            }

            return "Unknown Action";
        }


        private string GetNameForKey(Keys key)
        {
            switch (key)
            {
                case Keys.W:
                    return "Camera Move Forward";
                case Keys.S:
                    return "Camera Move Backward";
                case Keys.A:
                    return "Camera Move Left";
                case Keys.D:
                    return "Camera Move Right";
                case Keys.ShiftKey:
                    return "Camera Move Down";
                case Keys.ControlKey:
                    return "Camera Move Up";
                case Keys.Up:
                    return "Camera Pitch Up";
                case Keys.Down:
                    return "Camera Pitch Down";
                case Keys.Left:
                    return "Camera Yaw Left";
                case Keys.Right:
                    return "Camera Yaw Right";
                case Keys.E:
                    return "Camera Roll Left";
                case Keys.Q:
                    return "Camera Roll Right";

                case Keys.F1:
                    return "Default Preset 1";
                case Keys.F2:
                    return "Default Preset 2";
                case Keys.F3:
                    return "Default Preset 3";
                case Keys.F4:
                    return "Default Preset 4";
                case Keys.F5:
                    return "Toggle Freecam";
                case Keys.F6:
                    return "Toggle Hide Nametags";
                case Keys.F7:
                    return "Toggle Hide UI";
                case Keys.F8:
                    return "Add Frame";
                case Keys.F9:
                    return "Previous Frame";
                case Keys.F10:
                    return "Next Frame";
                case Keys.F11:
                    return "Play Animation";
                default:
                    return key.ToString(); // fallback, shows key name like "F12"
            }
        }
    }
}
