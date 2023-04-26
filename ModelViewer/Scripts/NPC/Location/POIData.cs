using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;
using Toys;

namespace ModelViewer
{
    public struct POIData
    {
        public string Name { get; private set; }
        public Vector3 Direction { get; private set; }
        public Vector3 Position { get; private set; }

        public POIData(string name, Vector3 dir, Vector3 pos)
        {
            Name = name;
            Direction = dir;
            Position = pos;
        }
    }
}
