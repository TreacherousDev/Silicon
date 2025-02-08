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


namespace Silicon
{


    public partial class SiliconForm : MetroSetForm
    {
        public Mem m = new Mem();
        private System.Timers.Timer processCheckTimer;
        private HashSet<Keys> pressedKeys = new HashSet<Keys>();
        private Thread keyPollingThread;
        private bool isRunning = true;
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
            FreecamSwitch.CheckColor = Color.Silver;
            HidePlayerModelSwitch.CheckColor = Color.Silver;
            List<MetroSet_UI.Controls.MetroSetButton> cinematicButtons = new List<MetroSet_UI.Controls.MetroSetButton>
            {
                AddAnimationFrameButton,
                PlayAnimationButton,
                DeleteAnimationFrameButton,
                GoToAnnimationFrameButton,
                SaveAnimationButton,
                LoadAnimationButton
            };
            foreach (MetroSet_UI.Controls.MetroSetButton button in cinematicButtons) 
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
            processCheckTimer = new System.Timers.Timer(1000);
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

        private double cameraMoveDirectionX;
        private double cameraMoveDirectionY;
        private double cameraMoveDirectionZ;
        private double cameraMoveDistance;
        private double cameraRotateDirectionPitch;
        private double cameraRotateDirectionYaw;
        private double cameraRotateDistance;

        private Timer _updateTimer;

        private void SiliconWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            _updateTimer = new Timer(10);
            _updateTimer.Elapsed += UpdateMemoryOnTimerTick;
            _updateTimer.AutoReset = true;
            _updateTimer.Start();
        }

        private void UpdateMemoryOnTimerTick(object sender, ElapsedEventArgs e)
        {
            CheckAndUpdateMemory();
        }

        // Persistent Functions (Required for the engine to function)
        readonly string cameraCoordinatesFunction = "90 90 90 90 90 90 90 90 90 90 90 90";

        readonly string cameraHeightInjection = "53 E8 00 00 00 00 5B F3 0F 5C 43 1B F3 0F 11 40 08 5B F3 0F 5C CB 8D 85 FC FE FF FF E9 F3 77 2C FF 66 66 A6 3F 00 00 00 00";
        readonly string cameraHeightFunctionEntry = "E9 F1 87 D3 00 0F 1F 44 00 00";

        readonly string unlockCameraArrowsInjection = "50 E8 00 00 00 00 58 F3 0F 10 58 1F 0F 2F D8 58 0F 86 2A 3B 2C FF C7 86 EC 09 00 00 00 00 B2 C2 E9 1B 3B 2C FF 00 00 B2 C2 00 00 00 00";
        readonly string unlockCameraArrowsFunctionEntry = "E9 C7 C4 D3 00";

        readonly string unlockCameraRMBInjection = "50 E8 00 00 00 00 58 F3 0F 10 70 1F 0F 2F F1 58 0F 86 D2 51 2D FF C7 80 EC 09 00 00 00 00 B2 C2 E9 C3 51 2D FF 00 00 B2 C2 00 00 00 00";
        readonly string unlockCameraRMBFunctionEntry = "E9 1F AE D2 00";

        readonly string unlockCameraFOVInjection = "50 E8 00 00 00 00 58 F3 0F 10 40 12 58 8D 85 D8 FE FF FF E9 58 77 2C FF 00 00 96 42 FF FF FF FF";
        readonly string unlockCameraFOVFunctionEntry = "E9 91 88 D3 00 90";

        readonly string adjustCameraDistanceInjection = "50 E8 00 00 00 00 58 F3 0F 59 40 24 F3 0F 59 58 24 F3 0F 59 60 24 58 E9 00 00 00 00 F3 0F 5C D0 F3 0F 10 40 08 E9 8F 75 2C FF 00 00 C8 41 FF FF FF FF";
        readonly string adjustCameraDistanceFunctionEntry = "E9 4B 8A D3 00 0F 1F 40 00";

        // Revertable functions (Optional switch states available)
        readonly string cameraLookAtEditorInjection = "50 E8 00 00 00 00 58 F3 0F 11 58 5D F3 0F 11 48 61 F3 0F 11 40 65 F3 0F 10 58 4D F3 0F 10 48 51 F3 0F 10 40 55 58 50 E8 00 00 00 00 58 F3 0F 11 58 37 F3 0F 11 48 3B F3 0F 11 40 3F 53 8D 5E 10 89 58 33 5B 58 F3 0F 11 1E F3 0F 11 4E 04 E9 D7 3D 2C FF 00 00 00 00 00 00 00 00 00 00 8C 42 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 FF FF FF FF";
        readonly string cameraLookAtEditorFunctionEntry = "E9 DA C1 D3 00 0F 1F 40 00";
        readonly string cameraLookAtEditorFunctionOriginal = "E9 00 C2 D3 00 0F 1F 40 00";

