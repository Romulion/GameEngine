using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toys
{
    class Animator : Component
    {
        BoneController bones;
        Animation anim;
        bool isPlaing = false;
        float time = 0f;
        float length = 0;
        float framelength = 0;

        public Animator(BoneController bc) : base(typeof(Animator))
        {
            bones = bc;
        }

        public void Update(float delta)
        {

        }

        public void Play(Animation anim)
        {
            this.anim = anim;
            isPlaing = true;
            length = anim.frames.Length / (float)anim.framerate;
            framelength = 1 / (float)anim.framerate;
        }

        internal override void Unload()
        {
    
        }
    }
}
