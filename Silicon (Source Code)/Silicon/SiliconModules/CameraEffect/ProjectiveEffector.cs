using System.Numerics;

namespace Silicon.CameraEffect
{
    struct ProjectiveEffector
    {
        internal Matrix4x4 EffectAffineMat;
        public double FovDelta;

        internal ProjectiveEffector(Matrix4x4 effectAffineMat, double fovDelta)
        {
            EffectAffineMat = effectAffineMat;
            FovDelta = fovDelta;
        }
    }
}