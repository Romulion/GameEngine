using System;
using BulletSharp;
using BulletSharp.Math;

namespace Toys
{
	public class Joint
	{
		JointContainer jcon;
		public TypedConstraint joint { get; private set; }

		public Joint(JointContainer jc, RigitBodyBone[] rbodies)
		{
			jcon = jc;
			Instalize(rbodies);
		}

		void Instalize(RigitBodyBone[] rbodies)
		{
			Matrix JointPos = Matrix.RotationYawPitchRoll(jcon.Rotation.X, jcon.Rotation.Y, jcon.Rotation.Z);
			JointPos *= Matrix.Translation(GetVec3(jcon.Position));
			var Body1 = rbodies[jcon.RigitBody1].Body;
			var Body2 = rbodies[jcon.RigitBody2].Body;

			switch (jcon.jType)
			{
				case JointType.ConeTwist:
					var jointCone= new ConeTwistConstraint(Body1, Body2, JointPos, JointPos);
					break;
				case JointType.SpringSixDOF: //the only one used
					var jointSpring6 = new Generic6DofSpring2Constraint(Body1, Body2, JointPos, JointPos);
					jointSpring6.AngularLowerLimit = GetVec3(jcon.RotMin);
					jointSpring6.AngularUpperLimit = GetVec3(jcon.RotMax);
					jointSpring6.LinearLowerLimit = GetVec3(jcon.PosMin);
					jointSpring6.LinearLowerLimit = GetVec3(jcon.PosMax);
					//jointSpring6.
					break;
			}
			
		}


		private Vector3 GetVec3(OpenTK.Vector3 vec3)
		{
			return new Vector3(vec3.X, vec3.Y, vec3.Z);
		}
	}
}
