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
        private double currentCameraSightRange = 100;

        private double targetCameraLookAtX;
        private double targetCameraLookAtY;
        private double targetCameraLookAtZ;
        private double targetCameraPitch;
        private double targetCameraYaw;
        private double targetCameraRoll;
        private double targetCameraFOV = 33;
        private double targetCameraSightRange = 100;

        private double startCameraLookAtX = 0;
        private double startCameraLookAtY = 0;
        private double startCameraLookAtZ = 0;
        private double startCameraPitch = 0;
        private double startCameraYaw = 0;
        private double startCameraRoll = 0;
        private double startCameraFOV = 33;
        private double startCameraSightRange = 100;

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
        private double cameraRotateSpeed = 1.5;


        private bool isHeadBobEnabled = false;
        private double headBobTimer = 0;
        private double headBobAmplitude = 0.04;   // height of bob
        private double headBobFrequency = 60;   // speed of bob
        private double headBobCurrentOffset = 0;
        private float baseCameraHeight = 1.1f;      // original value
        private float currentCameraHeight = 1.1f;
        private float currentFOVOffset = 0f;
        private float walkFOVBoost = 4.0f;   // how much FOV increases
        private float fovLerpSpeed = 0.08f;  // smoothness (0–1)

        private void HeadBob()
        {
            if (isFreecamEnabled) return;
            if (isChatting) return;

            bool isMoving = movementState.Forward || movementState.Backward || movementState.Left || movementState.Right;
            if (isHeadBobEnabled)
            {
                if (isMoving)
                {
                    // 5ms tick = 0.005 seconds
                    headBobTimer += 0.005;
                    headBobCurrentOffset = Math.Sin(headBobTimer * headBobFrequency) * headBobAmplitude;
                    currentFOVOffset += (walkFOVBoost - currentFOVOffset) * fovLerpSpeed;
                }
                else
                {
                    // Smooth reset
                    headBobCurrentOffset *= 0.85;
                    if (Math.Abs(headBobCurrentOffset) < 0.0001) headBobCurrentOffset = 0;
                    currentFOVOffset += (0 - currentFOVOffset) * fovLerpSpeed;
                }

                float finalHeight = baseCameraHeight + (float)headBobCurrentOffset;
                float finalFOV = CameraFOVSlider.Value + currentFOVOffset;
                targetCameraFOV = finalFOV;
                currentCameraHeight = finalHeight;

            }
            else
            {
                // Restore original height if disabled
                currentCameraHeight = baseCameraHeight;
            }
        }
        private void HandleCameraController(double yawRotation)
        {
            if (!IsCubicWindowFocused() || isChatting) return;

            double moveX = 0, moveY = 0, moveZ = 0;
            double rotatePitch = 0, rotateYaw = 0, rotateRoll = 0;

            if (movementState.Forward)
            {
                double radians = (yawRotation - 90) * Math.PI / 180;
                moveX += Math.Cos(radians);
                moveY += Math.Sin(radians);
            }
            if (movementState.Backward)
            {
                double radians = (yawRotation + 90) * Math.PI / 180;
                moveX += Math.Cos(radians);
                moveY += Math.Sin(radians);
            }
            if (movementState.Left)
            {
                double radians = yawRotation * Math.PI / 180;
                moveX -= Math.Cos(radians);
                moveY -= Math.Sin(radians);
            }
            if (movementState.Right)
            {
                double radians = yawRotation * Math.PI / 180;
                moveX += Math.Cos(radians);
                moveY += Math.Sin(radians);
            }

            if (movementState.Down) moveZ -= 1;
            if (movementState.Up) moveZ += 1;
            if (movementState.PitchUp) rotatePitch -= 1;
            if (movementState.PitchDown) rotatePitch += 1;
            if (movementState.YawLeft) rotateYaw -= 1;
            if (movementState.YawRight) rotateYaw += 1;
            if (movementState.RollLeft) rotateRoll -= 1;
            if (movementState.RollRight) rotateRoll += 1;

            // Normalize and apply speed
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

            targetCameraPitch += rotatePitch * cameraRotateSpeed;
            targetCameraYaw += rotateYaw * cameraRotateSpeed;
            targetCameraRoll += rotateRoll * cameraRotateSpeed;
            targetCameraPitch = Clamp(targetCameraPitch, -89, 89);
        }


        public static double Clamp(double value, double min, double max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        private void InterpolateCamera()
        {
            // Look at X, Y, Z
            Interpolate(ref currentCameraLookAtX, startCameraLookAtX, targetCameraLookAtX);
            Interpolate(ref currentCameraLookAtY, startCameraLookAtY, targetCameraLookAtY);
            Interpolate(ref currentCameraLookAtZ, startCameraLookAtZ, targetCameraLookAtZ);
            // Pitch, Yaw, Roll, Up Vector
            Interpolate(ref currentCameraPitch, startCameraPitch, targetCameraPitch);
            Interpolate(ref currentCameraYaw, startCameraYaw, targetCameraYaw);
            Interpolate(ref currentCameraRoll, startCameraRoll, targetCameraRoll);
            upVector = ComputeUpVectorFromRoll((float)currentCameraRoll);
            // FOV
            Interpolate(ref currentCameraFOV, startCameraFOV, targetCameraFOV);
            // Sight Range
            Interpolate(ref currentCameraSightRange, startCameraSightRange, targetCameraSightRange);
        }

        private void Interpolate(ref double current, double start, double target)
        {
            double alpha = GetAlpha();
            current = (alpha >= 1.0 - equalityTolerance) ? target : _interpolator(start, target, alpha);
        }

        private double GetAlpha()
        {
            double elapsedTime = (Environment.TickCount / 10000.0) - animationStartTime;
            double alpha = elapsedTime / animationDuration;
            return Clamp(alpha, 0.0, 1.0);
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

        private Vector3 ComputeUpVectorFromRoll(float rollDegrees)
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


        private void ResetCameraRoll()
        {
            upVector = new Vector3(0, 0, -1);
            currentCameraRoll = 0;
            targetCameraRoll = 0;
        }
    }
}