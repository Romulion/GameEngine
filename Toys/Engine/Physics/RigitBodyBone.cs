using System;
using BulletSharp;
using BulletSharp.Math;

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
		CollisionShape cshape;


		public Bone bone { get; set; }
		public RigidBody Body { get; private set; }

		public RigitBodyBone(RigitContainer rcon)
		{
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
			cshape = shape;

			Vector3 inertia = shape.CalculateLocalInertia(rcon.Mass);

			var rbInfo = new RigidBodyConstructionInfo(rcon.Mass, null, shape, inertia);
			rbInfo.MotionState = new DefaultMotionState(Matrix.Translation(GetVec3(rcon.Position)));
			Body= new RigidBody(rbInfo);
			Body.Friction = rcon.Friction;
			Body.SetDamping(rcon.MassAttenuation, rcon.RotationDamping);
			rbInfo.Dispose();
		}

		public void SyncBone2Body(Matrix world)
		{
			Body.WorldTransform = GetMat(bone.localSpace) * world;
		}

		public void SyncBody2Bone(Matrix world)
		{
			bone.localSpace = GetMat(Body.WorldTransform) * GetMat(world);
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

		private OpenTK.Matrix4 GetMat(Matrix mat)
		{
			return new OpenTK.Matrix4(mat.M11, mat.M12, mat.M13, mat.M14,
					  		  		  mat.M21, mat.M22, mat.M23, mat.M24,
					  		  		  mat.M31, mat.M32, mat.M33, mat.M34,
							  		  mat.M41, mat.M42, mat.M43, mat.M44);
		}
	}
}
