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
        BinaryReader _file;
        Reader _reader;
        float _speed = 1;

        Dictionary<string, int> bones = new Dictionary<string, int>();
        AnimationFrame[] frames;

        public AnimationLMD(string path)
        {
            _path = path;
            using (Stream fs = File.OpenRead(_path))
            {
                _file = new BinaryReader(fs);
                _reader = new Reader(_file);
                _reader.Encoding = 1;
                ReadBones();

                _file.BaseStream.Position = 100;
                _speed = _file.ReadSingle();
            }
        }

        public Animation Load()
        {
            var animation = new Animation(frames.ToArray(), bones);
            animation.Type = Animation.RotationType.Quaternion;
            animation.TransType = Animation.TransformType.LocalAbsolute;
            animation.Framerate = (int)(frames.Length / _speed);
            return animation;
        }

        bool CheckHeader()
        {
            _file.BaseStream.Position = 28;
            //going to info block
            _file.BaseStream.Position = ReadPointer();
            int type = _file.ReadInt32();
            return (type == 4);
        }

        void ReadBones()
        {

            _file.BaseStream.Position = 116;
            int boneCount = _file.ReadInt32();
            int[] bonePosArray = new int[boneCount];
            for (int i = 0; i < boneCount; i++)
                bonePosArray[i] = ReadPointer();

            BoneUnsorted[] bua = new BoneUnsorted[boneCount];
            int n = 0;
            foreach (int offset in bonePosArray)
            {
                int Magic = _file.ReadInt32();
                _file.BaseStream.Position = offset + 4;
                _file.BaseStream.Position = ReadPointer();
                string boneName = _reader.readString();
                _file.BaseStream.Position = offset + 20;
                _file.BaseStream.Position = ReadPointer();
                //4 bytes id; 4 *2 bytes unknown anim strucs pointers
                _file.BaseStream.Position += 12;

                var bu = new BoneUnsorted();     
                int rotTransfOffset = ReadPointer();
                int tranTransfOffset = ReadPointer();
                //reading rotations | skipping id
                _file.BaseStream.Position = rotTransfOffset + 4;
                int rotTransfTimeOffset = ReadPointer();
                _file.BaseStream.Position = ReadPointer();
                int transformElCount = _file.ReadInt32();
                transformElCount /= 4;
                bu.rotations = new Quaternion[transformElCount];
                for (int i = 0; i < transformElCount; i++)
                    bu.rotations[i] = new Quaternion(_reader.readVector3(), _file.ReadSingle());
                //rotation transform time table
                _file.BaseStream.Position = rotTransfTimeOffset;
                int timeElCount = _file.ReadInt32();
                bu.rotationsTimestamps = new float[timeElCount];
                for (int i = 0; i < timeElCount; i++)
                    bu.rotationsTimestamps[i] = _file.ReadSingle();

                //read transforms
                _file.BaseStream.Position = tranTransfOffset + 4;
                int transTransfTimeOffset = ReadPointer();
                _file.BaseStream.Position = ReadPointer();
                transformElCount = _file.ReadInt32();
                transformElCount /= 3;
                bu.positions = new Vector3[transformElCount];
                for (int i = 0; i < transformElCount; i++)
                    bu.positions[i] = _reader.readVector3();
                //rotation transform time table
                _file.BaseStream.Position = transTransfTimeOffset;
                timeElCount = _file.ReadInt32();
                bu.positionsTimestamps = new float[timeElCount];
                for (int i = 0; i < timeElCount; i++)
                    bu.positionsTimestamps[i] = _file.ReadSingle();

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
            return (int)_file.BaseStream.Position + _file.ReadInt32();
        }
    }
}
