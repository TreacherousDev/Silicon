using System;
using System.Numerics;

namespace Silicon.Helpers
{
    internal static partial class Mathematical
    {
        internal static Matrix4x4 AffineTrs(
            Vector3 position,
            double pitch, double yaw, double roll
        )
        {
            Matrix4x4 rotation = Matrix4x4.CreateFromYawPitchRoll(
                pitch: (float)pitch,
                yaw: (float)yaw,
                roll: (float)roll
            );

            Matrix4x4 translate = Matrix4x4.CreateTranslation(position);

            return rotation * translate;
        }

        internal static Vector3 GetXYZ(this Matrix4x4 m)
        {
            return new Vector3(m.M41, m.M42, m.M43);
        }

        internal static Vector3 GetPitchRollYaw(this Matrix4x4 matrix)
        {
            float pitch, yaw, roll;

            if (Math.Abs(matrix.M32) < 1.0 - 1e-5)
            {
                pitch = (float)Math.Asin(-matrix.M32);
                roll = (float)Math.Atan2(matrix.M12, matrix.M22);
                yaw = (float)Math.Atan2(matrix.M31, matrix.M33);
            }
            else
            {
                pitch = (float)Math.PI / 2 * -Math.Sign(matrix.M32);
                roll = 0f;
                yaw = (float)Math.Atan2(-matrix.M13, matrix.M11);
            }

            Vector3 pry = new Vector3(pitch, roll, yaw);
            return pry;
        }

        public static Vector3 SphericalToCartesian(
            double centerX, double centerY, double centerZ,
            double pitch, double yaw, double radius
        )
        {
            double x = radius * Math.Sin(pitch) * Math.Cos(yaw);
            double y = radius * Math.Sin(pitch) * Math.Sin(yaw);
            double z = radius * Math.Cos(pitch);
            return new Vector3(
                (float)(x + centerX),
                (float)(y + centerY),
                (float)(z + centerZ)
            );
        }

        public static double ComputeCameraStateDistance(
            double x1, double y1, double z1, double pitch1, double yaw1,
            double x2, double y2, double z2, double pitch2, double yaw2,
            double cameraDistance
        )
        {
            Vector3 state1 = SphericalToCartesian(x1, y1, z1, pitch1, yaw1, cameraDistance);
            Vector3 state2 = SphericalToCartesian(x2, y2, z2, pitch2, yaw2, cameraDistance);

            double travelDistance = Vector3.Distance(state1, state2);

            return travelDistance;
        }
    }
}