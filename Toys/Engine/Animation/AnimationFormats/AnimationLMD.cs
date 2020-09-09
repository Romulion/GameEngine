using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using OpenTK;

namespace Toys
{
    class AnimationLMD :IAnimationLoader
    {
        private class BoneUnsorted
        {
            internal Vector3[] positions;
            internal float[] positionsTimestamps;
            internal Quaternion[] rotations;
            internal float[] rotationsTimestamps;
        }

        string _path;
        Reader _reader;
        float _speed = 1;

        Dictionary<string, int> bones = new Dictionary<string, int>();
        AnimationFrame[] frames;

        public AnimationLMD(string path)
        {
            _path = path;
            using (Stream fs = File.OpenRead(_path))
            {
                _reader = new Reader(fs);
                _reader.EncodingType = 1;
                ReadBones();

                _reader.BaseStream.Position = 100;
                _speed = _reader.ReadSingle();
            }
        }

        public Animation Load()
        {
            var animation = new Animation(frames.ToArray(), bones);
            animation.GetRotationType = Animation.RotationType.Quaternion;
            animation.TransType = Animation.TransformType.LocalAbsolute;
            animation.Framerate = (int)(frames.Length / _speed);
            return animation;
        }

        bool CheckHeader()
        {
            _reader.BaseStream.Position = 28;
            //going to info block
            _reader.BaseStream.Position = ReadPointer();
            int type = _reader.ReadInt32();
            return (type == 4);
        }

        void ReadBones()
        {

            _reader.BaseStream.Position = 116;
            int boneCount = _reader.ReadInt32();
            int[] bonePosArray = new int[boneCount];
            for (int i = 0; i < boneCount; i++)
                bonePosArray[i] = ReadPointer();

            BoneUnsorted[] bua = new BoneUnsorted[boneCount];
            int n = 0;
            foreach (int offset in bonePosArray)
            {
                int Magic = _reader.ReadInt32();
                _reader.BaseStream.Position = offset + 4;
                _reader.BaseStream.Position = ReadPointer();
                string boneName = _reader.readString();
                _reader.BaseStream.Position = offset + 20;
                _reader.BaseStream.Position = ReadPointer();
                //4 bytes id; 4 *2 bytes unknown anim strucs pointers
                _reader.BaseStream.Position += 12;

                var bu = new BoneUnsorted();     
                int rotTransfOffset = ReadPointer();
                int tranTransfOffset = ReadPointer();
                //reading rotations | skipping id
                _reader.BaseStream.Position = rotTransfOffset + 4;
                int rotTransfTimeOffset = ReadPointer();
                _reader.BaseStream.Position = ReadPointer();
                int transformElCount = _reader.ReadInt32();
                transformElCount /= 4;
                bu.rotations = new Quaternion[transformElCount];
                for (int i = 0; i < transformElCount; i++)
                    bu.rotations[i] = new Quaternion(_reader.readVector3(), _reader.ReadSingle());
                //rotation transform time table
                _reader.BaseStream.Position = rotTransfTimeOffset;
                int timeElCount = _reader.ReadInt32();
                bu.rotationsTimestamps = new float[timeElCount];
                for (int i = 0; i < timeElCount; i++)
                    bu.rotationsTimestamps[i] = _reader.ReadSingle();

                //read transforms
                _reader.BaseStream.Position = tranTransfOffset + 4;
                int transTransfTimeOffset = ReadPointer();
                _reader.BaseStream.Position = ReadPointer();
                transformElCount = _reader.ReadInt32();
                transformElCount /= 3;
                bu.positions = new Vector3[transformElCount];
                for (int i = 0; i < transformElCount; i++)
                    bu.positions[i] = _reader.readVector3();
                //rotation transform time table
                _reader.BaseStream.Position = transTransfTimeOffset;
                timeElCount = _reader.ReadInt32();
                bu.positionsTimestamps = new float[timeElCount];
                for (int i = 0; i < timeElCount; i++)
                    bu.positionsTimestamps[i] = _reader.ReadSingle();

                bones.Add(boneName, n);
                bua[n] = bu;
                n++;
            }


            NormalizeFrames(bua);

        }

        //creaating frames with aproximation
        void NormalizeFrames(BoneUnsorted[] bonesRaw)
        {
            int maxFrames = bonesRaw.Max((BoneUnsorted b) => { return Math.Max(b.positions.Length, b.rotations.Length); });
            frames = new AnimationFrame[maxFrames];

            List<BonePosition>[] bonePosesInterpretated = new List<BonePosition>[maxFrames];
            for (int i = 0; i < maxFrames; i++)
                bonePosesInterpretated[i] = new List<BonePosition>();

            float framePart = 1f / maxFrames;
            for (int n = 0; n < bones.Count; n++)
            {
                var boneFrames = bonesRaw[n];
                float boneTransPart = 1f / boneFrames.positions.Length - 1;
                float boneRotPart = 1f / boneFrames.rotations.Length - 1;

                for (int f = 0; f < maxFrames; f++)
                {
                    
                    float framepos = f * framePart * (boneFrames.positions.Length -1);
                    float framerot = f * framePart * (boneFrames.rotations.Length -1);
                    var prevFrameTrans = boneFrames.positions[(int)Math.Floor(framepos)];
                    var prevFrameRot = boneFrames.rotations[(int)Math.Floor(framerot)];
                    
                    var nextFrameTrans = boneFrames.positions[(int)Math.Ceiling(framepos)];
                    var nextFrameRot = boneFrames.rotations[(int)Math.Ceiling(framerot)];

                    Vector3 stepPos = (nextFrameTrans - prevFrameTrans) * (float)(framepos - Math.Truncate(framepos));

                    var intQuat = Quaternion.Slerp(prevFrameRot, nextFrameRot, (float)(framerot - Math.Truncate(framerot)));

                    bonePosesInterpretated[f].Add(new BonePosition(
                        prevFrameTrans + stepPos,
                        new Vector4(intQuat.Xyz, intQuat.W), n));

                }

            }

            for (int i = 0; i < frames.Length; i++)
            {
                frames[i] = new AnimationFrame(bonePosesInterpretated[i].ToArray());
            }
            
        }

        int ReadPointer()
        {
            return (int)_reader.BaseStream.Position + _reader.ReadInt32();
        }
    }
}
