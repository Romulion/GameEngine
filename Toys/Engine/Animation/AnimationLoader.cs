using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using OpenTK;

namespace Toys
{
    class AnimationLoader
    {
        Dictionary<int, string>  bones = new Dictionary<int, string>();
        List<AnimationFrame> frames = new List<AnimationFrame>();
        private AnimationLoader() { }

        public static Animation Load(string path)
        {
            AnimationLoader anim = new AnimationLoader();
            string line;
            var file =  new StreamReader(path);
            
            file.ReadLine();

                while ((line = file.ReadLine()) != null)
                {
                    if (line == "nodes")
                        anim.ReadNodes(file);
                    if (line == "skeleton")
                        anim.ReadFrames(file);
                }

            return new Animation(anim.frames.ToArray(),anim.bones);

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
                    string name = "";
                    if (bones.ContainsKey(id))
                        name = bones[id];

                    float[] coord = StringParser.readFloat(elements[1]);
                    Vector3 p = new Vector3(coord[0], coord[1] , coord[2]);
                    Vector3 r = new Vector3(coord[3], coord[4], coord[5]);
                    var bone = new BonePosition(p * 0.01f, r, name);
                    bone.boneId = id;
                    pos.Add(bone);
                }
            }
        }

        private void ReadNodes(StreamReader file)
        {
            string line;
            while ((line = file.ReadLine()) != "end")
            {
                string[] elements =  line.Split(new char[] { ' ' }, 2);
                int id = Int32.Parse(elements[0]);
                string[] elements2 = line.Split(new char[] { '"' }, 2);
                bones.Add(id,elements2[1]);
            }

        }
    }
}
