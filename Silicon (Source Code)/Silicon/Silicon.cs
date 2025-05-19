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
using System.Diagnostics;
using System.Text;
using System.Linq;



namespace Silicon
{
    public partial class SiliconForm : MetroSetForm
    {
        private Dictionary<IntPtr, PictureBox> cubicThumbnails = new Dictionary<IntPtr, PictureBox>();
        private IntPtr currentConnectedHwnd = IntPtr.Zero;
        private List<IntPtr> currentCubicWindows = new List<IntPtr>();


        public Mem m = new Mem();
        private System.Timers.Timer processCheckTimer;
        private HashSet<Keys> pressedKeys = new HashSet<Keys>();
        private Thread keyPollingThread;
        private bool isRunning = true;
        private bool isChatting = false;

        private const float equalityTolerance = 5e-5f;

        private Interpolator.MethodDelegate _interpolator = Interpolator.GetMethodWithIndex(0);
        private double animationStartTime;
        private double animationDuration;
        private GlobalMouseHook mouseHook;

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(Keys key);

        [DllImport("user32.dll")]
        static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("dwmapi.dll")]
        static extern int DwmRegisterThumbnail(IntPtr dest, IntPtr src, out IntPtr thumb);

        [DllImport("dwmapi.dll")]
        static extern int DwmUnregisterThumbnail(IntPtr thumb);

        [DllImport("dwmapi.dll")]
        static extern int DwmUpdateThumbnailProperties(IntPtr hThumbnail, ref DWM_THUMBNAIL_PROPERTIES props);

        [StructLayout(LayoutKind.Sequential)]
        struct DWM_THUMBNAIL_PROPERTIES
        {
            public uint dwFlags;
            public RECT rcDestination;
            public RECT rcSource;
            public byte opacity;
            public bool fVisible;
            public bool fSourceClientAreaOnly;

            public const uint DWM_TNP_RECTDESTINATION = 0x00000001;
            public const uint DWM_TNP_RECTSOURCE = 0x00000002;
            public const uint DWM_TNP_OPACITY = 0x00000004;
            public const uint DWM_TNP_VISIBLE = 0x00000008;
            public const uint DWM_TNP_SOURCECLIENTAREAONLY = 0x00000010;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        static extern bool PrintWindow(IntPtr hwnd, IntPtr hDC, uint nFlags);

