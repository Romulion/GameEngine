using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

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
            for (int i = 0; i < faces; i++)
            {
                int offset = i * 3;
                faceNormals[i] = CalcualteFaceNormal(vers[idexes[offset]].Position, vers[idexes[offset+1]].Position, vers[idexes[offset+2]].Position);
            }
            
            for (int i = 0; i < vers.Length; i++)
            {
                Vector3 normal = Vector3.Zero;
                for (int n = 0; n < idexes.Length; n++)
                {
                    if (idexes[n] == i)
                        normal += faceNormals[i / 3];
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
    }
}
