using System;
using System.Linq;
using static Silicon.Helpers.Mathematical;

namespace Silicon.Helpers
{
    internal class Perlin1D
    {
        private readonly double _scale;
        private readonly int _octaves;
        private readonly double _persistence;
        private readonly double _lacunarity;
        private readonly int[] _permutation;

        public Perlin1D(
            double scale = 1.0,
            int octaves = 1,
            double persistence = 0.5,
            double lacunarity = 2.0,
            int seed = 0
        )
        {
            _scale = scale;
            _octaves = octaves;
            _persistence = persistence;
            _lacunarity = lacunarity;
            _permutation = GeneratePermutation(seed);
        }

        private static int[] GeneratePermutation(int seed)
        {
            Random random = new Random(seed);
            int[] array = Enumerable.Range(0, 256).ToArray();
            for (int i = array.Length - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (array[i], array[j]) = (array[j], array[i]);
            }

            return array;
        }

        private static double PerlinGradient(int hash, double x)
        {
            return (hash & 1) == 0 ? x : -x;
        }

        private double SampleOctave(double x)
        {
            int xi = (int)Math.Floor(x) & 255;
            double xf = x - Math.Floor(x);

            double u = Smoothstep5(xf);
            int a = _permutation[xi];
            int b = _permutation[xi + 1];
            double res = Lerp(PerlinGradient(a, xf), PerlinGradient(b, xf - 1), u);
            return res;
        }

        public double Sample(double x)
        {
            double amplitude = 1.0;
            double frequency = 1.0;
            double maxAmplitude = 0.0;
            double total = 0.0;

            for (int i = 0; i < _octaves; i++)
            {
                double noise = SampleOctave(x * frequency / _scale);
                total += noise * amplitude;

                maxAmplitude += amplitude;
                amplitude *= _persistence;
                frequency *= _lacunarity;
            }

            return (total / maxAmplitude + 1.0) / 2.0;
        }
    }
}