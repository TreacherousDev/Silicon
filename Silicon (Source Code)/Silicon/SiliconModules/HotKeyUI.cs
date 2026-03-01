using System;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.IO;

namespace Silicon
{
    public partial class SiliconForm
    {

        // ─── Save / Load ──────────────────────────────────────────────────────────

        private static string KeyBindingsPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),"Silicon", "keybindings.json");

        public void SaveKeyBindings()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(KeyBindingsPath));
            // Save as an ordered list of Keys values, one per action index.
            var keys = actions.Select(a => (int)a.BoundKey).ToList();
            File.WriteAllText(KeyBindingsPath, JsonSerializer.Serialize(keys));
        }

        public void LoadKeyBindings()
        {
            if (!File.Exists(KeyBindingsPath)) return;
            var saved = JsonSerializer.Deserialize<List<int>>(File.ReadAllText(KeyBindingsPath));

            for (int i = 0; i < saved.Count && i < actions.Count; i++) 
                actions[i].BoundKey = (Keys)saved[i];

            RebuildKeyMap();
        }

        // ─── Settings Panel ───────────────────────────────────────────────────────
        private void PopulateHotkeyPanel()
        {
            HotkeyPanel.Controls.Clear();

            foreach (var action in actions)
            {
                // Capture for closures
                KeyAction currentAction = action;

                Label lbl = new Label
                {
                    Text = currentAction.DisplayName,
                    ForeColor = Color.White,
                    Width = 150,
                    AutoSize = false,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Margin = new Padding(3, 6, 3, 3),
                };

                TextBox txtKey = new TextBox
                {
                    Width = 100,
                    Text = currentAction.BoundKey.ToString(),
                    ReadOnly = true,
                    BackColor = SystemColors.Window,
                    Margin = new Padding(3, 3, 20, 3),
                };

                Button btnClear = new Button
                {
                    Text = "×",
                    Width = 24,
                    Height = 24,
                    Font = new Font(FontFamily.GenericSansSerif, 10, FontStyle.Bold),
                    FlatStyle = FlatStyle.Flat,
                    TextAlign = ContentAlignment.TopCenter,
                    Margin = new Padding(0, 0, 3, 0),
                };

                // Highlight while waiting for input
                txtKey.Enter += (s, e) =>
                {
                    txtKey.BackColor = Color.LightYellow;
                    txtKey.Text = "Press any key...";
                };

                // Restore display if focus lost without a new key
                txtKey.Leave += (s, e) =>
                {
                    txtKey.BackColor = SystemColors.Window;
                    txtKey.Text = currentAction.BoundKey.ToString();
                };

                // Capture new key
                txtKey.KeyDown += (s, e) =>
                {
                    e.SuppressKeyPress = true;
                    Keys newKey = e.KeyCode;

                    // Reject duplicate bindings
                    bool alreadyUsed = actions.Any(a => a != currentAction && a.BoundKey == newKey);
                    if (!alreadyUsed && newKey != Keys.None)
                    {
                        currentAction.BoundKey = newKey;
                        RebuildKeyMap();
                        SaveKeyBindings();
                    }

                    txtKey.Text = currentAction.BoundKey.ToString();
                    txtKey.BackColor = SystemColors.Window;
                    HotkeyPanel.Focus();
                };

                // Clear binding
                btnClear.Click += (s, e) =>
                {
                    currentAction.BoundKey = Keys.None;
                    txtKey.Text = "None";
                    RebuildKeyMap();
                    SaveKeyBindings();
                };

                Panel row = new Panel
                {
                    Width = HotkeyPanel.ClientSize.Width - 25,
                    Height = 30,
                    Margin = new Padding(0),
                };

                lbl.Location = new Point(0, 5);
                txtKey.Location = new Point(160, 2);
                btnClear.Location = new Point(txtKey.Right + 5, 2);

                row.Controls.Add(lbl);
                row.Controls.Add(txtKey);
                row.Controls.Add(btnClear);
                HotkeyPanel.Controls.Add(row);
            }
        }
    }
}