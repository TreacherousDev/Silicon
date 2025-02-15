﻿using Memory;
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

        private const float equalityTolerance = 5e-5f;

        private Interpolator.MethodDelegate _interpolator = Interpolator.GetMethodWithIndex(0);
        private double animationStartTime;
        private double animationDuration;

        private DateTime recordingStartTime;
        private int frameCounter = 0;
        private bool isRecording = false;
        private List<List<double>> recordedFrames = new List<List<double>>();

        private const int defaultCameraFov = 33;
        private const int defaultCameraDistance = 22;

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

        private double cameraMoveDistance = 0.0; // TODO: Fix

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
                if (isFreecamEnabled)
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
            UpdateLabel(CameraLookAtInfoLabel2, $"X: {currentCameraLookAtX:F2}     Y: {currentCameraLookAtY:F2}     Z: {currentCameraLookAtZ:F2}", Color.White);
            UpdateLabel(CameraRotationInfoLabel, $"Pitch: {currentCameraPitch:F2}    Yaw: {currentCameraYaw:F2}     FOV: {cameraFOVSliderValue} Dist: {cameraDistanceSliderValue}", Color.White);
            UpdateLabel(CameraRotationInfoLabel2, $"Pitch: {currentCameraPitch:F2}    Yaw: {currentCameraYaw:F2}     FOV: {cameraFOVSliderValue} Dist: {cameraDistanceSliderValue}", Color.White);

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
            if (timeToTarget < equalityTolerance) return 0;
            return rotateDistance / timeToTarget;
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
                        currentCameraLookAtX = _interpolator(startX, endX, alpha);
                        currentCameraLookAtY = _interpolator(startY, endY, alpha);
                        currentCameraLookAtZ = _interpolator(startZ, endZ, alpha);
                        (currentCameraPitch, currentCameraYaw) = Interpolator.LerpRotation(startYaw, startPitch, endYaw, endPitch, alpha);

                        // Stop interpolating if alpha reaches 1
                        if (alpha >= 1.0)
                            break;

                        await Task.Delay(10);
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
                Keys.Up, Keys.Down, Keys.Left, Keys.Right,
                Keys.LShiftKey, Keys.LControlKey,
                Keys.I, Keys.K, Keys.J, Keys.L,
                Keys.F1, Keys.C, Keys.V, Keys.B, Keys.N, Keys.M
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

        private void HandleKeyDown(Keys key) {
            switch (key) {
                case Keys.F1: FreecamSwitch.Switched = !isFreecamEnabled;
                    break;
                case Keys.C:  ActivateGoToFrame(0);
                    break;
                case Keys.V:  ActivateGoToFrame(1);
                    break;
                case Keys.B:  ActivateGoToFrame(2);
                    break;
                case Keys.N:  ActivateGoToFrame(3);
                    break;
                case Keys.M:  ActivateGoToFrame(4);
                    break;
            }
        }

        private void HandleCameraController(double yawRotation)
        {
            double moveX = 0, moveY = 0, moveZ = 0;
            double rotatePitch = 0, rotateYaw = 0;

            if (pressedKeys.Contains(Keys.Up))
            {
                double radians = (yawRotation - 90) * Math.PI / 180;
                moveX += Math.Cos(radians);
                moveY += Math.Sin(radians);
            }
            if (pressedKeys.Contains(Keys.Down))
            {
                double radians = (yawRotation + 90) * Math.PI / 180;
                moveX += Math.Cos(radians);
                moveY += Math.Sin(radians);
            }
            if (pressedKeys.Contains(Keys.Left))
            {
                double radians = yawRotation * Math.PI / 180;
                moveX -= Math.Cos(radians);
                moveY -= Math.Sin(radians);
            }
            if (pressedKeys.Contains(Keys.Right))
            {
                double radians = yawRotation * Math.PI / 180;
                moveX += Math.Cos(radians);
                moveY += Math.Sin(radians);
            }
            if (pressedKeys.Contains(Keys.LShiftKey)) moveZ -= 1;
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
            } else {
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
            } else {
                double startYaw = m.ReadFloat(yawAddress);
                double startPitch = m.ReadFloat(pitchAddress);
                (double newYaw, double newPitch) = Interpolator.LerpRotation(startYaw, startPitch, targetCameraYaw, targetCameraPitch, alpha);
                currentCameraYaw = newYaw;
                currentCameraPitch = newPitch;

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
            cameraMoveSpeed = goToFrame[5];

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

            animationDuration = distance / cameraMoveSpeed;
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

        private void RecordButton_Click(object sender, EventArgs e)
        {
            isRecording = !isRecording;

            if (isRecording)
            {
                frameCounter = 0; // Reset the frame counter
                recordedFrames.Clear(); // Clear any previous data
                recordingStartTime = DateTime.Now; // Set the recording start time
                (sender as MetroSet_UI.Controls.MetroSetButton).Text = "STOP";
            }
            else
            {
                (sender as MetroSet_UI.Controls.MetroSetButton).Text = "RECORD";
                ExportRecordedFrames();
            }
        }


        private void ExportRecordedFrames()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
                DefaultExt = "csv",
                AddExtension = true,
                Title = "Save Recorded Frames"
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (StreamWriter writer = new StreamWriter(saveFileDialog.FileName))
                    {
                        writer.WriteLine("frame,time,x,y,z,pitch,azimuth");
                        foreach (var frame in recordedFrames)
                        {
                            writer.WriteLine($"{frame[0]},{frame[1]:F4},{frame[2]},{frame[3]},{frame[4]},{frame[5]},{frame[6]}");
                        }
                    }
                    MessageBox.Show("Recorded frames exported successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }


        private void ResetButton_Click(object sender, EventArgs e)
        {
            cameraFOVSliderValue = defaultCameraFov;
            CameraFOVSlider.Value = defaultCameraFov;
            m.WriteMemory("Cubic.exe+EF4E1D", "float", cameraFOVSliderValue.ToString());

            cameraDistanceSliderValue = defaultCameraDistance;
            CameraDistanceSlider.Value = defaultCameraDistance;
            m.WriteMemory("Cubic.exe+EF4FAC", "float", cameraDistanceSliderValue.ToString());
        }

        private void interpComboBox_SelectedIndexChanged(object sender, EventArgs e) {
            _interpolator = Interpolator.GetMethodWithIndex(interpComboBox.SelectedIndex);
        }
    }
}