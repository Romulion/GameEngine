using System;
using OpenTK;
using System.Linq;
using System.Collections.Generic;


namespace Toys
{
	public class BoneController
	{
        BoneTransform[] _bones;
		Matrix4[] _skeleton;
        BoneTransform[] _bonesOrdered;

		public BoneController(BoneTransform[] bones)
		{
			_bones = bones;
			//making skeleton matrix
			_skeleton = new Matrix4[bones.Length];
			DefaultPos();
		}

        public BoneController(Bone[] bones,bool order = false)
        {
            _bones = new BoneTransform[bones.Length];
            Initialize(bones);

            UpdateOrder(order);

            _skeleton = new Matrix4[bones.Length];
            
            DefaultPos();
            UpdateSkeleton();
        }

        public BoneController(Bone[] bones, int[] bonesOrder)
        {
            _bones = new BoneTransform[bones.Length];

            Initialize(bones);

            _bonesOrdered = new BoneTransform[bones.Length];
            for (int i = 0; i < bonesOrder.Length; i++)
                _bonesOrdered[i] = _bones[bonesOrder[i]];

            SetWorldTransform();

            _skeleton = new Matrix4[bones.Length];

            DefaultPos();
            UpdateSkeleton();
        }

        public BoneTransform[] GetBones
		{
			get
			{
				return _bones;
			}
		}

		public Matrix4[] GetSkeleton
		{
			get
			{
				return _skeleton;
			}
		}



		public BoneTransform GetBone(string name)
		{
			var bone = Array.Find(_bones, (obj) => obj.Bone.Name == name);

			if (bone == null)
				Console.WriteLine("bone named '{0}' not found", name);

			return bone;
		}

        public BoneTransform GetBone(int id)
        {

            if (id >= _bones.Length)
                return null;
            
            return _bones[id];
        }

        void UpdateOrder(bool reorder)
        {
            _bonesOrdered = new BoneTransform[_bones.Length];
            if (!reorder)
            {
                for (int i = 0; i < _bones.Length; i++)
                    _bonesOrdered[i] = _bones[i];
            }
            else
            {
                int index = 0;
                List<BoneTransform> locks = _bones.ToList();
                var query = from bone in _bones
                            where bone.Bone.ParentIndex == -1
                            select bone;
                foreach (var bone in query)
                {
                    BoneLurker(locks, bone, ref index);
                }
            }

            SetWorldTransform();
        }

        void SetWorldTransform()
        {
            for (int i = 0; i < _bones.Length; i++)
            {
                if (_bonesOrdered[i].Parent != null)
                {
                    if (i == 82)
                    {
                        //Console.WriteLine(bonesOrdered[i].Parent.World2BoneInitial);
                        //Console.WriteLine(bonesOrdered[i].InitialLocalTransform);
                    }
                    _bonesOrdered[i].World2BoneInitial = _bonesOrdered[i].InitialLocalTransform * _bonesOrdered[i].Parent.World2BoneInitial;
                }
            }
        }

        void BoneLurker(List<BoneTransform> locks,BoneTransform boneT,ref int index)
        {
            _bonesOrdered[index++] = boneT;
            locks.Remove(boneT);
            var query = from bone in _bones
                        where bone.Bone.ParentIndex == boneT.Bone.Index
                        select bone;
            foreach (var bone in query)
            {
                BoneLurker(locks, bone, ref index);
            }
        }

        void Initialize(Bone[] bones){
            
            _bones = new BoneTransform[bones.Length];
            for (int i = 0; i < bones.Length; i++)
            {
                _bones[i] = new BoneTransform(bones[i]);
            }
            for (int i = 0; i < bones.Length; i++)
            {
                
                Bone boneData = bones[i];
                boneData.Index = i;
                BoneTransform boneTransform = _bones[i];
                if (boneData.ParentIndex < _bones.Length && boneData.ParentIndex >= 0)
                {
                    boneTransform.Parent = _bones[boneData.ParentIndex];
                }
                else
                    boneTransform.Parent = null;
                    
                boneTransform.IsAddLocal = boneData.IsAddLocal;
                boneTransform.IsRotateAdd = boneData.InheritRotation;
                boneTransform.IsTranslateAdd = boneData.InheritTranslation;
                if (boneTransform.IsRotateAdd || boneTransform.IsTranslateAdd)
                {
                    if (_bones.Length > boneData.ParentInheritIndex)
                    {
                        boneTransform.AddParent = _bones[boneData.ParentInheritIndex];
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
			        boneTransform.IK = new IKResolver(_bones, i);
                    
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
                _bones[i] = boneTransform;
            }
 
        }

        public void UpdateSkeleton()
        {
            for(int i = 0; i < _bones.Length; i++)
            {
                _bonesOrdered[i].UpdateLocalMatrix();
            }
            for (int i = 0; i < _bones.Length; i++)
            {
                _bones[i].UpdateTransformMatrix();
                _skeleton[i] = _bones[i].TransformMatrix;
            }
        }

        public void DefaultPos()
        {
            for (int n = 0; n < _skeleton.Length; n++)
            {
                _skeleton[n] = Matrix4.Identity;
                _bones[n].ResetTransform(false);
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
