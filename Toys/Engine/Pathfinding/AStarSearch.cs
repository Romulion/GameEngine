using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace Toys
{
    public class AStarSearch
    {
        NavigationMesh navMesh;
        Dictionary<NavigationCell, float> costTable = new Dictionary<NavigationCell, float>();
        Dictionary<NavigationCell, NavigationCell> parent = new Dictionary<NavigationCell, NavigationCell>();
        public AStarSearch(NavigationMesh navigation)
        {
            navMesh = navigation;
        }

        public NavigationCell[] CalculatePath(Vector3 start, Vector3 finish)
        {
            costTable.Clear();
            parent.Clear();
            NavigationCell startCell = navMesh.GetCellFromPosition(start);
            NavigationCell finishCell = navMesh.GetCellFromPosition(finish);
            if (startCell == null || finishCell == null)
                return null;
            var frontier = new PriorityQueue<NavigationCell>();
            frontier.Enqueue(startCell, 0);

            parent[startCell] = startCell;
            costTable[startCell] = 0;
            while (frontier.Count > 0)
            {
                var current = frontier.Dequeue();
                if (current.Equals(finishCell))
                {
                    break;
                }
                foreach (var next in current.linkedCells)
                {
                    //skip border edges
                    if (next == null)
                        continue;
                    //cost as distance between mesh centres
                    float newCost = costTable[current] + Heuristic.EuclideanDistance(next.Center, current.Center);
                    if (!costTable.ContainsKey(next) || newCost < costTable[next])
                    {
                        costTable[next] = newCost;
                        float priority = newCost + Heuristic.EuclideanDistance(next.Center,finish);
                        frontier.Enqueue(next, priority);
                        parent[next] = current;
                    }
                }
                
            }

            //path not found
            if (!parent.ContainsKey(finishCell))
                return new NavigationCell[0];

            var cell = finishCell;
            var path = new List<NavigationCell>();
            path.Add(finishCell);
            while (cell != startCell)
            {
                cell = parent[cell];
                path.Add(cell);
            }
            path.Reverse();
            return path.ToArray();
        }
    }
}
