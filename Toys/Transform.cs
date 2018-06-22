using System;
using OpenTK;
namespace Toys
{
	public class Transform
	{
		Vector3 worldPos;
		Quaternion rotation;
		Matrix4 transform;

		public Transform()
		{
			worldPos = Vector3.Zero;
		//	rotation = Quaternion.Identity;
		}
		/*
		public void Rotate(Quaternion rot)
		{
			transform
		}

		public void Rotate(Vector3 rot)
		{
			transform
		}
*/
		public Matrix4 GetWorld
		{
			get { return Matrix4.CreateTranslation(worldPos) * Matrix4.CreateFromQuaternion(rotation); }
		}
	}
}
