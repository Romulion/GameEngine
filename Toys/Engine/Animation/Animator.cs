using System;
using OpenTK;
using System.Collections.Generic;

namespace Toys
{
    class Animator : Component
    {
        BoneController bones;
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

			time += (delta / 1000);

			time = (time >= length + framelength) ? time % length : time;

			int prevFrame = (int)(time / framelength);
			int curFrame = (prevFrame == anim.frames.Length - 1) ? 0 : prevFrame + 1;

			float frameDelta = time / framelength;
			frameDelta = frameDelta - (int)frameDelta;

			AnimationFrame frame1 = anim.frames[prevFrame], frame2 = anim.frames[curFrame];

            foreach (var pair in boneReference)
            {
                Matrix4 mat = Matrix4.Identity;
                Matrix4 localInv = Matrix4.Identity;

                if (anim.Type == Animation.RotationType.Quaternion)
                {
                    Quaternion prevQuat = new Quaternion(frame1.bones[pair.Value].rotation.Xyz, frame1.bones[pair.Value].rotation.W);
                    Quaternion nextQuat = new Quaternion(frame2.bones[pair.Value].rotation.Xyz, frame2.bones[pair.Value].rotation.W);
                    var intQuat = Quaternion.Slerp(prevQuat, nextQuat, frameDelta);
                    mat = Matrix4.CreateFromQuaternion(intQuat);
                }
                else
                {
                    var deltaRot = frame2.bones[pair.Value].rotation - frame1.bones[pair.Value].rotation;
                    CycleDeltaCheck(ref deltaRot.X);
                    CycleDeltaCheck(ref deltaRot.Y);
                    CycleDeltaCheck(ref deltaRot.Z);
                    Vector4 rot = frame1.bones[pair.Value].rotation + deltaRot * frameDelta;
                    mat = Matrix4.CreateRotationX(rot.X) * Matrix4.CreateRotationY(rot.Y) * Matrix4.CreateRotationZ(rot.Z);

                   // localInv = bones.GetBones[pair.Key].localSpaceDefault.Inverted();
                }

				Vector3 pos = frame1.bones[pair.Value].position + (frame2.bones[pair.Value].position - frame1.bones[pair.Value].position) * frameDelta;

                //var quat = new Quaternion(rot);
                //Matrix4 trans = Matrix4.CreateFromQuaternion(quat) * Matrix4.CreateTranslation(pos);
                
                Matrix4 trans = mat * Matrix4.CreateTranslation(pos);
                //bones.SetTransformDelayedUpdate(pair.Key, trans * localInv);
			}
        }

        public void Play(Animation anim)
        {
            
            this.anim = anim;
            UpdateBoneReference();

            isPlaing = true;
            try
            {
                length = (anim.frames.Length - 1) / (float)anim.framerate;
                framelength = 1 / (float)anim.framerate;
                Instalize(anim.frames[0]);
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
                /*
                if (boneRefNew.ContainsKey(bone.Name))
                {
                    boneReference.Add(bone.Index, boneRefNew[bone.Name]);
                    //Console.WriteLine("{0} => {1}", bone.Index, boneRefNew[bone.Name]);
                }
                */
            }
        }

		void Instalize(AnimationFrame start)
		{
			foreach (var pair in boneReference)
			{
                /*
                if (pair.Key < 198 || pair.Key < 201)
                    continue;
                    */
                Vector4 rot = start.bones[pair.Value].rotation;
                Vector3 pos = start.bones[pair.Value].position;
                Matrix4 mat = Matrix4.Identity;
                Matrix4 localInv = Matrix4.Identity;

                if (anim.Type == Animation.RotationType.Quaternion)
                {
                    mat = Matrix4.CreateFromQuaternion(new Quaternion(rot.Xyz,rot.W));
                }
                else
                {
                    mat = Matrix4.CreateRotationX(rot.X) * Matrix4.CreateRotationY(rot.Y) * Matrix4.CreateRotationZ(rot.Z);
                    //localInv = bones.GetBones[pair.Key].localSpaceDefault.Inverted();
                }

                
                Matrix4 trans = mat * Matrix4.CreateTranslation(pos);
                //if ()
                Console.WriteLine(trans);

                //bones.SetTransformDelayedUpdate(pair.Key, trans * localInv);
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
    }
}
