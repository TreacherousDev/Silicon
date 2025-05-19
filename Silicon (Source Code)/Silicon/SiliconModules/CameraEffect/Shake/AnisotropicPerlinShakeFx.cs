using System;
using System.Numerics;
using Silicon.Helpers;

namespace Silicon.CameraEffect
{
    internal class AnisotropicPerlinShakeFx : ICameraEffect
    {
        private readonly Vector3 _amplitudes;
        private readonly Vector3 _t0;
        private double _time;

        private readonly Perlin1D _perlin;
        private readonly Random _random = new Random();

        public AnisotropicPerlinShakeFx(
            Vector3 amplitudes,
            double scale = 520,
            int octaves = 5,
            double persistence = 0.4,
            double lacunarity = 0.8,
            int seed = 0
        )
        {
            _perlin = new Perlin1D(scale, octaves, persistence, lacunarity, seed);
            _amplitudes = amplitudes;

            // Any number is ok, they just need to be different from one another
            _t0 = new Vector3(
                (float)_random.NextDouble() * 961,
                (float)_random.NextDouble() * 961 + 31,
                (float)_random.NextDouble() * 961 + 79
            );
        }

        public void IncrementDelta(double dt)
        {
            _time += dt;
        }

        public ProjectiveEffector GetEffector()
        {
            return new ProjectiveEffector
            {
                EffectAffineMat = Matrix4x4.CreateTranslation(
                    xPosition: (float)(_perlin.Sample(_time + _t0.Y) * 2 - 1) * _amplitudes.X,
                    yPosition: (float)(_perlin.Sample(_time + _t0.X) * 2 - 1) * _amplitudes.Y,
                    zPosition: (float)(_perlin.Sample(_time + _t0.Z) * 2 - 1) * _amplitudes.Z
                ),
                FovDelta = 0
            };
        }
    }
}