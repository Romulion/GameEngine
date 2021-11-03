using System;
using System.Collections.Generic;
using System.Text;
using BulletSharp;
using BulletSharp.Math;

namespace Toys.Physics
{
    public class Generic6DofSpring2Constraint : ConstraintBase
    {

        public Generic6DofSpring2Constraint(RigidBodyComponent rigidBody1, RigidBodyComponent rigidBody2 = null)
        {
			/*
			var jointSpace = Matrix.RotationYawPitchRoll(JointParameters.Rotation.Y, JointParameters.Rotation.X, JointParameters.Rotation.Z) * Matrix.Translation(GetVec3(JointParameters.Position));

			//Convert left to right coordinates
			var reverce = Matrix.Scaling(new Vector3(1, 1, -1));
			jointSpace = reverce * jointSpace * reverce;

			var temp1 = body1.WorldTransform;
			temp1.Invert();
			var conn1 = jointSpace * temp1;
			//var Conn1 = Matrix.Translation(GetVec3(rbodies[jcon.RigitBody1].rigCon.Position - jcon.Position));

			Matrix conn2 = Matrix.Identity;
			if (body2 != null)
			{
				//calculating joint arms space
				var temp2 = body2.WorldTransform;
				temp2.Invert();
				conn2 = jointSpace * temp2;
			}
			switch (JointParameters.Type)
			{
				case JointType.ConeTwist:
					ConeTwistConstraint jointCone = null;
					if (body2 != null)
						jointCone = new ConeTwistConstraint(body1, body2, conn1, conn2);
					else
						jointCone = new ConeTwistConstraint(body1, conn1);

					break;
				case JointType.SpringSixDOF: //the only one used
					Generic6DofSpring2Constraint jointSpring6 = null;
					if (body2 != null)
						jointSpring6 = new Generic6DofSpring2Constraint(body1, body2, conn1, conn2);
					else
						jointSpring6 = new Generic6DofSpring2Constraint(body1, conn1);

					jointSpring6.AngularLowerLimit = GetVec3(JointParameters.RotMin);
					jointSpring6.AngularUpperLimit = GetVec3(JointParameters.RotMax);

					jointSpring6.LinearLowerLimit = GetVec3(JointParameters.PosMin);
					jointSpring6.LinearUpperLimit = GetVec3(JointParameters.PosMax);


					jointSpring6.EnableSpring(0, true);
					jointSpring6.EnableSpring(1, true);
					jointSpring6.EnableSpring(2, true);
					jointSpring6.SetStiffness(0, JointParameters.PosSpring.X);
					jointSpring6.SetStiffness(1, JointParameters.PosSpring.Y);
					jointSpring6.SetStiffness(2, JointParameters.PosSpring.Z);

					//TODO fix not working rotation spring
					jointSpring6.EnableSpring(3, true);
					jointSpring6.EnableSpring(4, true);
					jointSpring6.EnableSpring(5, true);
					jointSpring6.SetStiffness(3, JointParameters.RotSpring.X);
					jointSpring6.SetStiffness(4, JointParameters.RotSpring.Y);
					jointSpring6.SetStiffness(5, JointParameters.RotSpring.Z);

					/*
					jointSpring6.SetBounce(3, JointParameters.RotSpring.X);
					jointSpring6.SetBounce(4, JointParameters.RotSpring.Y);
					jointSpring6.SetBounce(5, JointParameters.RotSpring.Z);
					*/
					/*
					jointSpring6.SetDamping(0, 1f);
					jointSpring6.SetDamping(1, 1f);
					jointSpring6.SetDamping(2, 1f);
					jointSpring6.SetDamping(3, 1f);
					jointSpring6.SetDamping(4, 1f);
					jointSpring6.SetDamping(5, 1f);
					*/
					//jointSpring6.SetEquilibriumPoint();
					//Constraint = jointSpring6;
			
			}

    }
}
