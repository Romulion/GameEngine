using System;
using OpenTK;
using System.Linq;

namespace Toys
{
	public class BoneController
	{
		Bone[] bones;
		Matrix4[] skeleton;

		public BoneController(Bone[] bones)
		{
			this.bones = bones;
			//making skeleton matrix
			skeleton = new Matrix4[bones.Length];
			DefaultPos();
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

        public Bone GetBone(int id)
        {

            if (id >= bones.Length)
                return null;
            
            return bones[id];
        }

        public void Rotate(string name, Quaternion quat)
		{
			Bone bone = GetBone(name);
			if (bone == null)
				return;

			Rotate(bone.Index, quat);
		}

		public void Rotate(int boneID, Quaternion quat)
		{
			if (boneID >= bones.Length)
				return;
			Bone bone = bones[boneID];

			Matrix4 rot = bone.localSpaceInverted * Matrix4.CreateFromQuaternion(quat) * bone.localSpace;
			bone.localCoordinate = rot;

			//update skeleton
			Matrix4 model = Matrix4.Identity;
			if (bone.ParentIndex >= 0)
				model = rot * skeleton[bone.ParentIndex];
			skeleton[bone.Index] = model;
			UpdatePositionTree(bone, model);
		}

		/// <summary>
		/// for physics transformations
		/// </summary>
		/// <param name="boneID">Bone identifier.</param>
		/// <param name="mat">Mat.</param>
        public void SetTransformWorld(int boneID, Matrix4 mat)
        {
            if (boneID >= bones.Length)
                return;
            Bone bone = bones[boneID];

            Matrix4 chMat = mat;
            skeleton[bone.Index] = chMat;

            var childs = bone.childs;
            foreach (var child in childs)
                skeleton[child] = chMat;
        }

		public void SetTransform(int boneID, Matrix4 mat)
		{
			if (boneID >= bones.Length)
				return;
			Bone bone = bones[boneID];
			//Console.WriteLine(bone.localSpace);
			//Matrix4 localTransform = bone.localSpace * Matrix4.CreateTranslation(bone.Position);
			Matrix4 chMat = mat;
			if (bone.ParentIndex >= 0)
				chMat *= skeleton[bone.ParentIndex];
			
			skeleton[bone.Index] = chMat;
            UpdatePositionTree(bone, chMat);
		}

        public void DefaultPos()
		{
			for (int n = 0; n < skeleton.Length; n++)
			{
				bones[n].localCoordinate = Matrix4.Identity;
				skeleton[n] = Matrix4.Identity;
			}
		}

		void UpdatePositionTree(Bone bone, Matrix4 model)
		{
			var childs = bone.childs;
			foreach (var child in childs)
			{
				if (child == bone.Index)
					continue;
				Matrix4 mat = bones[child].localCoordinate * model;
				skeleton[child] = mat;
				UpdatePositionTree(bones[child], mat);
			}
		}

	}
}