        readonly string hidePlayerAvatarInjection = "53 E8 00 00 00 00 5B F3 0F 10 7D 08 F3 0F 5C BB 65 01 00 00 50 F3 0F 11 7B 6B 8B 43 6B 23 43 63 66 0F 6E F8 58 0F 2E 7B 67 0F 83 2F 00 00 00 F3 0F 10 7D 0C F3 0F 5C BB 69 01 00 00 50 F3 0F 11 7B 6B 8B 43 6B 23 43 63 66 0F 6E F8 58 0F 2E 7B 67 0F 83 07 00 00 00 C7 45 10 00 00 C8 C2 5B F3 0F 10 45 10 E9 3D A8 2A FF FF FF FF 7F 9A 99 99 3E 00 00 00 00 AA AA AA AA";
        readonly string hidePlayerAvatarFunctionEntry = "E9 5A 57 D5 00";
        readonly string hidePlayerAvatarFunctionOriginal = "F3 0F 10 45 10";

        // Mod menu checker variables
        private bool isFreecamEnabled = false;
        private bool isHidePlayerModelEnabled = false;
        private int cameraFOVSliderValue = 0;
        private int cameraDistanceSliderValue = 0;
        private double cameraMoveSpeed = 0.1;
        private double cameraRotateSpeed = 0.5;
        private void CheckAndUpdateMemory()
        {
            if (isFreecamEnabled) 
            {
                HandleCameraController(currentCameraYaw);
            }
            
            uint intRotationAddress = m.ReadUInt("Cubic.exe+EF503E");
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
                if (isFreecamEnabled == true)
                {
                    // Start Freecam camera position and rotation at the current camera position and rotation
                    m.WriteMemory("Cubic.exe+1B8E00", "bytes", cameraLookAtEditorFunctionEntry);
                    targetCameraLookAtX = m.ReadFloat("Cubic.exe+EF5042");
                    targetCameraLookAtY = m.ReadFloat("Cubic.exe+EF5046");
                    targetCameraLookAtZ = m.ReadFloat("Cubic.exe+EF504A");
                    targetCameraPitch = currentCameraPitch;
                    targetCameraYaw = currentCameraYaw;
                }
                else
                {
                    m.WriteMemory("Cubic.exe+1B8E00", "bytes", cameraLookAtEditorFunctionOriginal);
                    StopAnimation();
                }
            }

            if (HidePlayerModelSwitch.Switched != isHidePlayerModelEnabled) 
            {
                isHidePlayerModelEnabled = HidePlayerModelSwitch.Switched;
                if (isHidePlayerModelEnabled == true)
                {
                    m.WriteMemory("Cubic.exe+19F778", "bytes", hidePlayerAvatarFunctionEntry);
                }
                else
                {
                    m.WriteMemory("Cubic.exe+19F778", "bytes", hidePlayerAvatarFunctionOriginal);
                }
            }

            if (CameraFOVSlider.Value != cameraFOVSliderValue)
            {
                cameraFOVSliderValue = CameraFOVSlider.Value;
                m.WriteMemory("Cubic.exe+EF4E1D", "float", CameraFOVSlider.Value.ToString());
            }

            if (CameraDistanceSlider.Value != cameraDistanceSliderValue)
            {
                cameraDistanceSliderValue = CameraDistanceSlider.Value;
                m.WriteMemory("Cubic.exe+EF4FAC", "float", CameraDistanceSlider.Value.ToString());
            }

