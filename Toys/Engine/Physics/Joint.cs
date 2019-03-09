using System;
using BulletSharp;
using BulletSharp.Math;

namespace Toys
{
	public class Joint
	{
		public JointContainer jcon { get; private set; }
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

            //calculating joi9nt arms space
            var temp1 = Body1.WorldTransform;
            var temp2 = Body2.WorldTransform;
            //temp1.Invert();
            //temp2.Invert();
            temp1 = RigitBodyBone.GetMat(RigitBodyBone.GetMat(temp1).Inverted());
            temp2 = RigitBodyBone.GetMat(RigitBodyBone.GetMat(temp2).Inverted());

            var JointSpace = Matrix.RotationYawPitchRoll(jcon.Rotation.X, jcon.Rotation.Y, jcon.Rotation.Z) * Matrix.Translation(GetVec3(jcon.Position));
            //JointSpace = RigitBodyBone.GetMat(RigitBodyBone.GetMat(JointSpace).Inverted());
            //var JointSpace = Matrix.Translation(GetVec3(jcon.Position));
            //var Conn1 = Matrix.Translation(GetVec3(jcon.Position) - GetVec3(rbodies[jcon.RigitBody1].rigCon.Position));
            //var Conn2 = Matrix.Translation(GetVec3(jcon.Position) - GetVec3(rbodies[jcon.RigitBody2].rigCon.Position));
            var Conn1 = JointSpace * temp1;
            var Conn2 = JointSpace * temp2;

            switch (jcon.jType)
			{
				case JointType.ConeTwist:
					var jointCone= new ConeTwistConstraint(Body1, Body2, Conn1, Conn2);
					break;
				case JointType.SpringSixDOF: //the only one used

                    
                    var jointSpring6 = new Generic6DofSpring2Constraint(Body1, Body2, Conn1, Conn2);

                    jointSpring6.AngularLowerLimit = GetVec3(jcon.RotMin);
					jointSpring6.AngularUpperLimit = GetVec3(jcon.RotMax);
                    
                    jointSpring6.LinearLowerLimit = GetVec3(jcon.PosMin);
					jointSpring6.LinearUpperLimit = GetVec3(jcon.PosMax);

                    //jointSpring6.SetEquilibriumPoint();
                    //jointSpring6.BreakingImpulseThreshold = -1f;
                    //jointSpring6.
                    joint = jointSpring6;
                    //if (jcon.Name == "左うさみみ1")
                    //{
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
                     //}

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
