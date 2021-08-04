using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

namespace Toys
{
    public class NavigationAgent
    {
        AStarSearch aStar;
        NavigationMesh navigationMesh;


        int[] bindedIndexes;
        int[] notBindedEdges;
        Dictionary<Vector3, Vector3> nextBorderNode = new Dictionary<Vector3, Vector3>();
        //edge node sets
        int[][] vertexPairs = { new int[] { 0, 1 }, new int[] { 1, 2 }, new int[] { 2, 0 } };


        public float AgentSize = 0.2f;
        public NavigationCell[] pathMesh { get; private set; }

        public NavigationAgent(NavigationMesh navMesh)
        {
            navigationMesh = navMesh;
            aStar = new AStarSearch(navMesh);
        }

        public Task<Vector3[]> SearchPathAsync(Vector3 start, Vector3 goal)
        {
            return Task.Run(() => SearchPath(start, goal));
        }

        public Vector3[] SearchPath(Vector3 start, Vector3 goal)
        {
            nextBorderNode.Clear();
            pathMesh = aStar.CalculatePath(start, goal);
            if (pathMesh == null)
                return new Vector3[] { };
            //if point in one
            if (pathMesh.Length < 2)
                return new Vector3[] { goal };

            //fill path indexes
            bindedIndexes = new int[pathMesh.Length];
            for (int i = 0; i < pathMesh.Length; i++)
            {
                bindedIndexes[i] = pathMesh[i].Index;
            }
            //find linked border ondes
            int id = Array.IndexOf(pathMesh[0].linkedCells, pathMesh[1]);
            var pair = vertexPairs[id];
            var rightNode = pathMesh[0].NodeVertex[pair[0]];
            var leftNode = pathMesh[0].NodeVertex[pair[1]];
            //fill number of not binded edge for every path element
            notBindedEdges = new int[pathMesh.Length];
            for (int i = 0; i < pathMesh.Length; i++)
            {
                bool lb = false, rb = false;
                for (int n = 0; n < 3; n++)
                {
                    if (pathMesh[i].linkedCells[n] == null || !bindedIndexes.Contains(pathMesh[i].linkedCells[n].Index))
                    {
                        notBindedEdges[i] = n;
                        if (i > 0)
                        {

                            var vert1 = pathMesh[i].NodeVertex[vertexPairs[n][0]];
                            var vert2 = pathMesh[i].NodeVertex[vertexPairs[n][1]];
                            if (!rb && vert1 == rightNode)
                            {
                                rb = true;
                                nextBorderNode[rightNode] = vert2;
                                rightNode = vert2;
                            }
                            else if (!rb && vert2 == rightNode)
                            {
                                rb = true;
                                nextBorderNode[rightNode] = vert1;
                                rightNode = vert1;
                            }
                            else if (!lb && vert1 == leftNode)
                            {
                                lb = true;
                                nextBorderNode[leftNode] = vert2;
                                leftNode = vert2;
                            }
                            else if (!lb && vert2 == leftNode)
                            {
                                lb = true;
                                nextBorderNode[leftNode] = vert1;
                                leftNode = vert1;
                            }
                        }
                    }
                }
            }

            return OptimizePathNew(pathMesh, start, goal).ToArray();
        }

