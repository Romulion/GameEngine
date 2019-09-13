using System;
using System.Collections.Generic;
using System.IO;
using OpenTK;

namespace Toys
{
    internal class AnimationSMD : IAnimationLoader
    {
        string Path;
        Dictionary<string, int> bones = new Dictionary<string, int>();
        List<AnimationFrame> frames = new List<AnimationFrame>();

        internal AnimationSMD(string path)
        {
            Path = path;

        }
        
        public Animation Load()
        {
            string line;
            var file = new StreamReader(Path);

            file.ReadLine();

            while ((line = file.ReadLine()) != null)
            {
                if (line == "nodes")
                    ReadNodes(file);
                if (line == "skeleton")
                    ReadFrames(file);
            }

            return new Animation(frames.ToArray(), bones,Animation.RotationType.EulerXYZ);
        }
        
        private void ReadFrames(StreamReader file)
        {
            string line;
            List<BonePosition> pos = null;
            while ((line = file.ReadLine()) != null)
            {

                if (line.StartsWith("time") || line == "end")
                {
                    if (pos != null)
                        frames.Add(new AnimationFrame(pos.ToArray()));
                    pos = new List<BonePosition>();
                }
                else
                {
                    string[] elements = line.Split(new char[] { ' ' }, 2);
                    int id = Int32.Parse(elements[0]);

                    float[] coord = StringParser.readFloat(elements[1]);
                    Vector3 p = new Vector3(coord[0], coord[1], coord[2]);
                    Vector4 r = new Vector4(coord[3], coord[4], coord[5],0);
                    var bone = new BonePosition(p * 0.01f, r, id);
                    pos.Add(bone);
                }
            }
        }

        private void ReadNodes(StreamReader file)
        {
            string line;
            while ((line = file.ReadLine()) != "end")
            {
                string[] elements = line.Split(new char[] { ' ' }, 2);
                int id = Int32.Parse(elements[0]);
                string[] elements2 = line.Split(new char[] { '"' }, 3);
                bones.Add(elements2[1], id);
            }

        }
    }
}
