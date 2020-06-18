using System;
using OpenTK;
using System.Collections.Generic;

namespace Toys
{
    public class Animator : Component
    {
        Logger logger = new Logger("Animator");
        public BoneController BoneController { get; private set; }
        Animation _animation;
        bool _isPlaing = false;
        float _time = 0f;
        float _length = 0;
        float _frameLength = 0;

        //model bone => anim bone reference
        Dictionary<int, int> boneReference = new Dictionary<int, int>();

        public Animator(BoneController bc) : base(typeof(Animator))
        {
            BoneController = bc;
        }

		internal void Update(float delta)
        {
            
            if (!_isPlaing)
				return;

            _time += delta;

			_time = (_time >= _length + _frameLength) ? _time % _length : _time;

			int prevFrame = (int)(_time / _frameLength);
			int curFrame = (prevFrame == _animation.frames.Length - 1) ? 0 : prevFrame + 1;

			float frameDelta = _time / _frameLength;
			frameDelta = frameDelta - (int)frameDelta;

			AnimationFrame frame1 = _animation.frames[prevFrame], frame2 = _animation.frames[curFrame];

            foreach (var pair in boneReference)
            {
                Quaternion rotation = Quaternion.Identity;

                Vector3 pos = frame1.BonePoritions[pair.Value].Position + (frame2.BonePoritions[pair.Value].Position - frame1.BonePoritions[pair.Value].Position) * frameDelta;
                if (_animation.Type == Animation.RotationType.Quaternion)
                {
                    Quaternion prevQuat = new Quaternion(frame1.BonePoritions[pair.Value].Rotation.Xyz, frame1.BonePoritions[pair.Value].Rotation.W);
                    Quaternion nextQuat = new Quaternion(frame2.BonePoritions[pair.Value].Rotation.Xyz, frame2.BonePoritions[pair.Value].Rotation.W);
                    rotation = Quaternion.Slerp(prevQuat, nextQuat, frameDelta);
                }
                else
                {
                    var deltaRot = frame2.BonePoritions[pair.Value].Rotation - frame1.BonePoritions[pair.Value].Rotation;
                    CycleDeltaCheck(ref deltaRot.X);
                    CycleDeltaCheck(ref deltaRot.Y);
                    CycleDeltaCheck(ref deltaRot.Z);
                    Vector4 rot = frame1.BonePoritions[pair.Value].Rotation + deltaRot * frameDelta;
                    rotation = Quaternion.FromAxisAngle(Vector3.UnitZ, rot.Z) * Quaternion.FromAxisAngle(Vector3.UnitY, rot.Y) * Quaternion.FromAxisAngle(Vector3.UnitX, rot.X);
                }

                if (_animation.TransType == Animation.TransformType.LocalRelative)
                    BoneController.GetBone(pair.Key).SetTransform(rotation, pos);
                else if (_animation.TransType == Animation.TransformType.LocalAbsolute)
                    BoneController.GetBone(pair.Key).InitialLocalTransform = Matrix4.CreateFromQuaternion(rotation) * Matrix4.CreateTranslation(pos);
            }
        }

        public void Play(Animation anim)
        {
            _animation = anim;
            UpdateBoneReference();
            try
            {
                _length = (anim.frames.Length - 1) / (float)anim.Framerate;
                _frameLength = 1 / (float)anim.Framerate;
                Instalize(anim.frames[0]);

                if (anim.frames.Length > 1)
                    _isPlaing = true;
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e.StackTrace);
            }
        }

		public void Stop()
		{
			_isPlaing = false;
			_time = 0;
            Instalize(_animation.frames[0]);
        }

        public void Pause()
        {
            _isPlaing = false;
        }

        public void Resume()
        {
            _isPlaing = true;
        }

        void UpdateBoneReference()
        {
            boneReference.Clear();
            var boneRefNew = _animation.bones;
            foreach (var bone in BoneController.GetBones)
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
                Vector4 rot = start.BonePoritions[pair.Value].Rotation;
                Vector3 pos = start.BonePoritions[pair.Value].Position;
                
                Matrix4 localInv = Matrix4.Identity;

                Quaternion rotation = Quaternion.Identity;
                if (_animation.Type == Animation.RotationType.Quaternion)
                    rotation = new Quaternion(rot.Xyz,rot.W);
                else
                    rotation = Quaternion.FromAxisAngle(Vector3.UnitZ, rot.Z) * Quaternion.FromAxisAngle(Vector3.UnitY, rot.Y) * Quaternion.FromAxisAngle(Vector3.UnitX, rot.X);

                if (_animation.TransType == Animation.TransformType.LocalRelative)
                    BoneController.GetBone(pair.Key).SetTransform(rotation, pos);
                else if (_animation.TransType == Animation.TransformType.LocalAbsolute)
                    BoneController.GetBone(pair.Key).InitialLocalTransform = Matrix4.CreateFromQuaternion(rotation) * Matrix4.CreateTranslation(pos);
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
