using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace Toys
{
    public class NavigationAgent
    {
        AStarSearch aStar;
        NavigationMesh navigationMesh;
        int[] bindedIndexes;
        int[] notBindedEdges;
        public float AgentSize = 0.2f;
        public NavigationCell[] pathMesh { get; private set; }

        public NavigationAgent(NavigationMesh navMesh)
        {
            navigationMesh = navMesh;
            aStar = new AStarSearch(navMesh);
        }

        public Vector3[] SearchPath(Vector3 start, Vector3 goal)
        {
            pathMesh = aStar.CalculatePath(start, goal);
            if (pathMesh == null)
                return null;

            //if point in one
            if (pathMesh.Length < 2)
                return new Vector3[] { goal };

            //fill path indexes
            bindedIndexes = new int[pathMesh.Length];
            for (int i = 0; i < pathMesh.Length; i++)
            {
                bindedIndexes[i] = pathMesh[i].Index;
            }

            //fill number of not binded edge for every path element
            notBindedEdges = new int[pathMesh.Length];
            for (int i = 0; i < pathMesh.Length; i++)
            {
                for (int n = 0; n < 3; n++)
                {
                    if (pathMesh[i].linkedCells[n] == null || !bindedIndexes.Contains(pathMesh[i].linkedCells[n].Index))
                        notBindedEdges[i] = n;
                }
            }

            return OptimizePath(pathMesh, start, goal).ToArray();
        }

        //using funnel algorith
        List<Vector3> OptimizePath(NavigationCell[] path, Vector3 start, Vector3 end, int startPoint = 0)
        {
            List<Vector3> pathOptimized = new List<Vector3>();
            Vector3[] leftEdgeBacklog = new Vector3[2];
            Vector3[] rightEdgeBacklog = new Vector3[2];
            //edge node sets
            int[][] vertexPairs = { new int[] { 0, 1 }, new int[] { 1, 2 }, new int[] { 2, 0 } };

            var startNode = path[startPoint];
            //find linked edge
            int id = Array.IndexOf(startNode.linkedCells, path[startPoint + 1]);
            var pair = vertexPairs[id];

            Vector3 leftNode = startNode.NodeVertex[pair[0]],
                    rightNode = startNode.NodeVertex[pair[1]];


            //for storaging prev vector
            Vector3 prevValue;
            Console.WriteLine(start);
            bool leftBlock = false, rightBlock = false;
            Vector3 leftVec = leftNode - start,
                    rightVec = rightNode - start;

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
                var freeEdge = node.Edges[notBindedEdges[i]];

                Console.WriteLine(i);
                //left side
                if (!leftBlock && (node.NodeVertex[pairSet[0]] == leftNode || node.NodeVertex[pairSet[1]] == leftNode))
                {
                    //rotate edges log
                    if (leftEdgeBacklog[1] != null)
                        leftEdgeBacklog[0] = leftEdgeBacklog[1];
                    leftEdgeBacklog[1] = freeEdge;

                    prevValue = leftNode;

                    //move to next point
                    if (node.NodeVertex[pairSet[0]] == leftNode)
                        leftNode = node.NodeVertex[pairSet[1]];
                    else
                        leftNode = node.NodeVertex[pairSet[0]];

                    //check inside
                    leftVec = leftNode - start;
                    var result = Vector3.Dot(referenceVector, Vector3.Cross(leftVec, freeEdge));
                    if (result < 0)
                    {
                        leftNode = prevValue;
                        leftVec = leftNode - start;
                        Console.WriteLine("left block {0}", i);
                        leftBlock = true;
                    }

                }
                //right side
                else if (!rightBlock && (node.NodeVertex[pairSet[0]] == rightNode || node.NodeVertex[pairSet[1]] == rightNode))
                {
                    //rotate edges log
                   rightEdgeBacklog[1] = freeEdge;
                    if (rightEdgeBacklog[1] != null)
                        rightEdgeBacklog[0] = rightEdgeBacklog[1];

                    prevValue = rightNode;
                    //move to next point
                    if (node.NodeVertex[pairSet[0]] == rightNode)
                        rightNode = node.NodeVertex[pairSet[1]];
                    else
                        rightNode = node.NodeVertex[pairSet[0]];

                    //check inside
                    rightVec = rightNode - start;
                    var result = Vector3.Dot(referenceVector, Vector3.Cross(rightVec,freeEdge));
                    if (result < 0)
                    {
                        rightNode = prevValue;
                        rightVec = rightNode - start;
                        Console.WriteLine("right block {0}", i);
                        rightBlock = true;
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
                Console.WriteLine(i);
                Console.WriteLine("{0} {1}", leftVec, rightVec);
                Console.WriteLine(Vector3.Cross(leftVec, rightVec));
                Console.WriteLine(Vector3.Dot(referenceVector, Vector3.Cross(leftVec, rightVec)));
                if (Vector3.Dot(referenceVector, Vector3.Cross(leftVec, rightVec)) < 0)
                {
                    Console.WriteLine("path break line {0}", i);
                    
                    //
                    if (leftVec.LengthSquared < rightVec.LengthSquared)
                    {
                        start = leftNode + (leftEdgeBacklog[0] + leftEdgeBacklog[1]).Normalized() * AgentSize;
                    }
                    else
                    {
                        start = rightNode - (rightEdgeBacklog[0] + rightEdgeBacklog[1]).Normalized() * AgentSize;
                    }

                    for (int n = 0; n < path.Length; n++)
                    {
                        if (path[n].IsInside(start))
                        {
                            i = n;
                            Console.WriteLine(i);
                            break;
                        }
                    }

                    
                    pathOptimized.Add(start);
                    Console.WriteLine(start);
                    pathOptimized.AddRange(OptimizePath(path,start,end, i));
                    break;
                }

                
            }
            return pathOptimized;
        }
    }
}
