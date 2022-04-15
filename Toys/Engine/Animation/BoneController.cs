using System;
using OpenTK.Mathematics;
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
            CheckBonesCount();
            DefaultPos();
		}

        public BoneController(Bone[] bones,bool order = false)
        {
            _bones = new BoneTransform[bones.Length];
            Initialize(bones);

            UpdateOrder(order);

            _skeleton = new Matrix4[bones.Length];

            CheckBonesCount();
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

            CheckBonesCount();
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
                    /*
                    if (i == 67 || i == 10 || i ==66 || i == 127 | i == 0)
                    {
                        Console.WriteLine(i);
                        Console.WriteLine(_bonesOrdered[i].Parent.Bone.Index);
                        Console.WriteLine(_bonesOrdered[i].Parent.World2BoneInitial);
                        Console.WriteLine(_bonesOrdered[i].InitialLocalTransform);
                    }
                    */
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


        private void CheckBonesCount()
        {
            var settings = Settings.GetInstance();
            if (settings.System.MaxBonesCount < _bones.Length)
                Logger.Error(String.Format("bone count {0} exceed maximum value  {1}", _bones.Length, settings.System.MaxBonesCount), "");
        }
    }
}
