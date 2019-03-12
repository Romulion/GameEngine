using System;
using BulletSharp;
using BulletSharp.Math;

namespace Toys
{
	public class Joint
	{
		public JointContainer jcon { get; private set; }
		public TypedConstraint joint { get; private set; }

		int NoBody = 255;

		public Joint(JointContainer jc, RigidBodyBone[] rbodies)
		{
			jcon = jc;
			Instalize(rbodies);
		}

		void Instalize(RigidBodyBone[] rbodies)
		{
			//Matrix JointPos = Matrix.RotationYawPitchRoll(jcon.Rotation.Y, jcon.Rotation.X, jcon.Rotation.Z);
			//JointPos *= Matrix.Translation(GetVec3(jcon.Position));

			//look for no second body
			RigidBody Body1 = null;
			RigidBody Body2 = null;

			try
			{
				Body1 = rbodies[jcon.RigitBody1].Body;

				try
				{
					Body2 = rbodies[jcon.RigitBody2].Body;
				}
				catch (IndexOutOfRangeException) { }
			}
			catch (IndexOutOfRangeException)
			{
				try
				{
					Body1 = rbodies[jcon.RigitBody2].Body;
				}
				catch (IndexOutOfRangeException) { return; }
			}


			var JointSpace = Matrix.RotationYawPitchRoll(jcon.Rotation.Y, jcon.Rotation.X, jcon.Rotation.Z) * Matrix.Translation(GetVec3(jcon.Position));
			var temp1 = Body1.WorldTransform;
			temp1.Invert();
			var Conn1 = JointSpace * temp1;

			Matrix Conn2 = Matrix.Identity;
			if (Body2 != null)
			{
				//calculating joi9nt arms space
				var temp2 = Body2.WorldTransform;
				temp2.Invert();
				Conn2 = JointSpace* temp2;

				//for the older version of BulletSharp
				//temp1 = RigitBodyBone.GetMat(RigitBodyBone.GetMat(temp1).Inverted());
				//temp2 = RigitBodyBone.GetMat(RigitBodyBone.GetMat(temp2).Inverted());

			}
            switch (jcon.jType)
			{
				case JointType.ConeTwist:
					ConeTwistConstraint jointCone = null;
					if (Body2 != null)
						jointCone = new ConeTwistConstraint(Body1, Body2, Conn1, Conn2);
					else
						jointCone = new ConeTwistConstraint(Body1, Conn1);
					
					break;
				case JointType.SpringSixDOF: //the only one used
					Generic6DofSpring2Constraint jointSpring6 = null;
					if (Body2 != null)
						jointSpring6 = new Generic6DofSpring2Constraint(Body1, Body2, Conn1, Conn2);
					else 
						jointSpring6 = new Generic6DofSpring2Constraint(Body1, Conn1);
					
                    jointSpring6.AngularLowerLimit = GetVec3(jcon.RotMin);
					jointSpring6.AngularUpperLimit = GetVec3(jcon.RotMax);
                    
                    jointSpring6.LinearLowerLimit = GetVec3(jcon.PosMin);
					jointSpring6.LinearUpperLimit = GetVec3(jcon.PosMax);

                    //jointSpring6.SetEquilibriumPoint();
                    //jointSpring6.BreakingImpulseThreshold = -1f;
                    //jointSpring6.
                    joint = jointSpring6;
                   
                    /*
                    Console.WriteLine(jcon.Position);
                    Console.WriteLine(rbodies[jcon.RigitBody1].rigCon.Position);
                    Console.WriteLine(rbodies[jcon.RigitBody2].rigCon.Position);
                    //Console.WriteLine(Matrix.RotationYawPitchRoll(0, 0, -(float)(Math.PI / 4) * 5) * Matrix.Translation(-0.18f, -0.18f, 0));
                    */

                    //jointSpring6.DebugDrawSize = 2f;
                    /*
                    Console.WriteLine(111);
                    Console.WriteLine(Body1.WorldTransform);
                    Console.WriteLine(Body2.WorldTransform);
                    Console.WriteLine(Conn1);
                    Console.WriteLine(Conn2);
                    Console.WriteLine(jointSpring6.CalculatedTransformA.Row4);
                    Console.WriteLine(jointSpring6.CalculatedTransformA.Row4);
                    Console.WriteLine(222);
                    */
                    break;
			}



        }


		private Vector3 GetVec3(OpenTK.Vector3 vec3)
		{
			return new Vector3(vec3.X, vec3.Y, vec3.Z);
		}
	}
}
