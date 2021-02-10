using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

namespace Toys
{
    class CalculateVertNormals
    {
        
        public static void CalculateNormals(Vertex3D[] vers, int[] idexes)
        {
            int faces = idexes.Length / 3;
            Vector3[] faceNormals = new Vector3[faces];
            for (int i = 0; i < faces; i++)
            {
                int offset = i * 3;
                faceNormals[i] = CalcualteFaceNormal(vers[idexes[offset]].Position, vers[idexes[offset + 1]].Position, vers[idexes[offset + 2]].Position);
            }

            for (int i = 0; i < vers.Length; i++)
            {
                Vector3 normal = Vector3.Zero;
                for(int n = 0; n < idexes.Length; n++)
                {
                    if (idexes[n] == i)
                        normal += faceNormals[i / 3];
                }
                vers[i].Normal = normal.Normalized();
            }
        }

        public static void CalculateNormals(VertexRigged3D[] vers, int[] idexes)
        {
            int faces = (idexes.Length) / 3;
            Vector3[] faceNormals = new Vector3[faces];
            float[] vertFaceNormalWeigth = new float[idexes.Length];
            for (int i = 0; i < faces; i++)
            {
                int offset = i * 3;
                Vector3 ver1 = vers[idexes[offset]].Position;
                Vector3 ver2 = vers[idexes[offset+1]].Position;
                Vector3 ver3 = vers[idexes[offset+2]].Position;
                faceNormals[i] = CalcualteFaceNormal(ver1, ver2, ver3);

                vertFaceNormalWeigth[offset] = CalcualteFaceNormalWeigth(ver3, ver1, ver2);
                vertFaceNormalWeigth[offset+1] = CalcualteFaceNormalWeigth(ver1, ver2, ver3);
                vertFaceNormalWeigth[offset+2] = CalcualteFaceNormalWeigth(ver2, ver3, ver1);
            }

            for (int i = 0; i < vers.Length; i++)
            {
                Vector3 normal = Vector3.Zero;
                List<Vector3> normals = new List<Vector3>();
                List<float> normalWeigth = new List<float>();
                for (int n = 0; n < idexes.Length; n++)
                {
                    if (idexes[n] == i)
                    {
                        normalWeigth.Add(vertFaceNormalWeigth[n]);
                        normals.Add(faceNormals[n / 3]);
                        //normal += faceNormals[i / 3];
                    }
                }
                float totalWeigth = normalWeigth.Sum();

                //weghtning normals
                for (int n = 0; n < normals.Count; n++)
                {
                    normal += normals[n] * (normalWeigth[n] / totalWeigth);
                    //normal += normals[n];
                }

                vers[i].Normal = normal.Normalized();
            }
        }

        static Vector3 CalcualteFaceNormal(Vector3 vec1, Vector3 vec2, Vector3 vec3)
        {
            Vector3 edge1 = vec3 - vec2;
            Vector3 edge2 = vec1 - vec2;
            return Vector3.Cross(edge1, edge2);
        }

        static float CalcualteFaceNormalWeigth(Vector3 vec1, Vector3 vec2, Vector3 vec3)
        {
            Vector3 edge1 = vec3 - vec2;
            Vector3 edge2 = vec1 - vec2;
            edge1.Normalize();
            edge2.Normalize();
            return (float)Math.Acos(Vector3.Dot(edge1, edge2));
        }
    }
}
