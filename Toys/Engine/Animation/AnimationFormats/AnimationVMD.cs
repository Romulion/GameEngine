using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using OpenTK.Mathematics;
using System.Runtime.InteropServices;
namespace Toys
{

    class AnimationVMD : IAnimationLoader
    {

        float _multipler = 0.1f;

        string _path;
        Reader _reader;
        Stream _stream;

        Dictionary<string, int> _bones = new Dictionary<string, int>();
        List<AnimationFrame> _frames;

        //Dictionary<string, List<BoneKeyFrame>> _boneAnimation = new Dictionary<string, List<BoneKeyFrame>>();
        Animation _animation;

        public AnimationVMD(Stream stream, string path)
        {
            _path = path;
            _stream = stream;
        }

        public Animation Load()
        {
            using (_stream)
            {
                _animation = new Animation();
                _reader = new Reader(_stream);

                string header = new string(_reader.ReadChars(30));

                byte[] name = _reader.ReadBytes(20);

                //converting SHIFT-JIS to utf16
                string modelName = Reader.jis2utf(name);

                ReadBoneFrames();
                ReadMorphFrames();
                _reader.Dispose();
            }
            
            //var animation = new Animation(_frames.ToArray(), _bones);
            _animation.GetRotationType = Animation.RotationType.Quaternion;
            _animation.TransType = Animation.TransformType.LocalRelative;
            _animation.Framerate = 30;
            _animation.bones = _bones;
            return _animation;
        }

        void ReadBoneFrames()
        {
            int length = _reader.ReadInt32();
            int framesCount = 0;
            int lastBoneIndex = 0;

            Dictionary<int, List<KeyFrameBone>> bonePositions = new Dictionary<int, List<KeyFrameBone>>();

            for (int i = 0; i < length; i++) {

                string boneName = Reader.jis2utf(_reader.ReadBytes(15));
                //remove trash bytes
                int removeStart = boneName.IndexOf('\0');
                if (removeStart >= 0)
                    boneName = boneName.Remove(removeStart);
                int boneIndex = 0;

                //bones referense
                if (!_bones.ContainsKey(boneName))
                {  
                    _bones.Add(boneName, lastBoneIndex);
                    boneIndex = lastBoneIndex++;
                }
                else
                    boneIndex = _bones[boneName];

                int frame = _reader.ReadInt32();

                List<KeyFrameBone> framebones;

                if (bonePositions.ContainsKey(boneIndex))
                {
                    framebones = bonePositions[boneIndex];
                }
                else
                {
                    framebones = new List<KeyFrameBone>();
                    bonePositions.Add(boneIndex, framebones);
                }

                Vector3 pos = _reader.readVector3() * _multipler;
                Vector4 rot = _reader.readVector4();

                //Convert left to right coordinates
                pos.Z = -pos.Z;
                rot.Z = -rot.Z;
                rot.W = -rot.W;

                if (frame > framesCount)
                    framesCount = frame;
                //??? interpolation data ???

                var bonePos = new KeyFrameBone(frame / _animation.Framerate, new BonePosition(pos, new Quaternion(rot.Xyz, rot.W), boneIndex));

                bonePos.CurveX1.X = _reader.ReadByte();
                _reader.BaseStream.Position += 3;
                bonePos.CurveX1.Y = _reader.ReadByte();
                _reader.BaseStream.Position += 3;
                bonePos.CurveX2.X = _reader.ReadByte();
                _reader.BaseStream.Position += 3;
                bonePos.CurveX2.Y = _reader.ReadByte();
                _reader.BaseStream.Position += 3;
                bonePos.bezierX = new BezierCurveCubic(new Vector2(0,0), new Vector2(1, 1), bonePos.CurveX1 / 127, bonePos.CurveX2 / 127);

                bonePos.CurveY1.X = _reader.ReadByte();
                _reader.BaseStream.Position += 3;
                bonePos.CurveY1.Y = _reader.ReadByte();
                _reader.BaseStream.Position += 3;
                bonePos.CurveY2.X = _reader.ReadByte();
                _reader.BaseStream.Position += 3;
                bonePos.CurveY2.Y = _reader.ReadByte();
                _reader.BaseStream.Position += 3;
                bonePos.bezierY = new BezierCurveCubic(new Vector2(0, 0), new Vector2(1, 1), bonePos.CurveY1 / 127, bonePos.CurveY2 / 127);

                bonePos.CurveZ1.X = _reader.ReadByte();
                _reader.BaseStream.Position += 3;
                bonePos.CurveZ1.Y = _reader.ReadByte();
                _reader.BaseStream.Position += 3;
                bonePos.CurveZ2.X = _reader.ReadByte();
                _reader.BaseStream.Position += 3;
                bonePos.CurveZ2.Y = _reader.ReadByte();
                _reader.BaseStream.Position += 3;
                bonePos.bezierZ = new BezierCurveCubic(new Vector2(0, 0), new Vector2(1, 1), bonePos.CurveZ1 / 127, bonePos.CurveZ2 / 127);

                bonePos.CurveR1.X = _reader.ReadByte();
                _reader.BaseStream.Position += 3;
                bonePos.CurveR1.Y = _reader.ReadByte();
                _reader.BaseStream.Position += 3;
                bonePos.CurveR2.X = _reader.ReadByte();
                _reader.BaseStream.Position += 3;
                bonePos.CurveR2.Y = _reader.ReadByte();
                _reader.BaseStream.Position += 3;
                bonePos.bezierR = new BezierCurveCubic(new Vector2(0, 0), new Vector2(1, 1), bonePos.CurveR1 / 127, bonePos.CurveR2 / 127);

                bonePos.InterpolateCurve = true;

                _animation.AddBoneKey(boneName, bonePos);
                //framebones.Add(new BoneKeyFrame(frame, new BonePosition(pos, rot, boneIndex)));
            }

            //for poses
            if (framesCount == 0)
                framesCount = 1;

            _frames = new List<AnimationFrame>(framesCount);

            //InterpolateFramesBones(bonePositions);
        }

