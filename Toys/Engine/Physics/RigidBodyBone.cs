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


	public class RigidBodyBone
	{
        Matrix startTransform;

		public int BoneID { get; set; }
		public RigidBody Body { get; set; }
        public BoneController BoneController { get; set; }
        RigidContainer bodyContainer { get; set; }
		RigidBodyConstructionInfo rbInfo;
        public RigidBodyBone(RigidContainer rcon)
		{
            bodyContainer = rcon;
            CollisionShape shape = null;
			switch (rcon.PrimitiveType)
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

            if (rcon.Phys == PhysType.FollowBone)
                rcon.Mass = 0;
            
            bool isDynamic = (rcon.Mass != 0.0f);

            Vector3 inertia = Vector3.Zero;
            if (isDynamic)
                shape.CalculateLocalInertia(rcon.Mass, out inertia);
			startTransform = Matrix.RotationYawPitchRoll(rcon.Rotation.Y, rcon.Rotation.X, rcon.Rotation.Z) * Matrix.Translation(GetVec3(rcon.Position));
            rbInfo = new RigidBodyConstructionInfo(rcon.Mass, new DefaultMotionState(startTransform), shape, inertia);
			Body = new RigidBody(rbInfo);

            Body.ActivationState = ActivationState.DisableDeactivation;
            Body.Friction = rcon.Friction;
            Body.SetDamping(rcon.MassAttenuation, rcon.RotationDamping);
            Body.Restitution = rcon.Restitution;

            if (rcon.Phys == PhysType.FollowBone)
            {
                Body.SetCustomDebugColor(new Vector3(0, 1, 0));
                Body.CollisionFlags = Body.CollisionFlags | CollisionFlags.KinematicObject;
            }
            else if (rcon.Phys == PhysType.Gravity)
                Body.SetCustomDebugColor(new Vector3(1, 0, 0));
            else if (rcon.Phys == PhysType.GravityBone)
                Body.SetCustomDebugColor(new Vector3(0, 0, 1));
        }


       

        public void SyncBone2Body(OpenTK.Matrix4 world)
		{
            var mat = GetMat(BoneController.GetBone(BoneID).TransformMatrix * world);
            Body.MotionState.WorldTransform = startTransform * mat;
        }

		public void SyncBody2Bone(OpenTK.Matrix4 world)
		{
            var mat = GetMat(startTransform).Inverted() * GetMat(Body.WorldTransform);
            BoneController.GetBones[BoneID].PhysTransform = mat * world;
            BoneController.GetBones[BoneID].Phys = true;
        }


        private Vector3 GetVec3(OpenTK.Vector3 vec3)
		{
			return new Vector3(vec3.X, vec3.Y, vec3.Z);
		}

        
		private Matrix GetMat(OpenTK.Matrix4 mat)
		{
			return new Matrix(mat.M11, mat.M12, mat.M13, mat.M14, 
			                  mat.M21, mat.M22, mat.M23, mat.M24,
			                  mat.M31, mat.M32, mat.M33, mat.M34, 
			                  mat.M41, mat.M42, mat.M43, mat.M44);
		}
        

		public static OpenTK.Matrix4 GetMat(Matrix mat)
		{
			return new OpenTK.Matrix4(mat.M11, mat.M12, mat.M13, mat.M14,
					  		  		  mat.M21, mat.M22, mat.M23, mat.M24,
					  		  		  mat.M31, mat.M32, mat.M33, mat.M34,
							  		  mat.M41, mat.M42, mat.M43, mat.M44);
		}
        /*
        public static Matrix GetMat(OpenTK.Matrix4 mat)
        {
            IntPtr ptr = Marshal.AllocHGlobal(64);
            Marshal.StructureToPtr(mat, ptr, false);
            Matrix mat1 = (Matrix) Marshal.PtrToStructure(ptr, typeof(Matrix));
            Marshal.FreeHGlobal(ptr);
            return mat1;
        }
        */
		public void Reinstalize(OpenTK.Matrix4 world)
		{
			Body.WorldTransform = startTransform * GetMat(world);
			Body.LinearVelocity = Vector3.Zero;
			Body.AngularVelocity = Vector3.Zero;
			Body.ClearForces();
		}
    }
}
