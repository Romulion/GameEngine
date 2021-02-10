using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

namespace Toys
{
    public static class Heuristic
    {
        public static float ManhattanDistance(Vector2 point1, Vector2 point2)
        {
            return (point2- point1).LengthFast;
        }

        public static float DiagonalDistance()
        {
            return 0;
        }

        public static float EuclideanDistance(Vector2 point1, Vector2 point2)
        {
            return Vector2.Distance(point1, point2);
        }

        public static float EuclideanDistance(Vector3 point1, Vector3 point2)
        {
            return Vector3.Distance(point1, point2);
        }
    }
}
