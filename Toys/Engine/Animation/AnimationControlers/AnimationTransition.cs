using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toys
{
    class AnimationTransition
    {
        public Predicate<AnimationController> Condition { get; private set; }
        public AnimationNode TargetAnimation { get; private set; }

        public AnimationTransition(Predicate<AnimationController> condition, AnimationNode target)
        {
            Condition = condition;
            TargetAnimation = target;
        }
    }
}
