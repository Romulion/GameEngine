using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toys
{
    class Animation
    {
        public int framerate = 30;
        public readonly AnimationFrame[] frames;

        public Animation (AnimationFrame[] frams)
        {
            frames = frams;
        }
    }
}
