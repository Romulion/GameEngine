using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using OpenTK;
using System.Windows.Forms;

namespace Toys
{
    internal struct BoneMotionData
    {
        internal int frame;
        internal BonePosition bonepos;

        internal BoneMotionData(int frm, BonePosition bonp)
        {
            frame = frm;
            bonepos = bonp;
        }
    }

    class AnimationVMD : IAnimationLoader
    {

        float multipler = 0.1f;

        string Path;
        BinaryReader file;
        Reader reader;

        Dictionary<string, int> bones = new Dictionary<string, int>();
        AnimationFrame[] frames;

        public AnimationVMD(string path)
        {
            Path = path;
        }

        public Animation Load()
        {
            using (Stream fs = File.OpenRead(Path))
            {

                file = new BinaryReader(fs);
                reader = new Reader(file);

                string header = new string(file.ReadChars(30));

                byte[] name = file.ReadBytes(20);

                //converting SHIFT-JIS to utf16
                string modelName = jis2utf(name);

                ReadFrames();
            }
            var animation = new Animation(frames.ToArray(), bones);
            animation.Type = Animation.RotationType.Quaternion;
            animation.TransType = Animation.TransformType.LocalRelative;
            animation.framerate = 30;
            return animation;
        }

        void ReadFrames()
        {
            int length = file.ReadInt32();
            int framesCount = 0;
            int lastBoneIndex = 0;

            Dictionary<int, List<BoneMotionData>> bonposes = new Dictionary<int, List<BoneMotionData>>();

            for (int i = 0; i < length; i++) {

                string bone = jis2utf(file.ReadBytes(15));
                //remove trash bytes
                bone = bone.Remove(bone.IndexOf('\0'));
                
                int boneIndex = 0;

                //bones referense
                if (!bones.ContainsKey(bone))
                {  
                    bones.Add(bone, lastBoneIndex);
                    boneIndex = lastBoneIndex++;
                }
                else
                    boneIndex = bones[bone];

                int frame = file.ReadInt32();

                List<BoneMotionData> framebones = null;

                if (bonposes.ContainsKey(boneIndex))
                {
                    framebones = bonposes[boneIndex];
                }
                else
                {
                    framebones = new List<BoneMotionData>();
                    bonposes.Add(boneIndex, framebones);
                }

                Vector3 pos = reader.readVector3() * multipler;
                Vector4 rot = reader.readVector4();
                if (frame > framesCount)
                    framesCount = frame;
                //??? interpolation data ???
                file.ReadBytes(64);

                frames = new AnimationFrame[framesCount];
                framebones.Add(new BoneMotionData(frame, new BonePosition(pos, rot, boneIndex)));
            }
            NormalizeFrames(bonposes);

        }

        //interpolating data on frames
        void NormalizeFrames(Dictionary<int, List<BoneMotionData>> bonposes)
        {
            List<BonePosition>[] bonePosesIntepr = new List<BonePosition>[frames.Length];
            for(int i = 0; i < bonePosesIntepr.Length; i++)
                bonePosesIntepr[i] = new List<BonePosition>();

            for (int n = 0; n < bones.Count; n++)
            {
                var boneFrames = bonposes[n];
                boneFrames = (boneFrames.OrderBy(b => b.frame)).ToList();
                
                for(int f = 0; f < boneFrames.Count; f++)
                {
                    var prevFrame = boneFrames[f];
                    var nextFrame = (f == boneFrames.Count-1) ? boneFrames[0] : boneFrames[ f + 1 ];

                    int fcount = nextFrame.frame - prevFrame.frame;
                    var prev = prevFrame.bonepos;
                    Quaternion prevQuat = new Quaternion(prev.rotation.Xyz, prev.rotation.W);
                    Quaternion nextQuat = new Quaternion(nextFrame.bonepos.rotation.Xyz, nextFrame.bonepos.rotation.W); 

                    if (fcount > 0)
                    {
                        
                        Vector3 stepPos = (nextFrame.bonepos.position - prev.position) / (float)fcount;
                        float stepRot = 1 / (float)fcount;
                        
                        for (int frm = 0; frm < fcount; frm++)
                        {
                            var intQuat = Quaternion.Slerp(prevQuat, nextQuat, (float)frm * stepRot);
                            bonePosesIntepr[prevFrame.frame + frm].Add(new BonePosition(
                                prev.position + stepPos * frm,
                                new Vector4(intQuat.Xyz, intQuat.W), prev.boneId));
                        }
                    }
                    else if (fcount < 0)
                    {
                        fcount += frames.Length;                      
                        int firstPart = frames.Length - prevFrame.frame;
                        Vector3 stepPos = (nextFrame.bonepos.position - prev.position) / (float)fcount;
                        float stepRot = 1 / (float)fcount;

                        //to last frame
                        for (int frm = 0; frm < firstPart; frm++)
                        {
                            var intQuat = Quaternion.Slerp(prevQuat, nextQuat, (float)frm * stepRot);
                            bonePosesIntepr[prevFrame.frame + frm].Add(new BonePosition(
                                prev.position + stepPos * frm,
                                new Vector4(intQuat.Xyz, intQuat.W), prev.boneId));
                        }

                        //from first frame
                        for (int frm = 0; frm < nextFrame.frame; frm++)
                        {
                            var intQuat = Quaternion.Slerp(prevQuat, nextQuat, (float)(firstPart + frm) * stepRot);
                            bonePosesIntepr[frm].Add(new BonePosition(
                                prev.position + stepPos * (firstPart + frm),
                                new Vector4(intQuat.Xyz, intQuat.W), prev.boneId));
                        }
                    }
                    else
                    {
                        for (int frm = 0; frm < frames.Length; frm++)
                        {
                            bonePosesIntepr[prevFrame.frame + frm].Add(new BonePosition(
                                prev.position,
                                prev.rotation, prev.boneId));
                        }
                    }
                }
                
            }

            for (int i = 0; i < frames.Length; i++)
            {
                frames[i] = new AnimationFrame(bonePosesIntepr[i].ToArray());
                //if (i < 100)
                    //Console.WriteLine("{0}  {1}",i,frames[i].bones.Length);
            }
            //Console.WriteLine(bones.Count);
        }

        string jis2utf(byte[] bytes)
        {
            return Encoding.Unicode.GetString(Encoding.Convert(Encoding.GetEncoding(932), Encoding.Unicode, bytes));
        }
    }
}