        //using funnel algorith
        List<Vector3> OptimizePath(NavigationCell[] path, Vector3 start, Vector3 end, int startPoint = 0)
        {
            List<Vector3> pathOptimized = new List<Vector3>();
            Vector3[] rightNodeBacklog = new Vector3[3];
            Vector3[] leftNodeBacklog = new Vector3[3];


            List<Vector3> leftBoundary = new List<Vector3>();
            List<Vector3> rightBoundary = new List<Vector3>();

            var startNode = path[startPoint];

            //find linked edge
            int id = Array.IndexOf(startNode.linkedCells, path[startPoint + 1]);
            var pair = vertexPairs[id];

            rightNodeBacklog[2] = startNode.NodeVertex[pair[0]];
            leftNodeBacklog[2] = startNode.NodeVertex[pair[1]];


            //for storaging prev vector
            bool leftBlock = false, rightBlock = false;
            Vector3 rightVec = rightNodeBacklog[2] - start,
                    leftVec = leftNodeBacklog[2] - start;

            var referenceVector = Vector3.Cross(leftVec, rightVec);
            for (int i = startPoint + 1; i < path.Length; i++)
            {
                
                //if last node exit loop
                if (i == path.Length - 1)
                {
                    pathOptimized.Add(end);
                    break;
                }

                int[] pairSet = vertexPairs[notBindedEdges[i]];
                var node = path[i];

                //left side
                if (!rightBlock && (node.NodeVertex[pairSet[0]] == rightNodeBacklog[2] || node.NodeVertex[pairSet[1]] == rightNodeBacklog[2]))
                {
                    //rotate edges log
                    rightNodeBacklog[0] = rightNodeBacklog[1];
                    rightNodeBacklog[1] = rightNodeBacklog[2];
                    Console.WriteLine("left {0}", i);
                    //move to next point
                    rightNodeBacklog[2] = nextBorderNode[rightNodeBacklog[2]];

                    //check inside
                    rightVec = rightNodeBacklog[2] - start;
                    var result = Vector3.Dot(referenceVector, Vector3.Cross(rightVec, rightNodeBacklog[1]- start));
                    if (result < 0)
                    {
                        rightBoundary.Add(rightNodeBacklog[2]);
                        rightVec = rightNodeBacklog[1] - start;
                        Console.WriteLine("right block {0}", i);
                        rightBlock = true;
                    }

                }
                //right side
                else if (!leftBlock && (node.NodeVertex[pairSet[0]] == leftNodeBacklog[2] || node.NodeVertex[pairSet[1]] == leftNodeBacklog[2]))
                {
                    Console.WriteLine("right {0}", i);
                    //rotate edges log
                    leftNodeBacklog[0] = leftNodeBacklog[1];
                    leftNodeBacklog[1] = leftNodeBacklog[2];

                    //move to next point
                    leftNodeBacklog[2] = nextBorderNode[leftNodeBacklog[2]];

                    //check inside
                    leftVec = leftNodeBacklog[2] - start;
                    var result = Vector3.Dot(referenceVector, Vector3.Cross(leftNodeBacklog[1] - start, leftVec));
                    //path corner detected
                    if (result < 0)
                    {
                        leftBoundary.Add(leftNodeBacklog[2]);
                        leftVec = leftNodeBacklog[1] - start;
                        Console.WriteLine("left block {0}", i);
                        leftBlock = true;
                    }
                }
                //prevent blocking
                else if (rightBlock ^ leftBlock)
                    continue;
                //TODO resolve both sides blocking
                else
                {
                    //Console.WriteLine("{0} {1}", rightNode, leftNode);
                    Console.WriteLine("path tracing blocked on {0}", i);
                    break;
                }

                if (Vector3.Dot(referenceVector, Vector3.Cross(leftVec, rightVec)) < 0)
                {
                    Console.WriteLine("path break line {0}", i);
                    Console.WriteLine(leftVec);
                    Console.WriteLine(rightVec);
                    Console.WriteLine(Vector3.Dot(referenceVector, Vector3.Cross(leftVec, rightVec)));

                    float leftLength = leftVec.LengthSquared, rightLength = rightVec.LengthSquared;
                    if (rightBlock && rightLength < leftLength)
                    {
                        start = rightNodeBacklog[1] + (2* rightNodeBacklog[1] - rightNodeBacklog[0] - rightNodeBacklog[2]).Normalized() * AgentSize;
                    }
                    else if (leftBlock && rightLength >= leftLength)
                    {
                        start = leftNodeBacklog[1] + (2* leftNodeBacklog[1] - leftNodeBacklog[0] - leftNodeBacklog[2]).Normalized() * AgentSize;
                    }
                    //search next point
                    else if (rightLength < leftLength)
                    {
                        start = rightNodeBacklog[2] + (2 * rightNodeBacklog[2] - rightNodeBacklog[1] - nextBorderNode[rightNodeBacklog[2]]).Normalized() * AgentSize;
                    }
                    else
                    {
                        start = leftNodeBacklog[2] + (2 * leftNodeBacklog[2] - leftNodeBacklog[1] - nextBorderNode[leftNodeBacklog[2]]).Normalized() * AgentSize;
                    }

                    //find waypoint location
                    for (int n = 0; n < path.Length; n++)
                    {
                        if (path[n].IsInside(start))
                        {
                            i = n;
                            break;
                        }
                    }

                    Console.WriteLine("new point {0} {1}", start, i);
                    pathOptimized.Add(start);
                    pathOptimized.AddRange(OptimizePath(path,start,end, i));
                    
                    break;
                }

                
            }

            Console.WriteLine("Path:");
            foreach (var val in pathOptimized)
                Console.WriteLine(val);
            return pathOptimized;
        }