            if (FreecamSwitch.Switched == true)
            {
                // Overwrite camera position, pitch and yaw using Silicon as the new controller
                m.WriteMemory(pitchAddress, "bytes", ConvertDoubleToFloatBytes(currentCameraPitch));
                m.WriteMemory(yawAddress, "bytes", ConvertDoubleToFloatBytes(currentCameraYaw));
                m.WriteMemory("Cubic.exe+EF5032", "bytes", ConvertDoubleToFloatBytes(currentCameraLookAtX));
                m.WriteMemory("Cubic.exe+EF5036", "bytes", ConvertDoubleToFloatBytes(currentCameraLookAtY));
                m.WriteMemory("Cubic.exe+EF503A", "bytes", ConvertDoubleToFloatBytes(currentCameraLookAtZ));
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
            UpdateLabel(CameraLookAtInfoLabel, $"X: {currentCameraLookAtX:F2}     Y: {currentCameraLookAtY:F2}     Z: {currentCameraLookAtZ:F2}", Color.White);
            UpdateLabel(CameraRotationInfoLabel, $"Pitch: {currentCameraPitch:F2}        Yaw: {currentCameraYaw:F2}", Color.White);
            UpdateLabel(CameraLookAtInfoLabel2, $"X: {currentCameraLookAtX:F2}     Y: {currentCameraLookAtY:F2}     Z: {currentCameraLookAtZ:F2}", Color.White);
            UpdateLabel(CameraRotationInfoLabel2, $"Pitch: {currentCameraPitch:F2}        Yaw: {currentCameraYaw:F2}", Color.White);

        }

        // Calculate rotation speed dynamically based on distance
        // This allows us to haave smoother transitions as rotations will end on hte exact keyframe
        // that the camera arrives to its destination
        double GetDynamicRotationSpeed()
        {
            // Distance to target formula for 3D vector
            double moveDistance = Math.Sqrt(
                Math.Pow(targetCameraLookAtX - currentCameraLookAtX, 2) +
                Math.Pow(targetCameraLookAtY - currentCameraLookAtY, 2) +
                Math.Pow(targetCameraLookAtZ - currentCameraLookAtZ, 2)
            );

            // Distance to target formula for 2D vector
            double rotateDistance = Math.Sqrt(
                Math.Pow(targetCameraPitch - currentCameraPitch, 2) +
                Math.Pow(targetCameraYaw - currentCameraYaw, 2)
            );

            // Calculate new rotation speed
            double timeToTarget = moveDistance / cameraMoveSpeed;
            return rotateDistance / (timeToTarget); 
        }

        // Base injection code caves and function jumps to allow Silicon to modify game behaviour
        private void InjectBaseFunctions()
        {
            m.WriteMemory("Cubic.exe+1BC526", "bytes", cameraCoordinatesFunction);

            // Special case for cameraLookAtEditor
            // Always inject, but skip assignment if deactivated (inject to the adress after assignment)
            m.WriteMemory("Cubic.exe+EF4FDF", "bytes", cameraLookAtEditorInjection);
            m.WriteMemory("Cubic.exe+1B8E00", "bytes", cameraLookAtEditorFunctionOriginal);

            m.WriteMemory("Cubic.exe+EF4D31", "bytes", cameraHeightInjection);
            m.WriteMemory("Cubic.exe+1BC53B", "bytes", cameraHeightFunctionEntry);
            m.WriteMemory("Cubic.exe+EF4D7A", "bytes", unlockCameraArrowsInjection);
            m.WriteMemory("Cubic.exe+1B88AE", "bytes", unlockCameraArrowsFunctionEntry);
            m.WriteMemory("Cubic.exe+EF4DC8", "bytes", unlockCameraRMBInjection);
            m.WriteMemory("Cubic.exe+1C9FA4", "bytes", unlockCameraRMBFunctionEntry);
            m.WriteMemory("Cubic.exe+EF4E05", "bytes", unlockCameraFOVInjection);
            m.WriteMemory("Cubic.exe+1BC56F", "bytes", unlockCameraFOVFunctionEntry);
            m.WriteMemory("Cubic.exe+EF4F82", "bytes", adjustCameraDistanceInjection);
            m.WriteMemory("Cubic.exe+1BC532", "bytes", adjustCameraDistanceFunctionEntry);

            //Revertable, injections only as set to false by default
            m.WriteMemory("Cubic.exe+EF4ED7", "bytes", hidePlayerAvatarInjection);

        }

        // Play button state, alternates between play and stop when clicked
        private enum PlayButtonState { Play,  Stop }
        private PlayButtonState playButtonState = PlayButtonState.Play;
        
