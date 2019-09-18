using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toys
{
    class MaterialCustom : Material
    {
        string vs;
        string fs;

        public MaterialCustom(ShaderSettings shdrsett, RenderDirectives rdir,string vertShader, string fragShader) : base()
        {
            shdrSettings = shdrsett;
            rndrDirrectives = rdir;
            vs = vertShader;
            fs = fragShader;
            CreateShader();
        }

        private void CreateShader()
        {
            shdr = ShaderConstructor.CreateShader(vs, fs);
            shdr.ApplyShader();
            CreateShader(shdr);
        }

        public override void ApplyMaterial()
        {
            base.ApplyMaterial();
            foreach (var uni in UniManager.uniforms)
                uni.Assign();
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
