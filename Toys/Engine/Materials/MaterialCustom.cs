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
            ShaderSettings = shdrsett;
            RenderDirrectives = rdir;
            vs = vertShader;
            fs = fragShader;
            CreateShader();
        }

        private void CreateShader()
        {
            shaderProgram = ShaderConstructor.CreateShader(vs, fs);
            shaderProgram.ApplyShader();
            CreateShader(shaderProgram);
        }

        internal override void ApplyMaterial()
        {
            base.ApplyMaterial();
            foreach (var uni in UniManager.uniforms)
                uni.Assign();
        }

        public override Material Clone()
        {
            var material = new MaterialPMX(ShaderSettings, RenderDirrectives);
            foreach (var texture in textures)
                material.SetTexture(texture.Value, texture.Key);

            return material;
        }
    }
}
