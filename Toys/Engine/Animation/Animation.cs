using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

namespace Toys
{
    
    public class Animation : Resource
    {
        public enum RotationType
        {
            Quaternion,
            EulerXYZ,
            EulerZXY,
            EulerYZX,
        }

        public enum TransformType
        {
            LocalRelative,
            LocalAbsolute,
        }

        public float Framerate = 30;

        public float Length { get; private set; }

        internal readonly AnimationFrame[] frames;
        internal Dictionary<string, int> bones = new Dictionary<string, int>();

        internal readonly Dictionary<string, List<KeyFrameBone>> bonesData = new Dictionary<string, List<KeyFrameBone>>();
        internal readonly Dictionary<string, List<KeyFrameMorph>> morphData = new Dictionary<string, List<KeyFrameMorph>>();
        internal readonly List<KeyFrameTrigger> triggerData = new List<KeyFrameTrigger>();

        public RotationType GetRotationType { get; internal set; }
        public TransformType TransType { get; internal set; }

        public Animation (AnimationFrame[] frams, Dictionary<string, int> boneReference)
        {
            frames = frams;
			bones = boneReference;
        }

        public Animation()
        {
            Length = 0;
        }

        public void AddBoneKey(string boneName, KeyFrameBone keyFrame)
        {
            UpdateFrameCount(keyFrame.FrameId);
            if (bonesData.ContainsKey(boneName))
            {
                var frames = bonesData[boneName];
                //Check if key already exists
                var index = frames.FindIndex((a) => a.FrameId == keyFrame.FrameId);
                if (index < 0)
                {
                    index =  frames.FindLastIndex(x => x.FrameId < keyFrame.FrameId);
                    if (index < 0)
                        frames.Insert(0, keyFrame);
                    else
                        frames.Insert(index + 1, keyFrame);
                }
                else
                    frames[index] = keyFrame;
            }
            else
                bonesData.Add(boneName, new List<KeyFrameBone> { keyFrame });
        }

        public void AddMophKey(string morphName, KeyFrameMorph keyFrame)
        {
            UpdateFrameCount(keyFrame.FrameId);
            if (morphData.ContainsKey(morphName))
            {
                var frames = morphData[morphName];
                //Check if key already exists
                var index = frames.FindIndex((a) => a.FrameId == keyFrame.FrameId);

                if (index < 0)
                {
                    index = frames.FindLastIndex(x => x.FrameId < keyFrame.FrameId);
                    if (index < 0)
                        frames.Insert(0, keyFrame);
                    else
                        frames.Insert(index + 1, keyFrame);
                }
                else
                    frames[index] = keyFrame;

            }
            else
                morphData.Add(morphName, new List<KeyFrameMorph> { keyFrame });
        }

        public void AddTriggerKey(KeyFrameTrigger keyFrame)
        {
            UpdateFrameCount(keyFrame.FrameId);
            var frames = triggerData;

            var index = frames.FindLastIndex(x => x.FrameId < keyFrame.FrameId);
            if (index < 0)
                frames.Add(keyFrame);
            else
                frames.Insert(index + 1, keyFrame);
        }


