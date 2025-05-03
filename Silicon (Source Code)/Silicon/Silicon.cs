using Memory;
using MetroSet_UI.Forms;
using System;
using System.IO;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Timers;
using Sunny.UI;
using Timer = System.Timers.Timer;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlTypes;
using System.Threading;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Text.RegularExpressions;



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

        private DateTime recordingStartTime;
        private int frameCounter = 0;
        private bool isRecording = false;
        private List<List<double>> recordedFrames = new List<List<double>>();

        //private const int defaultCameraFov = 33;
        //private const int defaultCameraDistance = 22;

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

        private void Form1_Load(object sender, EventArgs e)
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

        private double currentCameraLookAtX;
        private double currentCameraLookAtY;
        private double currentCameraLookAtZ;
        private double currentCameraPitch;
        private double currentCameraYaw;

        private double targetCameraLookAtX;
        private double targetCameraLookAtY;
        private double targetCameraLookAtZ;
        private double targetCameraPitch;
        private double targetCameraYaw;

        private Timer _updateTimer;

        private void SiliconWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            _updateTimer = new Timer(5);
            _updateTimer.Elapsed += UpdateMemoryOnTimerTick;
            _updateTimer.AutoReset = true;
            _updateTimer.Start();
        }

        private void UpdateMemoryOnTimerTick(object sender, ElapsedEventArgs e)
        {
            CheckAndUpdateMemory();

            // If recording, store the current frame data
            if (isRecording)
            {
                double elapsedTime = (DateTime.Now - recordingStartTime).TotalSeconds;
                recordedFrames.Add(new List<double>
                {
                    frameCounter++,
                    elapsedTime,
                    currentCameraLookAtX,
                    currentCameraLookAtY,
                    currentCameraLookAtZ,
                    currentCameraPitch,
                    currentCameraYaw
                });
            }
        }


        // Persistent Functions (Required for the engine to function)
        readonly string cameraCoordinatesFunction = "90 90 90 90 90 90 90 90 90 90 90 90";
        readonly string adjustFogFunction = "90 90 90 90 90 90 90 90";

        readonly string cameraHeightInjection = "53 E8 00 00 00 00 5B F3 0F 5C 43 1B F3 0F 11 40 08 5B F3 0F 5C CB 8D 85 FC FE FF FF E9 CB BA 39 FF 66 66 A6 3F 00 00 00 00";
        readonly string cameraHeightFunctionEntry = "E9 19 45 C6 00 0F 1F 44 00 00";

        readonly string unlockCameraArrowsInjection = "50 E8 00 00 00 00 58 F3 0F 10 58 1F 0F 2F D8 58 0F 86 04 7E 39 FF C7 86 EC 09 00 00 00 00 B2 C2 E9 F5 7D 39 FF 00 00 B2 C2 00 00 00";
        readonly string unlockCameraArrowsFunctionEntry = "E9 ED 81 C6 00";

        readonly string unlockCameraRMBInjection = "50 E8 00 00 00 00 58 F3 0F 10 70 1F 0F 2F F1 58 0F 86 AA 94 3A FF C7 80 EC 09 00 00 00 00 B2 C2 E9 9B 94 3A FF 00 00 B2 C2 00 00 00";
        readonly string unlockCameraRMBFunctionEntry = "E9 47 6B C5 00";

        readonly string unlockCameraFOVInjection = "50 E8 00 00 00 00 58 F3 0F 10 40 12 58 8D 85 D8 FE FF FF E9 30 BA 39 FF 00 00 96 42 FF FF FF FF";
        readonly string unlockCameraFOVFunctionEntry = "E9 B9 45 C6 00 90";

        readonly string adjustCameraDistanceInjection = "50 E8 00 00 00 00 58 F3 0F 59 40 24 F3 0F 59 58 24 F3 0F 59 60 24 58 E9 00 00 00 00 F3 0F 5C D0 F3 0F 10 40 08 E9 67 B8 39 FF 00 00 C8 41 FF FF FF FF";
        readonly string adjustCameraDistanceFunctionEntry = "E9 73 47 C6 00 0F 1F 40 00";

        // Revertable functions (Optional switch states available)
        readonly string cameraLookAtEditorInjection = "50 E8 00 00 00 00 58 F3 0F 11 58 5D F3 0F 11 48 61 F3 0F 11 40 65 F3 0F 10 58 4D F3 0F 10 48 51 F3 0F 10 40 55 58 50 E8 00 00 00 00 58 F3 0F 11 58 37 F3 0F 11 48 3B F3 0F 11 40 3F 53 8D 5E 10 89 58 33 5B 58 F3 0F 11 1E F3 0F 11 4E 04 E9 B1 80 39 FF 00 00 00 00 00 00 00 00 00 00 8C 42 40 D8 7D 10 00 00 00 00 00 00 00 00 00 00 8C 42 FF FF FF FF";
        readonly string cameraLookAtEditorFunctionEntry = "E9 00 7F C6 00 0F 1F 40 00";
        readonly string cameraLookAtEditorFunctionOriginal = "E9 26 7F C6 00 0F 1F 40 00";

        readonly string hidePlayerAvatarInjection = "53 E8 00 00 00 00 5B F3 0F 10 7D 08 F3 0F 5C BB 65 01 00 00 50 F3 0F 11 7B 6B 8B 43 6B 23 43 63 66 0F 6E F8 58 0F 2E 7B 67 0F 83 2F 00 00 00 F3 0F 10 7D 0C F3 0F 5C BB 69 01 00 00 50 F3 0F 11 7B 6B 8B 43 6B 23 43 63 66 0F 6E F8 58 0F 2E 7B 67 0F 83 07 00 00 00 C7 45 10 00 00 C8 C2 5B F3 0F 10 45 10 E9 18 EB 37 FF FF FF FF 7F 9A 99 99 3E 8C 10 C1 40 AA AA AA AA";
        readonly string hidePlayerAvatarFunctionEntry = "E9 7F 14 C8 00";
        readonly string hidePlayerAvatarFunctionOriginal = "F3 0F 10 45 10";

        readonly string disableMovement1Function = "90 90 90 90";
        readonly string disableMovement1Original = "F3 0F 11 02";
        readonly string disableMovement2Function = "90 90 90 90 90 90 90 90";
        readonly string disableMovement2Original = "F3 0F 11 87 80 00 00 00";



        // Mod menu checker variables
        private bool isFreecamEnabled = false;
        private bool isHidePlayerModelEnabled = false;
        private bool isHideUserInterfaceEnabled = false;
        private bool isHideNametagsEnabled = false;
        private int cameraFOVSliderValue = 0;
        private int cameraDistanceSliderValue = 0;
        private int gameFogSliderValue = 110;
        private double cameraMoveSpeed = 0.1;
        private double cameraRotateSpeed = 0.5;
        private void CheckAndUpdateMemory()
        {
            if (isFreecamEnabled) 
            {
                HandleCameraController(currentCameraYaw);
            }
            
            uint intRotationAddress = m.ReadUInt("Cubic.exe+E2103E");
            string pitchAddress = (intRotationAddress + 4).ToString("X");
            string yawAddress = (intRotationAddress).ToString("X");
            string lookAtXAddress = (intRotationAddress - 16).ToString("X");
            string lookAtYAddress = (intRotationAddress - 12).ToString("X");
            string lookAtZAddress = (intRotationAddress - 8).ToString("X");

            InterpolateCameraMovement(lookAtXAddress, lookAtYAddress, lookAtZAddress);
            InterpolateCameraRotation(pitchAddress, yawAddress);

            if (FreecamSwitch.Switched != isFreecamEnabled)
            {
                isFreecamEnabled = FreecamSwitch.Switched;
                if (isFreecamEnabled)
                {
                    // Start Freecam camera position and rotation at the current camera position and rotation
                    m.WriteMemory("Cubic.exe+1B90DA", "bytes", cameraLookAtEditorFunctionEntry);

                    m.WriteMemory("Cubic.exe+21AE4B", "bytes", disableMovement1Function);
                    m.WriteMemory("Cubic.exe+21AE7A", "bytes", disableMovement1Function);
                    m.WriteMemory("Cubic.exe+21ADE1", "bytes", disableMovement2Function);
                    m.WriteMemory("Cubic.exe+21AE18", "bytes", disableMovement2Function);

                }
                else
                {
                    m.WriteMemory("Cubic.exe+1B90DA", "bytes", cameraLookAtEditorFunctionOriginal);
                    m.WriteMemory("Cubic.exe+21AE4B", "bytes", disableMovement1Original);
                    m.WriteMemory("Cubic.exe+21AE7A", "bytes", disableMovement1Original);
                    m.WriteMemory("Cubic.exe+21ADE1", "bytes", disableMovement2Original);
                    m.WriteMemory("Cubic.exe+21AE18", "bytes", disableMovement2Original);
                    StopAnimation();
                }
            }

            if (HidePlayerModelSwitch.Switched != isHidePlayerModelEnabled) 
            {
                isHidePlayerModelEnabled = HidePlayerModelSwitch.Switched;
                if (isHidePlayerModelEnabled == true)
                {
                    m.WriteMemory("Cubic.exe+19FA53", "bytes", hidePlayerAvatarFunctionEntry);
                }
                else
                {
                    m.WriteMemory("Cubic.exe+19FA53", "bytes", hidePlayerAvatarFunctionOriginal);
                }
            }


            if (HideUserInterfaceSwitch.Switched != isHideUserInterfaceEnabled)
            {
                isHideUserInterfaceEnabled = HideUserInterfaceSwitch.Switched;
                if (isHideUserInterfaceEnabled == true)
                {
                    m.WriteMemory("Cubic.exe+1BEB57", "bytes", "84");
                    m.WriteMemory("Cubic.exe+230C0F", "bytes", "C7 82 38 02 00 00 00 00 7A C4 90 90 90 90 90 90");
                }
                else
                {
                    m.WriteMemory("Cubic.exe+1BEB57", "bytes", "85");
                    m.WriteMemory("Cubic.exe+230C0F", "bytes", "F3 0F 11 82 38 02 00 00 F3 0F 58 8A 3C 02 00 00");      
                }
            }


            if (HideNametagsSwitch.Switched != isHideNametagsEnabled)
            {
                isHideNametagsEnabled = HideNametagsSwitch.Switched;
                if (isHideNametagsEnabled == true)
                {
                    m.WriteMemory("Cubic.exe+1A7AFD", "bytes", "01");
                }
                else
                {
                    m.WriteMemory("Cubic.exe+1A7AFD", "bytes", "00");
                }
            }



            if (CameraFOVSlider.Value != cameraFOVSliderValue)
            {
                cameraFOVSliderValue = CameraFOVSlider.Value;
                m.WriteMemory("Cubic.exe+E20E1D", "float", CameraFOVSlider.Value.ToString());
            }

            if (CameraDistanceSlider.Value != cameraDistanceSliderValue)
            {
                cameraDistanceSliderValue = CameraDistanceSlider.Value;
                m.WriteMemory("Cubic.exe+E20FAC", "float", CameraDistanceSlider.Value.ToString());
            }

            if (GameFogSlider.Value != gameFogSliderValue)
            {
                gameFogSliderValue = GameFogSlider.Value;
                m.WriteMemory("Cubic.exe+2FFEC8", "float", GameFogSlider.Value.ToString());
            }

            // Code updates camera labels regardless of freecam toggle
            if (FreecamSwitch.Switched == true)
            {
                // Overwrite camera position, pitch and yaw using Silicon as the new controller
                // Handle vanilla screen orbiting
                if (IsRightMouseButtonDown())
                {
                    targetCameraPitch = m.ReadFloat(pitchAddress);
                    targetCameraYaw = m.ReadFloat(yawAddress);
                }
                else
                {
                    m.WriteMemory(pitchAddress, "bytes", ConvertDoubleToFloatBytes(currentCameraPitch));
                    m.WriteMemory(yawAddress, "bytes", ConvertDoubleToFloatBytes(currentCameraYaw));
                }
                m.WriteMemory("Cubic.exe+E21032", "bytes", ConvertDoubleToFloatBytes(currentCameraLookAtX));
                m.WriteMemory("Cubic.exe+E21036", "bytes", ConvertDoubleToFloatBytes(currentCameraLookAtY));
                m.WriteMemory("Cubic.exe+E2103A", "bytes", ConvertDoubleToFloatBytes(currentCameraLookAtZ));
            }
            else
            {
                targetCameraLookAtX = m.ReadFloat("Cubic.exe+E21042");
                targetCameraLookAtY = m.ReadFloat("Cubic.exe+E21046");
                targetCameraLookAtZ = m.ReadFloat("Cubic.exe+E2104A");
                targetCameraPitch = m.ReadFloat(pitchAddress);
                targetCameraYaw = m.ReadFloat(yawAddress);
            }

            string ConvertDoubleToFloatBytes(double num)
            {
                // Workaround for unexpected behaviour with the API WriteMemory "float".
                float floatValue = (float)num;
                byte[] byteArray = BitConverter.GetBytes(floatValue);
                string byteString = BitConverter.ToString(byteArray).Replace("-", " ");

                return byteString;
            }
                
            //UpdateLabel(CameraPositionDataLabel, $"X: {currentCameraLookAtX:F2} Y: {currentCameraLookAtY:F2} Z: {currentCameraLookAtZ:F2} Pitch: {currentCameraPitch:F2} Yaw: {currentCameraYaw:F2}", Color.Red);
            UpdateLabel(CameraLookAtInfoLabel, $"X: {currentCameraLookAtX:F2}\nY: {currentCameraLookAtY:F2}\nZ: {currentCameraLookAtZ:F2}", Color.White);
            UpdateLabel(CameraLookAtInfoLabel2, $"X: {currentCameraLookAtX:F2}\nY: {currentCameraLookAtY:F2}\nZ: {currentCameraLookAtZ:F2}", Color.White);
            UpdateLabel(CameraRotationInfoLabel, $"Pitch: {currentCameraPitch:F2}\nYaw: {currentCameraYaw:F2}\n🔎:  {cameraDistanceSliderValue} | {cameraFOVSliderValue}", Color.White);
            UpdateLabel(CameraRotationInfoLabel2, $"Pitch: {currentCameraPitch:F2}\nYaw: {currentCameraYaw:F2}\n🔎:  {cameraDistanceSliderValue} | {cameraFOVSliderValue}", Color.White);

        }


        // Base injection code caves and function jumps to allow Silicon to modify game behaviour
        private void InjectBaseFunctions()
        {
            m.WriteMemory("Cubic.exe+1BC7FE", "bytes", cameraCoordinatesFunction);
            m.WriteMemory("Cubic.exe+1BC915", "bytes", adjustFogFunction);

            // Special case for cameraLookAtEditor
            // Always inject, but skip assignment if deactivated (inject to the adress after assignment)
            m.WriteMemory("Cubic.exe+E20FDF", "bytes", cameraLookAtEditorInjection);
            m.WriteMemory("Cubic.exe+1B90DA", "bytes", cameraLookAtEditorFunctionOriginal);

            m.WriteMemory("Cubic.exe+E20D31", "bytes", cameraHeightInjection);
            m.WriteMemory("Cubic.exe+1BC813", "bytes", cameraHeightFunctionEntry);
            m.WriteMemory("Cubic.exe+E20D7A", "bytes", unlockCameraArrowsInjection);
            m.WriteMemory("Cubic.exe+1B8B88", "bytes", unlockCameraArrowsFunctionEntry);
            m.WriteMemory("Cubic.exe+E20DC8", "bytes", unlockCameraRMBInjection);
            m.WriteMemory("Cubic.exe+1CA27C", "bytes", unlockCameraRMBFunctionEntry);
            m.WriteMemory("Cubic.exe+E20E05", "bytes", unlockCameraFOVInjection);
            m.WriteMemory("Cubic.exe+1BC847", "bytes", unlockCameraFOVFunctionEntry);
            m.WriteMemory("Cubic.exe+E20F82", "bytes", adjustCameraDistanceInjection);
            m.WriteMemory("Cubic.exe+1BC80A", "bytes", adjustCameraDistanceFunctionEntry);

            //Revertable, injections only as set to false by default
            m.WriteMemory("Cubic.exe+E20ED7", "bytes", hidePlayerAvatarInjection);
        }

        // Play button state, alternates between play and stop when clicked
        private enum PlayButtonState { Play, Stop }
        private PlayButtonState playButtonState = PlayButtonState.Play;

        // Animation stopping mechanism for when stop button is pressed or freecam is disabled
        private CancellationTokenSource animationCancellationTokenSource;
        List<List<double>> animationFrames = new List<List<double>>();
        private async void PlayAnimationButton_Click(object sender, EventArgs e)
        {
            if (animationFrames.Count == 0)
            {
                MessageBox.Show("No keyframes found. Please add at least one keyframe before playing the animation.",
                                "Animation Error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                return;
            }

            if (playButtonState == PlayButtonState.Play)
            {
                playButtonState = PlayButtonState.Stop;
                PlayAnimationButton.Text = "◼";
                animationCancellationTokenSource = new CancellationTokenSource();
                CancellationToken token = animationCancellationTokenSource.Token;

                // Teleport immediately to first frame
                List<double> firstFrame = animationFrames[0];
                currentCameraLookAtX = firstFrame[0];
                currentCameraLookAtY = firstFrame[1];
                currentCameraLookAtZ = firstFrame[2];
                currentCameraPitch = firstFrame[3];
                currentCameraYaw = firstFrame[4];
                targetCameraLookAtX = firstFrame[0];
                targetCameraLookAtY = firstFrame[1];
                targetCameraLookAtZ = firstFrame[2];
                targetCameraPitch = firstFrame[3];
                targetCameraYaw = firstFrame[4];
                await Task.Delay(20);

                for (int i = 0; i < animationFrames.Count - 1; i++)
                {
                    if (token.IsCancellationRequested)
                        break;

                    List<double> startFrame = animationFrames[i];
                    List<double> endFrame = animationFrames[i + 1];

                    double startX = startFrame[0], startY = startFrame[1], startZ = startFrame[2];
                    double startPitch = startFrame[3], startYaw = startFrame[4];
                    double moveSpeed = startFrame[5];

                    double endX = endFrame[0], endY = endFrame[1], endZ = endFrame[2];
                    double endPitch = endFrame[3], endYaw = endFrame[4];
                    Interpolator.MethodDelegate frameInterpolation = Interpolator.GetMethodWithIndex((int)startFrame[6]);

                    double distance = Math.Sqrt(
                        Math.Pow(endX - startX, 2) +
                        Math.Pow(endY - startY, 2) +
                        Math.Pow(endZ - startZ, 2));

                    double duration = distance / moveSpeed;
                    double startTime = Environment.TickCount;

                    while (true)
                    {
                        if (token.IsCancellationRequested)
                            return;

                        double elapsedTime = (Environment.TickCount - startTime) / 1000.0;
                        double alpha = elapsedTime / duration;
                        alpha = Clamp(alpha, 0.0, 1.0);

                        // Use selected interpolation method
                        targetCameraLookAtX = frameInterpolation(startX, endX, alpha);
                        targetCameraLookAtY = frameInterpolation(startY, endY, alpha);
                        targetCameraLookAtZ = frameInterpolation(startZ, endZ, alpha);
                        targetCameraPitch = frameInterpolation(startPitch, endPitch, alpha);
                        targetCameraYaw = frameInterpolation(startYaw, endYaw, alpha);

                        // Stop interpolating if alpha reaches 1
                        if (alpha >= 1.0)
                            break;

                        await Task.Delay(5);
                    }
                }

                // Reset button state after animation completes
                playButtonState = PlayButtonState.Play;
                PlayAnimationButton.Text = " ►";
                PlayAnimationButton.Refresh();
            }
            else if (playButtonState == PlayButtonState.Stop)
            {
                StopAnimation();
                playButtonState = PlayButtonState.Play;
                PlayAnimationButton.Text = " ►";
                PlayAnimationButton.Refresh();
            }
        }

        private void StopAnimation()
        {
            animationCancellationTokenSource?.Cancel();
            targetCameraLookAtX = currentCameraLookAtX;
            targetCameraLookAtY = currentCameraLookAtY;
            targetCameraLookAtZ = currentCameraLookAtZ;
            targetCameraPitch = currentCameraPitch;
            targetCameraYaw = currentCameraYaw;
        }


        private void AddAnimationFrameButton_Click(object sender, EventArgs e)
        {
            List<double> frame = new List<double>();
            frame.Add(currentCameraLookAtX);
            frame.Add(currentCameraLookAtY);
            frame.Add(currentCameraLookAtZ);
            frame.Add(currentCameraPitch);
            frame.Add(currentCameraYaw);
            double speed;
            if (!double.TryParse(CinematicSpeedTextBox.Text, out speed))
            {
                speed = 10.0; // default value if parsing fails
            }
            frame.Add(speed);
            frame.Add(interpComboBox.SelectedIndex);

            animationFrames.Add(frame);
            UpdateListView();
        }






        private bool IsRightMouseButtonDown()
        {
            return (GetAsyncKeyState(Keys.RButton) & 0x8000) != 0;
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


        private void HandleCameraController(double yawRotation)
        {
            double moveX = 0, moveY = 0, moveZ = 0;
            double rotatePitch = 0, rotateYaw = 0;

            if (pressedKeys.Contains(Keys.W))
            {
                double radians = (yawRotation - 90) * Math.PI / 180;
                moveX += Math.Cos(radians);
                moveY += Math.Sin(radians);
            }
            if (pressedKeys.Contains(Keys.S))
            {
                double radians = (yawRotation + 90) * Math.PI / 180;
                moveX += Math.Cos(radians);
                moveY += Math.Sin(radians);
            }
            if (pressedKeys.Contains(Keys.A))
            {
                double radians = yawRotation * Math.PI / 180;
                moveX -= Math.Cos(radians);
                moveY -= Math.Sin(radians);
            }
            if (pressedKeys.Contains(Keys.D))
            {
                double radians = yawRotation * Math.PI / 180;
                moveX += Math.Cos(radians);
                moveY += Math.Sin(radians);
            }
            if (pressedKeys.Contains(Keys.Space)) moveZ -= 1;
            if (pressedKeys.Contains(Keys.ControlKey)) moveZ += 1;
            if (pressedKeys.Contains(Keys.Up)) rotatePitch -= 1;
            if (pressedKeys.Contains(Keys.Down)) rotatePitch += 1;
            if (pressedKeys.Contains(Keys.Left)) rotateYaw -= 1;
            if (pressedKeys.Contains(Keys.Right)) rotateYaw += 1;


            
            double moveMagnitude = Math.Sqrt(moveX * moveX + moveY * moveY + moveZ * moveZ);
            if (moveMagnitude > 0.05)
            {
                moveX /= moveMagnitude;
                moveY /= moveMagnitude;
                moveZ /= moveMagnitude;
            }

            targetCameraLookAtX += moveX * cameraMoveSpeed;
            targetCameraLookAtY += moveY * cameraMoveSpeed;
            targetCameraLookAtZ += moveZ * cameraMoveSpeed;

            
            if (targetCameraLookAtZ > 100) targetCameraLookAtZ = 100;

            
            double rotateMagnitude = Math.Sqrt(rotatePitch * rotatePitch + rotateYaw * rotateYaw);
            if (rotateMagnitude > 0.05)
            {
                rotatePitch /= rotateMagnitude;
                rotateYaw /= rotateMagnitude;
            }

            targetCameraPitch += rotatePitch * cameraRotateSpeed;
            targetCameraYaw += rotateYaw * cameraRotateSpeed;

            // Limit pitch angle using the custom Clamp
            targetCameraPitch = Clamp(targetCameraPitch, -89, 89);
        }

        public static double Clamp(double value, double min, double max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        private void Silicon_OnFormClosing(object sender, FormClosingEventArgs e)
        {
            isRunning = false;
            keyPollingThread.Join();
        }
        // Here ends the code edited by Hispano

        private void InterpolateCameraMovement(string lookAtXAddress, string lookAtYAddress, string lookAtZAddress)
        {
            double elapsedTime = (Environment.TickCount / 100.0) - animationStartTime;
            double alpha = elapsedTime / animationDuration;
            alpha = Clamp(alpha, 0.0, 1.0);

            // Stop interpolation when alpha reaches 1
            if (alpha >= 1.0 - equalityTolerance)
            {
                currentCameraLookAtX = targetCameraLookAtX;
                currentCameraLookAtY = targetCameraLookAtY;
                currentCameraLookAtZ = targetCameraLookAtZ;
            } 
            else {
                currentCameraLookAtX = _interpolator(m.ReadFloat(lookAtXAddress), targetCameraLookAtX, alpha);
                currentCameraLookAtY = _interpolator(m.ReadFloat(lookAtYAddress), targetCameraLookAtY, alpha);
                currentCameraLookAtZ = _interpolator(m.ReadFloat(lookAtZAddress), targetCameraLookAtZ, alpha);
            }
        }

        private void InterpolateCameraRotation(string pitchAddress, string yawAddress)
        {
            double elapsedTime = (Environment.TickCount / 100.0) - animationStartTime;
            double alpha = elapsedTime / animationDuration;
            alpha = Clamp(alpha, 0.0, 1.0);

            // Stop interpolation when alpha reaches 1
            if (alpha >= 1.0 - equalityTolerance)
            {
                currentCameraPitch = targetCameraPitch;
                currentCameraYaw = targetCameraYaw;
            } 
            else 
            {
                currentCameraPitch = _interpolator(m.ReadFloat(pitchAddress), targetCameraPitch, alpha);
                currentCameraYaw = _interpolator(m.ReadFloat(yawAddress), targetCameraYaw, alpha);
            }
        }


        private void CameraMoveSpeedSlider_Scroll(object sender)
        {
            cameraMoveSpeed = (double)CameraMoveSpeedSlider.Value / 500;
        }

        private void CameraRotateSpeedSlider_Scroll(object sender)
        {
            cameraRotateSpeed = (double)CameraRotateSpeedSlider.Value / 100;
        }


        private void UpdateListView()
        {
            // Update the ListView with the current animationFrames
            listViewFrames.Items.Clear();
            int i = 1;
            foreach (var frame in animationFrames)
            {
                ListViewItem item = new ListViewItem(i.ToString()); // LookAtX
                item.SubItems.Add(frame[0].ToString("F1"));                   // LookAtX
                item.SubItems.Add(frame[1].ToString("F1"));                   // LookAtY
                item.SubItems.Add(frame[2].ToString("F1"));                   // LookAtZ
                item.SubItems.Add(frame[3].ToString("F1"));                   // Pitch
                item.SubItems.Add(frame[4].ToString("F1"));                   // Yaw
                item.SubItems.Add(frame[5].ToString("F1"));                   // Speed
                item.SubItems.Add(frame[6].ToString("F0"));                   // Interpolation
                listViewFrames.Items.Add(item);
                i++;
            }
        }

        private void ListViewFrames_ItemDrag(object sender, ItemDragEventArgs e)
        {
            // Start dragging the selected item
            DoDragDrop(e.Item, DragDropEffects.Move);
        }

        private void ListViewFrames_DragEnter(object sender, DragEventArgs e)
        {
            // Allow dragging into the ListView
            e.Effect = DragDropEffects.Move;
        }

        private void ListViewFrames_DragDrop(object sender, DragEventArgs e)
        {
            // Handle reordering of frames
            Point cp = listViewFrames.PointToClient(new Point(e.X, e.Y));
            ListViewItem dragToItem = listViewFrames.GetItemAt(cp.X, cp.Y);

            if (dragToItem != null)
            {
                int dragToIndex = dragToItem.Index;
                ListViewItem dragItem = (ListViewItem)e.Data.GetData(typeof(ListViewItem));
                int dragFromIndex = dragItem.Index;

                // Reorder animationFrames
                var frame = animationFrames[dragFromIndex];
                animationFrames.RemoveAt(dragFromIndex);
                animationFrames.Insert(dragToIndex, frame);

                UpdateListView();
            }
        }

        private void DeleteAnimationFrameButton_Click(object sender, EventArgs e)
        {
            if (playButtonState == PlayButtonState.Stop)
            {
                MessageBox.Show("Cannot delete while animation is in progress", "Delete Frame", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (listViewFrames.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select a frame to delete.", "Delete Frame", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            foreach (ListViewItem frame in listViewFrames.SelectedItems.Cast<ListViewItem>().ToList())
            {
                int selectedIndex = frame.Index;
                animationFrames.RemoveAt(selectedIndex);
                listViewFrames.Items.RemoveAt(selectedIndex);
            }

            UpdateListView();
        }


        private async void ActivateGoToFrame(int selectedIndex) {
            if (selectedIndex >= animationFrames.Count) return;

            FreecamSwitch.Switched = true;


            await Task.Delay(20);

            List<double> goToFrame = animationFrames[selectedIndex];

            // Set new target
            targetCameraLookAtX = goToFrame[0];
            targetCameraLookAtY = goToFrame[1];
            targetCameraLookAtZ = goToFrame[2];
            targetCameraPitch = goToFrame[3];
            targetCameraYaw = goToFrame[4];

            // Compute animation duration based on distance
            double distance = Math.Sqrt(
                Math.Pow(targetCameraLookAtX - currentCameraLookAtX, 2) +
                Math.Pow(targetCameraLookAtY - currentCameraLookAtY, 2) +
                Math.Pow(targetCameraLookAtZ - currentCameraLookAtZ, 2)
            );

            if (distance < equalityTolerance) {
                currentCameraLookAtX = targetCameraLookAtX;
                currentCameraLookAtY = targetCameraLookAtY;
                currentCameraLookAtZ = targetCameraLookAtZ;
            }

            double speed = double.TryParse(CinematicSpeedTextBox.Text, out var s) ? s : 10.0;
            animationDuration = distance / (speed / 100);
            animationStartTime = (Environment.TickCount / 100.0);
        }

        private void GoToAnnimationFrameButton_Click(object sender, EventArgs e)
        {
            if (listViewFrames.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select a frame to view.", "Delete Frame", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (listViewFrames.SelectedItems.Count > 1)
            {

                MessageBox.Show("multiple frames selected. Please select only one.", "Delete Frame", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int selectedIndex = listViewFrames.SelectedItems[0].Index;
            ActivateGoToFrame(selectedIndex);
        }

        private void SaveAnimationButton_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*",
                DefaultExt = "json",
                AddExtension = true,
                Title = "Save Animation Frames"
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // Serialize the animationFrames list to JSON
                    string json = System.Text.Json.JsonSerializer.Serialize(animationFrames);

                    // Write to the selected file
                    File.WriteAllText(saveFileDialog.FileName, json);

                    MessageBox.Show("Animation frames saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void LoadAnimationButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*",
                Title = "Load Animation Frames"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // Read the file content
                    string json = File.ReadAllText(openFileDialog.FileName);

                    // Deserialize JSON to List<List<double>>
                    animationFrames = System.Text.Json.JsonSerializer.Deserialize<List<List<double>>>(json);

                    // Refresh the ListView to reflect loaded data
                    listViewFrames.Items.Clear();
                    int i = 1;
                    foreach (var frame in animationFrames)
                    {
                        ListViewItem item = new ListViewItem(i.ToString());
                        item.SubItems.Add(frame[0].ToString("F1"));                   // LookAtX
                        item.SubItems.Add(frame[1].ToString("F1"));                   // LookAtY
                        item.SubItems.Add(frame[2].ToString("F1"));                   // LookAtZ
                        item.SubItems.Add(frame[3].ToString("F1"));                   // Pitch
                        item.SubItems.Add(frame[4].ToString("F1"));                   // Yaw
                        item.SubItems.Add(frame[5].ToString("F1"));                   // Speed
                        item.SubItems.Add(frame[6].ToString("F0"));                   // Interpolation                                                              
                        listViewFrames.Items.Add(item);
                        i++;
                    }

                    MessageBox.Show("Animation frames loaded successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }


        private void interpComboBox_SelectedIndexChanged(object sender, EventArgs e) {
            _interpolator = Interpolator.GetMethodWithIndex(interpComboBox.SelectedIndex);
        }


        private void Preset1Button_Click(object sender, EventArgs e)
        {
            
            cameraFOVSliderValue = 22;
            CameraFOVSlider.Value = 22;
            m.WriteMemory("Cubic.exe+E20E1D", "float", cameraFOVSliderValue.ToString());

            cameraDistanceSliderValue = 33;
            CameraDistanceSlider.Value = 33;
            m.WriteMemory("Cubic.exe+E20FAC", "float", cameraDistanceSliderValue.ToString());

            gameFogSliderValue = 110;
            GameFogSlider.Value = 110;
            m.WriteMemory("Cubic.exe+2FFEC8", "float", GameFogSlider.Value.ToString());

            HidePlayerModelSwitch.Switched = false;
            HideUserInterfaceSwitch.Switched = false;
            HideNametagsSwitch.Switched = false;
            FreecamSwitch.Switched = false;
        }

        private void Preset2Button_Click(object sender, EventArgs e)
        {
            cameraFOVSliderValue = 45;
            CameraFOVSlider.Value = 45;
            m.WriteMemory("Cubic.exe+E20E1D", "float", cameraFOVSliderValue.ToString());

            cameraDistanceSliderValue = 33;
            CameraDistanceSlider.Value = 33;
            m.WriteMemory("Cubic.exe+E20FAC", "float", cameraDistanceSliderValue.ToString());

            gameFogSliderValue = 110;
            GameFogSlider.Value = 110;
            m.WriteMemory("Cubic.exe+2FFEC8", "float", GameFogSlider.Value.ToString());

            HidePlayerModelSwitch.Switched = false;
            HideUserInterfaceSwitch.Switched = false;
            HideNametagsSwitch.Switched = false;
            FreecamSwitch.Switched = false;
        }

        private void Preset3Button_Click(object sender, EventArgs e)
        {
            cameraFOVSliderValue = 70;
            CameraFOVSlider.Value = 70;
            m.WriteMemory("Cubic.exe+E20E1D", "float", cameraFOVSliderValue.ToString());

            cameraDistanceSliderValue = 1;
            CameraDistanceSlider.Value = 1;
            m.WriteMemory("Cubic.exe+E20FAC", "float", cameraDistanceSliderValue.ToString());

            gameFogSliderValue = 110;
            GameFogSlider.Value = 110;
            m.WriteMemory("Cubic.exe+2FFEC8", "float", GameFogSlider.Value.ToString());

            HidePlayerModelSwitch.Switched = true;
            HideUserInterfaceSwitch.Switched = false;
            HideNametagsSwitch.Switched = true;
            FreecamSwitch.Switched = false;
        }

        private void CinematicSpeedTextBox_TextChanged(object sender, EventArgs e)
        {
            var metroBox = sender as MetroSet_UI.Controls.MetroSetTextBox;
            var innerTextBox = metroBox.Controls[0] as TextBox;

            if (innerTextBox != null)
            {
                int caret = innerTextBox.SelectionStart;

                if (!Regex.IsMatch(metroBox.Text, @"^-?\d*\.?\d*$"))
                {
                    metroBox.Text = Regex.Replace(metroBox.Text, @"[^0-9.-]", "");
                    innerTextBox.SelectionStart = Math.Min(caret, metroBox.Text.Length);
                }
            }
        }
    }
}