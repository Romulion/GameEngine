using System;
using System.Collections.Generic;
using System.Text;
//using OpenTK.Mathematics;
using BulletSharp;
using BulletSharp.Math;

namespace Toys.Physics
{
    public class RigitBodyBox : RigidBodyComponent
    {
        OpenTK.Mathematics.Vector3 size = OpenTK.Mathematics.Vector3.One;

        public RigitBodyBox()
        { 
            Resize(Vector3.One);

        }

        public OpenTK.Mathematics.Vector3 Size
        {
            get{ return size; }
            set
            {
                size = value;
                Resize(new Vector3(size.X, size.Y, size.Z));
            }
        }

        void Resize(Vector3 dims)
        {
            dims /= 2;
            shape = new BoxShape(dims);
            RecalculateInertia(body != null);
        }
    }
}
