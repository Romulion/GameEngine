using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toys
{
    
    public class Animation
    {
        public enum RotationType
        {
            Quaternion,
            EulerXYZ,
            EulerZXY,
            EulerYZX,
        }

        public enum TransformType
        {
            LocalRelative,
            LocalAbsolute,
        }

        public int Framerate = 24;
        internal readonly AnimationFrame[] frames;
        internal readonly Dictionary<string, int> bones = new Dictionary<string, int>();
        public RotationType Type { get; internal set; }
        public TransformType TransType { get; internal set; }

        public Animation (AnimationFrame[] frams, Dictionary<string, int> boneReference)
        {
            frames = frams;
			bones = boneReference;
        }
    }
}
