using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace Toys
{
    public abstract class BackgroundBase
    {
        protected static Shader backgroundShader;
        protected Texture texture;
        public abstract void DrawBackground(Camera cam);
    }
}
