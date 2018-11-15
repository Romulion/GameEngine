using System;
using OpenTK;
using System.Linq;

namespace Toys
{
	public class AnimController
	{
		Mesh mesh;
		Bone[] bones;
		Matrix4[] skeleton;
		ModelSkinning skinner;

		public AnimController(Bone[] bones,Mesh mesh)
		{
			this.bones = bones;
			this.mesh = mesh;
			//making skeleton matrix
			skeleton = new Matrix4[bones.Length];
			DefaultPos();

			//skinner = new ModelSkinning(mesh);
		}

		public Bone[] GetBones
		{
			get
			{
				return bones;
			}
		}

		public Matrix4[] GetSkeleton
		{
			get
			{
				return skeleton;
			}
		}

		public Bone GetBone(string name)
		{

			var bone = Array.Find(bones, (obj) => obj.Name == name);

			if (bone == null)
				Console.WriteLine("bone named '{0}' not found", name);

			return bone;
		}

		public void Rotate(string name, Quaternion quat)
		{
			Bone bone = GetBone(name);
			if (bone == null)
				return;
			Matrix4 rotation = Matrix4.CreateFromQuaternion(quat);
			//rotation.Invert();
			Matrix4 rot = Matrix4.CreateTranslation(-bone.Position);
			if (bone.LocalCoordinate || bone.FixedAxis)
				rot *= bone.localSpace.Inverted() * rotation * bone.localSpace;
			else
				rot *= rotation;
			rot *= Matrix4.CreateTranslation(bone.Position);
			//update skeleton
			skeleton[bone.Index] = rot;
			var childs = bone.childs;
			foreach (var child in childs)
				skeleton[child] = rot;

		}

		public void Rotate(int boneID, Quaternion quat)
		{
			if (boneID >= bones.Length)
				return;
			Bone bone = bones[boneID];
			Matrix4 rotation = Matrix4.CreateFromQuaternion(quat);
			//rotation.Invert();
			Matrix4 rot = Matrix4.CreateTranslation(-bone.Position);
			if (bone.LocalCoordinate || bone.FixedAxis)
				rot *= bone.localSpace.Inverted() * rotation * bone.localSpace;
			else
				rot *= rotation;
			rot *= Matrix4.CreateTranslation(bone.Position);
			//update skeleton
			skeleton[bone.Index] = rot;
			var childs = bone.childs;
			foreach (var child in childs)
				skeleton[child] = rot;
		}

		public void DefaultPos()
		{
			for (int n = 0; n<skeleton.Length; n++)
				skeleton[n] = Matrix4.Identity;
		}

		public void SkinMesh()
		{
			skinner.Skin();
		}
	}
}
