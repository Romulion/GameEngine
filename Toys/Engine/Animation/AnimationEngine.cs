using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toys
{
    class AnimationEngine
    {
        internal List<Animator> animators = new List<Animator>();
        internal List<AnimationController> controllers = new List<AnimationController>();
        internal void Upadate(float time)
        {
            for (int i = 0; i < controllers.Count; i++)
                controllers[i].Update();

            for (int i = 0; i < animators.Count; i++)
                animators[i].Update(time);
        }
    }
}
