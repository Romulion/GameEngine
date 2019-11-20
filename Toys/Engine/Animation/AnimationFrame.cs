using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toys
{
    public class AnimationFrame
    {
        internal BonePosition[] BonePoritions { get; private set; }

        internal AnimationFrame(BonePosition[] pos)
        {
            BonePoritions = pos;
        }
    }
}
