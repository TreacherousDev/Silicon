using System;
using System.Windows.Forms;

namespace Silicon
{
    public partial class SiliconForm
    {
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


        private bool IsRightMouseButtonDown()
        {
            return (GetAsyncKeyState(Keys.RButton) & 0x8000) != 0;
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
            else
            {
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

    }
}