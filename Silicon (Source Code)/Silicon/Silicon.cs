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
        private Thread keyPollingThread;
        private bool isRunning = true;
        private bool isChatting = false;
        private int cinematicLoadedCameraDistance;

        private const float equalityTolerance = 5e-5f;

        private Interpolator.MethodDelegate _interpolator = Interpolator.GetMethodWithIndex(0);
        private double animationStartTime;
        private double animationDuration;
        private GlobalMouseHook mouseHook;

        private bool wasConnected = false;
        private int connectedPID = -1;

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

        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        static extern bool PrintWindow(IntPtr hwnd, IntPtr hDC, uint nFlags);

        [DllImport("user32.dll")]
        static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

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


            // Some color overrides since the TabControl seems to be bugged
            // and sets by default the ForeColor to Black even when the default color has been changed.
            Hispano.ForeColor = Color.Gray;
            proID.ForeColor = Color.White;
            Status.ForeColor = Color.White;
            procIDLabel.ForeColor = Color.Red;
            getStatus.ForeColor = Color.Red;
            TabBorder1.BackColor = Color.Transparent;
            List<MetroSet_UI.Controls.MetroSetButton> interfaceButtons = new List<MetroSet_UI.Controls.MetroSetButton>
            {
                Preset1Button,
                Preset2Button,
                Preset3Button,
                Preset4Button,
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
            InitializeKeyBindings();
            LoadKeyBindings();
            PopulateHotkeyPanel();
            

            if (!SiliconWorker.IsBusy)
                SiliconWorker.RunWorkerAsync();

            mouseHook = new GlobalMouseHook();
            InitMouseDrag();
            InitMouseScroll();
        }

        private void StartProcessCheckTimer()
        {
            processCheckTimer = new Timer(3000);
            processCheckTimer.Elapsed += ProcessCheckTimer_Elapsed;
            processCheckTimer.AutoReset = true;
            processCheckTimer.Enabled = true;
        }

        // Called every 3000 ms, connects to Cubic or updates thumbnails
        private void ProcessCheckTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            processCheckTimer.Enabled = false;

            if (connectedPID > 0) HandleManualProcess();
            else AutoDetectAndConnect();

            processCheckTimer.Enabled = true;
        }

        private void UpdateMemoryOnTimerTick(object sender, ElapsedEventArgs e)
        {
            if (!wasConnected) return;
            UpdateMemory();
            UpdateUtilityUI();
            InterpolateCamera();
        }

        private void HandleManualProcess()
        {
            // Try to get the process by the manual PID
            Process proc = null;
            try { proc = Process.GetProcessById(connectedPID); } catch { }

            if (proc != null && !proc.HasExited && m.OpenProcess(connectedPID))
            {
                SafeInvoke(() => UpdateConnectionStatus(true, connectedPID));

                if (!wasConnected)
                {
                    wasConnected = true;

                    InjectBaseFunctions();
                }
                return;
            }

            // Reset if process is gone
            connectedPID = -1;
            wasConnected = false;
            SafeInvoke(() =>
            {
                UpdateConnectionStatus(false);
                PopulateCubicWindowThumbnails();
                CubicWindows.Visible = true;
            });
        }

        private void AutoDetectAndConnect()
        {
            var cubicProcesses = Process.GetProcessesByName("Cubic");
            if (cubicProcesses.Length == 0)
            {
                connectedPID = -1;
                wasConnected = false;
                SafeInvoke(() =>
                {
                    UpdateConnectionStatus(false);
                    CubicWindows.Visible = false;
                });
            }
            else if (cubicProcesses.Length == 1)
            {
                Process targetProc = cubicProcesses[0]; // Get the specific process object
                connectedPID = targetProc.Id;

                if (m.OpenProcess(connectedPID))
                {
                    SafeInvoke(() =>
                    {
                        UpdateConnectionStatus(true, connectedPID);
                        CubicWindows.Visible = false;
                    });

                    if (!wasConnected)
                    {
                        wasConnected = true;

                        InjectBaseFunctions();
                    }
                }
            }
            else
            {
                // Multiple processes found - user must select one from thumbnails
                SafeInvoke(() =>
                {
                    PopulateCubicWindowThumbnails();
                    CubicWindows.Visible = true;
                });
            }
        }

        private void UpdateConnectionStatus(bool connected, int pid = -1)
        {
            if (connected)
            {
                UpdateLabel(procIDLabel, pid.ToString(), Color.Green);
                UpdateLabel(getStatus, "CONNECTED", Color.Green);
            }
            else
            {
                UpdateLabel(procIDLabel, "DISCONNECTED", Color.Red);
                UpdateLabel(getStatus, "DISCONNECTED", Color.Red);
            }
        }

        private void SafeInvoke(Action action)
        {
            if (this.InvokeRequired) this.Invoke(action);
            else action();
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

            processCheckTimer?.Stop();
            processCheckTimer?.Dispose();

            _updateTimer?.Stop();
            _updateTimer?.Dispose();

            mouseHook?.Dispose();

            // Only join if the thread is actually running
            if (keyPollingThread != null && keyPollingThread.IsAlive)
                keyPollingThread.Join(2000); // timeout after 2 seconds max
        }

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

        private void ConnectToCubicWindow(IntPtr hWnd)
        {
            GetWindowThreadProcessId(hWnd, out uint pid);

            if (m.OpenProcess((int)pid))
            {
                connectedPID = (int)pid;
                wasConnected = true;

                // GET PROCESS OBJECT
                Process proc = Process.GetProcessById((int)pid);

                UpdateLabel(procIDLabel, pid.ToString(), Color.Green);
                UpdateLabel(getStatus, "CONNECTED", Color.Green);

                InjectBaseFunctions();
                // --- NEW LOGIC END ---

                CubicWindows.Visible = false;
            }
        }

        private void PopulateCubicWindowThumbnails()
        {
            CubicWindows.Controls.Clear();

            // Create a list to store found windows
            currentCubicWindows.Clear();

            // Use a background thread to find windows
            ThreadPool.QueueUserWorkItem(state =>
            {
                // Find all Cubic windows
                List<IntPtr> cubicWindows = FindCubicWindows();

                this.Invoke(new Action(() =>
                {
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
                    currentCubicWindows = cubicWindows;

                    // Create thumbnails asynchronously
                    foreach (var hWnd in cubicWindows)
                    {
                        // Create a placeholder initially 
                        PictureBox thumbnail = CreatePlaceholderThumbnail(hWnd);
                        CubicWindows.Controls.Add(thumbnail);

                        // Then load the real screenshot asynchronously
                        LoadThumbnailAsync(thumbnail, hWnd);
                    }
                }));
            });
        }

        private List<IntPtr> FindCubicWindows()
        {
            List<IntPtr> cubicWindows = new List<IntPtr>();

            EnumWindows((hWnd, lParam) =>
            {
                if (!IsWindowVisible(hWnd))
                    return true;

                GetWindowThreadProcessId(hWnd, out uint pid);

                try
                {
                    using (Process proc = Process.GetProcessById((int)pid))
                    {
                        string exeName = Path.GetFileName(proc.MainModule.FileName);

                        if (string.Equals(exeName, "Cubic.exe", StringComparison.OrdinalIgnoreCase))
                        {
                            cubicWindows.Add(hWnd);
                        }
                    }
                }
                catch
                {
                    // Ignore inaccessible/exited processes
                }

                return true;
            }, IntPtr.Zero);

            return cubicWindows;
        }

        private PictureBox CreatePlaceholderThumbnail(IntPtr hWnd)
        {
            // Get window title for display
            StringBuilder sb = new StringBuilder(256);
            GetWindowText(hWnd, sb, sb.Capacity);
            string title = sb.ToString();

            // Create a simple placeholder
            Bitmap placeholderBmp = new Bitmap(200, 120);
            using (Graphics g = Graphics.FromImage(placeholderBmp))
            {
                g.Clear(Color.FromArgb(40, 40, 40));
                g.DrawString("Loading...", SystemFonts.DefaultFont, Brushes.White, 10, 50);
                g.DrawString(title, SystemFonts.DefaultFont, Brushes.LightGray, 10, 70);
            }

            PictureBox picBox = new PictureBox
            {
                Image = placeholderBmp,
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

        private void LoadThumbnailAsync(PictureBox pictureBox, IntPtr hWnd)
        {
            ThreadPool.QueueUserWorkItem(state =>
            {
                Bitmap bmp = null;

                try
                {
                    bmp = CaptureWindow(hWnd, 200, 120);

                    if (IsDisposed || pictureBox == null || pictureBox.IsDisposed)
                    {
                        bmp?.Dispose();
                        return;
                    }

                    SafeInvoke(() =>
                    {
                        UpdatePictureBoxImage(pictureBox, bmp);
                        SetWindowTitle(pictureBox, hWnd); // optional UI title logic
                    });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error in LoadThumbnailAsync: " + ex.Message);
                    bmp?.Dispose();
                }
            });
        }

        private void UpdatePictureBoxImage(PictureBox pictureBox, Bitmap bmp)
        {
            Image oldImage = pictureBox.Image;
            pictureBox.Image = bmp;
            oldImage?.Dispose();
        }

        private void SetWindowTitle(PictureBox pictureBox, IntPtr hWnd)
        {
            StringBuilder sb = new StringBuilder(256);
            GetWindowText(hWnd, sb, sb.Capacity);
            string title = sb.ToString();

            // Placeholder: Use this string for labeling if needed
            // pictureBox.Text = title;
        }

        private Bitmap CaptureWindow(IntPtr hWnd, int width, int height)
        {
            // Get window size to maintain aspect ratio
            RECT rect;
            GetClientRect(hWnd, out rect);
            int winWidth = Math.Max(rect.Right - rect.Left, 1);
            int winHeight = Math.Max(rect.Bottom - rect.Top, 1);

            // First capture at full size
            Bitmap fullSizeBmp = new Bitmap(winWidth, winHeight);

            using (Graphics gfx = Graphics.FromImage(fullSizeBmp))
            {
                IntPtr hdc = gfx.GetHdc();

                try
                {
                    // Use the PrintWindow API to capture the window
                    bool success = PrintWindow(hWnd, hdc, 0);

                    if (!success)
                    {
                        // If PrintWindow fails, create a simple error indicator
                        gfx.ReleaseHdc(hdc);
                        gfx.Clear(Color.Black);
                        gfx.DrawString("Preview not available", SystemFonts.DefaultFont, Brushes.White, 10, 40);
                        return fullSizeBmp; // Return the error bitmap
                    }
                }
                finally
                {
                    gfx.ReleaseHdc(hdc);
                }
            }

            // Calculate target size with proper aspect ratio
            double aspectRatio = (double)winWidth / winHeight;
            int targetWidth = width;
            int targetHeight = (int)(width / aspectRatio);

            // Adjust if height exceeds target
            if (targetHeight > height)
            {
                targetHeight = height;
                targetWidth = (int)(height * aspectRatio);
            }

            // Create a properly sized thumbnail by scaling down the full capture
            Bitmap thumbnailBmp = new Bitmap(targetWidth, targetHeight);
            using (Graphics g = Graphics.FromImage(thumbnailBmp))
            {
                // Use high quality settings for better looking thumbnails
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;

                // Draw the full bitmap scaled down to our target size
                g.DrawImage(fullSizeBmp, 0, 0, targetWidth, targetHeight);
            }

            // Dispose of the full-size bitmap since we no longer need it
            fullSizeBmp.Dispose();

            return thumbnailBmp;
        }
    }
}