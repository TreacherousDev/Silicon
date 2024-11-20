﻿using Memory;
using MetroSet_UI.Forms;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Timers;
using Sunny.UI;
using Timer = System.Timers.Timer;
using NHotkey;
using NHotkey.WindowsForms;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Data.SqlTypes;


namespace Silicon
{


    public partial class SiliconForm : MetroSetForm
    {
        public Mem m = new Mem();
        private System.Timers.Timer processCheckTimer;
        private readonly Dictionary<String, int> _hotkeyMap;
        private readonly Dictionary<int, ToggleAction> _scriptActions;
        public delegate void ToggleAction();

        public SiliconForm()
        {
            InitializeComponent();
            this.KeyDown += Game_KeyDown;
            this.KeyUp += Game_KeyUp;
            this.KeyPreview = true;

            _hotkeyMap = new Dictionary<string, int>
            {
                { "F1", 1 },
                { "F2", 2 },
                { "F3", 3 },
                { "F4", 4 },
                { "F5", 5 },
            };


            foreach (var kvp in _hotkeyMap)
            {
                HotkeyManager.Current.AddOrReplace(kvp.Key, (Keys)Enum.Parse(typeof(Keys), kvp.Key), HotKeyshandler);
            }

            // Some color overrides since the TabControl seems to be bugged and sets by default the ForeColor to Black even when the default color has been changed.
            Hispano.ForeColor = Color.Gray;
            proID.ForeColor = Color.White;
            Status.ForeColor = Color.White;
            FreecamSwitch.CheckColor = Color.Silver;
            HidePlayerModelSwitch.CheckColor = Color.Silver;
            AddAnimationFrameButton.NormalBorderColor = Color.FromArgb(80, 160, 255);
            AddAnimationFrameButton.NormalColor = Color.FromArgb(80, 160, 255);
            AddAnimationFrameButton.PressBorderColor = Color.FromArgb(64, 128, 204);
            AddAnimationFrameButton.PressColor = Color.FromArgb(64, 128, 204);
            PlayAnimationButton.NormalBorderColor = Color.FromArgb(80, 160, 255);
            PlayAnimationButton.NormalColor = Color.FromArgb(80, 160, 255);
            PlayAnimationButton.PressBorderColor = Color.FromArgb(64, 128, 204);
            PlayAnimationButton.PressColor = Color.FromArgb(64, 128, 204);

        }

        private void HotKeyshandler(object sender, HotkeyEventArgs e)
        {
            if (_hotkeyMap.TryGetValue(e.Name, out int scriptNumber))
            {
                ScriptHandler(scriptNumber);
            }
            e.Handled = true;
        }