        List<Vector3> OptimizePathNew(NavigationCell[] path, Vector3 start, Vector3 end, int startPoint = 0)
        {
            List<Vector3> pathOptimized = new List<Vector3>();
            //Vector3[] rightNodeBacklog = new Vector3[3];
            //Vector3[] leftNodeBacklog = new Vector3[3];

            List<Vector3> leftBorder = new List<Vector3>();
            List<Vector3> rightBorder = new List<Vector3>();

            var startNode = path[startPoint];

            //find linked edge
            int id = Array.IndexOf(startNode.linkedCells, path[startPoint + 1]);
            var pair = vertexPairs[id];

            Vector3 leftNodeLast = startNode.NodeVertex[pair[1]];
            Vector3 rightNodeLast = startNode.NodeVertex[pair[0]];
            leftBorder.Add(start);
            leftBorder.Add(leftNodeLast);
            rightBorder.Add(start);
            rightBorder.Add(rightNodeLast);

            Vector3 rightVec = rightNodeLast - start,
                    leftVec = leftNodeLast - start;

            var referenceVector = Vector3.Cross(leftVec, rightVec);
            for (int i = startPoint + 1; i < path.Length - 1; i++)
            {

                int[] pairSet = vertexPairs[notBindedEdges[i]];
                var node = path[i];

                //Console.WriteLine("Node {0} {1} {2}", i, node.NodeVertex[pairSet[0]], node.NodeVertex[pairSet[1]]) ;

                //left side
                if (node.NodeVertex[pairSet[0]] == rightNodeLast || node.NodeVertex[pairSet[1]] == rightNodeLast)
                {
                    //move to next point
                    //Console.WriteLine("Right {0}", rightNodeLast);
                    rightNodeLast = nextBorderNode[rightNodeLast];
                    //check inside

                    rightVec = rightNodeLast - rightBorder[rightBorder.Count - 2];
                    var result = Vector3.Dot(referenceVector, Vector3.Cross(rightVec, rightBorder[rightBorder.Count - 1] - rightBorder[rightBorder.Count - 2]));
                    if (result < 0)
                    {
                        rightBorder.Add(rightNodeLast);
                        Console.WriteLine("right block {0}", i);
                        //rightBlock = true;
                    }
                    else
                    {
                        rightBorder[rightBorder.Count - 1] = rightNodeLast;
                    }

                    //check Intersect with right
                    for (int n = 1; n < leftBorder.Count; n++)
                    {
                        if (Vector3.Dot(referenceVector, Vector3.Cross(leftBorder[n] - leftBorder[n - 1], rightVec)) < 0)
                        {
                            rightBorder[rightBorder.Count - 2] = leftBorder[n];
                            pathOptimized.Add(leftBorder[n]);
                            leftBorder.RemoveRange(0, n);
                            break;
                        }
                    }

                }
                //right side
                else if (node.NodeVertex[pairSet[0]] == leftNodeLast || node.NodeVertex[pairSet[1]] == leftNodeLast)
                {
                    //move to next point
                    leftNodeLast = nextBorderNode[leftNodeLast];
                    //Console.WriteLine("Left {0}", leftNodeLast);
                    //check inside
                    leftVec = leftNodeLast - leftBorder[leftBorder.Count - 2];
                    var result = Vector3.Dot(referenceVector, Vector3.Cross(leftBorder[leftBorder.Count - 1] - leftBorder[leftBorder.Count - 2], leftVec));
                    //path corner detected
                    if (result < 0)
                    {
                        leftBorder.Add(leftNodeLast);
                        Console.WriteLine("left block {0}", i);
                    }
                    else
                    {
                        leftBorder[leftBorder.Count - 1] = leftNodeLast;
                    }

                    //check Intersect with left
                    for (int n = 1; n < rightBorder.Count; n++)
                    {
                        if (Vector3.Dot(referenceVector, Vector3.Cross(rightBorder[n] - rightBorder[n-1], leftVec)) > 0)
                        {
                            
                            leftBorder[leftBorder.Count - 2] = rightBorder[n];
                            pathOptimized.Add(rightBorder[n]);
                            rightBorder.RemoveRange(0, n);
                            break;
                        }
                    }
                }
            }


            //Finish corner mapping
            var strt = (pathOptimized.Count == 0) ? start : pathOptimized[pathOptimized.Count - 1];
            leftVec = end - strt;
            for (int n = 1; n < rightBorder.Count; n++)
            {
                if (Vector3.Dot(referenceVector, Vector3.Cross(rightBorder[n] - rightBorder[n - 1], leftVec)) > 0)
                    pathOptimized.Add(rightBorder[n]);
            }

            for (int n = 1; n < leftBorder.Count; n++)
            {
                if (Vector3.Dot(referenceVector, Vector3.Cross(leftBorder[n] - leftBorder[n - 1], leftVec)) < 0)
                    pathOptimized.Add(leftBorder[n]);
            }
            pathOptimized.Add(end);

            /*
            Console.WriteLine("Path:");
            foreach (var val in pathOptimized)
                Console.WriteLine(val);
            Console.WriteLine("Path end");
            
            Console.WriteLine("Path l:");
            foreach (var val in leftBorder)
                Console.WriteLine(val);
            Console.WriteLine("Path end");
            Console.WriteLine("Path r:");
            foreach (var val in rightBorder)
                Console.WriteLine(val);
            Console.WriteLine("Path end");
            */
            
            //Avoid corners
            if (pathOptimized.Count > 1)
                for (int m = 0; m < pathOptimized.Count - 1; m++)
                {
                    strt = (m == 0) ? start : pathOptimized[m - 1];
                    Vector3 direction = (strt - pathOptimized[m]).Normalized() + (pathOptimized[m + 1] - pathOptimized[m]).Normalized();
                    direction.Normalize();
                    pathOptimized[m] -= direction * AgentSize;
                }

            /*
            Console.WriteLine("Path corner:");
            foreach (var val in pathOptimized)
                Console.WriteLine(val);
            Console.WriteLine("Path end");
            */



            return pathOptimized;
        }
    }
}
