using System;
using System.Collections.Generic;
using System.Text;
using BulletSharp;

namespace Toys.Physics
{
    public class RigidBodySphere: RigidBodyComponent
    {
        float radius;

        public RigidBodySphere(float radius)
        {
            Size = radius;
        }

        public float Size
        {
            get { return radius; }
            set
            {
                radius = value;
                Resize();
            }
        }

        void Resize()
        {
            shape = new SphereShape(radius);
            RecalculateInertia(body != null);
        }
    }
}
