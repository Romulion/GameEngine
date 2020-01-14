using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace Toys
{
    public class BackgroundPlane : BackgroundBase
    {

        public BackgroundPlane()
        {

        }

        public override void DrawBackground(Camera cam)
        {
            backgroundShader.ApplyShader();
        }

        internal override void Unload()
        {
           
        }
    }
}
