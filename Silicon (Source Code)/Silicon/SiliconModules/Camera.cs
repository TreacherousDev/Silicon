using System;
using System.Numerics;
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
        private double currentCameraRoll;
        private double currentCameraFOV = 33;

        private double targetCameraLookAtX;
        private double targetCameraLookAtY;
        private double targetCameraLookAtZ;
        private double targetCameraPitch;
        private double targetCameraYaw;
        private double targetCameraFOV = 33;

        private Vector3 upVector = new Vector3(0, 0, -1);


        // Mod menu checker variables
        private bool isFreecamEnabled = false;
        private bool isHidePlayerModelEnabled = false;
        private bool isHideUserInterfaceEnabled = false;
        private bool isHideNametagsEnabled = false;
        private int cameraFOVSliderValue = 33;
        private int cameraDistanceSliderValue = 0;
        private int gameFogSliderValue = 110;
        private double cameraMoveSpeed = 0.1;
        private double cameraRotateSpeed = 0.5;


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

            if (pressedKeys.Contains(Keys.ShiftKey)) moveZ -= 1;
            if (pressedKeys.Contains(Keys.ControlKey)) moveZ += 1;
            if (pressedKeys.Contains(Keys.Up)) rotatePitch -= 1;
            if (pressedKeys.Contains(Keys.Down)) rotatePitch += 1;
            if (pressedKeys.Contains(Keys.Left)) rotateYaw -= 1;
            if (pressedKeys.Contains(Keys.Right)) rotateYaw += 1;


            //// Optional: re-orthogonalize the basis
            //Vector3 right = Vector3.Normalize(Vector3.Cross(forward, up));
            //up = Vector3.Normalize(Vector3.Cross(right, forward));


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

        private void InterpolateCameraFOV(string FOVAddress)
        {
            double elapsedTime = (Environment.TickCount / 100.0) - animationStartTime;
            double alpha = elapsedTime / animationDuration;
            alpha = Clamp(alpha, 0.0, 1.0);

            // Stop interpolation when alpha reaches 1
            if (alpha >= 1.0 - equalityTolerance)
            {
                currentCameraFOV = targetCameraFOV;
            }
            else
            {
                currentCameraFOV = _interpolator(m.ReadFloat(FOVAddress), targetCameraFOV, alpha);
            }
        }

        private Vector3 GetForwardVector(double pitchDeg, double yawDeg)
        {
            double pitch = Math.PI / 180f * pitchDeg;
            double yaw = Math.PI / 180f * yawDeg;

            double x = Math.Cos(pitch) * Math.Sin(yaw);
            double y = Math.Cos(pitch) * Math.Cos(yaw);
            double z = Math.Sin(pitch);

            Vector3 forward = new Vector3((float)x, (float)y, (float)z);
            return Vector3.Normalize(forward);
        }


        private Vector3 ComputeUpVectorFromYawRoll(float rollDegrees)
        {
            // Convert angles to radians
            double rollRad = rollDegrees * (Math.PI / 180f);

            // Get forward vector
            Vector3 forward = GetForwardVector(-currentCameraPitch, -currentCameraYaw);
            Vector3 forwardAxis = Vector3.Normalize(forward);

            // Pre-existing world up vector
            Vector3 worldUp = new Vector3(0, 0, -1);

            // Orthogonalize worldUp against forward
            Vector3 projUpAxis = worldUp - forwardAxis * Vector3.Dot(worldUp, forwardAxis);
            projUpAxis = Vector3.Normalize(projUpAxis);
            // Computing this new up axis before computing the right vector avoids numerical instability near the pole

            // Construct new orthonormal basis
            Vector3 v = projUpAxis;
            Vector3 kCrossV = Vector3.Cross(forwardAxis, projUpAxis);
            // Vector3 v = forwardAxis;

            // Compute coefficients for linear combination
            // This rotates the up vector around the flat forward direction
            float c1 = (float)Math.Cos(rollRad);
            float c2 = (float)Math.Sin(rollRad);
            // Remaining term in Rodrigues' formula, but we don't need it because it's always 0
            // float c3 = (float)(Vector3.Dot(forwardAxis, projUpAxis) * (1 - Math.Cos(rollRad)));

            return Vector3.Normalize(c1 * v + c2 * kCrossV);
        }


        private void UpdateCameraRoll()
        {
            // if (pressedKeys.Contains(Keys.Up) || pressedKeys.Contains(Keys.Down) || pressedKeys.Contains(Keys.Left)
            //     || pressedKeys.Contains(Keys.Right) || IsRightMouseButtonDown())
            // {
            //     ResetCameraRoll();
            //     return;
            // }

            if (pressedKeys.Contains(Keys.Q))
                currentCameraRoll -= 2;

            if (pressedKeys.Contains(Keys.E))
                currentCameraRoll += 2;

            Console.WriteLine(
                $@"Pitch: {currentCameraPitch:F2}, Yaw: {currentCameraYaw:F2}, Roll: {currentCameraRoll:F2}");
            upVector = ComputeUpVectorFromYawRoll((float)currentCameraRoll);
            Console.WriteLine($@"Up: {upVector.X:F2}, {upVector.Y:F2}, {upVector.Z:F2}");
        }

        private void ResetCameraRoll()
        {
            upVector = new Vector3(0, 0, -1);
            currentCameraRoll = 0;
        }
    }
}