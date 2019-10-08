using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace Toys
{
    class BackgroundPlane : BackgroundBase
    {

        public BackgroundPlane()
        {

        }

        public override void DrawBackground(Camera cam)
        {
            backgroundShader.ApplyShader();
        }
    }
}
