using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toys
{
    
    class Animation
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

        public int framerate = 24;
        public readonly AnimationFrame[] frames;
		public readonly Dictionary<string, int> bones = new Dictionary<string, int>();
        public RotationType Type { get; internal set; }
        public TransformType TransType { get; internal set; }

        public Animation (AnimationFrame[] frams, Dictionary<string, int> boneReference)
        {
            frames = frams;
			bones = boneReference;
        }
    }
}
