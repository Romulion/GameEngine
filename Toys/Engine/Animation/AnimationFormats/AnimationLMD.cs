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
            internal float[] posTime;
            internal Quaternion[] rotations;
            internal float[] rotTime;
        }

        string Path;
        BinaryReader file;
        Reader reader;
        float speed = 1;

        Dictionary<string, int> bones = new Dictionary<string, int>();
        AnimationFrame[] frames;

        public AnimationLMD(string path)
        {
            Path = path;
            using (Stream fs = File.OpenRead(Path))
            {
                file = new BinaryReader(fs);
                reader = new Reader(file);
                reader.encoding = 1;
                ReadBones();

                file.BaseStream.Position = 100;
                speed = file.ReadSingle();
            }
        }

        public Animation Load()
        {
            var animation = new Animation(frames.ToArray(), bones);
            animation.Type = Animation.RotationType.Quaternion;
            animation.TransType = Animation.TransformType.LocalAbsolute;
            animation.framerate = (int)(frames.Length / speed);
            return animation;
        }

        bool CheckHeader()
        {
            file.BaseStream.Position = 28;
            //going to info block
            file.BaseStream.Position = ReadPointer();
            int type = file.ReadInt32();
            return (type == 4);
        }

        void ReadBones()
        {

            file.BaseStream.Position = 116;
            int boneCount = file.ReadInt32();
            int[] bonePosArray = new int[boneCount];
            for (int i = 0; i < boneCount; i++)
                bonePosArray[i] = ReadPointer();

            BoneUnsorted[] bua = new BoneUnsorted[boneCount];
            int n = 0;
            foreach (int offset in bonePosArray)
            {
                int Magic = file.ReadInt32();
                file.BaseStream.Position = offset + 4;
                file.BaseStream.Position = ReadPointer();
                string boneName = reader.readString();
                file.BaseStream.Position = offset + 20;
                file.BaseStream.Position = ReadPointer();
                //4 bytes id; 4 *2 bytes unknown anim strucs pointers
                file.BaseStream.Position += 12;

                var bu = new BoneUnsorted();     
                int rotTransfOffset = ReadPointer();
                int tranTransfOffset = ReadPointer();
                //reading rotations | skipping id
                file.BaseStream.Position = rotTransfOffset + 4;
                int rotTransfTimeOffset = ReadPointer();
                file.BaseStream.Position = ReadPointer();
                int transformElCount = file.ReadInt32();
                transformElCount /= 4;
                bu.rotations = new Quaternion[transformElCount];
                for (int i = 0; i < transformElCount; i++)
                    bu.rotations[i] = new Quaternion(reader.readVector3(), file.ReadSingle());
                //rotation transform time table
                file.BaseStream.Position = rotTransfTimeOffset;
                int timeElCount = file.ReadInt32();
                bu.rotTime = new float[timeElCount];
                for (int i = 0; i < timeElCount; i++)
                    bu.rotTime[i] = file.ReadSingle();

                //read transforms
                file.BaseStream.Position = tranTransfOffset + 4;
                int transTransfTimeOffset = ReadPointer();
                file.BaseStream.Position = ReadPointer();
                transformElCount = file.ReadInt32();
                transformElCount /= 3;
                bu.positions = new Vector3[transformElCount];
                for (int i = 0; i < transformElCount; i++)
                    bu.positions[i] = reader.readVector3();
                //rotation transform time table
                file.BaseStream.Position = transTransfTimeOffset;
                timeElCount = file.ReadInt32();
                bu.posTime = new float[timeElCount];
                for (int i = 0; i < timeElCount; i++)
                    bu.posTime[i] = file.ReadSingle();

                bones.Add(boneName, n);
                bua[n] = bu;
                n++;
            }


            NormalizeFrames(bua);
            /*
            while (file.BaseStream.Position < dest)
            {
                int stride = file.ReadInt32();
                if (( stride + file.BaseStream.Position - 4) == dest)
                    Console.WriteLine("{0} {1}", file.BaseStream.Position - 4, stride + file.BaseStream.Position - 4);

                //Console.WriteLine(stride + file.BaseStream.Position -4);
                file.BaseStream.Position -= 3;
            }
            */
        }

        //creaating frames with aproximation
        void NormalizeFrames(BoneUnsorted[] bu)
        {
            int MaxFrames = bu.Max((BoneUnsorted b) => { return Math.Max(b.positions.Length, b.rotations.Length); });
            frames = new AnimationFrame[MaxFrames];

            List<BonePosition>[] bonePosesIntepr = new List<BonePosition>[MaxFrames];
            for (int i = 0; i < MaxFrames; i++)
                bonePosesIntepr[i] = new List<BonePosition>();

            float framePart = 1f / MaxFrames;
            for (int n = 0; n < bones.Count; n++)
            {
                var boneFrames = bu[n];
                float boneTransPart = 1f / boneFrames.positions.Length - 1;
                float boneRotPart = 1f / boneFrames.rotations.Length - 1;

                for (int f = 0; f < MaxFrames; f++)
                {
                    
                    float framepos = f * framePart * (boneFrames.positions.Length -1);
                    float framerot = f * framePart * (boneFrames.rotations.Length -1);
                    var prevFrameTrans = boneFrames.positions[(int)Math.Floor(framepos)];
                    var prevFrameRot = boneFrames.rotations[(int)Math.Floor(framerot)];
                    
                    var nextFrameTrans = boneFrames.positions[(int)Math.Ceiling(framepos)];
                    var nextFrameRot = boneFrames.rotations[(int)Math.Ceiling(framerot)];

                    Vector3 stepPos = (nextFrameTrans - prevFrameTrans) * (float)(framepos - Math.Truncate(framepos));

                    var intQuat = Quaternion.Slerp(prevFrameRot, nextFrameRot, (float)(framerot - Math.Truncate(framerot)));

                    bonePosesIntepr[f].Add(new BonePosition(
                        prevFrameTrans + stepPos,
                        new Vector4(intQuat.Xyz, intQuat.W), n));

                }

            }

            for (int i = 0; i < frames.Length; i++)
            {
                frames[i] = new AnimationFrame(bonePosesIntepr[i].ToArray());
            }
            
        }

        int ReadPointer()
        {
            return (int)file.BaseStream.Position + file.ReadInt32();
        }
    }
}
