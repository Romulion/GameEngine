using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toys
{
    public class BoneIK
    {
        public readonly int Target;

        public int LoopCount;
        public float AngleLimit;
        public readonly IKLink[] Links;

        public BoneIK(int targ, IKLink[] lnks)
        {
            Target = targ;
            Links = lnks;
        }

        
    }
}
