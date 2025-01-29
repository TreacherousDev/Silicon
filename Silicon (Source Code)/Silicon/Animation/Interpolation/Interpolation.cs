namespace Silicon {
    internal class Interpolation {
        public double Lerp(double start, double end, double alpha) {
            return alpha * end + (1 - alpha) * start;
        }

        public double CubicBezier(
            double start, double end, double alpha,
            double px, double py, double qx, double qy
        ) {
            CubicBezierInterpolator cbi = new CubicBezierInterpolator(px, py, qx, qy);
            return cbi.Compute(start, end, alpha);
        }
    }
}