using System.Numerics;

namespace Silicon.CameraEffect
{
    internal interface ICameraEffect
    {
        void IncrementDelta(double dt);
        ProjectiveEffector GetEffector();
    }
}