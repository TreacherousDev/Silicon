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

        internal static (double, double) SlerpRotation(
            double startYaw, double startPitch,
            double endYaw, double endPitch, double alpha)
        {
            double phi1 = Math.PI * startYaw / 180.0;
            double theta1 = Math.PI * startPitch / 180.0;
            double phi2 = Math.PI * endYaw / 180.0;
            double theta2 = Math.PI * endPitch / 180.0;

            Quaternion q1 = Quaternion.CreateFromYawPitchRoll((float)phi1, (float)theta1, 0);
            Quaternion q2 = Quaternion.CreateFromYawPitchRoll((float)phi2, (float)theta2, 0);
            Quaternion q = Quaternion.Slerp(q1, q2, (float)alpha);

            double yaw = Math.Atan2(2.0 * (q.W * q.Y + q.X * q.Z), 1.0 - 2.0 * (q.Y * q.Y + q.Z * q.Z));
            double pitch = Math.Asin(2.0 * (q.W * q.X - q.Y * q.Z));

            return (yaw * 180.0 / Math.PI, pitch * 180.0 / Math.PI);
        }

        internal static (double, double) LerpRotation(
            double startYaw, double startPitch,
            double endYaw, double endPitch, double alpha)
        {
            double yaw = LerpDegrees(startYaw, endYaw, alpha);
            double pitch = LerpDegrees(startPitch, endPitch, alpha);
            return (yaw, pitch);
        }

        private static double LerpDegrees(double start, double end, double alpha)
        {
            double delta = (end - start) % 360.0;

            if (delta > 180.0) delta -= 360.0;
            if (delta < -180.0) delta += 360.0;

            return (start + alpha * delta) % 360.0;
        }
    }
}