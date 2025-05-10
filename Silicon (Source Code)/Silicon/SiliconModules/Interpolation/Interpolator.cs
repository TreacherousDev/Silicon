using System;
using System.Collections.Generic;
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
                case 8: return Lerp; //CatmulRom code defaults to linear
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

    }
}