        [DllImport("user32.dll")]
        static extern IntPtr GetWindowDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("user32.dll")]
        static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);




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
            procIDLabel.ForeColor = Color.Red;
            getStatus.ForeColor = Color.Red;
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
            PopulateCubicWindowThumbnails();

            if (!SiliconWorker.IsBusy)
                SiliconWorker.RunWorkerAsync();

            mouseHook = new GlobalMouseHook();
            mouseHook.OnScrollDown += () =>
            {
                if (!IsCubicWindowFocused() && !IsSiliconWindowFocused())
                    return;

                if ((Control.ModifierKeys & Keys.Alt) == Keys.Alt)
                {
                    CameraDistanceSlider.Value = Math.Min(100, CameraDistanceSlider.Value + 1);
                }
                else
                {
                    CameraFOVSlider.Value = Math.Min(135, CameraFOVSlider.Value + 1);
                }
            };

            mouseHook.OnScrollUp += () =>
            {
                if (!IsCubicWindowFocused() && !IsSiliconWindowFocused())
                    return;

                if ((Control.ModifierKeys & Keys.Alt) == Keys.Alt)
                {
                    CameraDistanceSlider.Value = Math.Max(1, CameraDistanceSlider.Value - 1);
                }
                else
                {
                    CameraFOVSlider.Value = Math.Max(10, CameraFOVSlider.Value - 1);
                }
            };
            InitMouseDrag();


        }

        private void StartProcessCheckTimer()
        {
            processCheckTimer = new Timer(3000);
            processCheckTimer.Elapsed += ProcessCheckTimer_Elapsed;
            processCheckTimer.AutoReset = true;
            processCheckTimer.Enabled = true;
        }

        private bool wasConnected = false;
        private int connectedPID = -1;

        private void ProcessCheckTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            processCheckTimer.Enabled = false;

            if (connectedPID > 0)
            {
                // Check if the process we selected manually is still running
                bool stillExists = Process.GetProcesses().Any(p => p.Id == connectedPID);

                if (stillExists)
                {
                    // Try to open the same process again
                    bool openProc = m.OpenProcess(connectedPID);
                    if (openProc)
                    {
                        this.Invoke(new Action(() =>
                        {
                            UpdateLabel(procIDLabel, connectedPID.ToString(), Color.Green);
                            UpdateLabel(getStatus, "CONNECTED", Color.Green);
                        }));

                        if (!wasConnected)
                        {
                            wasConnected = true;
                            InjectBaseFunctions();
                        }

                        processCheckTimer.Enabled = true;
                        return; // connectedPID is still valid and connected, no further action needed
                    }
                }

                // If the selected PID doesn't exist or can't be opened, reset connection
                connectedPID = -1;
                wasConnected = false;

                this.Invoke(new Action(() =>
                {
                    UpdateLabel(procIDLabel, "DISCONNECTED", Color.Red);
                    UpdateLabel(getStatus, "DISCONNECTED", Color.Red);
                }));

                // Show your Cubic window selector panel again to allow user to select window
                this.Invoke(new Action(() =>
                {
                    PopulateCubicWindowThumbnails();
                    CubicWindows.Visible = true;
                }));

                processCheckTimer.Enabled = true;
                return;
            }
            else
            {
                // No manual selection yet, try to detect Cubic windows

                var cubicProcesses = Process.GetProcessesByName("Cubic");
                if (cubicProcesses.Length == 0)
                {
                    // No Cubic windows at all
                    connectedPID = -1;
                    wasConnected = false;
                    this.Invoke(new Action(() =>
                    {
                        UpdateLabel(procIDLabel, "DISCONNECTED", Color.Red);
                        UpdateLabel(getStatus, "DISCONNECTED", Color.Red);
                        CubicWindows.Visible = false;
                    }));
                }
                else if (cubicProcesses.Length == 1)
                {
                    // Auto-connect to the only Cubic window
                    connectedPID = cubicProcesses[0].Id;
                    bool openProc = m.OpenProcess(connectedPID);

                    this.Invoke(new Action(() =>
                    {
                        UpdateLabel(procIDLabel, connectedPID.ToString(), Color.Green);
                        UpdateLabel(getStatus, "CONNECTED", Color.Green);
                        CubicWindows.Visible = false;
                    }));

                    if (!wasConnected)
                    {
                        wasConnected = true;
                        InjectBaseFunctions();
                    }
                }
                else
                {
                    // Multiple Cubic windows found: Show selector panel
                    this.Invoke(new Action(() =>
                    {
                        PopulateCubicWindowThumbnails();
                        CubicWindows.Visible = true;
                    }));
                }

                processCheckTimer.Enabled = true;
            }
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


        private void Silicon_OnFormClosing(object sender, FormClosingEventArgs e)
        {
            isRunning = false;
            keyPollingThread.Join();
            mouseHook?.Dispose();
        }
        // Here ends the code edited by Hispano



        private bool IsCubicWindowFocused()
        {
            if (connectedPID <= 0)
                return false;

            IntPtr foregroundWindow = GetForegroundWindow();
            if (foregroundWindow == IntPtr.Zero)
                return false;

            GetWindowThreadProcessId(foregroundWindow, out uint foregroundPID);
            return foregroundPID == (uint)connectedPID;
        }

        private bool IsSiliconWindowFocused()
        {
            IntPtr foregroundWindow = GetForegroundWindow();
            if (foregroundWindow == IntPtr.Zero)
                return false;

            GetWindowThreadProcessId(foregroundWindow, out uint foregroundPID);
            uint currentPID = (uint)Process.GetCurrentProcess().Id;
            return foregroundPID == currentPID;
        }


        private void PopulateCubicWindowThumbnails()
        {
            CubicWindows.Controls.Clear();

            List<IntPtr> cubicWindows = new List<IntPtr>();

            EnumWindows((hWnd, lParam) =>
            {
                if (!IsWindowVisible(hWnd))
                    return true;

                GetWindowThreadProcessId(hWnd, out uint pid);

                try
                {
                    Process proc = Process.GetProcessById((int)pid);
                    string exeName = Path.GetFileName(proc.MainModule.FileName);

                    if (string.Equals(exeName, "Cubic.exe", StringComparison.OrdinalIgnoreCase))
                    {
                        cubicWindows.Add(hWnd);
                    }
                }
                catch
                {
                    // Ignore inaccessible/exited processes
                }

                return true;
            }, IntPtr.Zero);

            if (cubicWindows.Count == 0)
            {
                CubicWindows.Visible = false;
                return;
            }

            if (cubicWindows.Count == 1)
            {
                // Auto-connect to the only one
                ConnectToCubicWindow(cubicWindows[0]);
                CubicWindows.Visible = false;
                return;
            }

            // Multiple Cubic windows: Show panel
            CubicWindows.Visible = true;

            foreach (var hWnd in cubicWindows)
            {
                StringBuilder sb = new StringBuilder(256);
                GetWindowText(hWnd, sb, sb.Capacity);
                string title = sb.ToString();

                PictureBox thumbnail = CreateWindowThumbnail(hWnd, title);
                CubicWindows.Controls.Add(thumbnail);
            }
        }

        private void ConnectToCubicWindow(IntPtr hWnd)
        {
            GetWindowThreadProcessId(hWnd, out uint pid);

            if (m.OpenProcess((int)pid))
            {
                connectedPID = (int)pid;
                wasConnected = true;
                UpdateLabel(procIDLabel, pid.ToString(), Color.Green);
                UpdateLabel(getStatus, "CONNECTED", Color.Green);
                InjectBaseFunctions();
                CubicWindows.Visible = false;
            }
            else
            {
                MessageBox.Show("Failed to connect to PID " + pid);
            }
        }


        private PictureBox CreateWindowThumbnail(IntPtr hWnd, string title)
        {
            RECT rect;
            GetClientRect(hWnd, out rect);
            int width = rect.Right - rect.Left;
            int height = rect.Bottom - rect.Top;

            Bitmap bmp = new Bitmap(Math.Max(width, 1), Math.Max(height, 1));
            Graphics gfx = Graphics.FromImage(bmp);
            IntPtr hdc = gfx.GetHdc();

            bool success = PrintWindow(hWnd, hdc, 0);

            gfx.ReleaseHdc(hdc);
            gfx.Dispose();

            if (!success)
            {
                bmp = new Bitmap(200, 100);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.Black);
                    g.DrawString("Preview not available", SystemFonts.DefaultFont, Brushes.White, 10, 40);
                }
            }

            PictureBox picBox = new PictureBox
            {
                Image = bmp,
                Width = 200,
                Height = 120,
                SizeMode = PictureBoxSizeMode.Zoom,
                Margin = new Padding(10),
                Cursor = Cursors.Hand,
                Tag = hWnd
            };

            picBox.Click += (s, e) =>
            {
                IntPtr selectedHwnd = (IntPtr)((PictureBox)s).Tag;
                ConnectToCubicWindow(selectedHwnd);
            };


            return picBox;
        }
    }
}