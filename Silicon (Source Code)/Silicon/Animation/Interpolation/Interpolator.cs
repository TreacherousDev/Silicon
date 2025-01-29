using System;
using System.Numerics;

namespace Silicon {
    internal static class Interpolator {
        internal delegate double MethodDelegate(double start, double end, double alpha);

        internal static MethodDelegate GetMethodWithIndex(int idx) {
            switch (idx) {
                case 0: return Lerp;
                case 1: return Ease;
                case 2: return EaseIn;
                case 3: return EaseOut;
                case 4: return EaseInOut;
                case 5: return ExponentialIn;
                case 6: return ExponentialOut;
                case 7: return ExponentialInOut;
                default: return Lerp;
            }
        }

        internal static double Lerp(double start, double end, double alpha) {
            return alpha * end + (1 - alpha) * start;
        }

        internal static double CubicBezier(
            double start, double end, double alpha,
            double px, double py, double qx, double qy
        ) {
            CubicBezierInterpolator cbi = new CubicBezierInterpolator(px, py, qx, qy);
            return cbi.Compute(start, end, alpha);
        }

        internal static double Ease(double start, double end, double alpha) {
            return CubicBezier(start, end, alpha, 0.25, 0.1, 0.25, 1);
        }

        internal static double EaseIn(double start, double end, double alpha) {
            return CubicBezier(start, end, alpha, 0.42, 0, 1, 1);
        }

        internal static double EaseOut(double start, double end, double alpha) {
            return CubicBezier(start, end, alpha, 0, 0, 0.58, 1);
        }

        internal static double EaseInOut(double start, double end, double alpha) {
            return CubicBezier(start, end, alpha, 0.42, 0, 0.58, 1);
        }

        internal static double ExponentialIn(double start, double end, double alpha) {
            if (alpha <= 0) return start;
            return start + (end - start) * Math.Pow(2, 10 * (alpha - 1));
        }

        internal static double ExponentialOut(double start, double end, double alpha) {
            if (alpha >= 1) return end;
            return start + (end - start) * (1 - Math.Pow(2, -10 * alpha));
        }

        internal static double ExponentialInOut(double start, double end, double alpha) {
            if (alpha <= 0) return start;
            if (alpha >= 1) return end;

            if (alpha < 0.5) return start + (end - start) * 0.5 * Math.Pow(2, (20 * alpha) - 10);
            return start + (end - start) * (1 - 0.5 * Math.Pow(2, -20 * alpha + 10));
        }

        internal static (double, double) SlerpCamera(
            double startYaw, double startPitch,
            double endYaw, double endPitch, double alpha)
        {
            // Convert degrees to radians
            double startYawRad = Math.PI * startYaw / 180.0;
            double startPitchRad = Math.PI * startPitch / 180.0;
            double endYawRad = Math.PI * endYaw / 180.0;
            double endPitchRad = Math.PI * endPitch / 180.0;

            // Convert Yaw and Pitch to Quaternions
            Quaternion startQuat = Quaternion.CreateFromYawPitchRoll((float)startYawRad, (float)startPitchRad, 0);
            Quaternion endQuat = Quaternion.CreateFromYawPitchRoll((float)endYawRad, (float)endPitchRad, 0);

            // Perform Spherical Linear Interpolation (SLERP)
            Quaternion resultQuat = Quaternion.Slerp(startQuat, endQuat, (float)alpha);

            // Extract interpolated Yaw and Pitch
            Vector3 QuaternionToYawPitchRoll(Quaternion q)
            {
                double yaw = Math.Atan2(2.0 * (q.W * q.Y + q.X * q.Z), 1.0 - 2.0 * (q.Y * q.Y + q.Z * q.Z));
                double pitch = Math.Asin(2.0 * (q.W * q.X - q.Y * q.Z));
                return new Vector3((float)pitch, (float)yaw, 0);
            }

            Vector3 euler = QuaternionToYawPitchRoll(resultQuat);

            // Convert radians back to degrees
            double interpolatedYaw = euler.Y * 180.0 / Math.PI;
            double interpolatedPitch = euler.X * 180.0 / Math.PI;

            return (interpolatedYaw, interpolatedPitch);
        }
    }
}