        internal BonePosition GetInterpolatedFrameBone(string boneName, float frame)
        {
            var boneSequence = bonesData[boneName];
            //Looking For neaerst frames
            int prevFrame = -2;
            for (int i = 0; i < boneSequence.Count; i++)
            {
                if (boneSequence[i].FrameId >= frame)
                {
                    prevFrame = i - 1;
                    break;
                }
            }

            //No animation data
            if (prevFrame < -1)
                return null;

            //Single Frame
            if (boneSequence.Count == 1 || prevFrame < 0)
                return boneSequence[0].BonePosition;

            int nextFrame = (boneSequence.Count > (prevFrame + 1)) ? prevFrame + 1 : 0;           

            KeyFrameBone frame1 = boneSequence[prevFrame], frame2 = boneSequence[nextFrame];

            var frameDelta = (frame - frame1.FrameId) / (frame2.FrameId - frame1.FrameId);

            Quaternion rotation = Quaternion.Identity;
            Vector3 pos = frame1.BonePosition.Position;
            Vector3 scl = frame1.BonePosition.Scale;
            if (frame2.InterpolateCurve)
            {
                var posDelta = frame2.BonePosition.Position - frame1.BonePosition.Position;
                pos += BezierVector3(frame2, posDelta, frameDelta);
                rotation = BezierQuaternion(frame2, frame1.BonePosition.Rotation, frame2.BonePosition.Rotation, frameDelta);   
            }
            else
            {
                pos += (frame2.BonePosition.Position - frame1.BonePosition.Position) * frameDelta;
                scl += (frame2.BonePosition.Scale - frame1.BonePosition.Scale) * frameDelta;
                if (GetRotationType == RotationType.Quaternion)
                {
                    rotation = Quaternion.Slerp(frame1.BonePosition.Rotation, frame2.BonePosition.Rotation, frameDelta);
                }
                else
                {
                    var deltaRot = frame2.BonePosition.RotationVec - frame1.BonePosition.RotationVec;
                    CycleDeltaCheck(ref deltaRot.X);
                    CycleDeltaCheck(ref deltaRot.Y);
                    CycleDeltaCheck(ref deltaRot.Z);
                    Vector4 rot = frame1.BonePosition.RotationVec + deltaRot * frameDelta;
                    rotation = Quaternion.FromAxisAngle(Vector3.UnitZ, rot.Z) * Quaternion.FromAxisAngle(Vector3.UnitY, rot.Y) * Quaternion.FromAxisAngle(Vector3.UnitX, rot.X);
                }
            }
            return new BonePosition(pos, rotation, scl, 0);
        }

        internal float GetInterpolatedFrameMorph(string morphName, float frame)
        {
            var morphSequence = morphData[morphName];
            //Looking For neaerst frames
            int prevFrame = -1;
            for (int i = 0; i < morphSequence.Count; i++)
            {
                if (morphSequence[i].FrameId > frame)
                {
                    prevFrame = i - 1;
                    break;
                }
            }
            //No animation data
            if (prevFrame < 0)
                return -1;

            //Single Frame
            if (morphSequence.Count == 1)
                return morphSequence[0].Value;

            int nextFrame = (morphSequence.Count > (prevFrame + 1)) ? prevFrame + 1 : 0;
            
            KeyFrameMorph frame1 = morphSequence[prevFrame], frame2 = morphSequence[nextFrame];

            var frameDelta = (frame - frame1.FrameId) / (frame2.FrameId - frame1.FrameId);


            float value = frame1.Value;
            var valDelta = frame2.Value - frame1.Value;

            if (frame2.InterpolateCurve)
            {
                value += valDelta * frame2.bezierCurve.CalculatePoint(frameDelta).Y;
            }
            else
            {
                value += valDelta * frameDelta;
            }

            return value;
        }

        internal KeyFrameTrigger[] GetPassedFrameTrigger(float frameStart, float frameEnd)
        {
            var result = from t in triggerData
                         where t.FrameId >= frameStart && t.FrameId < frameEnd
                         select t;
            return result.ToArray();
        }

        Vector3 BezierVector3(KeyFrameBone keyFrame, Vector3 pos, float t)
        {
            Vector3 result = Vector3.Zero;
            result.X = pos.X * keyFrame.bezierX.CalculatePoint(t).Y;
            result.Y = pos.Y * keyFrame.bezierY.CalculatePoint(t).Y;
            result.Z = pos.Z * keyFrame.bezierZ.CalculatePoint(t).Y;

            return result;
        }

        Quaternion BezierQuaternion(KeyFrameBone keyFrame, Quaternion quat1, Quaternion quat2, float t)
        {
            return Quaternion.Slerp(quat1, quat2, keyFrame.bezierR.CalculatePoint(t).Y);
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

        void UpdateFrameCount(float frameNumber)
        {
            Length = Math.Max(Length, frameNumber);
        }

        protected override void Unload()
        {
        }
    }
}
