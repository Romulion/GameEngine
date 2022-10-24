using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using OpenTK.Mathematics;

namespace Toys
{
    class AnimationLMD : IAnimationLoader
    {
        private class BoneUnsorted
        {
            internal Vector3[] position;
            internal float[] positionTimestamps;
            internal Quaternion[] rotation;
            internal float[] rotationTimestamps;
            internal Vector3[] scale;
            internal float[] scaleTimestamps;
        }

        string _path;
        Reader _reader;
        float _speed = 1;

        Dictionary<string, int> bones = new Dictionary<string, int>();
        AnimationFrame[] frames;
        Animation _animation = new Animation();

        public AnimationLMD(Stream stream, string path)
        {
            _path = path;
            using (stream)
            {
                _reader = new Reader(stream);
                _reader.EncodingType = 1;
                ReadBones();

                _reader.BaseStream.Position = 100;
                _speed = _reader.ReadSingle();
            }
        }

        public Animation Load()
        {
            //var animation = new Animation(frames.ToArray(), bones);
            _animation.bones = bones;
            _animation.GetRotationType = Animation.RotationType.Quaternion;
            _animation.TransType = Animation.TransformType.LocalAbsolute;
            _animation.Framerate = 30;
            return _animation;
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
            foreach (int bonePointer in bonePosArray)
            {
                _reader.BaseStream.Position = ReadPointer(bonePointer + 4);
                string boneName = _reader.readString();
                int animComponentPointer = ReadPointer(bonePointer + 20);

                _reader.BaseStream.Position = bonePointer + 12;
                int animType = _reader.ReadInt32();

                //Anim pointers
                var bu = new BoneUnsorted();
                int scalePointer = ReadPointer(animComponentPointer + 8);
                int rotationPointer = ReadPointer(animComponentPointer + 12);
                int translationPointer = ReadPointer(animComponentPointer + 16);
                
                //Scale
                int scaleTimeOffset = ReadPointer(scalePointer + 4);
                bu.scale = ReadVector3Table(ReadPointer(scalePointer + 8));
                bu.scaleTimestamps = ReadTimeTable(scaleTimeOffset);
                

                //Rotation
                int rotTransfTimeOffset = ReadPointer(rotationPointer + 4);
                _reader.BaseStream.Position = ReadPointer(rotationPointer + 8);
                int transformElCount = _reader.ReadInt32();
                if (animType == 1)
                {
                    transformElCount /= 4;
                    bu.rotation = new Quaternion[transformElCount];
                    for (int i = 0; i < transformElCount; i++)
                        bu.rotation[i] = new Quaternion(_reader.readVector3(), _reader.ReadSingle());
                }
                else if (animType == 4)
                {
                    transformElCount /= 3;
                    bu.rotation = new Quaternion[transformElCount];
                    for (int i = 0; i < transformElCount; i++)
                        bu.rotation[i] = new Quaternion(_reader.readVector3());
                }
                //Time
                bu.rotationTimestamps = ReadTimeTable(rotTransfTimeOffset);

                //Translation
                int transTransfTimeOffset = ReadPointer(translationPointer + 4);
                bu.position = ReadVector3Table(ReadPointer(translationPointer + 8));
                //Time
                bu.positionTimestamps = ReadTimeTable(transTransfTimeOffset);

                bones.Add(boneName, n);
                bua[n] = bu;
                n++;
            }

            NormalizeFrames(bua);

        }

        Vector3[] ReadVector3Table(int offset)
        {
            _reader.BaseStream.Position = offset;
            var transformElCount = _reader.ReadInt32();
            transformElCount /= 3;
            var data = new Vector3[transformElCount];
            for (int i = 0; i < transformElCount; i++)
                data[i] = _reader.readVector3();
            return data;
        }

        float[] ReadTimeTable(int offset)
        {
            _reader.BaseStream.Position = offset;
            var transformElCount = _reader.ReadInt32();
            var data = new float[transformElCount];            
            for (int i = 0; i < transformElCount; i++)
                data[i] = _reader.ReadSingle();
            return data;
        }

        //creaating frames with aproximation
        void NormalizeFrames(BoneUnsorted[] bonesRaw)
        {
            int maxFrames = bonesRaw.Max((BoneUnsorted b) => { return Math.Max(b.position.Length, b.rotation.Length); });
            //frames = new AnimationFrame[maxFrames];


            List<BonePosition>[] bonePosesInterpretated = new List<BonePosition>[maxFrames];


            for (int i = 0; i < bonesRaw.Length; i++)
            {

                var boneName = bones.FirstOrDefault(x => x.Value == i).Key;
                var data = bonesRaw[i];
                var frames = GetFrames(data);

                for (int n = 0; n < frames.Length; n++)
                {
                    var pos = Vector3.Zero;
                    var rot = Quaternion.Identity;
                    var scl = Vector3.One;

                    //scale
                    float time = frames[n];
                    for (int b = 0; b < data.scaleTimestamps.Length; b++)
                    {
                        if (data.scaleTimestamps[b] == time)
                        {
                            scl = data.scale[b];
                            break;
                        }
                        else if (data.scaleTimestamps[b] >= time)
                        {
                            float delta = (time - data.scaleTimestamps[b - 1]) / (data.scaleTimestamps[b] - data.scaleTimestamps[b - 1]);
                            scl = data.scale[b - 1] + (data.scale[b] - data.scale[b - 1]) * delta;
                            break;
                        }
                    }

                    //position
                    for (int b = 0; b < data.positionTimestamps.Length; b++)
                    {
                        if (data.positionTimestamps[b] == time)
                        {
                            pos = data.position[b];
                            break;
                        }
                        else if (data.positionTimestamps[b] >= time)
                        {
                            float delta = (time - data.positionTimestamps[b - 1]) / (data.positionTimestamps[b] - data.positionTimestamps[b - 1]);
                            pos = data.position[b - 1] + (data.position[b] - data.position[b - 1]) * delta;
                            break;
                        }
                    }

                    //rotation
                    for (int b = 0; b < data.rotationTimestamps.Length; b++)
                    {


                        if (data.rotationTimestamps[b] == time)
                        {
                            rot = data.rotation[b];
                            break;
                        }
                        else if (data.rotationTimestamps[b] > time)
                        {

                            float delta = (time - data.rotationTimestamps[b - 1]) / (data.rotationTimestamps[b] - data.rotationTimestamps[b - 1]);
                            rot = Quaternion.Slerp(data.rotation[b - 1], data.rotation[b], delta);
                            break;
                        }
                    }


                    var bonePos = new KeyFrameBone(frames[n] * _speed * 2, new BonePosition(pos, rot, scl, i));
                    _animation.AddBoneKey(boneName, bonePos);

                }

            }
        }

        float[] GetFrames(BoneUnsorted data)
        {
            List<float> frames = new List<float>();
            for (int i = 0; i < data.positionTimestamps.Length; i++)
                frames.Add(data.positionTimestamps[i]);

            for (int i = 0; i < data.rotationTimestamps.Length; i++)
                frames.Add(data.rotationTimestamps[i]);

            for (int i = 0; i < data.scaleTimestamps.Length; i++)
                frames.Add(data.scaleTimestamps[i]);

            return frames.Distinct().OrderBy(x => x).ToArray();
        }

        int ReadPointer(int offset)
        {
            _reader.BaseStream.Position = offset;
            return offset + _reader.ReadInt32();
        }

        int ReadPointer()
        {
            return (int)_reader.BaseStream.Position + _reader.ReadInt32();
        }
    }
}