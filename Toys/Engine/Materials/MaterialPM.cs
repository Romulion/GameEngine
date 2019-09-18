using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toys
{
    class MaterialPM : Material
    {
        public MaterialPM(ShaderSettings shdrsett, RenderDirectives rdir) : base()
        {
            shdrSettings = shdrsett;
            rndrDirrectives = rdir;
            CreateShader();
        }

        private void CreateShader()
        {
            string path = "Toys.Resourses.shaders.";
            string vs = ShaderManager.ReadFromAssetStream(path + "PM.vsh");
            string fs = ShaderManager.ReadFromAssetStream(path + "PM.fsh");
            shdr = ShaderConstructor.CreateShader(vs, fs);
            CreateShader(shdr);
        }

        public override Material Clone()
        {
            var material = new MaterialPMX(shdrSettings, rndrDirrectives);
            foreach (var texture in textures)
                material.SetTexture(texture.Value, texture.Key);

            return material;
        }
    }
}