        // Animation stopping mechanism for when stop button is pressed or freecam is disabled
        private CancellationTokenSource animationCancellationTokenSource;
        List<List<double>> animationFrames = new List<List<double>>();
        private async void PlayAnimationButton_Click(object sender, EventArgs e)
        {
            if (playButtonState == PlayButtonState.Play)
            {
                playButtonState = PlayButtonState.Stop;
                PlayAnimationButton.Text = "◼";
                animationCancellationTokenSource = new CancellationTokenSource();
                CancellationToken token = animationCancellationTokenSource.Token;

                // Iterate over each frame, moving only to the next target once the distance is small enough
                int frameIndex = 0;
                foreach (List<double> frame in animationFrames)
                {
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }
                    targetCameraLookAtX = frame[0];
                    targetCameraLookAtY = frame[1];
                    targetCameraLookAtZ = frame[2];
                    targetCameraPitch = frame[3];
                    targetCameraYaw = frame[4];
                    cameraMoveSpeed = frame[5];

                    cameraRotateSpeed = GetDynamicRotationSpeed();

                    // Triggered on the first animation frame
                    // Warp to the first frame quickly and wait for some time before playing the rest at normal speed
                    if (frameIndex == 0)
                    {
                        cameraMoveSpeed = 1;
                        cameraRotateSpeed = 5;
                        while (cameraMoveDistance > 0.05)
                        {
                            if (token.IsCancellationRequested)
                            {
                                return;
                            }
                            await Task.Delay(10);
                        }
                        await Task.Delay(1000);
                    }
                    frameIndex++;

                    await Task.Delay(20);
                    while (cameraMoveDistance > 1)
                    {
                        if (token.IsCancellationRequested)
                        {
                            return;
                        }
                        await Task.Delay(10);
                    }
                }

                // Change the button state to play after animation is done
                playButtonState = PlayButtonState.Play;
                PlayAnimationButton.Text = " ►";
                PlayAnimationButton.Refresh();
            }
            
