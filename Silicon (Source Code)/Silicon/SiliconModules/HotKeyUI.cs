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
                            "Duplicate Hotkey", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            info.CurrentKey = currentKey; 
                        }
                        else
                        {
                            info.CurrentKey = newKey;   
                        }

                        tb.Text = currentKey.ToString();
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


        