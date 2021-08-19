using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Mathematics;

namespace Toys
{
    public class MathHelper
    {
        /// <summary>
        /// Represent vector as direction in scperical angles
        /// </summary>
        /// <param name="vector"></param>
        /// <returns>vector2 x - phi, y - theta in radians</returns>
        public static Vector2 ConvertVector2SphereAngles(Vector3 vector)
        {
            var result = Vector2.Zero;
            result.X = MathF.Atan2(vector.Z, vector.X);
            result.Y = MathF.Acos(vector.Y / vector.Xzy.Length);
            //           result.Z = vector.LengthFast;
            return result;
        }

        public static float ConvertGrad2Radians(float degrees)
        {
            return MathF.PI * degrees / 180.0f;
        }
        public static float ConvertRadians2Grad(float degrees)
        {
            return  degrees * 180.0f / MathF.PI;
        }
    }
}
