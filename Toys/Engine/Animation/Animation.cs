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

        public int framerate = 24;
        public readonly AnimationFrame[] frames;
		public readonly Dictionary<string, int> bones = new Dictionary<string, int>();
        public readonly RotationType Type;


        public Animation (AnimationFrame[] frams, Dictionary<string, int> boneReference, RotationType type)
        {
            Type = type;
            frames = frams;
			bones = boneReference;
        }
    }
}
