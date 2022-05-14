using System;
using System.Collections.Generic;
using System.Text;
using BulletSharp;
using BulletSharp.Math;

namespace Toys.Physics
{
    public class ConstraintBase : Resource
    {
        internal TypedConstraint Constraint;
        protected ConstraintBase() { }
        protected override void Unload()
        {
           
        }
    }
}
