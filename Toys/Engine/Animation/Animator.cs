using System;
using OpenTK.Mathematics;
using System.Collections.Generic;

namespace Toys
{
    public class Animator : Component
    {
        /// <summary>
        /// Animation skeleton
        /// </summary>
        public BoneController BoneController { get; private set; }
        Animation _animation;
        AnimationController animController;
        List<Morph> morphs;
        bool _isPlaing = false;
        float _time = 0f;
        float _prevTime = 0f;
        float _length = 0;
        float _frameLength = 0;
        bool _isEnded = false;
        /// <summary>
        /// Relative animation speed
        /// </summary>
        public float Speed = 1f;
        /// <summary>
        /// repeat animation
        /// </summary>
        public bool IsRepeat = true;

        public bool IsEnded
        {
            get
            {
                return _isEnded;
            }
        }

        //model bone => anim bone reference
        Dictionary<string, int> boneReference = new Dictionary<string, int>();
        Dictionary<string, int> morphReference = new Dictionary<string, int>();


        public Animator(BoneController bc)
        {
            BoneController = bc;
            morphs = new List<Morph>();
        }

        public Animator(BoneController bc, List<Morph> md)
        {
            morphs = md;
            BoneController = bc;
        }

        internal void Update(float delta)
        {
            animController?.Update();
            if (!_isPlaing)
				return;
            _isEnded = false;
            
            _prevTime = _time;
            _time += delta;

            if (_time >= _length)
            {
                if (!IsRepeat)
                {
                    _isPlaing = false;
                    _isEnded = true;
                }
                else
                 _time %= _length;
            }
		    //_time = (_time >= _length ) ? _time % _length : _time;

            float frame = _time;
            foreach (var pair in boneReference)
            {
                //skip masked out bone
                if (!BoneController.GetBone(pair.Value).FollowAnimation)
                    continue;
                var boneData = _animation.GetInterpolatedFrameBone(pair.Key, frame);
                if (boneData == null)
                    continue;

                if (_animation.TransType == Animation.TransformType.LocalRelative)
                    BoneController.GetBone(pair.Value).SetTransform(boneData.Rotation, boneData.Position);
                else if (_animation.TransType == Animation.TransformType.LocalAbsolute)
                    BoneController.GetBone(pair.Value).InitialLocalTransform = Matrix4.CreateScale(boneData.Scale) * Matrix4.CreateFromQuaternion(boneData.Rotation) * Matrix4.CreateTranslation(boneData.Position);
            }

            
            foreach (var morphData in morphReference)
            {
                var start = _animation.GetInterpolatedFrameMorph(morphData.Key, frame);
                //skip empty frame
                if (start < 0)
                    continue;

                morphs[morphReference[morphData.Key]].MorphDegree = start;
            }

            var triggerList = _animation.GetPassedFrameTrigger(_prevTime, _time);
            foreach (var trigger in triggerList)
                trigger.Trigger.Invoke();

        }

        public AnimationController Controller 
        {
            get
            {
                return animController;
            }
            set
            {
                animController = value;
                animController.TargetAnimator = this;
            }
        }


        public Animation AnimationData
        {
            get { return _animation; }
            set
            {
                _animation = value;
                UpdateReferences();
                try
                {
                    _length = _animation.Length;
                    //_frameLength = 1;
                    Instalize();
                    _time = 0;
                }
                catch (Exception e)
                {
                    Logger.Error(e.Message, e.StackTrace);
                    _animation = null;
                }
            }
        }

        public void Play()
        {
            if (_animation == null)
                return;
            if (_animation.Length > 0)
                _isPlaing = true;
        }

		public void Stop()
		{
            if (_animation == null)
                return;

            _isPlaing = false;
			_time = 0;
            Instalize();
        }

        public void Pause()
        {
            if (_animation == null)
                return;

            _isPlaing = false;
        }

        public void Resume()
        {
            if (_animation == null)
                return;

            _isPlaing = true;
        }

        void UpdateReferences()
        {
            boneReference.Clear();
            foreach (var bone in _animation.bonesData)
            {
                var bn = BoneController.GetBone(bone.Key);
                if (bn != null)
                {
                    boneReference.Add(bone.Key, bn.Bone.Index);
                }
            }
            
            morphReference.Clear();
            if (_animation.morphData.Count > 0)
            {
                foreach (var morph in _animation.morphData)
                {
                    
                    var id = morphs.FindIndex(x => x.Name == morph.Key);
                    //Console.WriteLine("'{0}' {1}", morph.Key, id);
                    if (id >= 0)
                        morphReference.Add(morph.Key, id);
                }
            }
            
        }

		void Instalize()
		{
            foreach (var boneData in boneReference)
            {
                //skip busy bones
                if (!BoneController.GetBone(boneReference[boneData.Key]).FollowAnimation)
                    continue;
                var start = _animation.GetInterpolatedFrameBone(boneData.Key,0);
                //skip empty frame
                if (start == null)
                    continue;
                Vector3 pos = start.Position;
                //Matrix4 localInv = Matrix4.Identity;


                Quaternion rotation;
                if (_animation.GetRotationType == Animation.RotationType.Quaternion)
                    rotation = start.Rotation;
                else
                    rotation = Quaternion.FromAxisAngle(Vector3.UnitZ, start.RotationVec.Z) * Quaternion.FromAxisAngle(Vector3.UnitY, start.RotationVec.Y) * Quaternion.FromAxisAngle(Vector3.UnitX, start.RotationVec.X);

                if (_animation.TransType == Animation.TransformType.LocalRelative)
                    BoneController.GetBone(boneReference[boneData.Key]).SetTransform(rotation, pos);
                else if (_animation.TransType == Animation.TransformType.LocalAbsolute)
                    BoneController.GetBone(boneReference[boneData.Key]).InitialLocalTransform = Matrix4.CreateScale(start.Scale) * Matrix4.CreateFromQuaternion(rotation) * Matrix4.CreateTranslation(pos);
            }
            
            /*
            foreach (var morphData in morphReference)
            {
                var start = _animation.GetInterpolatedFrameMorph(morphData.Key, 0);

                //skip empty frame
                if (start < 0)
                    continue;

                morphs[morphReference[morphData.Key]].MorphDegree = start;
            }
            */
        }

        protected override void Unload()
        {
    
        }

        internal override void AddComponent(SceneNode node)
        {
            CoreEngine.AnimEngine.animators.Add(this);
        }

        internal override void RemoveComponent()
        {
            CoreEngine.AnimEngine.animators.Remove(this);
        }

        internal override Component Clone()
        {
            return new Animator(BoneController, morphs);
        }
    }
}
