using System;
using OpenTK;

namespace Toys
{
	public enum JointType
	{
		SpringSixDOF,
		SixDOF,
		P2P,
		ConeTwist,
		Slider,
		Hinge,
	}

	public class JointContainer
	{
		public string Name;
		public string NameEng;
		public JointType jType;
		public int RigitBody1;
		public int RigitBody2;
		public Vector3 Position;
		public Vector3 Rotation;
		public Vector3 PosMin;
		public Vector3 PosMax;
		public Vector3 RotMin;
		public Vector3 RotMax;
		public Vector3 PosSpring;
		public Vector3 RotSpring;
	}
}
