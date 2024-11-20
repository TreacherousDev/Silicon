
namespace Silicon
{
    partial class SiliconForm
    {
        /// <summary>
        /// Variable del diseñador necesaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpiar los recursos que se estén usando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de Windows Forms

        /// <summary>
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido de este método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SiliconForm));
            this.metroSetControlBox1 = new MetroSet_UI.Controls.MetroSetControlBox();
            this.HeliumWorker = new System.ComponentModel.BackgroundWorker();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.Information = new System.Windows.Forms.TabPage();
            this.Status = new MetroSet_UI.Controls.MetroSetLabel();
            this.getStatus = new MetroSet_UI.Controls.MetroSetLabel();
            this.Hispano = new MetroSet_UI.Controls.MetroSetLabel();
            this.proID = new MetroSet_UI.Controls.MetroSetLabel();
            this.procIDLabel = new MetroSet_UI.Controls.MetroSetLabel();
            this.Utility = new System.Windows.Forms.TabPage();
            this.PlayAnimationButton = new MetroSet_UI.Controls.MetroSetButton();
            this.AddAnimationFrameButton = new MetroSet_UI.Controls.MetroSetButton();
            this.CameraRotationInfoLabel = new Sunny.UI.UILabel();
            this.CameraLookAtInfoLabel = new Sunny.UI.UILabel();
            this.uiLabel6 = new Sunny.UI.UILabel();
            this.uiLabel5 = new Sunny.UI.UILabel();
            this.HidePlayerModelSwitch = new MetroSet_UI.Controls.MetroSetSwitch();
            this.CameraFOVSlider = new MetroSet_UI.Controls.MetroSetTrackBar();
            this.uiLabel4 = new Sunny.UI.UILabel();
            this.CameraDistanceSlider = new MetroSet_UI.Controls.MetroSetTrackBar();
            this.uiLabel3 = new Sunny.UI.UILabel();
            this.CameraRotateSpeedSlider = new MetroSet_UI.Controls.MetroSetTrackBar();
            this.CameraMoveSpeedSlider = new MetroSet_UI.Controls.MetroSetTrackBar();
            this.uiLabel2 = new Sunny.UI.UILabel();
            this.uiLabel1 = new Sunny.UI.UILabel();
            this.ToolTipLabel = new Sunny.UI.UILabel();
            this.FreecamLabel = new Sunny.UI.UILabel();
            this.FreecamSwitch = new MetroSet_UI.Controls.MetroSetSwitch();
            this.TabControl = new Sunny.UI.UITabControl();
            this.metroSetSetToolTip1 = new MetroSet_UI.Components.MetroSetSetToolTip();
            this.Information.SuspendLayout();
            this.Utility.SuspendLayout();
            this.TabControl.SuspendLayout();
            this.SuspendLayout();
            // 
            // metroSetControlBox1
            // 
            this.metroSetControlBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.metroSetControlBox1.CloseHoverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(183)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.metroSetControlBox1.CloseHoverForeColor = System.Drawing.Color.White;
            this.metroSetControlBox1.CloseNormalForeColor = System.Drawing.Color.Gray;
            this.metroSetControlBox1.DisabledForeColor = System.Drawing.Color.Silver;
            this.metroSetControlBox1.IsDerivedStyle = true;
            this.metroSetControlBox1.Location = new System.Drawing.Point(397, 11);
            this.metroSetControlBox1.MaximizeBox = false;
            this.metroSetControlBox1.MaximizeHoverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(238)))), ((int)(((byte)(238)))));
            this.metroSetControlBox1.MaximizeHoverForeColor = System.Drawing.Color.Gray;
            this.metroSetControlBox1.MaximizeNormalForeColor = System.Drawing.Color.Gray;
            this.metroSetControlBox1.MinimizeBox = true;
            this.metroSetControlBox1.MinimizeHoverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(238)))), ((int)(((byte)(238)))));
            this.metroSetControlBox1.MinimizeHoverForeColor = System.Drawing.Color.Gray;
            this.metroSetControlBox1.MinimizeNormalForeColor = System.Drawing.Color.Gray;
            this.metroSetControlBox1.Name = "metroSetControlBox1";
            this.metroSetControlBox1.Size = new System.Drawing.Size(100, 25);
            this.metroSetControlBox1.Style = MetroSet_UI.Enums.Style.Dark;
            this.metroSetControlBox1.StyleManager = null;
            this.metroSetControlBox1.TabIndex = 0;
            this.metroSetControlBox1.Text = "metroSetControlBox1";
            this.metroSetControlBox1.ThemeAuthor = "Narwin";
            this.metroSetControlBox1.ThemeName = "MetroDark";
            // 
            // HeliumWorker
            // 
            this.HeliumWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.HeliumWorker_DoWork);
            // 
            // Information
            // 
            this.Information.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.Information.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Information.Controls.Add(this.Status);
            this.Information.Controls.Add(this.getStatus);
            this.Information.Controls.Add(this.Hispano);
            this.Information.Controls.Add(this.proID);
            this.Information.Controls.Add(this.procIDLabel);
            this.Information.ForeColor = System.Drawing.Color.White;
            this.Information.Location = new System.Drawing.Point(0, 40);
            this.Information.Name = "Information";
            this.Information.Size = new System.Drawing.Size(200, 60);
            this.Information.TabIndex = 1;
            this.Information.Text = "Information";
            // 
            // Status
            // 
            this.Status.Font = new System.Drawing.Font("Impact", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Status.IsDerivedStyle = true;
            this.Status.Location = new System.Drawing.Point(12, 37);
            this.Status.Name = "Status";
            this.Status.Size = new System.Drawing.Size(77, 23);
            this.Status.Style = MetroSet_UI.Enums.Style.Light;
            this.Status.StyleManager = null;
            this.Status.TabIndex = 5;
            this.Status.Text = "STATUS:";
            this.Status.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.Status.ThemeAuthor = "Narwin";
            this.Status.ThemeName = "MetroLite";
            // 
            // getStatus
            // 
            this.getStatus.Font = new System.Drawing.Font("Impact", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.getStatus.IsDerivedStyle = true;
            this.getStatus.Location = new System.Drawing.Point(85, 37);
            this.getStatus.Name = "getStatus";
            this.getStatus.Size = new System.Drawing.Size(108, 23);
            this.getStatus.Style = MetroSet_UI.Enums.Style.Light;
            this.getStatus.StyleManager = null;
            this.getStatus.TabIndex = 4;
            this.getStatus.Text = "DISCONNECTED";
            this.getStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.getStatus.ThemeAuthor = "Narwin";
            this.getStatus.ThemeName = "MetroLite";
            // 
            // Hispano
            // 
            this.Hispano.Font = new System.Drawing.Font("Impact", 9.75F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Hispano.IsDerivedStyle = true;
            this.Hispano.Location = new System.Drawing.Point(94, 186);
            this.Hispano.Name = "Hispano";
            this.Hispano.Size = new System.Drawing.Size(293, 23);
            this.Hispano.Style = MetroSet_UI.Enums.Style.Light;
            this.Hispano.StyleManager = null;
            this.Hispano.TabIndex = 3;
            this.Hispano.Text = "Developed by TreacherousDev";
            this.Hispano.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.Hispano.ThemeAuthor = "Narwin";
            this.Hispano.ThemeName = "MetroLite";
            // 
            // proID
            // 
            this.proID.Font = new System.Drawing.Font("Impact", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.proID.IsDerivedStyle = true;
            this.proID.Location = new System.Drawing.Point(12, 14);
            this.proID.Name = "proID";
            this.proID.Size = new System.Drawing.Size(77, 23);
            this.proID.Style = MetroSet_UI.Enums.Style.Light;
            this.proID.StyleManager = null;
            this.proID.TabIndex = 2;
            this.proID.Text = "PROCESS:";
            this.proID.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.proID.ThemeAuthor = "Narwin";
            this.proID.ThemeName = "MetroLite";
            // 
            // procIDLabel
            // 
            this.procIDLabel.Font = new System.Drawing.Font("Impact", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.procIDLabel.IsDerivedStyle = true;
            this.procIDLabel.Location = new System.Drawing.Point(85, 14);
            this.procIDLabel.Name = "procIDLabel";
            this.procIDLabel.Size = new System.Drawing.Size(108, 23);
            this.procIDLabel.Style = MetroSet_UI.Enums.Style.Light;
            this.procIDLabel.StyleManager = null;
            this.procIDLabel.TabIndex = 1;
            this.procIDLabel.Text = "DISCONNECTED";
            this.procIDLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.procIDLabel.ThemeAuthor = "Narwin";
            this.procIDLabel.ThemeName = "MetroLite";
            // 
            // Utility
            // 
            this.Utility.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.Utility.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Utility.Controls.Add(this.PlayAnimationButton);
            this.Utility.Controls.Add(this.AddAnimationFrameButton);
            this.Utility.Controls.Add(this.CameraRotationInfoLabel);
            this.Utility.Controls.Add(this.CameraLookAtInfoLabel);
            this.Utility.Controls.Add(this.uiLabel6);
            this.Utility.Controls.Add(this.uiLabel5);
            this.Utility.Controls.Add(this.HidePlayerModelSwitch);
            this.Utility.Controls.Add(this.CameraFOVSlider);
            this.Utility.Controls.Add(this.uiLabel4);
            this.Utility.Controls.Add(this.CameraDistanceSlider);
            this.Utility.Controls.Add(this.uiLabel3);
            this.Utility.Controls.Add(this.CameraRotateSpeedSlider);
            this.Utility.Controls.Add(this.CameraMoveSpeedSlider);
            this.Utility.Controls.Add(this.uiLabel2);
            this.Utility.Controls.Add(this.uiLabel1);
            this.Utility.Controls.Add(this.ToolTipLabel);
            this.Utility.Controls.Add(this.FreecamLabel);
            this.Utility.Controls.Add(this.FreecamSwitch);
            this.Utility.ForeColor = System.Drawing.Color.White;
            this.Utility.Location = new System.Drawing.Point(0, 30);
            this.Utility.Name = "Utility";
            this.Utility.Size = new System.Drawing.Size(482, 215);
            this.Utility.TabIndex = 0;
            this.Utility.Text = "Utility";
            // 
            // PlayAnimationButton
            // 
            this.PlayAnimationButton.DisabledBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(65)))), ((int)(((byte)(177)))), ((int)(((byte)(225)))));
            this.PlayAnimationButton.DisabledBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(65)))), ((int)(((byte)(177)))), ((int)(((byte)(225)))));
            this.PlayAnimationButton.DisabledForeColor = System.Drawing.Color.Gray;
            this.PlayAnimationButton.Font = new System.Drawing.Font("Impact", 15F);
            this.PlayAnimationButton.HoverBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(207)))), ((int)(((byte)(255)))));
            this.PlayAnimationButton.HoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(207)))), ((int)(((byte)(255)))));
            this.PlayAnimationButton.HoverTextColor = System.Drawing.Color.White;
            this.PlayAnimationButton.IsDerivedStyle = true;
            this.PlayAnimationButton.Location = new System.Drawing.Point(73, 149);
            this.PlayAnimationButton.Name = "PlayAnimationButton";
            this.PlayAnimationButton.NormalBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(65)))), ((int)(((byte)(177)))), ((int)(((byte)(225)))));
            this.PlayAnimationButton.NormalColor = System.Drawing.Color.FromArgb(((int)(((byte)(65)))), ((int)(((byte)(177)))), ((int)(((byte)(225)))));
            this.PlayAnimationButton.NormalTextColor = System.Drawing.Color.White;
            this.PlayAnimationButton.PressBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(147)))), ((int)(((byte)(195)))));
            this.PlayAnimationButton.PressColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(147)))), ((int)(((byte)(195)))));
            this.PlayAnimationButton.PressTextColor = System.Drawing.Color.White;
            this.PlayAnimationButton.Size = new System.Drawing.Size(42, 35);
            this.PlayAnimationButton.Style = MetroSet_UI.Enums.Style.Light;
            this.PlayAnimationButton.StyleManager = null;
            this.PlayAnimationButton.TabIndex = 41;
            this.PlayAnimationButton.Text = " ►";
            this.PlayAnimationButton.ThemeAuthor = "Narwin";
            this.PlayAnimationButton.ThemeName = "MetroLite";
            this.PlayAnimationButton.Click += new System.EventHandler(this.PlayAnimationButton_Click);
            // 
            // AddAnimationFrameButton
            // 
            this.AddAnimationFrameButton.DisabledBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(65)))), ((int)(((byte)(177)))), ((int)(((byte)(225)))));
            this.AddAnimationFrameButton.DisabledBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(65)))), ((int)(((byte)(177)))), ((int)(((byte)(225)))));
            this.AddAnimationFrameButton.DisabledForeColor = System.Drawing.Color.Gray;
            this.AddAnimationFrameButton.Font = new System.Drawing.Font("Impact", 12.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.AddAnimationFrameButton.HoverBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(207)))), ((int)(((byte)(255)))));
            this.AddAnimationFrameButton.HoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(207)))), ((int)(((byte)(255)))));
            this.AddAnimationFrameButton.HoverTextColor = System.Drawing.Color.White;
            this.AddAnimationFrameButton.IsDerivedStyle = true;
            this.AddAnimationFrameButton.Location = new System.Drawing.Point(19, 149);
            this.AddAnimationFrameButton.Name = "AddAnimationFrameButton";
            this.AddAnimationFrameButton.NormalBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(65)))), ((int)(((byte)(177)))), ((int)(((byte)(225)))));
            this.AddAnimationFrameButton.NormalColor = System.Drawing.Color.FromArgb(((int)(((byte)(65)))), ((int)(((byte)(177)))), ((int)(((byte)(225)))));
            this.AddAnimationFrameButton.NormalTextColor = System.Drawing.Color.White;
            this.AddAnimationFrameButton.PressBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(147)))), ((int)(((byte)(195)))));
            this.AddAnimationFrameButton.PressColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(147)))), ((int)(((byte)(195)))));
            this.AddAnimationFrameButton.PressTextColor = System.Drawing.Color.White;
            this.AddAnimationFrameButton.Size = new System.Drawing.Size(42, 35);
            this.AddAnimationFrameButton.Style = MetroSet_UI.Enums.Style.Light;
            this.AddAnimationFrameButton.StyleManager = null;
            this.AddAnimationFrameButton.TabIndex = 40;
            this.AddAnimationFrameButton.Text = "➕    ";
            this.AddAnimationFrameButton.ThemeAuthor = "Narwin";
            this.AddAnimationFrameButton.ThemeName = "MetroLite";
            this.AddAnimationFrameButton.Click += new System.EventHandler(this.AddAnimationFrameButton_Click);
            // 
            // CameraRotationInfoLabel
            // 
            this.CameraRotationInfoLabel.BackColor = System.Drawing.Color.Transparent;
            this.CameraRotationInfoLabel.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.CameraRotationInfoLabel.Font = new System.Drawing.Font("Leelawadee UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CameraRotationInfoLabel.ForeColor = System.Drawing.Color.White;
            this.CameraRotationInfoLabel.Location = new System.Drawing.Point(64, 98);
            this.CameraRotationInfoLabel.Name = "CameraRotationInfoLabel";
            this.CameraRotationInfoLabel.Size = new System.Drawing.Size(94, 44);
            this.CameraRotationInfoLabel.TabIndex = 39;
            this.CameraRotationInfoLabel.Text = "Pitch: 0.00    Yaw: 0.00";
            // 
            // CameraLookAtInfoLabel
            // 
            this.CameraLookAtInfoLabel.BackColor = System.Drawing.Color.Transparent;
            this.CameraLookAtInfoLabel.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.CameraLookAtInfoLabel.Font = new System.Drawing.Font("Leelawadee UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CameraLookAtInfoLabel.ForeColor = System.Drawing.Color.White;
            this.CameraLookAtInfoLabel.Location = new System.Drawing.Point(16, 98);
            this.CameraLookAtInfoLabel.Name = "CameraLookAtInfoLabel";
            this.CameraLookAtInfoLabel.Size = new System.Drawing.Size(61, 44);
            this.CameraLookAtInfoLabel.TabIndex = 38;
            this.CameraLookAtInfoLabel.Text = "X: 0.00     Y: 0.00    Z: 0.00";
            // 
            // uiLabel6
            // 
            this.uiLabel6.BackColor = System.Drawing.Color.Transparent;
            this.uiLabel6.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.uiLabel6.Font = new System.Drawing.Font("Impact", 10F);
            this.uiLabel6.ForeColor = System.Drawing.Color.White;
            this.uiLabel6.Location = new System.Drawing.Point(16, 78);
            this.uiLabel6.Name = "uiLabel6";
            this.uiLabel6.Size = new System.Drawing.Size(76, 17);
            this.uiLabel6.TabIndex = 37;
            this.uiLabel6.Text = "Camera Info";
            // 
            // uiLabel5
            // 
            this.uiLabel5.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.uiLabel5.Font = new System.Drawing.Font("Impact", 12F);
            this.uiLabel5.ForeColor = System.Drawing.Color.White;
            this.uiLabel5.Location = new System.Drawing.Point(84, 46);
            this.uiLabel5.Name = "uiLabel5";
            this.uiLabel5.Size = new System.Drawing.Size(127, 23);
            this.uiLabel5.TabIndex = 36;
            this.uiLabel5.Text = "Hide Player";
            // 
            // HidePlayerModelSwitch
            // 
            this.HidePlayerModelSwitch.BackColor = System.Drawing.Color.Transparent;
            this.HidePlayerModelSwitch.BackgroundColor = System.Drawing.Color.Empty;
            this.HidePlayerModelSwitch.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(165)))), ((int)(((byte)(159)))), ((int)(((byte)(147)))));
            this.HidePlayerModelSwitch.CheckColor = System.Drawing.Color.FromArgb(((int)(((byte)(65)))), ((int)(((byte)(177)))), ((int)(((byte)(225)))));
            this.HidePlayerModelSwitch.CheckState = MetroSet_UI.Enums.CheckState.Unchecked;
            this.HidePlayerModelSwitch.Cursor = System.Windows.Forms.Cursors.Hand;
            this.HidePlayerModelSwitch.DisabledBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(205)))), ((int)(((byte)(205)))), ((int)(((byte)(205)))));
            this.HidePlayerModelSwitch.DisabledCheckColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(65)))), ((int)(((byte)(177)))), ((int)(((byte)(225)))));
            this.HidePlayerModelSwitch.DisabledUnCheckColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(205)))), ((int)(((byte)(205)))), ((int)(((byte)(205)))));
            this.HidePlayerModelSwitch.IsDerivedStyle = true;
            this.HidePlayerModelSwitch.Location = new System.Drawing.Point(19, 46);
            this.HidePlayerModelSwitch.Name = "HidePlayerModelSwitch";
            this.HidePlayerModelSwitch.Size = new System.Drawing.Size(58, 22);
            this.HidePlayerModelSwitch.Style = MetroSet_UI.Enums.Style.Light;
            this.HidePlayerModelSwitch.StyleManager = null;
            this.HidePlayerModelSwitch.Switched = false;
            this.HidePlayerModelSwitch.SymbolColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(160)))), ((int)(((byte)(255)))));
            this.HidePlayerModelSwitch.TabIndex = 35;
            this.HidePlayerModelSwitch.ThemeAuthor = "Narwin";
            this.HidePlayerModelSwitch.ThemeName = "MetroLite";
            this.HidePlayerModelSwitch.UnCheckColor = System.Drawing.Color.White;
            // 
            // CameraFOVSlider
            // 
            this.CameraFOVSlider.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(205)))), ((int)(((byte)(205)))), ((int)(((byte)(205)))));
            this.CameraFOVSlider.Cursor = System.Windows.Forms.Cursors.Hand;
            this.CameraFOVSlider.DisabledBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(235)))), ((int)(((byte)(235)))));
            this.CameraFOVSlider.DisabledBorderColor = System.Drawing.Color.Empty;
            this.CameraFOVSlider.DisabledHandlerColor = System.Drawing.Color.FromArgb(((int)(((byte)(196)))), ((int)(((byte)(196)))), ((int)(((byte)(196)))));
            this.CameraFOVSlider.DisabledValueColor = System.Drawing.Color.FromArgb(((int)(((byte)(205)))), ((int)(((byte)(205)))), ((int)(((byte)(205)))));
            this.CameraFOVSlider.HandlerColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.CameraFOVSlider.IsDerivedStyle = true;
            this.CameraFOVSlider.Location = new System.Drawing.Point(314, 141);
            this.CameraFOVSlider.Maximum = 135;
            this.CameraFOVSlider.Minimum = 10;
            this.CameraFOVSlider.Name = "CameraFOVSlider";
            this.CameraFOVSlider.Size = new System.Drawing.Size(147, 16);
            this.CameraFOVSlider.Style = MetroSet_UI.Enums.Style.Light;
            this.CameraFOVSlider.StyleManager = null;
            this.CameraFOVSlider.TabIndex = 33;
            this.CameraFOVSlider.Text = "metroSetTrackBar6";
            this.CameraFOVSlider.ThemeAuthor = "Narwin";
            this.CameraFOVSlider.ThemeName = "MetroLite";
            this.CameraFOVSlider.Value = 60;
            this.CameraFOVSlider.ValueColor = System.Drawing.Color.FromArgb(((int)(((byte)(65)))), ((int)(((byte)(177)))), ((int)(((byte)(225)))));
            // 
            // uiLabel4
            // 
            this.uiLabel4.BackColor = System.Drawing.Color.Transparent;
            this.uiLabel4.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.uiLabel4.Font = new System.Drawing.Font("Impact", 9F);
            this.uiLabel4.ForeColor = System.Drawing.Color.White;
            this.uiLabel4.Location = new System.Drawing.Point(311, 123);
            this.uiLabel4.Name = "uiLabel4";
            this.uiLabel4.Size = new System.Drawing.Size(150, 23);
            this.uiLabel4.TabIndex = 34;
            this.uiLabel4.Text = "Field of View";
            // 
            // CameraDistanceSlider
            // 
            this.CameraDistanceSlider.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(205)))), ((int)(((byte)(205)))), ((int)(((byte)(205)))));
            this.CameraDistanceSlider.Cursor = System.Windows.Forms.Cursors.Hand;
            this.CameraDistanceSlider.DisabledBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(235)))), ((int)(((byte)(235)))));
            this.CameraDistanceSlider.DisabledBorderColor = System.Drawing.Color.Empty;
            this.CameraDistanceSlider.DisabledHandlerColor = System.Drawing.Color.FromArgb(((int)(((byte)(196)))), ((int)(((byte)(196)))), ((int)(((byte)(196)))));
            this.CameraDistanceSlider.DisabledValueColor = System.Drawing.Color.FromArgb(((int)(((byte)(205)))), ((int)(((byte)(205)))), ((int)(((byte)(205)))));
            this.CameraDistanceSlider.HandlerColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.CameraDistanceSlider.IsDerivedStyle = true;
            this.CameraDistanceSlider.Location = new System.Drawing.Point(314, 104);
            this.CameraDistanceSlider.Maximum = 100;
            this.CameraDistanceSlider.Minimum = 1;
            this.CameraDistanceSlider.Name = "CameraDistanceSlider";
            this.CameraDistanceSlider.Size = new System.Drawing.Size(147, 16);
            this.CameraDistanceSlider.Style = MetroSet_UI.Enums.Style.Light;
            this.CameraDistanceSlider.StyleManager = null;
            this.CameraDistanceSlider.TabIndex = 31;
            this.CameraDistanceSlider.Text = "metroSetTrackBar6";
            this.CameraDistanceSlider.ThemeAuthor = "Narwin";
            this.CameraDistanceSlider.ThemeName = "MetroLite";
            this.CameraDistanceSlider.Value = 10;
            this.CameraDistanceSlider.ValueColor = System.Drawing.Color.FromArgb(((int)(((byte)(65)))), ((int)(((byte)(177)))), ((int)(((byte)(225)))));
            // 
            // uiLabel3
            // 
            this.uiLabel3.BackColor = System.Drawing.Color.Transparent;
            this.uiLabel3.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.uiLabel3.Font = new System.Drawing.Font("Impact", 9F);
            this.uiLabel3.ForeColor = System.Drawing.Color.White;
            this.uiLabel3.Location = new System.Drawing.Point(311, 86);
            this.uiLabel3.Name = "uiLabel3";
            this.uiLabel3.Size = new System.Drawing.Size(150, 23);
            this.uiLabel3.TabIndex = 32;
            this.uiLabel3.Text = "Distance to Focal Point";
            // 
            // CameraRotateSpeedSlider
            // 
            this.CameraRotateSpeedSlider.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(205)))), ((int)(((byte)(205)))), ((int)(((byte)(205)))));
            this.CameraRotateSpeedSlider.Cursor = System.Windows.Forms.Cursors.Hand;
            this.CameraRotateSpeedSlider.DisabledBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(235)))), ((int)(((byte)(235)))));
            this.CameraRotateSpeedSlider.DisabledBorderColor = System.Drawing.Color.Empty;
            this.CameraRotateSpeedSlider.DisabledHandlerColor = System.Drawing.Color.FromArgb(((int)(((byte)(196)))), ((int)(((byte)(196)))), ((int)(((byte)(196)))));
            this.CameraRotateSpeedSlider.DisabledValueColor = System.Drawing.Color.FromArgb(((int)(((byte)(205)))), ((int)(((byte)(205)))), ((int)(((byte)(205)))));
            this.CameraRotateSpeedSlider.HandlerColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.CameraRotateSpeedSlider.IsDerivedStyle = true;
            this.CameraRotateSpeedSlider.Location = new System.Drawing.Point(314, 67);
            this.CameraRotateSpeedSlider.Maximum = 100;
            this.CameraRotateSpeedSlider.Minimum = 3;
            this.CameraRotateSpeedSlider.Name = "CameraRotateSpeedSlider";
            this.CameraRotateSpeedSlider.Size = new System.Drawing.Size(147, 16);
            this.CameraRotateSpeedSlider.Style = MetroSet_UI.Enums.Style.Light;
            this.CameraRotateSpeedSlider.StyleManager = null;
            this.CameraRotateSpeedSlider.TabIndex = 29;
            this.CameraRotateSpeedSlider.Text = "metroSetTrackBar6";
            this.CameraRotateSpeedSlider.ThemeAuthor = "Narwin";
            this.CameraRotateSpeedSlider.ThemeName = "MetroLite";
            this.CameraRotateSpeedSlider.Value = 40;
            this.CameraRotateSpeedSlider.ValueColor = System.Drawing.Color.FromArgb(((int)(((byte)(65)))), ((int)(((byte)(177)))), ((int)(((byte)(225)))));
            this.CameraRotateSpeedSlider.Scroll += new MetroSet_UI.Controls.MetroSetTrackBar.ScrollEventHandler(this.CameraRotateSpeedSlider_Scroll);
            // 
            // CameraMoveSpeedSlider
            // 
            this.CameraMoveSpeedSlider.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(205)))), ((int)(((byte)(205)))), ((int)(((byte)(205)))));
            this.CameraMoveSpeedSlider.Cursor = System.Windows.Forms.Cursors.Hand;
            this.CameraMoveSpeedSlider.DisabledBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(235)))), ((int)(((byte)(235)))));
            this.CameraMoveSpeedSlider.DisabledBorderColor = System.Drawing.Color.Empty;
            this.CameraMoveSpeedSlider.DisabledHandlerColor = System.Drawing.Color.FromArgb(((int)(((byte)(196)))), ((int)(((byte)(196)))), ((int)(((byte)(196)))));
            this.CameraMoveSpeedSlider.DisabledValueColor = System.Drawing.Color.FromArgb(((int)(((byte)(205)))), ((int)(((byte)(205)))), ((int)(((byte)(205)))));
            this.CameraMoveSpeedSlider.HandlerColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.CameraMoveSpeedSlider.IsDerivedStyle = true;
            this.CameraMoveSpeedSlider.Location = new System.Drawing.Point(314, 30);
            this.CameraMoveSpeedSlider.Maximum = 100;
            this.CameraMoveSpeedSlider.Minimum = 3;
            this.CameraMoveSpeedSlider.Name = "CameraMoveSpeedSlider";
            this.CameraMoveSpeedSlider.Size = new System.Drawing.Size(147, 16);
            this.CameraMoveSpeedSlider.Style = MetroSet_UI.Enums.Style.Light;
            this.CameraMoveSpeedSlider.StyleManager = null;
            this.CameraMoveSpeedSlider.TabIndex = 27;
            this.CameraMoveSpeedSlider.Text = "metroSetTrackBar6";
            this.CameraMoveSpeedSlider.ThemeAuthor = "Narwin";
            this.CameraMoveSpeedSlider.ThemeName = "MetroLite";
            this.CameraMoveSpeedSlider.Value = 40;
            this.CameraMoveSpeedSlider.ValueColor = System.Drawing.Color.FromArgb(((int)(((byte)(65)))), ((int)(((byte)(177)))), ((int)(((byte)(225)))));
            this.CameraMoveSpeedSlider.Scroll += new MetroSet_UI.Controls.MetroSetTrackBar.ScrollEventHandler(this.CameraMoveSpeedSlider_Scroll);
            // 
            // uiLabel2
            // 
            this.uiLabel2.BackColor = System.Drawing.Color.Transparent;
            this.uiLabel2.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.uiLabel2.Font = new System.Drawing.Font("Impact", 9F);
            this.uiLabel2.ForeColor = System.Drawing.Color.White;
            this.uiLabel2.Location = new System.Drawing.Point(311, 49);
            this.uiLabel2.Name = "uiLabel2";
            this.uiLabel2.Size = new System.Drawing.Size(127, 23);
            this.uiLabel2.TabIndex = 30;
            this.uiLabel2.Text = "Camera Rotation Speed";
            // 
            // uiLabel1
            // 
            this.uiLabel1.BackColor = System.Drawing.Color.Transparent;
            this.uiLabel1.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.uiLabel1.Font = new System.Drawing.Font("Impact", 9F);
            this.uiLabel1.ForeColor = System.Drawing.Color.White;
            this.uiLabel1.Location = new System.Drawing.Point(311, 12);
            this.uiLabel1.Name = "uiLabel1";
            this.uiLabel1.Size = new System.Drawing.Size(150, 23);
            this.uiLabel1.TabIndex = 28;
            this.uiLabel1.Text = "Camera Movement Speed";
            // 
            // ToolTipLabel
            // 
            this.ToolTipLabel.BackColor = System.Drawing.Color.Transparent;
            this.ToolTipLabel.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.ToolTipLabel.Font = new System.Drawing.Font("Leelawadee UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ToolTipLabel.ForeColor = System.Drawing.Color.White;
            this.ToolTipLabel.Location = new System.Drawing.Point(64, 190);
            this.ToolTipLabel.Name = "ToolTipLabel";
            this.ToolTipLabel.Size = new System.Drawing.Size(351, 23);
            this.ToolTipLabel.TabIndex = 26;
            this.ToolTipLabel.Text = "Freecam: W A S D + Shift + Space to move, I J K L to look around";
            this.ToolTipLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // FreecamLabel
            // 
            this.FreecamLabel.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.FreecamLabel.Font = new System.Drawing.Font("Impact", 12F);
            this.FreecamLabel.ForeColor = System.Drawing.Color.White;
            this.FreecamLabel.Location = new System.Drawing.Point(83, 13);
            this.FreecamLabel.Name = "FreecamLabel";
            this.FreecamLabel.Size = new System.Drawing.Size(151, 23);
            this.FreecamLabel.TabIndex = 25;
            this.FreecamLabel.Text = "Freecam Mode";
            // 
            // FreecamSwitch
            // 
            this.FreecamSwitch.BackColor = System.Drawing.Color.Transparent;
            this.FreecamSwitch.BackgroundColor = System.Drawing.Color.Empty;
            this.FreecamSwitch.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(165)))), ((int)(((byte)(159)))), ((int)(((byte)(147)))));
            this.FreecamSwitch.CheckColor = System.Drawing.Color.FromArgb(((int)(((byte)(65)))), ((int)(((byte)(177)))), ((int)(((byte)(225)))));
            this.FreecamSwitch.CheckState = MetroSet_UI.Enums.CheckState.Unchecked;
            this.FreecamSwitch.Cursor = System.Windows.Forms.Cursors.Hand;
            this.FreecamSwitch.DisabledBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(205)))), ((int)(((byte)(205)))), ((int)(((byte)(205)))));
            this.FreecamSwitch.DisabledCheckColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(65)))), ((int)(((byte)(177)))), ((int)(((byte)(225)))));
            this.FreecamSwitch.DisabledUnCheckColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(205)))), ((int)(((byte)(205)))), ((int)(((byte)(205)))));
            this.FreecamSwitch.IsDerivedStyle = true;
            this.FreecamSwitch.Location = new System.Drawing.Point(19, 13);
            this.FreecamSwitch.Name = "FreecamSwitch";
            this.FreecamSwitch.Size = new System.Drawing.Size(58, 22);
            this.FreecamSwitch.Style = MetroSet_UI.Enums.Style.Light;
            this.FreecamSwitch.StyleManager = null;
            this.FreecamSwitch.Switched = false;
            this.FreecamSwitch.SymbolColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(160)))), ((int)(((byte)(255)))));
            this.FreecamSwitch.TabIndex = 7;
            this.FreecamSwitch.ThemeAuthor = "Narwin";
            this.FreecamSwitch.ThemeName = "MetroLite";
            this.FreecamSwitch.UnCheckColor = System.Drawing.Color.White;
            // 
            // TabControl
            // 
            this.TabControl.Controls.Add(this.Utility);
            this.TabControl.Controls.Add(this.Information);
            this.TabControl.Cursor = System.Windows.Forms.Cursors.Default;
            this.TabControl.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
            this.TabControl.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.TabControl.Font = new System.Drawing.Font("Impact", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TabControl.ItemSize = new System.Drawing.Size(120, 30);
            this.TabControl.Location = new System.Drawing.Point(15, 93);
            this.TabControl.MainPage = "";
            this.TabControl.MenuStyle = Sunny.UI.UIMenuStyle.Custom;
            this.TabControl.Name = "TabControl";
            this.TabControl.SelectedIndex = 0;
            this.TabControl.Size = new System.Drawing.Size(482, 245);
            this.TabControl.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.TabControl.TabBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.TabControl.TabIndex = 1;
            this.TabControl.TabSelectedColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(160)))), ((int)(((byte)(255)))));
            this.TabControl.TabSelectedForeColor = System.Drawing.Color.White;
            this.TabControl.TabSelectedHighColor = System.Drawing.Color.RoyalBlue;
            this.TabControl.TabSelectedHighColorSize = 2;
            this.TabControl.TabUnSelectedForeColor = System.Drawing.Color.White;
            this.TabControl.TipsFont = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            // 
            // metroSetSetToolTip1
            // 
            this.metroSetSetToolTip1.BackColor = System.Drawing.Color.White;
            this.metroSetSetToolTip1.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(204)))), ((int)(((byte)(204)))), ((int)(((byte)(204)))));
            this.metroSetSetToolTip1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(170)))), ((int)(((byte)(170)))), ((int)(((byte)(170)))));
            this.metroSetSetToolTip1.IsDerivedStyle = true;
            this.metroSetSetToolTip1.OwnerDraw = true;
            this.metroSetSetToolTip1.Style = MetroSet_UI.Enums.Style.Light;
            this.metroSetSetToolTip1.StyleManager = null;
            this.metroSetSetToolTip1.ThemeAuthor = "Narwin";
            this.metroSetSetToolTip1.ThemeName = "MetroLite";
            // 
            // SiliconForm
            // 
            this.AllowResize = false;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.Transparent;
            this.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.BackgroundImageTransparency = 0.1F;
            this.ClientSize = new System.Drawing.Size(512, 353);
            this.Controls.Add(this.TabControl);
            this.Controls.Add(this.metroSetControlBox1);
            this.Font = new System.Drawing.Font("Impact", 13.2F);
            this.HeaderColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(160)))), ((int)(((byte)(255)))));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MaximumSize = new System.Drawing.Size(512, 353);
            this.MinimumSize = new System.Drawing.Size(512, 353);
            this.Name = "SiliconForm";
            this.Opacity = 0.95D;
            this.Padding = new System.Windows.Forms.Padding(12, 90, 12, 12);
            this.ShowTitle = false;
            this.SmallLineColor1 = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(160)))), ((int)(((byte)(255)))));
            this.SmallLineColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(160)))), ((int)(((byte)(255)))));
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Style = MetroSet_UI.Enums.Style.Custom;
            this.Text = "Silicon";
            this.TextColor = System.Drawing.Color.White;
            this.ThemeAuthor = "Hispano";
            this.ThemeName = "Helium-Red";
            this.UseSlideAnimation = true;
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Information.ResumeLayout(false);
            this.Utility.ResumeLayout(false);
            this.TabControl.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private MetroSet_UI.Controls.MetroSetControlBox metroSetControlBox1;
        private System.ComponentModel.BackgroundWorker HeliumWorker;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.TabPage Information;
        private MetroSet_UI.Controls.MetroSetLabel Status;
        private MetroSet_UI.Controls.MetroSetLabel getStatus;
        private MetroSet_UI.Controls.MetroSetLabel Hispano;
        private MetroSet_UI.Controls.MetroSetLabel proID;
        private MetroSet_UI.Controls.MetroSetLabel procIDLabel;
        private System.Windows.Forms.TabPage Utility;
        private Sunny.UI.UITabControl TabControl;
        private MetroSet_UI.Controls.MetroSetSwitch FreecamSwitch;
        private Sunny.UI.UILabel FreecamLabel;
        private Sunny.UI.UILabel ToolTipLabel;
        private MetroSet_UI.Controls.MetroSetTrackBar CameraRotateSpeedSlider;
        private MetroSet_UI.Controls.MetroSetTrackBar CameraMoveSpeedSlider;
        private Sunny.UI.UILabel uiLabel2;
        private Sunny.UI.UILabel uiLabel1;
        private Sunny.UI.UILabel uiLabel5;
        private MetroSet_UI.Controls.MetroSetSwitch HidePlayerModelSwitch;
        private MetroSet_UI.Controls.MetroSetTrackBar CameraFOVSlider;
        private Sunny.UI.UILabel uiLabel4;
        private MetroSet_UI.Controls.MetroSetTrackBar CameraDistanceSlider;
        private Sunny.UI.UILabel uiLabel3;
        private MetroSet_UI.Components.MetroSetSetToolTip metroSetSetToolTip1;
        private Sunny.UI.UILabel CameraLookAtInfoLabel;
        private Sunny.UI.UILabel uiLabel6;
        private Sunny.UI.UILabel CameraRotationInfoLabel;
        private MetroSet_UI.Controls.MetroSetButton AddAnimationFrameButton;
        private MetroSet_UI.Controls.MetroSetButton PlayAnimationButton;
    }
}

