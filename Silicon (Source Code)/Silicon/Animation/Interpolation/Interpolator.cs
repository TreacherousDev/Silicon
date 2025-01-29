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
            double endYaw, double endPitch, double alpha
        ) {
            float phi1 = (float)(startYaw * Math.PI / 180.0);
            float theta1 = (float)(startPitch * Math.PI / 180.0);
            float phi2 = (float)(endYaw * Math.PI / 180.0);
            float theta2 = (float)(endPitch * Math.PI / 180.0);

            Quaternion q1 = Quaternion.CreateFromYawPitchRoll(phi1, theta1, 0);
            Quaternion q2 = Quaternion.CreateFromYawPitchRoll(phi2, theta2, 0);

            Quaternion q = Quaternion.Slerp(q1, q2, (float)alpha);

            double yaw = Math.Atan2(2.0 * (q.Y * q.Z + q.W * q.X), q.W * q.W - q.X * q.X - q.Y * q.Y + q.Z * q.Z);
            double pitch = Math.Asin(-2.0 * (q.X * q.Z - q.W * q.Y));

            return (yaw / Math.PI * 180.0, pitch / Math.PI * 180.0);
        }
    }
}