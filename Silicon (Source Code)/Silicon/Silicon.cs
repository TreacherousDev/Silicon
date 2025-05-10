using Memory;
using MetroSet_UI.Forms;
using System;
using System.IO;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Timers;
using Timer = System.Timers.Timer;
using System.Collections.Generic;
using System.Threading;
using System.Runtime.InteropServices;



namespace Silicon
{
    public partial class SiliconForm : MetroSetForm
    {
        public Mem m = new Mem();
        private System.Timers.Timer processCheckTimer;
        private HashSet<Keys> pressedKeys = new HashSet<Keys>();
        private Thread keyPollingThread;
        private bool isRunning = true;

        private const float equalityTolerance = 5e-5f;

        private Interpolator.MethodDelegate _interpolator = Interpolator.GetMethodWithIndex(0);
        private double animationStartTime;
        private double animationDuration;


        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(Keys key);
        public SiliconForm()
        {
            InitializeComponent();
            StartKeyPolling();

            listViewFrames.View = View.Details;
            listViewFrames.FullRowSelect = true;
            listViewFrames.AllowDrop = true;

            // Drag-and-Drop event handlers
            listViewFrames.ItemDrag += ListViewFrames_ItemDrag;
            listViewFrames.DragEnter += ListViewFrames_DragEnter;
            listViewFrames.DragDrop += ListViewFrames_DragDrop;


            // Some color overrides since the TabControl seems to be bugged and sets by default the ForeColor to Black even when the default color has been changed.
            Hispano.ForeColor = Color.Gray;
            proID.ForeColor = Color.White;
            Status.ForeColor = Color.White;
            HotkeysLabel.ForeColor = Color.White;
            List<MetroSet_UI.Controls.MetroSetButton> interfaceButtons = new List<MetroSet_UI.Controls.MetroSetButton>
            {
                Preset1Button,
                Preset2Button,
                Preset3Button,
                AddAnimationFrameButton,
                PlayAnimationButton,
                DeleteAnimationFrameButton,
                GoToAnnimationFrameButton,
                SaveAnimationButton,
                LoadAnimationButton
            };
            foreach (MetroSet_UI.Controls.MetroSetButton button in interfaceButtons) 
            {
                button.NormalBorderColor = Color.FromArgb(80, 160, 255);
                button.NormalColor = Color.FromArgb(80, 160, 255);
                button.PressBorderColor = Color.FromArgb(64, 128, 204);
                button.PressColor = Color.FromArgb(64, 128, 204);
            }

        }

        private void SiliconWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            _updateTimer = new Timer(5);
            _updateTimer.Elapsed += UpdateMemoryOnTimerTick;
            _updateTimer.AutoReset = true;
            _updateTimer.Start();
        }

        private void Silicon_Load(object sender, EventArgs e)
        {
            StartProcessCheckTimer();

            if (!SiliconWorker.IsBusy)
                SiliconWorker.RunWorkerAsync();
        }

        private void StartProcessCheckTimer()
        {
            processCheckTimer = new Timer(1000);
            processCheckTimer.Elapsed += ProcessCheckTimer_Elapsed;
            processCheckTimer.AutoReset = true;
            processCheckTimer.Enabled = true;
        }

        private bool wasConnected = false;

        // This reads and tells you if the program can find the game process (Cubic.exe)
        private void ProcessCheckTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            processCheckTimer.Enabled = false;

            int pID = m.GetProcIdFromName("Cubic");
            bool openProc = pID > 0 && m.OpenProcess(pID);

            if (openProc)
            {
                this.Invoke(new Action(() =>
                {
                    UpdateLabel(procIDLabel, pID.ToString(), Color.Green);
                    UpdateLabel(getStatus, "CONNECTED", Color.Green);
                }));

                if (!wasConnected)
                {
                    wasConnected = true;
                    InjectBaseFunctions();
                }

            }
            else
            {
                this.Invoke(new Action(() =>
                {
                    UpdateLabel(procIDLabel, "DISCONNECTED", Color.Red);
                    UpdateLabel(getStatus, "DISCONNECTED", Color.Red);
                }));

                if (wasConnected)
                {
                    wasConnected = false;
                }
            }
            processCheckTimer.Enabled = true;
        }

        private void UpdateLabel(Label label, string text, Color color)
        {

            if (label.InvokeRequired)
            {
                label.Invoke((MethodInvoker)delegate
                {
                    label.Text = text;
                    label.ForeColor = color;
                });
            }
            else
            {
                label.Text = text;
                label.ForeColor = color;
            }
        }



        // This function gets called in the main update loop
        // It handles the movement and rotation of the camera when freecam is activated
        // Here starts the code edited by Hispano
        private void StartKeyPolling()
        {
            keyPollingThread = new Thread(() =>
            {
                while (isRunning)
                {
                    UpdateKeyStates();
                    Thread.Sleep(10);
                }
            })
            {
                IsBackground = true
            };
            keyPollingThread.Start();
        }

        private void UpdateKeyStates()
        {
            // List of keys to monitor
            Keys[] keysToMonitor = new Keys[]
            {
                Keys.W, Keys.S, Keys.A, Keys.D,
                Keys.Space, Keys.ControlKey,
                Keys.Up, Keys.Down, Keys.Left, Keys.Right,
                Keys.F1, Keys.F2, Keys.F3, Keys.F4, Keys.C, Keys.V, Keys.B, Keys.N, Keys.M
            };

            foreach (var key in keysToMonitor)
            {
                // Check if the key is currently pressed
                bool isPressed = (GetAsyncKeyState(key) & 0x8000) != 0;

                if (isPressed)
                {
                    // Add the key if is pressed
                    if (!pressedKeys.Contains(key))
                    {
                        pressedKeys.Add(key);
                        HandleKeyDown(key);
                    }
                }
                else
                {
                    // Remove the key if is released
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
            if (FreecamSwitch.InvokeRequired)
            {
                FreecamSwitch.Invoke(new Action(() => HandleKeyDown(key)));
                return;
            }

            switch (key)
            {
                case Keys.F1:
                    Preset1Button_Click(null, EventArgs.Empty);
                    break;
                case Keys.F2:
                    Preset2Button_Click(null, EventArgs.Empty);
                    break;
                case Keys.F3:
                    Preset3Button_Click(null, EventArgs.Empty);
                    break;
                case Keys.F4:
                    FreecamSwitch.Switched = !FreecamSwitch.Switched;
                    break;
                case Keys.C:
                    ActivateGoToFrame(0);
                    break;
                case Keys.V:
                    ActivateGoToFrame(1);
                    break;
                case Keys.B:
                    ActivateGoToFrame(2);
                    break;
                case Keys.N:
                    ActivateGoToFrame(3);
                    break;
                case Keys.M:
                    ActivateGoToFrame(4);
                    break;
            }
        }


        private void Silicon_OnFormClosing(object sender, FormClosingEventArgs e)
        {
            isRunning = false;
            keyPollingThread.Join();
        }
        // Here ends the code edited by Hispano

    }
}