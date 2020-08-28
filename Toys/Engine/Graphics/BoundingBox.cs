using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace Toys
{
    public class BoundingBox
    {
        public Vector3 MaxModel;
        public Vector3 MinModel;

        Vector3 Max;
        Vector3 Min;
        Vector4[] plates = new Vector4[6];

        public BoundingBox()
        {
            MaxModel = Vector3.Zero;
            MinModel = Vector3.Zero;
        }

        public bool CheckIntersection(Vector4[] frustumPlanes)
        {

            bool inside = true;
            for (int i = 0; i < 6; i++)
            {

                float d = Math.Max(Min.X * frustumPlanes[i].X, Max.X * frustumPlanes[i].X)
                    + Math.Max(Min.Y * frustumPlanes[i].Y, Max.Y * frustumPlanes[i].Y)
                    + Math.Max(Min.Z * frustumPlanes[i].Z, Max.Z * frustumPlanes[i].Z)
                    + frustumPlanes[i].W;
                inside &= d > 0;
            }

            return inside;
        }

        public bool CheckFrustrumPresence(Matrix4 mvp)
        {
            Max = MaxModel;
            Min = MinModel;
            return CheckIntersection(ExtractFrustrumPlates(mvp));
        }


        Vector4[] ExtractFrustrumPlates(Matrix4 mat)
        {
            
            int sign = 1;
            int k = 0;
            for (int n = 0; n < 6; n++)
            {
                for (int i = 0; i < 4; i++)
                    plates[n][i] = mat[i,3] + sign * mat[i,k];

                if (sign < 0)
                {
                    k++;
                }
                sign *= -1;
            }

            return plates;
        }

        public static BoundingBox FromVertexes3d(Vertex3D[] vertices)
        {
            var box = new BoundingBox();
            for (int i = 0; i < vertices.Length; i++)
            {
                if (vertices[i].Position.X > box.MaxModel.X)
                    box.MaxModel.X = vertices[i].Position.X;
                if (vertices[i].Position.Y > box.MaxModel.Y)
                    box.MaxModel.Y = vertices[i].Position.Y;
                if (vertices[i].Position.Z > box.MaxModel.Z)
                    box.MaxModel.Z = vertices[i].Position.Z;
                if (vertices[i].Position.X < box.MinModel.X)
                    box.MinModel.X = vertices[i].Position.X;
                if (vertices[i].Position.Y < box.MinModel.Y)
                    box.MinModel.Y = vertices[i].Position.Y;
                if (vertices[i].Position.Z < box.MinModel.Z)
                    box.MinModel.Z = vertices[i].Position.Z;
            }

            box.Max = box.MaxModel;
            box.Min = box.MinModel;
            return box;
        }
    }
}