            else if (playButtonState == PlayButtonState.Stop)
            {
                // Stop animation once stop button is pressed
                playButtonState = PlayButtonState.Play;
                PlayAnimationButton.Text = " ►";
                PlayAnimationButton.Refresh();
                StopAnimation();
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
            frame.Add(cameraMoveSpeed);

            animationFrames.Add(frame);
            UpdateListView();
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
                Keys.Space, Keys.LControlKey,
                Keys.I, Keys.K, Keys.J, Keys.L
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
            if (pressedKeys.Contains(Keys.LControlKey)) moveZ += 1;
            if (pressedKeys.Contains(Keys.I)) rotatePitch -= 1;
            if (pressedKeys.Contains(Keys.K)) rotatePitch += 1;
            if (pressedKeys.Contains(Keys.J)) rotateYaw -= 1;
            if (pressedKeys.Contains(Keys.L)) rotateYaw += 1;

            
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


        private void InterpolateCameraMovement(string lookAtXAddress,string lookAtYAddress, string lookAtZAddress)
        {
            currentCameraLookAtX = m.ReadFloat(lookAtXAddress);
            currentCameraLookAtY = m.ReadFloat(lookAtYAddress);
            currentCameraLookAtZ = m.ReadFloat(lookAtZAddress);

            // Calculate the direction vector towards the target
            cameraMoveDirectionX = targetCameraLookAtX - currentCameraLookAtX;
            cameraMoveDirectionY = targetCameraLookAtY - currentCameraLookAtY;
            cameraMoveDirectionZ = targetCameraLookAtZ - currentCameraLookAtZ;
            // Normalize distance
            cameraMoveDistance = Math.Sqrt(cameraMoveDirectionX * cameraMoveDirectionX + cameraMoveDirectionY * cameraMoveDirectionY + cameraMoveDirectionZ * cameraMoveDirectionZ);

            // Interpolate and move to target location
            // Snap to target if close enough
            if (cameraMoveDistance < cameraMoveSpeed)
            {
                currentCameraLookAtX = targetCameraLookAtX;
                currentCameraLookAtY = targetCameraLookAtY;
                currentCameraLookAtZ = targetCameraLookAtZ;
            }
            else
            {
                // Normalize the direction vector and move towards the target
                currentCameraLookAtX += cameraMoveDirectionX / cameraMoveDistance * cameraMoveSpeed;
                currentCameraLookAtY += cameraMoveDirectionY / cameraMoveDistance * cameraMoveSpeed;
                currentCameraLookAtZ += cameraMoveDirectionZ / cameraMoveDistance * cameraMoveSpeed;
            }           
        }
        private void InterpolateCameraRotation(string pitchAddress, string yawAddress)
        {
            currentCameraPitch = m.ReadFloat(pitchAddress);
            currentCameraYaw = m.ReadFloat(yawAddress);

            // Calculate the direction vector towards the target
            cameraRotateDirectionPitch = targetCameraPitch - currentCameraPitch;
            cameraRotateDirectionYaw = targetCameraYaw - currentCameraYaw;
            //Normalize distance
            cameraRotateDistance = Math.Sqrt(cameraRotateDirectionPitch * cameraRotateDirectionPitch + cameraRotateDirectionYaw * cameraRotateDirectionYaw);

            //Interpolate and move to target location
            //Snap to target if close enough
            if (cameraRotateDistance < cameraRotateSpeed)
            {
                currentCameraPitch = targetCameraPitch;
                currentCameraYaw = targetCameraYaw;
            }
            else
            {
                // Normalize the direction vector and move towards the target
                currentCameraPitch += cameraRotateDirectionPitch / cameraRotateDistance * cameraRotateSpeed;
                currentCameraYaw += cameraRotateDirectionYaw / cameraRotateDistance * cameraRotateSpeed;
            }
        }

        private void CameraMoveSpeedSlider_Scroll(object sender)
        {
            cameraMoveSpeed = (double)CameraMoveSpeedSlider.Value / 300;
        }

        private void CameraRotateSpeedSlider_Scroll(object sender)
        {
            cameraRotateSpeed = (double)CameraRotateSpeedSlider.Value / 60;
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
                item.SubItems.Add((frame[5] * 100).ToString("F1"));                   // MoveSpeed
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
            List<double> goToFrame = animationFrames[selectedIndex];
            targetCameraLookAtX = goToFrame[0];
            targetCameraLookAtY = goToFrame[1];
            targetCameraLookAtZ = goToFrame[2];
            targetCameraPitch = goToFrame[3];
            targetCameraYaw = goToFrame[4];
            cameraRotateSpeed = GetDynamicRotationSpeed();
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
                        item.SubItems.Add((frame[5] * 100).ToString("F1"));           // Speed      
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

    //    int[] realmBlockData;
    //    int realmSizeX = 1;
    //    int realmSizeY = 1;
    //    int realmSizeZ = 1;
    //    string startCoordinatesAddress;
    //    private void GetRealmSizeButton_Click(object sender, EventArgs e)
    //    {
    //        uint intInitialAddress = m.ReadUInt("Cubic.exe+E29296");
    //        string initialAddress = (intInitialAddress).ToString("X");

    //        int[] startCoordinates = new int[] { 0, 0, 0 };
    //        int[] coordinates = new int[3];
    //        coordinates[0] = m.ReadByte((intInitialAddress + 2).ToString("X"));
    //        coordinates[1] = m.ReadByte((intInitialAddress + 4).ToString("X"));
    //        coordinates[2] = m.ReadByte((intInitialAddress + 6).ToString("X"));
    //        int[] currentStartCoordinates = coordinates;


    //        int counter = 0;
    //        while (!currentStartCoordinates.SequenceEqual(startCoordinates))
    //        {
    //            counter++;
    //            currentStartCoordinates[0] = m.ReadByte((intInitialAddress + 2 - (counter * 28)).ToString("X"));
    //            currentStartCoordinates[1] = m.ReadByte((intInitialAddress + 4 - (counter * 28)).ToString("X"));
    //            currentStartCoordinates[2] = m.ReadByte((intInitialAddress + 6 - (counter * 28)).ToString("X"));
    //        }

    //        startCoordinatesAddress = (intInitialAddress - (counter * 28)).ToString("X");
    //        Console.WriteLine(startCoordinatesAddress);


    //        while (true)
    //        {
    //            int next_z = m.ReadByte(startCoordinatesAddress + "+" + (6 + (28 * realmSizeZ)).ToString("X"));
    //            //Console.WriteLine(next_z + " " + startCoordinatesAddress + "+" + (6 + (28 * z)).ToString("X"));
    //            if (next_z == 0)
    //            {
    //                break; // Exit the loop when next_z is 0
    //            }
    //            realmSizeZ++;
    //        }

    //        while (true)
    //        {
    //            int next_y = m.ReadByte(startCoordinatesAddress + "+" + (4 + (28 * realmSizeZ * realmSizeY)).ToString("X"));
    //            if (next_y == 0)
    //            {
    //                break; // Exit the loop when next_z is 0
    //            }
    //            realmSizeY++;
    //        }

    //        while (true)
    //        {
    //            int next_x = m.ReadByte(startCoordinatesAddress + "+" + (2 + (28 * realmSizeZ * realmSizeY * realmSizeX)).ToString("X"));
    //            if (next_x == 0)
    //            {
    //                break; // Exit the loop when next_z is 0
    //            }
    //            realmSizeX++;
    //        }
            
    //        //Console.WriteLine(realmSizeX + " " + y + " " + z);

    //        realmBlockData = new int[realmSizeX * realmSizeZ * realmSizeY];

    //        int realmBlockDataCounter = 0;
    //        for (int i = 0; i < realmBlockData.Length; i++)
    //        {
    //            int block = m.Read2Byte(startCoordinatesAddress + "+" + (realmBlockDataCounter * 28).ToString("X"));
    //            //block = (block & 0x0FFF);
    //            realmBlockData[i] = block;
    //            realmBlockDataCounter++;
    //        }


    //        //Console.WriteLine(realmBlockData.Length);
    //        int [] slicedRealmBlockData = FilterBlocksByRange(realmBlockData, realmSizeX - 1, realmSizeY - 1, realmSizeZ - 1, 99, true, 'z');
    //        for (int i = 0; i <slicedRealmBlockData.Length; i++)
    //        {
    //        if (slicedRealmBlockData[i] != 0)
    //        {
    //            Console.WriteLine($"Element at index {i}: {slicedRealmBlockData[i]}");
    //        }
    //        }

    //}
    //    int[] FilterBlocksByRange(int[] realmBlockData, int maxX, int maxY, int maxZ, int target, bool includeAbove, char axis)
    //    {
    //        int[] filteredRealmBlockData = new int[realmBlockData.Length]; // Create a new array with the same length

    //        // Loop over the coordinates based on the chosen axis
    //        for (int x = 0; x <= maxX; x++)
    //        {
    //            for (int y = 0; y <= maxY; y++)
    //            {
    //                for (int z = 0; z <= maxZ; z++)
    //                {
    //                    // Calculate the 1D array index for the (x, y, z) coordinates
    //                    int index = z + (maxZ + 1) * y + (maxZ + 1) * (maxY + 1) * x;

    //                    // Check if we should include the block based on the chosen axis
    //                    bool shouldInclude = false;
    //                    if (axis == 'x') // Filtering by X
    //                    {
    //                        int xStart = includeAbove ? target : 0;
    //                        int xEnd = includeAbove ? maxX : target - 1;
    //                        shouldInclude = x >= xStart && x <= xEnd;
    //                    }
    //                    else if (axis == 'v') // Filtering by Y
    //                    {
    //                        int yStart = includeAbove ? target : 0;
    //                        int yEnd = includeAbove ? maxY : target - 1;
    //                        shouldInclude = y >= yStart && y <= yEnd;
    //                    }
    //                    else if (axis == 'z') // Filtering by Z
    //                    {
    //                        int zStart = includeAbove ? target : 0;
    //                        int zEnd = includeAbove ? maxZ : target - 1;
    //                        shouldInclude = z >= zStart && z <= zEnd;
    //                    }

    //                    // Set the value to 0 if the block is outside the range, otherwise keep the original value
    //                    filteredRealmBlockData[index] = shouldInclude ? realmBlockData[index] : 0;
    //                }
    //            }
    //        }

    //        return filteredRealmBlockData; // Return the new array
    //    }

    //    private void SliceXButton_Click(object sender, EventArgs e)
    //    {
    //        int[] slicedRealmBlockData = FilterBlocksByRange(realmBlockData, realmSizeX - 1, realmSizeY - 1, realmSizeZ - 1, 13, false, 'x');

    //        int counter = 0;
    //        foreach (int block in slicedRealmBlockData)
    //        {
    //            Console.WriteLine(block);
    //            byte byte1 = (byte)(block & 0xFF); // Low byte
    //            byte byte2 = (byte)((block >> 8) & 0xFF); // High byte
    //            string blockBytes = $"{byte1:X2} {byte2:X2}";
    //            m.WriteMemory(startCoordinatesAddress + "+" + (counter * 28).ToString("X"), "bytes", blockBytes);
    //            counter++;
    //        }
    //    }

    //    private void metroSetButton1_Click(object sender, EventArgs e)
    //    {
    //        int counter = 0;
    //        foreach (int block in realmBlockData)
    //        {
    //            Console.WriteLine(block);
    //            byte byte1 = (byte)(block & 0xFF); // Low byte
    //            byte byte2 = (byte)((block >> 8) & 0xFF); // High byte
    //            string blockBytes = $"{byte1:X2} {byte2:X2}";
    //            m.WriteMemory(startCoordinatesAddress + "+" + (counter * 28).ToString("X"), "bytes", blockBytes);
    //            counter++;
    //        }
    //    }
    }
}