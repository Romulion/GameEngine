using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

namespace Toys
{
    public class UniformBufferSystem : UniformBuffer
    {
        const int defaultAligment = 16;
        const int size = 2 * defaultAligment;
        public UniformBufferSystem(int bindingPoint) : base(size, "system", bindingPoint)
        {
        }

        /// <summary>
        /// sets screen params (width, heigth, 1/width, 1/heigth)
        /// </summary>
        /// <param name="vec"></param>
        public void SetScreenSpace(Vector4 vec)
        {
            SetVector4(vec, 0);
        }

        /// <summary>
        /// time since program start (t/20, t, t*2, t*3)
        /// </summary>
        /// <param name="vec"></param>
        public void SetTime(Vector4 vec)
        {
            SetVector4(vec, defaultAligment);
        }
    }
}
