using System;
using System.Collections.Generic;
using System.Text;
using BulletSharp;

namespace Toys.Physics
{
    public class RigidBodyCapsule : RigidBodyComponent
    {
        OpenTK.Mathematics.Vector2 size;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="size">radius, height</param>
        public RigidBodyCapsule(OpenTK.Mathematics.Vector2 size)
        {
            Size = size;
        }

        /// <summary>
        /// Vector2 radius, height
        /// </summary>
        public OpenTK.Mathematics.Vector2 Size
        {
            get { return size; }
            set
            {
                size = value;
                Resize();
            }
        }

        void Resize()
        {
            shape = new CapsuleShape(size.X, size.Y);
            RecalculateInertia(body != null);
        }
    }
}
