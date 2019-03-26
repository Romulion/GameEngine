using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toys
{
    class AnimationFrame
    {
        public BonePosition[] bones { get; private set; }

        public AnimationFrame(BonePosition[] pos)
        {
            bones = pos;
        }
    }
}
