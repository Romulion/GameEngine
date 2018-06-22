using System;
using OpenTK;

namespace Toys
{
	public enum PhysType
	{
		FollowBone,
		Gravity,
		GravityBone	}

	public class RigitContainer
	{
		public int BoneIndex;
		public byte GroupId;
		public short NonCollisionGroup;
		public PhysPrimitiveType primitive;
		public Vector3 Size;
		public Vector3 Position;
		public Vector3 Rotation;
		public float Mass;
		public float MassAttenuation;
		public float RotationDamping;
		public float Repulsion;
		public float Friction;
		public PhysType Phys;

		public RigitContainer()
		{
		}

	}
}
