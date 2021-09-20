using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Mathematics;

namespace Toys
{
    public delegate void CollisionCallback(CollisionData data);
    public struct CollisionData
    {
        public readonly float Impulse;
        public readonly BulletSharp.CollisionObject Body;

        internal CollisionData(BulletSharp.CollisionObject body, float impulse)
        {
            Impulse = impulse;
            Body = body;
        }
    }
}
