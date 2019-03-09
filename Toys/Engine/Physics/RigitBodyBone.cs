using System.Runtime.InteropServices;
using BulletSharp;
using BulletSharp.Math;
using System;

namespace Toys
{
	public enum PhysPrimitiveType
	{
		Sphere,
		Box,
		Capsule,
	}


	public class RigitBodyBone
	{
        Matrix startTransform;

		public int bone { get; set; }
		public RigidBody Body { get; set; }
        public AnimController acon { get; set; }
        public RigitContainer rigCon { get; set; }

        public RigitBodyBone(RigitContainer rcon)
		{
            rigCon = rcon;
            CollisionShape shape = null;
			switch (rcon.primitive)
			{
				case PhysPrimitiveType.Box:
					shape = new BoxShape(GetVec3(rcon.Size));
					break;
				case PhysPrimitiveType.Capsule:
					shape = new CapsuleShape(rcon.Size.X, rcon.Size.Y);
					break;
				case PhysPrimitiveType.Sphere:
					shape = new SphereShape(rcon.Size.X);
					break;
			}

            startTransform = Matrix.RotationYawPitchRoll(rcon.Rotation.X, rcon.Rotation.Y, rcon.Rotation.Z) * Matrix.Translation(GetVec3(rcon.Position));
            if (rcon.Phys == PhysType.FollowBone)
                rcon.Mass = 0;

            //Body = LocalCreateRigidBody(rcon.Mass, startTransform, shape);

            
            //not working
            
            bool isDynamic = (rcon.Mass != 0.0f);

            Vector3 inertia = Vector3.Zero;
            if (isDynamic)
                shape.CalculateLocalInertia(rcon.Mass, out inertia);

            startTransform = Matrix.RotationYawPitchRoll(rcon.Rotation.X, rcon.Rotation.Y, rcon.Rotation.Z) * Matrix.Translation(GetVec3(rcon.Position));
            var rbInfo = new RigidBodyConstructionInfo(rcon.Mass, new DefaultMotionState(startTransform), shape, inertia);
			//rbInfo.MotionState = new DefaultMotionState(startTransform);
            Body = new RigidBody(rbInfo);
            rbInfo.Dispose();

            Body.ActivationState = ActivationState.DisableDeactivation;
            Body.Friction = rcon.Friction;
            Body.SetDamping(rcon.MassAttenuation, rcon.MassAttenuation);

            //Body.SetContactStiffnessAndDamping(0,0);
            //Body.SetContactStiffnessAndDamping(rcon.Repulsion, rcon.Repulsion);
            

        }


       

        public void SyncBone2Body(OpenTK.Matrix4 world)
		{
            //Console.WriteLine(111);
            //Console.WriteLine(Body.WorldTransform.Row4);
            //Console.WriteLine(acon.GetSkeleton[bone]);
            //Console.WriteLine(acon.GetBone(bone).localSpace);
            Body.WorldTransform = startTransform * GetMat(acon.GetSkeleton[bone]) * GetMat(world);
            //Console.WriteLine(Body.WorldTransform.Row4);
            //Console.WriteLine(222);
        }

		public void SyncBody2Bone(OpenTK.Matrix4 world)
		{
            

            var mat = world * GetMat(startTransform).Inverted() * GetMat(Body.WorldTransform);

            /*
            //math check
            Console.WriteLine(1111);
            if (bone == 221)
            {
                Console.WriteLine(GetMat(startTransform) * mat);
                //Console.WriteLine(GetMat(Body.WorldTransform).Row3);
                Console.WriteLine(GetMat(Body.WorldTransform));
            }
            */
           // Console.WriteLine(GetMat(Body.WorldTransform).Row3);

            acon.SetTransform(bone, mat);
        }


        private Vector3 GetVec3(OpenTK.Vector3 vec3)
		{
			return new Vector3(vec3.X, vec3.Y, vec3.Z);
		}

        /*
		private Matrix GetMat(OpenTK.Matrix4 mat)
		{
			return new Matrix(mat.M11, mat.M12, mat.M13, mat.M14, 
			                  mat.M21, mat.M22, mat.M23, mat.M24,
			                  mat.M31, mat.M32, mat.M33, mat.M34, 
			                  mat.M41, mat.M42, mat.M43, mat.M44);
		}
        */

		public static OpenTK.Matrix4 GetMat(Matrix mat)
		{
			return new OpenTK.Matrix4(mat.M11, mat.M12, mat.M13, mat.M14,
					  		  		  mat.M21, mat.M22, mat.M23, mat.M24,
					  		  		  mat.M31, mat.M32, mat.M33, mat.M34,
							  		  mat.M41, mat.M42, mat.M43, mat.M44);
		}

        public static Matrix GetMat(OpenTK.Matrix4 mat)
        {
            IntPtr ptr = Marshal.AllocHGlobal(64);
            Marshal.StructureToPtr(mat, ptr, false);
            Matrix mat1 = (Matrix) Marshal.PtrToStructure(ptr, typeof(Matrix));
            Marshal.FreeHGlobal(ptr);
            return mat1;
        }
    }
}
