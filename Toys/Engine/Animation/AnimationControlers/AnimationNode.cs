using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toys
{
    public class AnimationNode
    {
        public Animation MainAnimation { get; private set; } 
        public string Name { get; set; }
        /// <summary>
        /// Play this when Animation ends
        /// </summary>
        public AnimationNode NextAnimation;
        /// <summary>
        /// Dont Repeat
        /// </summary>
        public bool Repeat = true;
        public float Speed = 1;

        public List<AnimationTransition> Transitions { get; private set; }

        public AnimationNode(Animation anim)
        {
            MainAnimation = anim;
            Transitions = new List<AnimationTransition>();
        }
    }
}
