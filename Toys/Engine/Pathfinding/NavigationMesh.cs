using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace Toys
{
    public class NavigationMesh
    {
        public NavigationCell[] navigationCells { get; private set; }

        public NavigationMesh(Vertex3D[] mesh, int[] indexes)
        {
            navigationCells = new NavigationCell[indexes.Length / 3];
            CreateActiveArea(mesh, indexes);
        }


        public NavigationCell GetCellFromPosition(Vector3 position)
        {
            for (int i = 0; i < navigationCells.Length; i++)
                if (navigationCells[i].IsInside(position))
                    return navigationCells[i];
            return null;
        }

        void CreateActiveArea(Vertex3D[] mesh, int[] indexes)
        {
            //cells
            for (int i = 0; i < navigationCells.Length; i++)
            {
                navigationCells[i] = new NavigationCell(mesh[indexes[i * 3]].Position, mesh[indexes[i * 3 + 1]].Position, mesh[indexes[i * 3 + 2]].Position);
                navigationCells[i].Index = i;
            }

            //near cells
            for (int i = 0; i < navigationCells.Length; i++)
            {
                var connection = FindConnectedVertices(i * 3, indexes);
                for (int n = 0; n < 3; n++)
                {
                    if (connection[n] == -1)
                        continue;
                        
                    navigationCells[i].linkedCells[n] = navigationCells[connection[n]];
                }
            }
        }

        //one edge can have maximum 2 triangles
        int[] FindConnectedVertices(int offset, int[] indexes)
        {
            int[] connection = {-1, -1, -1};
            int[][] edges = {
                new int[] { indexes[offset], indexes[offset + 1] } ,
                new int[] { indexes[offset + 1], indexes[offset + 2] },
                new int[] { indexes[offset + 2], indexes[offset] }
            };
            for (int i = 0; i < indexes.Length; i += 3)
            {
                //skip this triangle
                if (i == offset)
                    continue;

                //check all combinations
                
                for (int n = 0; n < 3; n++)
                {
                    bool contain = false;
                    for (int m = 0; m < 3; m++)
                    {   
                        if (indexes[i + m] == edges[n][0] || indexes[i + m] == edges[n][1])
                        {
                            if (!contain)
                            {
                                contain = true;
                            }
                            else
                            {
                                connection[n] = i / 3;
                            }
                        }
                    }
                }
            }

            return connection;
        }
    }
}
