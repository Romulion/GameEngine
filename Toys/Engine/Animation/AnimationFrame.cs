using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toys
{
    class AnimationFrame
    {
        public BonePosition[] BonePoritions { get; private set; }

        public AnimationFrame(BonePosition[] pos)
        {
            BonePoritions = pos;
        }
    }
}
