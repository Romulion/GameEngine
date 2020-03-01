using System;
using OpenTK;

namespace Toys
{
	public enum PhysType
	{
		FollowBone,
		Gravity,
		GravityBone
	}

	public class RigidContainer
	{
        public string Name;
        public string NameEng;
        public int BoneIndex;
		public byte GroupId;
		public ushort NonCollisionGroup;
		public PhysPrimitiveType PrimitiveType;
		public Vector3 Size;
		public Vector3 Position;
		public Vector3 Rotation;
		public float Mass;
		public float MassAttenuation;
		public float RotationDamping;
		public float Restitution;
		public float Friction;
		public PhysType Phys;

	}
}
