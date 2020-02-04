using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace Toys
{
    /// <summary>
    /// Triangle cell on navigation map
    /// </summary>
    public class NavigationCell
    {
        public Vector3[] NodeVertex { get; private set; }
        public Vector3[] Edges { get; private set; }
        public NavigationCell[] linkedCells { get; private set; }

        public int Index { get; internal set; }
        public Vector3 Center { get; private set; }
        public NavigationCell(Vector3 node1, Vector3 node2, Vector3 node3)
        {
            NodeVertex = new Vector3[] { node1 , node2 , node3};
            linkedCells = new NavigationCell[3];
            Edges = new Vector3[] { node2 - node1, node3 - node2, node1 - node3 };
            Center = CalculateCentre();
        }

        NavigationCell() { }

        public bool IsInside(Vector3 point)
        {
            float d1, d2, d3;
            bool has_neg, has_pos;

            d1 = Sign(point.Xz, NodeVertex[0].Xz, NodeVertex[1].Xz);
            d2 = Sign(point.Xz, NodeVertex[1].Xz, NodeVertex[2].Xz);
            d3 = Sign(point.Xz, NodeVertex[2].Xz, NodeVertex[0].Xz);

            has_neg = (d1 < 0) || (d2 < 0) || (d3 < 0);
            has_pos = (d1 > 0) || (d2 > 0) || (d3 > 0);

                return !(has_neg && has_pos);
        }

        public NavigationCell Copy()
        {
            var navcell = new NavigationCell();
            navcell.NodeVertex = NodeVertex;
            navcell.Edges = Edges;
            navcell.linkedCells = linkedCells;
            navcell.Center = Center;
            return navcell;
        }

        float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            return (p1.X - p3.X) * (p2.Y - p3.Y) - (p2.X - p3.X) * (p1.Y - p3.Y);
        }

        Vector3 CalculateCentre()
        {
            return (NodeVertex[0] + NodeVertex[1] + NodeVertex[2]) / 3;
        }
    }
}
