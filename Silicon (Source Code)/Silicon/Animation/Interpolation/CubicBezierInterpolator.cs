using System;

namespace Silicon {

    internal class CubicBezierInterpolator : IInterpolator {
        private readonly double _a, _b, _c, _d;
        private const double SolverEpsilon = 1e-4;
        private const int SolverMaxIters = 24;

        public CubicBezierInterpolator(double px, double py, double qx, double qy) {
            this._a = px;
            this._b = py;
            this._c = qx;
            this._d = qy;
        }

        public double Compute(double start, double end, double alpha) {
            double t = SolveT(alpha);
            double bezierValue = BezierPolynomial(t, _b, _d);
            return bezierValue * end + (1 - bezierValue) * start;
        }

        private static double BezierPolynomial(double t, double p, double q) {
            double u = 1 - t;
            return 3 * u * u * t * p + 3 * u * t * t * q + t * t * t;
        }

        private static double PolynomialDerivative(double t, double p, double q) {
            double u = 1 - t;
            return 3 * u * u * p + 6 * u * t * (q - p) + 3 * t * t * (1 - q);
        }

        private double SolveT(double x) {
            double t = x;
            for (int i = 0; i < SolverMaxIters; i++) {
                double xEstimate = BezierPolynomial(t, _a, _c);
                double dx = xEstimate - x;
                if (Math.Abs(dx) < SolverEpsilon) return t;
                double derivative = PolynomialDerivative(t, _a, _c);
                if (Math.Abs(derivative) < SolverEpsilon) break;
                t -= dx / derivative;
            }
            return t;
        }
    }
}