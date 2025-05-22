using System;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;


namespace Silicon
{
    public partial class SiliconForm
    {
        // Helper classes to store information with the controls
        private class KeyCaptureInfo
        {
            public string ActionName { get; set; }
            public Keys CurrentKey { get; set; }
            public int Index { get; set; }
        }
        private class ClearButtonInfo
        {
            public int Index { get; set; }
        }
        public class KeyBindingData
        {
            public Keys Key { get; set; }
            public string OnPressActionName { get; set; }
            public string OnReleaseActionName { get; set; }
        }

        private readonly List<string> ActionNames = new List<string>()
        {
            "Camera Move Forward",
            "Camera Move Backward",
            "Camera Move Left",
            "Camera Move Right",
            "Camera Move Down",
            "Camera Move Up",
            "Camera Pitch Up",
            "Camera Pitch Down",
            "Camera Yaw Left",
            "Camera Yaw Right",
            "Camera Roll Left",
            "Camera Roll Right",
            "Default Preset 1",
            "Default Preset 2",
            "Default Preset 3",
            "Default Preset 4",
            "Toggle Freecam",
            "Toggle Hide Nametags",
            "Toggle Hide UI",
            "Add Frame",
            "Previous Frame",
            "Next Frame",
            "Play Animation"
        };

        public void SaveKeyBindings()
        {
            string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Silicon");
            Directory.CreateDirectory(appDataPath);

            string filePath = Path.Combine(appDataPath, "keybindings.json");

            var keysToSave = keyBindings.Keys.ToList();
            string json = JsonSerializer.Serialize(keysToSave);

            File.WriteAllText(filePath, json);
        }

        public void LoadKeyBindings()
        {
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Silicon", "keybindings.json");

            if (!File.Exists(filePath))
                return;

            string json = File.ReadAllText(filePath);
            var loadedKeys = JsonSerializer.Deserialize<List<Keys>>(json);

            keyBindings = new Dictionary<Keys, (Action onPress, Action onRelease)>();

            for (int i = 0; i < loadedKeys.Count && i < ActionNames.Count; i++)
            {
                var key = loadedKeys[i];
                Action onPress, onRelease;

                switch (i)
                {
                    case 0: onPress = () => movementState.Forward = true; onRelease = () => movementState.Forward = false; break;
                    case 1: onPress = () => movementState.Backward = true; onRelease = () => movementState.Backward = false; break;
                    case 2: onPress = () => movementState.Left = true; onRelease = () => movementState.Left = false; break;
                    case 3: onPress = () => movementState.Right = true; onRelease = () => movementState.Right = false; break;
                    case 4: onPress = () => movementState.Down = true; onRelease = () => movementState.Down = false; break;
                    case 5: onPress = () => movementState.Up = true; onRelease = () => movementState.Up = false; break;
                    case 6: onPress = () => movementState.PitchUp = true; onRelease = () => movementState.PitchUp = false; break;
                    case 7: onPress = () => movementState.PitchDown = true; onRelease = () => movementState.PitchDown = false; break;
                    case 8: onPress = () => movementState.YawLeft = true; onRelease = () => movementState.YawLeft = false; break;
                    case 9: onPress = () => movementState.YawRight = true; onRelease = () => movementState.YawRight = false; break;
                    case 10: onPress = () => movementState.RollLeft = true; onRelease = () => movementState.RollLeft = false; break;
                    case 11: onPress = () => movementState.RollRight = true; onRelease = () => movementState.RollRight = false; break;
                    case 12: onPress = () => Preset1Button_Click(null, EventArgs.Empty); onRelease = NoOp; break;
                    case 13: onPress = () => Preset2Button_Click(null, EventArgs.Empty); onRelease = NoOp; break;
                    case 14: onPress = () => Preset3Button_Click(null, EventArgs.Empty); onRelease = NoOp; break;
                    case 15: onPress = () => Preset4Button_Click(null, EventArgs.Empty); onRelease = NoOp; break;
                    case 16: onPress = () => FreecamSwitch.Switched = !FreecamSwitch.Switched; onRelease = NoOp; break;
                    case 17: onPress = () => HideNametagsSwitch.Switched = !HideNametagsSwitch.Switched; onRelease = NoOp; break;
                    case 18: onPress = () => HideUserInterfaceSwitch.Switched = !HideUserInterfaceSwitch.Switched; onRelease = NoOp; break;
                    case 19: onPress = () => AddAnimationFrameButton_Click(null, EventArgs.Empty); onRelease = NoOp; break;
                    case 20: onPress = GoToPreviousFrame; onRelease = NoOp; break;
                    case 21: onPress = GoToNextFrame; onRelease = NoOp; break;
                    case 22: onPress = () => PlayAnimationButton_Click(null, EventArgs.Empty); onRelease = NoOp; break;
                    default: onPress = NoOp; onRelease = NoOp; break;
                }

                keyBindings[key] = (onPress, onRelease);
            }
        }

        private void PopulateHotkeyPanel()
        {
            HotkeyPanel.Controls.Clear();
            int i = 0;
            foreach (var kvp in keyBindings)
            {
                Keys currentKey = kvp.Key;
                var (onPress, onRelease) = kvp.Value;
                string actionName = ActionNames[i];

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
                    //Tag = actionName,
                };

                // Placeholder
                txtKey.Tag = new KeyCaptureInfo
                {
                    ActionName = actionName,
                    CurrentKey = currentKey,
                    Index = i
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
                    TextAlign = ContentAlignment.TopCenter,
                    Tag = new ClearButtonInfo { Index = i }
                };

                i++;

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
                            info.CurrentKey = currentKey; 
                        }
                        else
                        {
                            info.CurrentKey = newKey;   
                        }

                        tb.Text = currentKey.ToString();
                        UpdateHotkey(info.ActionName, info.CurrentKey, info.Index);
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
                        UpdateHotkey(info.ActionName, Keys.None, info.Index);
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

        private void UpdateHotkey(string actionName, Keys newKey, int index)
        {
            // Find the tuple (onPress, onRelease) in the dictionary by name
            (Action onPress, Action onRelease) actionToUpdate = default;
            Keys oldKey = Keys.None;

            int i = 0;
            foreach (var kvp in keyBindings)
            {
                string currentActionName = ActionNames[i];
                if (currentActionName == actionName)
                {
                    actionToUpdate = kvp.Value;
                    oldKey = kvp.Key;
                    break;
                }

                i++;
            }

            if (actionToUpdate.onPress != null)
            {
                // Only update if key is valid and not already bound
                if (newKey != Keys.None && !keyBindings.ContainsKey(newKey))
                {
                    keyBindings.Remove(oldKey);
                    keyBindings[newKey] = actionToUpdate;
                    Console.WriteLine(index);
                }

                SaveKeyBindings();
            }
        }
    }
}


        