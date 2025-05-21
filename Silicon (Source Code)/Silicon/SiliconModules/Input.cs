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
        private Dictionary<Keys, Action> keyBindings = new Dictionary<Keys, Action>();

        private readonly object keyMonitorLock = new object();
        private HashSet<Keys> keysToMonitor = new HashSet<Keys>()
        {
            Keys.W, Keys.S, Keys.A, Keys.D, Keys.Q, Keys.E,
            Keys.ShiftKey, Keys.ControlKey,
            Keys.Up, Keys.Down, Keys.Left, Keys.Right,
            Keys.F1, Keys.F2, Keys.F3, Keys.F4, Keys.F5, Keys.F6, Keys.F7, Keys.F8, Keys.F9, Keys.F10, Keys.F11
        };

        private void InitializeKeyBindings()
        {
            keyBindings = new Dictionary<Keys, Action>
            {
                [Keys.F1] = () => Preset1Button_Click(null, EventArgs.Empty),
                [Keys.F2] = () => Preset2Button_Click(null, EventArgs.Empty),
                [Keys.F3] = () => Preset3Button_Click(null, EventArgs.Empty),
                [Keys.F4] = () => Preset4Button_Click(null, EventArgs.Empty),
                [Keys.F5] = () => FreecamSwitch.Switched = !FreecamSwitch.Switched,
                [Keys.F6] = () => AddAnimationFrameButton_Click(null, EventArgs.Empty),
                [Keys.F7] = GoToPreviousFrame,
                [Keys.F8] = GoToNextFrame,
                [Keys.F9] = () => PlayAnimationButton_Click(null, EventArgs.Empty),
                [Keys.F10] = () => HideNametagsSwitch.Switched = !HideNametagsSwitch.Switched,
                [Keys.F11] = () => HideUserInterfaceSwitch.Switched = !HideUserInterfaceSwitch.Switched
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

        public void SetKeysToMonitor(IEnumerable<Keys> newKeys)
        {
            lock (keyMonitorLock)
            {
                keysToMonitor = new HashSet<Keys>(newKeys);
            }
        }

        private void UpdateKeyStates()
        {
            if (!IsCubicWindowFocused() && !IsSiliconWindowFocused())
                return;

            HashSet<Keys> currentKeys;
            lock (keyMonitorLock)
            {
                currentKeys = new HashSet<Keys>(keysToMonitor); // Copy to avoid locking during the loop
            }

            foreach (var key in currentKeys)
            {
                bool isPressed = (GetAsyncKeyState(key) & 0x8000) != 0;

                if (isPressed)
                {
                    if (!pressedKeys.Contains(key))
                    {
                        pressedKeys.Add(key);
                        HandleKeyDown(key);
                    }
                }
                else
                {
                    if (pressedKeys.Contains(key))
                    {
                        pressedKeys.Remove(key);
                        if (pressedKeys.Count == 0)
                        {
                            Thread.Sleep(10);
                        }
                    }
                }
            }
        }

        private void HandleKeyDown(Keys key)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => HandleKeyDown(key)));
                return;
            }

            if (keyBindings.TryGetValue(key, out Action action))
            {
                action.Invoke();
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
                Action action = kvp.Value;
                string actionName = GetActionNameFromDelegate(action);

                // Label for action name
                Label lbl = new Label
                {
                    Text = actionName,
                    AutoSize = true,
                    Width = 150,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Margin = new Padding(3, 6, 3, 3)
                };

                // TextBox for key capture
                TextBox txtKey = new TextBox
                {
                    Width = 100,
                    Text = currentKey.ToString(),
                    ReadOnly = true,
                    BackColor = SystemColors.Window,
                    Margin = new Padding(3, 3, 20, 3),
                    Tag = actionName
                };

                // Add placeholders for storing data
                txtKey.Tag = new KeyCaptureInfo
                {
                    ActionName = actionName,
                    CurrentKey = currentKey
                };

                // Add a small button for clearing the key assignment
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

                // Setup event to capture keyboard input
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

                txtKey.KeyDown += (sender, e) =>
                {
                    e.SuppressKeyPress = true; // Prevent beep sound

                    if (sender is TextBox tb && tb.Tag is KeyCaptureInfo info)
                    {
                        // Skip modifier keys when pressed alone
                        if (e.KeyCode == Keys.ShiftKey || e.KeyCode == Keys.ControlKey ||
                            e.KeyCode == Keys.Alt || e.KeyCode == Keys.Menu)
                            return;

                        Keys newKey = e.KeyCode;

                        // Update the key capture info
                        info.CurrentKey = newKey;
                        tb.Text = newKey.ToString();

                        // Update the key bindings dictionary
                        UpdateHotkey(info.ActionName, info.CurrentKey);

                        // Remove focus from the textbox to indicate completion
                        HotkeyPanel.Focus();
                    }
                };

                btnClear.Click += (sender, e) =>
                {
                    if (txtKey.Tag is KeyCaptureInfo info)
                    {
                        // Clear the binding (set to None)
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

        // Helper method to update a hotkey mapping
        private void UpdateHotkey(string actionName, Keys newKey)
        {
            // Find the action in the dictionary by name
            Action actionToUpdate = null;
            Keys oldKey = Keys.None;

            foreach (var kvp in keyBindings)
            {
                string currentActionName = GetActionNameFromDelegate(kvp.Value);
                if (currentActionName == actionName)
                {
                    actionToUpdate = kvp.Value;
                    oldKey = kvp.Key;
                    break;
                }
            }

            if (actionToUpdate != null)
            {
                // Remove the old mapping
                keyBindings.Remove(oldKey);

                // Add the new mapping if the key is not None
                if (newKey != Keys.None)
                {
                    // Check if new key is already in use
                    if (keyBindings.ContainsKey(newKey))
                    {
                        // Handle conflict - remove the previous action using this key
                        keyBindings.Remove(newKey);

                        // Update the UI to reflect this change
                        // You might want to refresh the panel or update specific controls
                        RefreshHotkeyPanel();
                    }

                    keyBindings[newKey] = actionToUpdate;
                }

                // Save the updated key bindings
                SaveKeyBindings();
            }
        }

        // Add this method to refresh the panel when needed
        private void RefreshHotkeyPanel()
        {
            // Store the current scroll position
            Point scrollPosition = HotkeyPanel.AutoScrollPosition;

            // Repopulate the panel
            PopulateHotkeyPanel();

            // Restore scroll position (invert because AutoScrollPosition uses negative values)
            HotkeyPanel.AutoScrollPosition = new Point(-scrollPosition.X, -scrollPosition.Y);
        }

        // Optional: Method to save key bindings
        private void SaveKeyBindings()
        {
            // Implement save logic here if needed
            // This could save to user settings, a config file, etc.
        }

        private string GetActionNameFromDelegate(Action action)
        {
            if (action == null) return "Unknown Action";

            foreach (var kvp in keyBindings)
            {
                if (kvp.Value == action)
                    return GetNameForKey(kvp.Key); // or a dictionary from Keys -> string
            }

            return "Unknown Action";
        }

        private string GetNameForKey(Keys key)
        {
            switch (key)
            {
                case Keys.F1:
                    return "Preset 1";
                case Keys.F2:
                    return "Preset 2";
                case Keys.F3:
                    return "Preset 3";
                case Keys.F4:
                    return "Preset 4";
                case Keys.F5:
                    return "Toggle Freecam";
                case Keys.F6:
                    return "Add Frame";
                case Keys.F7:
                    return "Previous Frame";
                case Keys.F8:
                    return "Next Frame";
                case Keys.F9:
                    return "Play Animation";
                case Keys.F10:
                    return "Toggle Hide Nametags";
                case Keys.F11:
                    return "Toggle Hide UI";
                default:
                    return key.ToString(); // fallback, shows key name like "F12"
            }
        }


        private void ApplyHotkeyChanges()
        {
            // Temporary dictionary to build new bindings
            var newBindings = new Dictionary<Keys, Action>();

            foreach (Panel panel in HotkeyPanel.Controls)
            {
                Label lbl = panel.Controls.OfType<Label>().FirstOrDefault();
                ComboBox combo = panel.Controls.OfType<ComboBox>().FirstOrDefault();

                if (lbl == null || combo == null)
                    continue;

                string actionName = lbl.Text;
                Keys selectedKey = (Keys)combo.SelectedItem;

                Action action = GetActionFromName(actionName);

                if (action != null)
                {
                    // Check for duplicate keys - optional: you can warn user or overwrite
                    if (!newBindings.ContainsKey(selectedKey))
                        newBindings[selectedKey] = action;
                    else
                        MessageBox.Show($"Duplicate key {selectedKey} assigned to multiple actions.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            keyBindings = newBindings;
        }

        private Action GetActionFromName(string name)
        {
            switch (name)
            {
                case "Preset 1":
                    return () => Preset1Button_Click(null, EventArgs.Empty);
                case "Preset 2":
                    return () => Preset2Button_Click(null, EventArgs.Empty);
                case "Preset 3":
                    return () => Preset3Button_Click(null, EventArgs.Empty);
                case "Preset 4":
                    return () => Preset4Button_Click(null, EventArgs.Empty);
                case "Add Frame":
                    return () => AddAnimationFrameButton_Click(null, EventArgs.Empty);
                case "Previous Frame":
                    return GoToPreviousFrame;
                case "Next Frame":
                    return GoToNextFrame;
                case "Play Animation":
                    return () => PlayAnimationButton_Click(null, EventArgs.Empty);
                case "FreecamSwitchToggle":
                    return () => FreecamSwitch.Switched = !FreecamSwitch.Switched;
                case "Hide Nametags":
                    return () => HideNametagsSwitch.Switched = !HideNametagsSwitch.Switched;
                case "Hide UI":
                    return () => HideUserInterfaceSwitch.Switched = !HideUserInterfaceSwitch.Switched;
                default:
                    return null;
            }
        }



    }
}
