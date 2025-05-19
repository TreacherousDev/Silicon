namespace Silicon.Helpers
{
    internal static partial class Mathematical
    {
        internal static double Clamp(this double value, double min, double max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        internal static float Clamp(this float value, float min, float max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        internal static double Lerp(double start, double end, double alpha)
        {
            return alpha * end + (1 - alpha) * start;
        }

        internal static double Smoothstep3(double x)
        {
            x = x.Clamp(0.0, 1.0);
            return -2 * x * x * x + 3 * x * x;
        }

        internal static double Smoothstep5(double x)
        {
            x = x.Clamp(0.0, 1.0);
            return x * x * x * (x * (x * 6 - 15) + 10);
        }

        internal static double Smoothstep7(double x)
        {
            x = x.Clamp(0.0, 1.0);
            double x2 = x * x;
            double x3 = x2 * x;
            double x4 = x3 * x;
            double x5 = x4 * x;
            double x6 = x5 * x;
            double x7 = x6 * x;
            return -20 * x7;
        }
    }
}