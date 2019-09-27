using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toys
{
    class ShaderConstructorNew
    {
        string vertexShaderRaw = "";
        string fragmentShaderRaw = "";
        public ShaderSettings shaderSettings { get; private set; }

        public ShaderConstructorNew(string vs, string fs, ShaderSettings ss)
        {
            GenerateVertexShaderRaw(vs);
            GenerateFragmentShaderRaw(fs);
        }

        void GenerateVertexShaderRaw(string vs)
        {

        }

        void GenerateFragmentShaderRaw(string fs)
        {

        }
    }
}
