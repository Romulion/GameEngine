using System;
using OpenTK;
using System.Linq;
using System.Collections.Generic;

namespace Toys
{
	public class BoneController
	{
        BoneTransform[] bones;
		Matrix4[] skeleton;
        BoneTransform[] bonesOrdered;

		public BoneController(BoneTransform[] bones)
		{
			this.bones = bones;
			//making skeleton matrix
			skeleton = new Matrix4[bones.Length];
			DefaultPos();
		}

        public BoneController(Bone[] bones)
        {
            this.bones = new BoneTransform[bones.Length];
            Initialize(bones);

            skeleton = new Matrix4[bones.Length];
            
            DefaultPos();
            //for tests
            //this.bones[1].SetTransform(Quaternion.Identity,new Vector3(0f,-0.7f,0f));
            UpdateSkeleton();
        }

        public BoneTransform[] GetBones
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



		public BoneTransform GetBone(string name)
		{
			var bone = Array.Find(bones, (obj) => obj.Bone.Name == name);

			if (bone == null)
				Console.WriteLine("bone named '{0}' not found", name);

			return bone;
		}

        public BoneTransform GetBone(int id)
        {

            if (id >= bones.Length)
                return null;
            
            return bones[id];
        }

        void Initialize(Bone[] bones){
            this.bones = new BoneTransform[bones.Length];
            for (int i = 0; i < bones.Length; i++)
            {
                Bone boneData = bones[i];
                this.bones[i] = new BoneTransform(bones[i]);
            }
            for (int i = 0; i < bones.Length; i++)
            {
                Bone boneData = bones[i];
                boneData.Index = i;
                BoneTransform boneTransform = this.bones[i];
                boneTransform.InitialOffset = boneData.Position;
                if (boneData.ParentIndex < this.bones.Length && boneData.ParentIndex >= 0)
                {
                    boneTransform.Parent = this.bones[boneData.ParentIndex];
                    boneTransform.LocalSpaceDefault = boneTransform.Parent.LocalSpaceDefault * Matrix4.CreateTranslation(boneTransform.Bone.Position);
                    boneTransform.LocalSpaceInverted = boneTransform.LocalSpaceDefault.Inverted();
                }
                else
                    boneTransform.Parent = null;
                    
                boneTransform.IsAddLocal = boneData.IsAddLocal;
                boneTransform.IsRotateAdd = boneData.InheritRotation;
                boneTransform.IsTranslateAdd = boneData.InheritTranslation;
                if (boneTransform.IsRotateAdd || boneTransform.IsTranslateAdd)
                {
                    if (this.bones.Length > boneData.ParentInheritIndex)
                    {
                        boneTransform.AddParent = this.bones[boneData.ParentInheritIndex];
                        boneTransform.AddRatio = boneData.ParentInfluence;
                    }
                    else 
                    {
                        boneTransform.AddParent = null;
                    }
                }
                
                if (boneData.IK)
                {
                    boneTransform.IsIK = true;
			        boneTransform.IK = new IKResolver(this.bones, i);
                    
			        if (boneData.IKData != null)
			        {
				        bool isPhysicsAllLink = true;
                        /*
				        foreach (PmxIK.IKLink link in pmxBone2.IK.LinkList)
				        {
					        if (!dictionary.ContainsKey(link.Bone))
					        {
						        isPhysicsAllLink = false;
						        break;
					        }
				        }
                        */
				        boneTransform.IK.IsPhysicsAllLink = isPhysicsAllLink;
			        }
                    
                }
                this.bones[i] = boneTransform;
            }
 
        }

        public void UpdateSkeleton()
        {
            for(int i = 0; i < bones.Length; i++)
            {
                bones[i].UpdateLocalMatrix();
            }

            for(int i = 0; i < bones.Length; i++)
            {
                bones[i].UpdateTransformMatrix();
                skeleton[i] = bones[i].TransformMatrix;
            }
        }

        public void DefaultPos()
        {
            for (int n = 0; n < skeleton.Length; n++)
            {
                skeleton[n] = Matrix4.Identity;
                bones[n].ResetTransform(false);
            }
        }
        /* obsolette
        public void Rotate(string name, Quaternion quat)
		{
            BoneTransform bone = GetBone(name);
			if (bone == null)
				return;

			Rotate(bone.Bone.Index, quat);
		}

		public void Rotate(int boneID, Quaternion quat)
		{
			if (boneID >= bones.Length)
				return;
            BoneTransform bone = bones[boneID];

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

        public void SetTransformExperimantal(int boneID, Matrix4 mat)
        {
            if (boneID >= bones.Length)
                return;
            Bone bone = bones[boneID];
            Matrix4 rot = bone.localSpaceInverted * mat * bone.localSpace;
            bone.localCoordinate = rot;

            Matrix4 chMat = rot;
            if (bone.ParentIndex >= 0)
                chMat *= skeleton[bone.ParentIndex];
            skeleton[bone.Index] = chMat;
            UpdatePositionTree(bone, chMat);
        }
         
        public void SetTransformDelayedUpdate(int boneID, Matrix4 mat)
        {
            if (boneID >= bones.Length)
                return;
            Bone bone = bones[boneID];
            Matrix4 rot = bone.localSpaceInverted * mat * bone.localSpace;
            bone.localCoordinate = rot;

            Matrix4 chMat = rot;
            if (bone.ParentIndex >= 0)
                chMat *= skeleton[bone.ParentIndex];
            skeleton[bone.Index] = chMat; 
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

        internal void UpdatePositionTreeDelayed(Bone bone)
        {
            var childs = bone.childs;
            var model = bone.localCoordinate;

            foreach (var child in childs)
            {
                if (child == bone.Index)
                    continue;
                Matrix4 mat = bones[child].localCoordinate * model;
                skeleton[child] = mat;
                UpdatePositionTree(bones[child], mat);
            }
        }
        */

    }
}