        private void ScriptHandler(int scriptNumber)
        {
            if (_scriptActions.TryGetValue(scriptNumber, out ToggleAction action))
            {
                action();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            StartProcessCheckTimer();

            if (!HeliumWorker.IsBusy)
                HeliumWorker.RunWorkerAsync();
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
                    scriptInjected = false;
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

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (processCheckTimer != null)
            {
                processCheckTimer.Stop();
                processCheckTimer.Dispose();
            }

            HotkeyManager.Current.Remove("F1");
            HotkeyManager.Current.Remove("F2");
            HotkeyManager.Current.Remove("F3");
            HotkeyManager.Current.Remove("F4");
            HotkeyManager.Current.Remove("F5");
            base.OnFormClosing(e);
        }


        // From this point to the bottom is the main mod menu code

        private double currentCameraLookAtX;
        private double currentCameraLookAtY;
        private double currentCameraLookAtZ;
        private double targetCameraLookAtX;
        private double targetCameraLookAtY;
        private double targetCameraLookAtZ;
        private double cameraMoveDirectionX;
        private double cameraMoveDirectionY;
        private double cameraMoveDirectionZ;
        private double cameraMoveDistance;

        double currentCameraPitch = 30;
        double currentCameraYaw = 30;
        double targetCameraPitch = 20;
        double targetCameraYaw = 20;
        private double cameraRotateDirectionPitch;
        private double cameraRotateDirectionYaw;
        private double cameraRotateDistance;

        bool scriptInjected = false;
        private Timer _updateTimer;

        private void HeliumWorker_DoWork(object sender, DoWorkEventArgs e)
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

        readonly string cameraHeightInjection = "53 E8 00 00 00 00 5B F3 0F 5C 43 1B F3 0F 11 40 08 5B F3 0F 5C CB 8D 85 FC FE FF FF E9 72 34 39 FF 66 66 A6 3F";
        readonly string cameraHeightFunctionEntry = "E9 72 CB C6 00 0F 1F 44 00 00";

        readonly string unlockCameraArrowsInjection = "50 E8 00 00 00 00 58 F3 0F 10 58 1F 0F 2F D8 58 0F 86 AA F7 38 FF C7 86 EC 09 00 00 00 00 B2 C2 E9 9B F7 38 FF 00 00 B2 C2 86 92 F7 38 FF E9 86 F7 38 FF 00 00 B2 C2";
        readonly string unlockCameraArrowsFunctionEntry = "E9 47 08 C7 00";

        readonly string unlockCameraRMBInjection = "50 E8 00 00 00 00 58 F3 0F 10 70 1F 0F 2F F1 58 0F 86 55 0E 3A FF C7 80 EC 09 00 00 00 00 B2 C2 E9 46 0E 3A FF 00 00 B2 C2 FF 00 00 B2 C2";
        readonly string unlockCameraRMBFunctionEntry = "E9 9C F1 C5 00";

        readonly string unlockCameraFOVInjection = "50 E8 00 00 00 00 58 F3 0F 10 40 12 58 8D 85 D8 FE FF FF E9 D7 33 39 FF 00 00 70 42";
        readonly string unlockCameraFOVFunctionEntry = "E9 12 CC C6 00 90";

        readonly string adjustCameraDistanceInjection = "50 E8 00 00 00 00 58 F3 0F 59 40 24 F3 0F 59 58 24 F3 0F 59 60 24 58 E9 00 00 00 00 F3 0F 5C D0 F3 0F 10 40 08 E9 0E 32 39 FF 00 00 B4 41 FF FF FF FF";
        readonly string adjustCameraDistanceFunctionEntry = "E9 CC CD C6 00 0F 1F 40 00";

        // Revertable functions (Optional switch states available)
        readonly string cameraLookAtEditorInjection = "50 E8 00 00 00 00 58 F3 0F 11 58 5D F3 0F 11 48 61 F3 0F 11 40 65 F3 0F 10 58 4D F3 0F 10 48 51 F3 0F 10 40 55 58 50 E8 00 00 00 00 58 F3 0F 11 58 37 F3 0F 11 48 3B F3 0F 11 40 3F 53 8D 5E 10 89 58 33 5B 58 F3 0F 11 1E F3 0F 11 4E 04 E9 57 FA 38 FF DB B6 0D 42 28 49 F2 41 00 00 96 42 28 59 33 13 DB B6 0D 42 28 49 F2 41 00 00 96 42 44 42 F6 FF BF 42 00 00 00";
        readonly string cameraLookAtEditorFunctionEntry = "E9 5A 05 C7 00 0F 1F 40 00";
        readonly string cameraLookAtEditorFunctionOriginal = "E9 80 05 C7 00 0F 1F 40 00";

        readonly string hidePlayerAvatarInjection = "53 E8 00 00 00 00 5B F3 0F 10 7D 08 F3 0F 5C BB 65 01 00 00 50 F3 0F 11 7B 6B 8B 43 6B 23 43 63 66 0F 6E F8 58 0F 2E 7B 67 0F 83 2F 00 00 00 F3 0F 10 7D 0C F3 0F 5C BB 69 01 00 00 50 F3 0F 11 7B 6B 8B 43 6B 23 43 63 66 0F 6E F8 58 0F 2E 7B 67 0F 83 07 00 00 00 C7 45 10 00 00 C8 C2 5B F3 0F 10 45 10 E9 BD 64 37 FF FF FF FF 7F 9A 99 99 3E 00 00 00 00";
        readonly string hidePlayerAvatarFunctionEntry = "E9 DA 9A C8 00";
        readonly string hidePlayerAvatarFunctionOriginal = "F3 0F 10 45 10";

        
        private bool isFreecamEnabled = false;
        private bool isHidePlayerModelEnabled = false;
        private int cameraFOVSliderValue = 0;
        private int cameraDistanceSliderValue = 0;
        private double cameraMoveSpeed = 0.1;
        private double cameraRotateSpeed = 0.5;
        private void CheckAndUpdateMemory()
        {

            HandleCameraController(currentCameraYaw);

            uint intRotationAddress = m.ReadUInt("Cubic.exe+E2903E");
            string pitchAddress = (intRotationAddress + 4).ToString("X");
            string yawAddress = (intRotationAddress).ToString("X");
            string lookAtXAddress = (intRotationAddress - 16).ToString("X");
            string lookAtYAddress = (intRotationAddress - 12).ToString("X");
            string lookAtZAddress = (intRotationAddress - 8).ToString("X");

            InterpolateCameraMovement(lookAtXAddress, lookAtYAddress, lookAtZAddress);
            InterpolateCameraRotation(pitchAddress, yawAddress);


            //initial injection script that lays the foundation for the UI to access the camera settings addresses
            if (scriptInjected == false && getStatus.Text == "CONNECTED")
            {
                scriptInjected = true;
                m.WriteMemory("Cubic.exe+1BC1A5", "bytes", cameraCoordinatesFunction);

                // Special case for cameraLookAtEditor
                // Always inject, but skip assignment if deactivated (inject to the adress after assignment)
                m.WriteMemory("Cubic.exe+E28FDF", "bytes", cameraLookAtEditorInjection);
                m.WriteMemory("Cubic.exe+1B8A80", "bytes", cameraLookAtEditorFunctionOriginal);
                
                m.WriteMemory("Cubic.exe+E28D31", "bytes", cameraHeightInjection);
                m.WriteMemory("Cubic.exe+1BC1BA", "bytes", cameraHeightFunctionEntry);
                m.WriteMemory("Cubic.exe+E28D7A", "bytes", unlockCameraArrowsInjection);
                m.WriteMemory("Cubic.exe+1B852E", "bytes", unlockCameraArrowsFunctionEntry);
                m.WriteMemory("Cubic.exe+E28DC8", "bytes", unlockCameraRMBInjection);
                m.WriteMemory("Cubic.exe+1C9C27", "bytes", unlockCameraRMBFunctionEntry);
                m.WriteMemory("Cubic.exe+E28E05", "bytes", unlockCameraFOVInjection);
                m.WriteMemory("Cubic.exe+1BC1EE", "bytes", unlockCameraFOVFunctionEntry);
                m.WriteMemory("Cubic.exe+E28F82", "bytes", adjustCameraDistanceInjection);
                m.WriteMemory("Cubic.exe+1BC1B1", "bytes", adjustCameraDistanceFunctionEntry);

                //Revertable, injections only as set to false by default
                m.WriteMemory("Cubic.exe+E28ED7", "bytes", hidePlayerAvatarInjection);
            }

            if (FreecamSwitch.Switched != isFreecamEnabled)
            {
                isFreecamEnabled = FreecamSwitch.Switched;
                if (isFreecamEnabled == true)
                {
                    m.WriteMemory("Cubic.exe+1B8A80", "bytes", cameraLookAtEditorFunctionEntry);
                    targetCameraLookAtX = m.ReadFloat("Cubic.exe+E29042");
                    targetCameraLookAtY = m.ReadFloat("Cubic.exe+E29046");
                    targetCameraLookAtZ = m.ReadFloat("Cubic.exe+E2904A");
                    targetCameraPitch = currentCameraPitch;
                    targetCameraYaw = currentCameraYaw;
                }
                else
                {
                    m.WriteMemory("Cubic.exe+1B8A80", "bytes", cameraLookAtEditorFunctionOriginal);
                }
            }

            if (HidePlayerModelSwitch.Switched != isHidePlayerModelEnabled) 
            {
                isHidePlayerModelEnabled = HidePlayerModelSwitch.Switched;
                if (isHidePlayerModelEnabled == true)
                {
                    m.WriteMemory("Cubic.exe+19F3F8", "bytes", hidePlayerAvatarFunctionEntry);
                }
                else
                {
                    m.WriteMemory("Cubic.exe+19F3F8", "bytes", hidePlayerAvatarFunctionOriginal);
                }
            }

            if (CameraFOVSlider.Value != cameraFOVSliderValue)
            {
                cameraFOVSliderValue = CameraFOVSlider.Value;
                m.WriteMemory("Cubic.exe+E28E1D", "float", CameraFOVSlider.Value.ToString());
            }

            if (CameraDistanceSlider.Value != cameraDistanceSliderValue)
            {
                cameraDistanceSliderValue = CameraDistanceSlider.Value;
                m.WriteMemory("Cubic.exe+E28FAC", "float", CameraDistanceSlider.Value.ToString());
            }

            if (FreecamSwitch.Switched == true)
            {
                //Overwrite camera position, pitch and yaw using Silicon as the ew controller
                m.WriteMemory(pitchAddress, "float", currentCameraPitch.ToString());
                m.WriteMemory(yawAddress, "float", currentCameraYaw.ToString());
                m.WriteMemory("Cubic.exe+E29032", "float", currentCameraLookAtX.ToString());
                m.WriteMemory("Cubic.exe+E29036", "float", currentCameraLookAtY.ToString());
                m.WriteMemory("Cubic.exe+E2903A", "float", currentCameraLookAtZ.ToString());
            }
                
            //Console.WriteLine(currentCameraLookAtX + " " + currentCameraLookAtY + " " + currentCameraLookAtZ);
            //UpdateLabel(CameraPositionDataLabel, $"X: {currentCameraLookAtX:F2} Y: {currentCameraLookAtY:F2} Z: {currentCameraLookAtZ:F2} Pitch: {currentCameraPitch:F2} Yaw: {currentCameraYaw:F2}", Color.Red);
            UpdateLabel(CameraLookAtInfoLabel, $"X: {currentCameraLookAtX:F2}   Y: {currentCameraLookAtY:F2}   Z: {currentCameraLookAtZ:F2}", Color.White);
            UpdateLabel(CameraRotationInfoLabel, $"Pitch: {currentCameraPitch:F2}        Yaw: {currentCameraYaw:F2}", Color.White);
        }


       
       
        List<List<double>> animationFrames = new List<List<double>>();
        private async void PlayAnimationButton_Click(object sender, EventArgs e)
        {
            int frameIndex = 0;
            Console.WriteLine(animationFrames.Count);
            foreach (List<double> frame in animationFrames)
            {
                Console.WriteLine("a");
                targetCameraLookAtX = frame[0];
                targetCameraLookAtY = frame[1];
                targetCameraLookAtZ = frame[2];
                targetCameraPitch = frame[3];
                targetCameraYaw = frame[4];
                cameraMoveSpeed = frame[5];

                // Calculate rotation speed dynamically based on distance
                // This allows us to haave smoother transitions as rotations will end on hte exact keyframe
                // that the camera arrives to its destination

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
                cameraRotateSpeed = rotateDistance / timeToTarget;

                //warp to the first frame quickly and wait for some time
                if (frameIndex == 0)
                {
                    cameraMoveSpeed = 1;
                    cameraRotateSpeed = 5;
                    while (cameraMoveDistance > 0.05)
                    {
                        await Task.Delay(100);
                    }
                    await Task.Delay(1000);
                }
                frameIndex++;

                await Task.Delay(100);
                while (cameraMoveDistance > 0.5)
                {
                    await Task.Delay(100);
                }
            }
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
        }

        private HashSet<Keys> pressedKeys = new HashSet<Keys>();

        // This function gets called in the main update loop
        // It handles the movement and rotation of the camera when freecam is activated
        private void HandleCameraController(double yawRotation)
        {
            double moveX = 0, moveY = 0, moveZ = 0;
            double rotatePitch = 0, rotateYaw = 0;

            // Check pressed keys and calculate movement direction
            if (pressedKeys.Contains(Keys.W)) // Forward
            {
                double radians = (yawRotation - 90) * Math.PI / 180;
                moveX += Math.Cos(radians);
                moveY += Math.Sin(radians);
            }
            if (pressedKeys.Contains(Keys.S)) // Backward
            {
                double radians = (yawRotation + 90) * Math.PI / 180;
                moveX += Math.Cos(radians);
                moveY += Math.Sin(radians);
            }
            if (pressedKeys.Contains(Keys.A)) // Left
            {
                double radians = yawRotation * Math.PI / 180;
                moveX -= Math.Cos(radians);
                moveY -= Math.Sin(radians);
            }
            if (pressedKeys.Contains(Keys.D)) // Right
            {
                double radians = yawRotation * Math.PI / 180;
                moveX += Math.Cos(radians);
                moveY += Math.Sin(radians);
            }
            if (pressedKeys.Contains(Keys.Space)) // Up
            {
                moveZ -= 1;
            }
            if (pressedKeys.Contains(Keys.ShiftKey)) // Down
            {
                moveZ += 1;
            }
            if (pressedKeys.Contains(Keys.I))
            {
                rotatePitch -= 1;
            }
            if (pressedKeys.Contains(Keys.K))
            {
                rotatePitch += 1;
            }
            if (pressedKeys.Contains(Keys.J))
            {
                rotateYaw -= 1;
            }
            if (pressedKeys.Contains(Keys.L))
            {
                rotateYaw += 1;
            }

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
            if (targetCameraLookAtZ > 100)
            {
                targetCameraLookAtZ = 100;
            }

            double rotateMagnitude = Math.Sqrt(rotatePitch * rotatePitch + rotateYaw * rotateYaw);
            if (rotateMagnitude > 0.05)
            {
                rotatePitch /= rotateMagnitude;
                rotateYaw /= rotateMagnitude;
            }
            targetCameraPitch += rotatePitch * cameraRotateSpeed;
            targetCameraYaw += rotateYaw * cameraRotateSpeed;
            if (targetCameraPitch > 89) 
            {
                targetCameraPitch = 89;
            }
            if (targetCameraPitch < -89)
            {
                targetCameraPitch = -89;
            }

        }
        private void Game_KeyDown(object sender, KeyEventArgs e)
        {
            pressedKeys.Add(e.KeyCode);
        }
        private void Game_KeyUp(object sender, KeyEventArgs e)
        {
            pressedKeys.Remove(e.KeyCode);
        }
        private void InterpolateCameraMovement(string lookAtXAddress,string lookAtYAddress, string lookAtZAddress)
        {
            currentCameraLookAtX = m.ReadFloat(lookAtXAddress);
            currentCameraLookAtY = m.ReadFloat(lookAtYAddress);
            currentCameraLookAtZ = m.ReadFloat(lookAtZAddress);

            // Calculate the direction vector towards the target
            cameraMoveDirectionX = targetCameraLookAtX - currentCameraLookAtX;
            cameraMoveDirectionY = targetCameraLookAtY - currentCameraLookAtY;
            cameraMoveDirectionZ = targetCameraLookAtZ - currentCameraLookAtZ;
            //Normalize distance
            cameraMoveDistance = Math.Sqrt(cameraMoveDirectionX * cameraMoveDirectionX + cameraMoveDirectionY * cameraMoveDirectionY + cameraMoveDirectionZ * cameraMoveDirectionZ);

            //Interpolate and move to target location
            //Snap to target if close enough
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
    }
}