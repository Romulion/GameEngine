using System;
using BulletSharp;
using BulletSharp.Math;

namespace Toys
{
	public class Joint
	{
		public JointContainer JointParameters { get; private set; }
		public TypedConstraint Constraint { get; private set; }

		public Joint(JointContainer joint, RigidBodyBone[] rbodies)
		{
			JointParameters = joint;
			Instalize(rbodies);
		}

		void Instalize(RigidBodyBone[] rbodies)
		{
			//Matrix JointPos = Matrix.RotationYawPitchRoll(jcon.Rotation.Y, jcon.Rotation.X, jcon.Rotation.Z);
			//JointPos *= Matrix.Translation(GetVec3(jcon.Position));

			//look for no second body
			RigidBody body1 = null;
			RigidBody body2 = null;

			try
			{
				body1 = rbodies[JointParameters.RigitBody1].Body;

				try
				{
					body2 = rbodies[JointParameters.RigitBody2].Body;
				}
				catch (IndexOutOfRangeException) { }
			}
			catch (IndexOutOfRangeException)
			{
				try
				{
					body1 = rbodies[JointParameters.RigitBody2].Body;
				}
				catch (IndexOutOfRangeException) { return; }
			}


			var jointSpace = Matrix.RotationYawPitchRoll(JointParameters.Rotation.Y, JointParameters.Rotation.X, JointParameters.Rotation.Z) * Matrix.Translation(GetVec3(JointParameters.Position));
			//Convert left to right coordinates
			var reverce = Matrix.Scaling(new Vector3(1, 1, -1));
			jointSpace = reverce * jointSpace * reverce;
			var temp1 = body1.WorldTransform;
			temp1.Invert();
			var conn1 = jointSpace * temp1;
			Matrix conn2 = Matrix.Identity;
			if (body2 != null)
			{
				//calculating joint arms space
				var temp2 = body2.WorldTransform;
				//temp2.M43 += 0.1f;
				//body2.WorldTransform = temp2;
				temp2.Invert();
				conn2 = jointSpace * temp2;

			}
			//conn2 = CleanLowValues(conn2);
			//conn1 = CleanLowValues(conn1);
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

					//Jitter to hight
					/*
					Generic6DofSpringConstraint jointSpring6 = null;
					if (body2 != null)
						jointSpring6 = new Generic6DofSpringConstraint(body1, body2, conn1, conn2,true);
					else 
						jointSpring6 = new Generic6DofSpringConstraint(body1, conn1,true);
					*/				
					//Has low stiffness limit, disabling explode bodys TODO: increse limit
					Generic6DofSpring2Constraint jointSpring6 = null;
					if (body2 != null)
						jointSpring6 = new Generic6DofSpring2Constraint(body1, body2, conn1, conn2);
					else
						jointSpring6 = new Generic6DofSpring2Constraint(body1, conn1);
					
					jointSpring6.AngularLowerLimit = GetVec3(JointParameters.RotMin);
					jointSpring6.AngularUpperLimit = GetVec3(JointParameters.RotMax);
					jointSpring6.LinearLowerLimit = GetVec3(JointParameters.PosMin);
					jointSpring6.LinearUpperLimit = GetVec3(JointParameters.PosMax);

					for (int i = 0; i < 3; i++)
					{
						if (JointParameters.PosSpring[i] != 0)
						{
							jointSpring6.EnableSpring(i, true);
							jointSpring6.SetStiffness(i, JointParameters.PosSpring[i], false);
							jointSpring6.SetDamping(i, 0.9f);
						}
					}

					for (int i = 0; i < 3; i++)
					{
						if (JointParameters.RotSpring[i] != 0)
						{
							jointSpring6.EnableSpring(i + 3, true);
							jointSpring6.SetStiffness(i + 3, JointParameters.RotSpring[i],false);
							jointSpring6.SetDamping(i, 0.1f);
						}
					}

					//jointSpring6.SetParam(ConstraintParam.Erp, 0.7f);
					//jointSpring6.SetParam(ConstraintParam.Cfm, 0.0f);
					jointSpring6.SetEquilibriumPoint();
                    Constraint = jointSpring6;
                    break;
			}



        }


		private Vector3 GetVec3(OpenTK.Mathematics.Vector3 vec3)
		{
			return new Vector3(vec3.X, vec3.Y, vec3.Z);
		}


		
		float dec2rad(int grad)
		{
			return ((float)grad / 180 * (float)Math.PI);
		}


		/// <summary>
		/// Clean matrix elements lower than 10e-3
		/// To prevent bugs
		/// </summary>
		/// <param name="mat"></param>
		Matrix CleanLowValues(Matrix mat)
        {
			for (int i = 0; i < 16; i++)
				if (MathF.Abs(mat[i]) < 0.001)
					mat[i] = 0;
			return mat;
		}
	}
}
