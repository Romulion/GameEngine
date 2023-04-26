using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Mathematics;
using Toys;
using System.IO;

namespace ModelViewer
{
    /// <summary>
    /// Original idea from
    /// https://www.geeksforgeeks.org/how-to-check-if-a-given-point-lies-inside-a-polygon/  
    /// </summary>
    class Zone
    {
        private class Line
        {
            public Vector2 p1, p2;
            public Line(Vector2 p1, Vector2 p2)
            {
                this.p1 = p1;
                this.p2 = p2;
            }
        }

        public string Name { get; private set; }
        public Vector2[] points;
        private Zone()
        {

        }

        bool OnLine(Line l1, Vector2 p)
        {
            // Check whether p is on the line or not
            if (p.X <= Math.Max(l1.p1.X, l1.p2.X)
                && p.X <= Math.Min(l1.p1.X, l1.p2.X)
                && (p.Y <= Math.Max(l1.p1.Y, l1.p2.Y)
                    && p.Y <= Math.Min(l1.p1.Y, l1.p2.Y)))
                return true;

            return false;
        }

        int Direction(Vector2 a, Vector2 b, Vector2 c)
        {
            float val = (b.Y - a.Y) * (c.X - b.X)
              - (b.X - a.X) * (c.Y - b.Y);

            if (val == 0)

                // Collinear
                return 0;

            else if (val < 0)

                // Anti-clockwise direction
                return 2;

            // Clockwise direction
            return 1;
        }

        bool IsIntersect(Line l1, Line l2)
        {
            // Four direction for two lines and points of other line
            int dir1 = Direction(l1.p1, l1.p2, l2.p1);
            int dir2 = Direction(l1.p1, l1.p2, l2.p2);
            int dir3 = Direction(l2.p1, l2.p2, l1.p1);
            int dir4 = Direction(l2.p1, l2.p2, l1.p2);

            // When intersecting
            if (dir1 != dir2 && dir3 != dir4)
                return true;

            // When p2 of line2 are on the line1
            if (dir1 == 0 && OnLine(l1, l2.p1))
                return true;

            // When p1 of line2 are on the line1
            if (dir2 == 0 && OnLine(l1, l2.p2))
                return true;

            // When p2 of line1 are on the line2
            if (dir3 == 0 && OnLine(l2, l1.p1))
                return true;

            // When p1 of line1 are on the line2
            if (dir4 == 0 && OnLine(l2, l1.p2))
                return true;

            return false;
        }

        public bool CheckInside(Vector2 p)
        {

            // When polygon has less than 3 edge, it is not polygon
            if (points == null || points.Length < 3)
                return false;

            // Create a point at infinity, y is same as point p
            Vector2 pt = new Vector2(9999, p.Y);
            Line exline = new Line(p, pt);
            int count = 0;
            int i = 0;
            do
            {

                // Forming a line from two consecutive points of
                // poly
                Line side = new Line(points[i], points[(i + 1) % points.Length]);
                if (IsIntersect(side, exline))
                {

                    // If side is intersects exline
                    if (Direction(side.p1, p, side.p2) == 0)
                        return OnLine(side, p);
                    count++;
                }
                i = (i + 1) % points.Length;
            } while (i != 0);

            // When count is odd
            return (count & 1) >= 1;
        }

        public static List<Zone> LoadFromStream(Stream objStream)
        {
            var res = new List<Zone>();
            var reader = new StreamReader(objStream);
            string line;
            Zone zone = new Zone();
            List<Vector2> verts = new List<Vector2>() ;
            while(!reader.EndOfStream)
            {
                line = reader.ReadLine();
                if (line.StartsWith("o"))
                {
                    zone = new Zone();
                    zone.Name = line.Substring(2);
                    res.Add(zone);
                }
                else if (line.StartsWith("v"))
                {
                    var vecStr = line.Substring(2).Split(' ');
                    verts.Add(new Vector2(Single.Parse(vecStr[0]), Single.Parse(vecStr[2])));
                }
                else if (line.StartsWith("f"))
                {
                    var indxs = ParceInt(line.Substring(2));
                    zone.points = new Vector2[indxs.Length];
                    for (int i = 0; i < indxs.Length; i++)
                        zone.points[i] = verts[indxs[i] - 1];
                }
            }

            return res;
        }

        static int[] ParceInt(string line)
        {

            var subStr = line.Split(" ");
            var res = new int[subStr.Length];
            for(int i = 0; i < subStr.Length; i++)
            {
                res[i] = Int32.Parse(subStr[i]);
            }

            return res;
        }
    }
}
