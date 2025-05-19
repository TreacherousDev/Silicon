using System;
using System.Numerics;

namespace Silicon.CameraEffect
{
    internal class OrnsteinUhlenbeckShakeFx : ICameraEffect
    {
        private readonly double _theta;
        private readonly double _sigma;
        private Vector3 _steady = Vector3.Zero;
        private readonly Random _random = new Random();

        public OrnsteinUhlenbeckShakeFx(double theta, double sigma)
        {
            _theta = theta;
            _sigma = sigma;
        }

        public void IncrementDelta(double dt)
        {
            double sqrtDt = Math.Sqrt(dt);
            _steady.X += (float)(-_theta * _steady.X * dt + _sigma * sqrtDt * NextGaussian());
            _steady.Y += (float)(-_theta * _steady.Y * dt + _sigma * sqrtDt * NextGaussian());
            _steady.Z += (float)(-_theta * _steady.Z * dt + _sigma * sqrtDt * NextGaussian());
        }

        public ProjectiveEffector GetEffector()
        {
            return new ProjectiveEffector
            {
                EffectAffineMat = Matrix4x4.CreateTranslation(_steady),
                FovDelta = 0
            };
        }

        private double NextGaussian()
        {
            double u1 = 1.0 - _random.NextDouble();
            double u2 = 1.0 - _random.NextDouble();
            return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
        }
    }
}