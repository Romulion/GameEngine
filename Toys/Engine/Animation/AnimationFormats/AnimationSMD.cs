using System;
using System.Collections.Generic;
using System.IO;
using OpenTK.Mathematics;

namespace Toys
{
    /// <summary>
    /// valve's SMD animation parser
    /// transform calculated relative to parent's bone
    /// rotation performed in order x y z
    /// doesnt support scale
    /// </summary>
    internal class AnimationSMD : IAnimationLoader
    {
        string _path;
        Stream _stream;
        Dictionary<string, int> _bones = new Dictionary<string, int>();
        List<AnimationFrame> _frames = new List<AnimationFrame>();

        internal AnimationSMD(Stream stream, string path)
        {
            _path = path;
            _stream = stream;
        }
        
        public Animation Load()
        {
            string line;
            var file = new StreamReader(_stream);

            file.ReadLine();

            while ((line = file.ReadLine()) != null)
            {
                if (line == "nodes")
                    ReadNodes(file);
                if (line == "skeleton")
                    ReadFrames(file);
            }

            var animation = new Animation(_frames.ToArray(), _bones);
            animation.GetRotationType = Animation.RotationType.EulerXYZ;
            animation.TransType = Animation.TransformType.LocalAbsolute;
            return animation;
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
                        _frames.Add(new AnimationFrame(pos.ToArray()));
                    pos = new List<BonePosition>();
                }
                else
                {
                    string[] elements = line.Split(new char[] { ' ' }, 2);
                    int id = Int32.Parse(elements[0]);

                    float[] coord = StringParser.readFloatArray(elements[1]);
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
                _bones.Add(elements2[1], id);
            }

        }
    }
}
