using System;
using System.Collections.Generic;
using System.Numerics;

namespace Silicon.CameraEffect
{
    internal class CameraEffectManager
    {
        private readonly List<ICameraEffect> _effects = new List<ICameraEffect>();

        public void AddEffect(ICameraEffect e) => _effects.Add(e);

        public void ClearEffects()
        {
            _effects.Clear();
        }

        public ProjectiveEffector ComputeDelta(double deltaTime)
        {
            ProjectiveEffector netEffector = new ProjectiveEffector
            {
                EffectAffineMat = Matrix4x4.Identity,
                FovDelta = 0
            };

            foreach (ICameraEffect fx in _effects)
            {
                fx.IncrementDelta(deltaTime);
                ProjectiveEffector layer = fx.GetEffector();

                netEffector.EffectAffineMat = Matrix4x4.Multiply(
                    netEffector.EffectAffineMat,
                    layer.EffectAffineMat
                );
                netEffector.FovDelta += layer.FovDelta;
            }

            return netEffector;
        }
    }
}