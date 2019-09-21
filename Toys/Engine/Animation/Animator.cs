using System;
using OpenTK;
using System.Collections.Generic;

namespace Toys
{
    class Animator : Component
    {
        public BoneController bones { get; private set; }
        Animation anim;
        bool isPlaing = false;
        float time = 0f;
        float length = 0;
        float framelength = 0;

        //model bone => anim bone reference
        Dictionary<int, int> boneReference = new Dictionary<int, int>();

        public Animator(BoneController bc) : base(typeof(Animator))
        {
            bones = bc;
        }

		internal void Update(float delta)
        {
            
            if (!isPlaing)
				return;

            time += delta;

			time = (time >= length + framelength) ? time % length : time;

			int prevFrame = (int)(time / framelength);
			int curFrame = (prevFrame == anim.frames.Length - 1) ? 0 : prevFrame + 1;

			float frameDelta = time / framelength;
			frameDelta = frameDelta - (int)frameDelta;

			AnimationFrame frame1 = anim.frames[prevFrame], frame2 = anim.frames[curFrame];

            foreach (var pair in boneReference)
            {
                Quaternion rotation = Quaternion.Identity;

                Vector3 pos = frame1.bones[pair.Value].position + (frame2.bones[pair.Value].position - frame1.bones[pair.Value].position) * frameDelta;
                if (anim.Type == Animation.RotationType.Quaternion)
                {
                    Quaternion prevQuat = new Quaternion(frame1.bones[pair.Value].rotation.Xyz, frame1.bones[pair.Value].rotation.W);
                    Quaternion nextQuat = new Quaternion(frame2.bones[pair.Value].rotation.Xyz, frame2.bones[pair.Value].rotation.W);
                    rotation = Quaternion.Slerp(prevQuat, nextQuat, frameDelta);
                }
                else
                {
                    var deltaRot = frame2.bones[pair.Value].rotation - frame1.bones[pair.Value].rotation;
                    CycleDeltaCheck(ref deltaRot.X);
                    CycleDeltaCheck(ref deltaRot.Y);
                    CycleDeltaCheck(ref deltaRot.Z);
                    Vector4 rot = frame1.bones[pair.Value].rotation + deltaRot * frameDelta;
                    rotation = Quaternion.FromAxisAngle(Vector3.UnitZ, rot.Z) * Quaternion.FromAxisAngle(Vector3.UnitY, rot.Y) * Quaternion.FromAxisAngle(Vector3.UnitX, rot.X);
                }

                if (anim.TransType == Animation.TransformType.LocalRelative)
                    bones.GetBone(pair.Key).SetTransform(rotation, pos);
                else if (anim.TransType == Animation.TransformType.LocalAbsolute)
                    bones.GetBone(pair.Key).InitialLocalTransform = Matrix4.CreateFromQuaternion(rotation) * Matrix4.CreateTranslation(pos);
            }
        }

        public void Play(Animation anim)
        {
            
            this.anim = anim;
            UpdateBoneReference();
            try
            {
                length = (anim.frames.Length - 1) / (float)anim.framerate;
                framelength = 1 / (float)anim.framerate;
                Instalize(anim.frames[0]);
                isPlaing = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }

		public void Stop()
		{
			isPlaing = false;
			time = 0;
			bones.DefaultPos();
		}

        void UpdateBoneReference()
        {
            boneReference.Clear();
            var boneRefNew = anim.bones;
            foreach (var bone in bones.GetBones)
            {
                
                if (boneRefNew.ContainsKey(bone.Bone.Name))
                {
                    
                    boneReference.Add(bone.Bone.Index, boneRefNew[bone.Bone.Name]);
                }
                
            }
        }

		void Instalize(AnimationFrame start)
		{
			foreach (var pair in boneReference)
			{
                Vector4 rot = start.bones[pair.Value].rotation;
                Vector3 pos = start.bones[pair.Value].position;
                
                Matrix4 localInv = Matrix4.Identity;

                Quaternion rotation = Quaternion.Identity;
                if (anim.Type == Animation.RotationType.Quaternion)
                    rotation = new Quaternion(rot.Xyz,rot.W);
                else
                    rotation = Quaternion.FromAxisAngle(Vector3.UnitZ, rot.Z) * Quaternion.FromAxisAngle(Vector3.UnitY, rot.Y) * Quaternion.FromAxisAngle(Vector3.UnitX, rot.X);

                if (anim.TransType == Animation.TransformType.LocalRelative)
                    bones.GetBone(pair.Key).SetTransform(rotation, pos);
                else if (anim.TransType == Animation.TransformType.LocalAbsolute)
                    bones.GetBone(pair.Key).InitialLocalTransform = Matrix4.CreateFromQuaternion(rotation) * Matrix4.CreateTranslation(pos);
            }
		}

        void CycleDeltaCheck(ref float delta)
        {
            //prevent unnessesery rotation
            //4PI == -4PI
            float pi2 = (float)Math.PI * 2;
            //remove 2PI cycle
            delta %= pi2;

            //find closest rotation
            if (delta < 0)
                delta = ((delta + pi2) < -delta) ? delta + pi2 : delta;
            else
                delta = ((delta - pi2) > -delta) ? delta - pi2 : delta;
        }

        internal override void Unload()
        {
    
        }

        internal override void AddComponent(SceneNode node)
        {
        }

        internal override void RemoveComponent()
        {
        }
    }
}
