using System;

namespace Silicon {
    internal class Interpolation {
        internal double Lerp(double start, double end, double alpha) {
            return alpha * end + (1 - alpha) * start;
        }

        internal double CubicBezier(
            double start, double end, double alpha,
            double px, double py, double qx, double qy
        ) {
            CubicBezierInterpolator cbi = new CubicBezierInterpolator(px, py, qx, qy);
            return cbi.Compute(start, end, alpha);
        }

        internal double Ease(double start, double end, double alpha) {
            return CubicBezier(start, end, alpha, 0.25, 0.1, 0.25, 1);
        }

        internal double EaseIn(double start, double end, double alpha) {
            return CubicBezier(start, end, alpha, 0.42, 0, 1, 1);
        }

        internal double EaseOut(double start, double end, double alpha) {
            return CubicBezier(start, end, alpha, 0, 0, 0.58, 1);
        }

        internal double EaseInOut(double start, double end, double alpha) {
            return CubicBezier(start, end, alpha, 0.42, 0, 0.58, 1);
        }

        internal double ExponentialIn(double start, double end, double alpha) {
            if (alpha <= 0) return start;
            return start + (end - start) * Math.Pow(2, 10 * (alpha - 1));
        }

        internal double ExponentialOut(double start, double end, double alpha) {
            if (alpha >= 1) return end;
            return start + (end - start) * (1 - Math.Pow(2, -10 * alpha));
        }

        internal double ExponentialInOut(double start, double end, double alpha) {
            if (alpha <= 0) return start;
            if (alpha >= 1) return end;

            if (alpha < 0.5) return start + (end - start) * 0.5 * Math.Pow(2, (20 * alpha) - 10);
            return start + (end - start) * (1 - 0.5 * Math.Pow(2, -20 * alpha + 10));
        }
    }
}