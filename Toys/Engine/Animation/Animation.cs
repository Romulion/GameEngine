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
		Dictionary<int, string> bones = new Dictionary<int, string>();


		public Animation (AnimationFrame[] frams, Dictionary<int, string> boneReference)
        {
            frames = frams;
			bones = boneReference;
        }
    }
}
