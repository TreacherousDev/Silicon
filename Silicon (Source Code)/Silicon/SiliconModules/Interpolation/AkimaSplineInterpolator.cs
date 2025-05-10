using System;
using System.Collections.Generic;

namespace Silicon
{
    internal class AkimaSplineInterpolator
    {
        private readonly double[] xs;
        private readonly double[] ys;
        private readonly double[] coefficientsA;
        private readonly double[] coefficientsB;

        public AkimaSplineInterpolator(IReadOnlyList<double> xs, IReadOnlyList<double> ys)
        {
            if (xs.Count != ys.Count)
                throw new ArgumentException("xs and ys must have the same number of elements.");

            if (xs.Count < 2)
                throw new ArgumentException("At least two points are required.");

            this.xs = new double[xs.Count];
            this.ys = new double[ys.Count];
            for (int i = 0; i < xs.Count; i++)
            {
                this.xs[i] = xs[i];
                this.ys[i] = ys[i];
            }

            int n = xs.Count;

            // Fallback to linear
            if (n < 5)
            {
                coefficientsA = new double[n - 1];
                coefficientsB = new double[n - 1];
                for (int i = 0; i < n - 1; i++)
                {
                    double dx = xs[i + 1] - xs[i];
                    coefficientsA[i] = ys[i];
                    coefficientsB[i] = dx != 0 ? (ys[i + 1] - ys[i]) / dx : 0;
                }
                return;
            }

            // Compute slopes
            double[] m = new double[n - 1];
            for (int i = 0; i < n - 1; i++)
                m[i] = (ys[i + 1] - ys[i]) / (xs[i + 1] - xs[i]);

            // Extend slopes
            double[] extendedM = new double[n + 3];
            for (int i = 0; i < n - 1; i++)
                extendedM[i + 2] = m[i];

            // Use linear extrapolation for boundary slopes
            extendedM[1] = 2 * extendedM[2] - extendedM[3];
            extendedM[0] = 2 * extendedM[1] - extendedM[2];
            extendedM[n + 1] = 2 * extendedM[n] - extendedM[n - 1];
            extendedM[n + 2] = 2 * extendedM[n + 1] - extendedM[n];

            // Compute weights and tangents
            double[] t = new double[n];
            for (int i = 0; i < n; i++)
            {
                double w1 = Math.Abs(extendedM[i + 3] - extendedM[i + 2]);
                double w2 = Math.Abs(extendedM[i + 1] - extendedM[i]);

                if (w1 + w2 == 0)
                    t[i] = 0.5 * (extendedM[i + 1] + extendedM[i + 2]);
                else
                    t[i] = (w1 * extendedM[i + 1] + w2 * extendedM[i + 2]) / (w1 + w2);
            }

            // Compute coefficients
            coefficientsA = new double[n - 1];
            coefficientsB = new double[n - 1];
            for (int i = 0; i < n - 1; i++)
            {
                coefficientsA[i] = ys[i];
                coefficientsB[i] = t[i];
            }
        }

        public double Interpolate(double x)
        {
            int idx = FindSegment(x);
            double dx = x - xs[idx];
            return coefficientsA[idx] + coefficientsB[idx] * dx;
        }

        private int FindSegment(double x)
        {
            for (int i = 0; i < xs.Length - 1; i++)
            {
                if (x >= xs[i] && x <= xs[i + 1])
                    return i;
            }
            return xs.Length - 2; // Clamp to last segment
        }
    }
}
