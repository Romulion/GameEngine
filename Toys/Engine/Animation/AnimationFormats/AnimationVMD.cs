using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using OpenTK;

namespace Toys
{
    internal struct BoneMotionData
    {
        internal int _frame;
        internal BonePosition _boneposition;

        internal BoneMotionData(int frame, BonePosition boneposition)
        {
            _frame = frame;
            _boneposition = boneposition;
        }
    }

    class AnimationVMD : IAnimationLoader
    {

        float _multipler = 0.1f;

        string _path;
        BinaryReader _file;
        Reader _reader;

        Dictionary<string, int> _bones = new Dictionary<string, int>();
        AnimationFrame[] _frames;

        public AnimationVMD(string path)
        {
            _path = path;
        }

        public Animation Load()
        {
            using (Stream fileStream = File.OpenRead(_path))
            {

                _file = new BinaryReader(fileStream);
                _reader = new Reader(_file);

                string header = new string(_file.ReadChars(30));

                byte[] name = _file.ReadBytes(20);

                //converting SHIFT-JIS to utf16
                string modelName = jis2utf(name);

                ReadFrames();
            }
            var animation = new Animation(_frames.ToArray(), _bones);
            animation.Type = Animation.RotationType.Quaternion;
            animation.TransType = Animation.TransformType.LocalRelative;
            animation.Framerate = 30;
            return animation;
        }

        void ReadFrames()
        {
            int length = _file.ReadInt32();
            int framesCount = 0;
            int lastBoneIndex = 0;

            Dictionary<int, List<BoneMotionData>> bonePositions = new Dictionary<int, List<BoneMotionData>>();

            for (int i = 0; i < length; i++) {

                string bone = jis2utf(_file.ReadBytes(15));
                //remove trash bytes
                int removeStart = bone.IndexOf('\0');
                if (removeStart >= 0)
                    bone = bone.Remove(removeStart);
                
                int boneIndex = 0;

                //bones referense
                if (!_bones.ContainsKey(bone))
                {  
                    _bones.Add(bone, lastBoneIndex);
                    boneIndex = lastBoneIndex++;
                }
                else
                    boneIndex = _bones[bone];

                int frame = _file.ReadInt32();

                List<BoneMotionData> framebones = null;

                if (bonePositions.ContainsKey(boneIndex))
                {
                    framebones = bonePositions[boneIndex];
                }
                else
                {
                    framebones = new List<BoneMotionData>();
                    bonePositions.Add(boneIndex, framebones);
                }

                Vector3 pos = _reader.readVector3() * _multipler;
                Vector4 rot = _reader.readVector4();
                if (frame > framesCount)
                    framesCount = frame;
                //??? interpolation data ???
                _file.ReadBytes(64);
                
                
                framebones.Add(new BoneMotionData(frame, new BonePosition(pos, rot, boneIndex)));
            }

            //for poses
            if (framesCount == 0)
                framesCount = 1;

            _frames = new AnimationFrame[framesCount];

            NormalizeFrames(bonePositions);
        }

        //interpolating data on frames
        void NormalizeFrames(Dictionary<int, List<BoneMotionData>> bonePositions)
        {
            List<BonePosition>[] bonePosesIntepr = new List<BonePosition>[_frames.Length];
            for(int i = 0; i < bonePosesIntepr.Length; i++)
                bonePosesIntepr[i] = new List<BonePosition>();

            for (int n = 0; n < _bones.Count; n++)
            {
                var boneFrames = bonePositions[n];
                boneFrames = (boneFrames.OrderBy(b => b._frame)).ToList();
                
                for(int f = 0; f < boneFrames.Count; f++)
                {
                    var prevFrame = boneFrames[f];
                    var nextFrame = (f == boneFrames.Count-1) ? boneFrames[0] : boneFrames[ f + 1 ];

                    int frameCount = nextFrame._frame - prevFrame._frame;
                    var prev = prevFrame._boneposition;
                    Quaternion prevQuat = new Quaternion(prev.Rotation.Xyz, prev.Rotation.W);
                    Quaternion nextQuat = new Quaternion(nextFrame._boneposition.Rotation.Xyz, nextFrame._boneposition.Rotation.W); 

                    if (frameCount > 0)
                    {
                        
                        Vector3 stepPos = (nextFrame._boneposition.Position - prev.Position) / (float)frameCount;
                        float stepRot = 1 / (float)frameCount;
                        
                        for (int frm = 0; frm < frameCount; frm++)
                        {
                            var interpQuat = Quaternion.Slerp(prevQuat, nextQuat, (float)frm * stepRot);
                            bonePosesIntepr[prevFrame._frame + frm].Add(new BonePosition(
                                prev.Position + stepPos * frm,
                                new Vector4(interpQuat.Xyz, interpQuat.W), prev.BoneId));
                        }
                    }
                    else if (frameCount < 0)
                    {
                        frameCount += _frames.Length;                      
                        int firstPart = _frames.Length - prevFrame._frame;
                        Vector3 stepPos = (nextFrame._boneposition.Position - prev.Position) / (float)frameCount;
                        float stepRot = 1 / (float)frameCount;

                        //to last frame
                        for (int frm = 0; frm < firstPart; frm++)
                        {
                            var interpQuat = Quaternion.Slerp(prevQuat, nextQuat, (float)frm * stepRot);
                            bonePosesIntepr[prevFrame._frame + frm].Add(new BonePosition(
                                prev.Position + stepPos * frm,
                                new Vector4(interpQuat.Xyz, interpQuat.W), prev.BoneId));
                        }

                        //from first frame
                        for (int frm = 0; frm < nextFrame._frame; frm++)
                        {
                            var intQuat = Quaternion.Slerp(prevQuat, nextQuat, (float)(firstPart + frm) * stepRot);
                            bonePosesIntepr[frm].Add(new BonePosition(
                                prev.Position + stepPos * (firstPart + frm),
                                new Vector4(intQuat.Xyz, intQuat.W), prev.BoneId));
                        }
                    }
                    else
                    {
                        for (int frm = 0; frm < _frames.Length; frm++)
                        {
                            bonePosesIntepr[prevFrame._frame + frm].Add(new BonePosition(
                                prev.Position,
                                prev.Rotation, prev.BoneId));
                        }
                    }
                }
                
            }

            for (int i = 0; i < _frames.Length; i++)
            {
                _frames[i] = new AnimationFrame(bonePosesIntepr[i].ToArray());
            }
        }

        string jis2utf(byte[] bytes)
        {
            return Encoding.Unicode.GetString(Encoding.Convert(Encoding.GetEncoding(932), Encoding.Unicode, bytes));
        }
    }
}