        void ReadMorphFrames()
        {
            int length = _reader.ReadInt32();
            for (int i = 0; i < length; i++)
            {

                string morphName = Reader.jis2utf(_reader.ReadBytes(15));
                //remove trash bytes
                int removeStart = morphName.IndexOf('\0');
                if (removeStart >= 0)
                    morphName = morphName.Remove(removeStart);

                var keyFrame = new KeyFrameMorph();
                keyFrame.FrameId = _reader.ReadInt32() / _animation.Framerate;
                keyFrame.Value = _reader.ReadSingle();
                _animation.AddMophKey(morphName, keyFrame);
            }
        }
        /*
        //interpolating data on frames
        void InterpolateFramesBones(Dictionary<int, List<BoneKeyFrame>> bonePositions)
        {
            List<BonePosition>[] bonePosesIntepr = new List<BonePosition>[_frames.Count];
            for(int i = 0; i < bonePosesIntepr.Length; i++)
                bonePosesIntepr[i] = new List<BonePosition>();

            for (int n = 0; n < _bones.Count; n++)
            {
                var boneFrames = bonePositions[n];
                boneFrames = (boneFrames.OrderBy(b => b.frameId)).ToList();
                
                for(int f = 0; f < boneFrames.Count; f++)
                {
                    var prevFrame = boneFrames[f];
                    var nextFrame = (f == boneFrames.Count-1) ? boneFrames[0] : boneFrames[ f + 1 ];

                    float frameCount = nextFrame.frameId - prevFrame.frameId;
                    var prev = prevFrame.BonePosition;
                    Quaternion prevQuat = new Quaternion(prev.Rotation.Xyz, prev.Rotation.W);
                    Quaternion nextQuat = new Quaternion(nextFrame.BonePosition.Rotation.Xyz, nextFrame.BonePosition.Rotation.W); 

                    if (frameCount > 0)
                    {
                        
                        Vector3 stepPos = (nextFrame.BonePosition.Position - prev.Position) / (float)frameCount;
                        float stepRot = 1 / (float)frameCount;
                        
                        for (int frm = 0; frm < frameCount; frm++)
                        {
                            var interpQuat = Quaternion.Slerp(prevQuat, nextQuat, (float)frm * stepRot);
                            bonePosesIntepr[prevFrame.frameId + frm].Add(new BonePosition(
                                prev.Position + stepPos * frm,
                                new Vector4(interpQuat.Xyz, interpQuat.W), prev.BoneId));
                        }
                    }
                    else if (frameCount < 0)
                    {
                        frameCount += _frames.Count;                      
                        int firstPart = _frames.Count - prevFrame.frameId;
                        Vector3 stepPos = (nextFrame.BonePosition.Position - prev.Position) / (float)frameCount;
                        float stepRot = 1 / (float)frameCount;

                        //to last frame
                        for (int frm = 0; frm < firstPart; frm++)
                        {
                            var interpQuat = Quaternion.Slerp(prevQuat, nextQuat, (float)frm * stepRot);
                            bonePosesIntepr[prevFrame.frameId + frm].Add(new BonePosition(
                                prev.Position + stepPos * frm,
                                new Vector4(interpQuat.Xyz, interpQuat.W), prev.BoneId));
                        }

                        //from first frame
                        for (int frm = 0; frm < nextFrame.frameId; frm++)
                        {
                            var intQuat = Quaternion.Slerp(prevQuat, nextQuat, (float)(firstPart + frm) * stepRot);
                            bonePosesIntepr[frm].Add(new BonePosition(
                                prev.Position + stepPos * (firstPart + frm),
                                new Vector4(intQuat.Xyz, intQuat.W), prev.BoneId));
                        }
                    }
                    else
                    {
                        for (int frm = 0; frm < _frames.Count; frm++)
                        {
                            bonePosesIntepr[prevFrame.frameId + frm].Add(new BonePosition(
                                prev.Position,
                                prev.Rotation, prev.BoneId));
                        }
                    }
                }
                
            }

            for (int i = 0; i < _frames.Count; i++)
            {
                _frames[i] = new AnimationFrame(bonePosesIntepr[i].ToArray());
            }
        }
        */
    }
}
