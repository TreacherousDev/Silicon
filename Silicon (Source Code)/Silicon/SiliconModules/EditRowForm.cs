using MetroSet_UI.Controls;
using MetroSet_UI.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Silicon
{
    public partial class EditRowForm : MetroSetForm
    {
        public MetroSetTextBox[] TextBoxes;
        private bool[] IsNumericField;

        public EditRowForm(string[] columnNames, string[] values)
        {
            this.Text = "Edit Row";
            this.Size = new Size(420, 150 + (columnNames.Length * 40));
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.ThemeName = "MetroDark";
            this.Style = MetroSet_UI.Enums.Style.Dark;
            this.UseSlideAnimation = true;
            
        InitializeControls(columnNames, values);
        }

        private void InitializeControls(string[] columnNames, string[] values)
        {
            this.Font = new Font("Impact", 14F, FontStyle.Regular);
            TextBoxes = new MetroSetTextBox[columnNames.Length];
            IsNumericField = new bool[columnNames.Length];

            int startY = 90;

            for (int i = 1; i < columnNames.Length; i++)
            {
                var label = new MetroSetLabel();
                label.Text = columnNames[i];
                label.Location = new Point(30, startY);
                label.Size = new Size(120, 25);

                // 🔹 Make text white
                label.ForeColor = Color.White;

                // 🔹 Set font to Impact
                label.Font = new Font("Impact", 12F, FontStyle.Regular);

                var textBox = new MetroSetTextBox();
                textBox.Text = values[i];
                textBox.Location = new Point(160, startY);
                textBox.Size = new Size(200, 30);

                // Detect numeric field
                if (double.TryParse(values[i], out _))
                {
                    IsNumericField[i] = true;

                    textBox.KeyPress += (s, e) =>
                    {
                        // Allow digits, control keys, and decimal point
                        if (!char.IsControl(e.KeyChar) &&
                            !char.IsDigit(e.KeyChar) &&
                            e.KeyChar != '.')
                        {
                            e.Handled = true;
                        }

                        // Only one decimal point allowed
                        if (e.KeyChar == '.' &&
                            ((MetroSetTextBox)s).Text.Contains("."))
                        {
                            e.Handled = true;
                        }
                    };
                }

                TextBoxes[i] = textBox;

                this.Controls.Add(label);
                this.Controls.Add(textBox);

                startY += 40;
            }

            var btnSave = new MetroSetButton();
            btnSave.Text = "SAVE";
            btnSave.Font = new Font("Impact", 12F, FontStyle.Regular);
            btnSave.Location = new Point(100, startY + 10);
            btnSave.Size = new Size(90, 35);
            btnSave.NormalBorderColor = Color.FromArgb(80, 160, 255);
            btnSave.NormalColor = Color.FromArgb(80, 160, 255);
            btnSave.PressBorderColor = Color.FromArgb(64, 128, 204);
            btnSave.PressColor = Color.FromArgb(64, 128, 204);
            btnSave.Click += (s, e) =>
            {
                if (!ValidateInputs())
                    return;

                this.DialogResult = System.Windows.Forms.DialogResult.OK;
                this.Close();
            };

            var btnCancel = new MetroSetButton();
            btnCancel.Text = "CANCEL";
            btnCancel.Font = new Font("Impact", 12F, FontStyle.Regular);
            btnCancel.Location = new Point(210, startY + 10);
            btnCancel.Size = new Size(90, 35);
            btnCancel.NormalBorderColor = Color.FromArgb(80, 160, 255);
            btnCancel.NormalColor = Color.FromArgb(80, 160, 255);
            btnCancel.PressBorderColor = Color.FromArgb(64, 128, 204);
            btnCancel.PressColor = Color.FromArgb(64, 128, 204);
            btnCancel.Click += (s, e) =>
            {
                this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
                this.Close();
            };

            this.Controls.Add(btnSave);
            this.Controls.Add(btnCancel);
        }

        private bool ValidateInputs()
        {
            for (int i = 0; i < TextBoxes.Length; i++)
            {
                if (IsNumericField[i])
                {
                    if (!double.TryParse(TextBoxes[i].Text, out _))
                    {
                        TextBoxes[i].BorderColor = Color.Red;
                        return false;
                    }
                    else
                    {
                        TextBoxes[i].BorderColor = Color.FromArgb(65, 177, 225); // reset to default Metro
                    }
                }
            }
            return true;
        }
    }
}
