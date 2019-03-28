using System;
using OpenTK;

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

        public Animator(BoneController bc) : base(typeof(Animator))
        {
            bones = bc;
			//Console.WriteLine(bc.GetBones[4].localSpace);
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

			for (int i = 0; i < frame1.bones.Length; i++)
			{
				Vector3 rot = frame1.bones[i].rotation + (frame2.bones[i].rotation - frame1.bones[i].rotation) * frameDelta;
				Vector3 pos = frame1.bones[i].position + (frame2.bones[i].position - frame1.bones[i].position) * frameDelta;
				var quat = new Quaternion(rot);
				Matrix4 trans = Matrix4.CreateFromQuaternion(quat) * Matrix4.CreateTranslation(pos);
				bones.SetTransform(i,trans * bones.GetBones[i].localSpace.Inverted());
			}

        }

        public void Play(Animation anim)
        {
            this.anim = anim;
            isPlaing = true;
			length = (anim.frames.Length - 1) / (float)anim.framerate;
            framelength = 1 / (float)anim.framerate;
			Instalize(anim.frames[0]);
			//bones.GetBones[0].Name;
			//foreach (var id in anim.frames[0].bones)
			//	Console.WriteLine("{0} : {1} - {2}", id.boneId, id.position, id.rotation);

        }

		public void Stop()
		{
			isPlaing = false;
			time = 0;
			bones.DefaultPos();
		}

		void Instalize(AnimationFrame start)
		{
			
			//Matrix4 trans1 = Matrix4.CreateFromQuaternion(new Quaternion(start.bones[26].rotation)) * Matrix4.CreateTranslation(start.bones[26].position);
			//Console.WriteLine(trans1);
			// var lt  = bones.GetBones[26].localSpace;
			//Console.WriteLine(lt);
			//Console.WriteLine(trans1 * lt.Inverted());

			for (int i = 0; i < start.bones.Length; i++)
			{
				if (i < 26 || i > 26)
					continue;
				
				Vector3 rot = start.bones[i].rotation;
				Vector3 pos = start.bones[i].position;
				var quat = new Quaternion(rot);
				Matrix4 trans = Matrix4.CreateFromQuaternion(quat) * Matrix4.CreateTranslation(pos);
				//var mat = Matrix4.CreateRotationX(rot.X) * Matrix4.CreateRotationY(rot.Y) * Matrix4.CreateRotationZ(rot.Z);
				//Matrix4 trans = mat * Matrix4.CreateTranslation(pos);

				Matrix4 local = bones.GetBones[i].localSpace;
;
				bones.SetTransform(i, trans * local.Inverted());
				/*
				if (i == 26)
				{
					Console.WriteLine(local);
					Console.WriteLine(trans);
				Console.WriteLine("{0} : {1} \n{2}", i, bones.GetBones[i].Name,trans * local.Inverted());
				}
				*/
				//bones.Rotate(i, quat);
			}
		}

        internal override void Unload()
        {
    
        }
    }
}
