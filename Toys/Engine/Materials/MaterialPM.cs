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
            ShaderSettings = shdrsett;
            RenderDirrectives = rdir;
            CreateShader();
        }
        private MaterialPM(ShaderSettings shdrsett, RenderDirectives rdir, Shader shader)
        {
            ShaderSettings = shdrsett;
            RenderDirrectives = rdir;
            shaderProgram = shader;
            CreateShader(shaderProgram);
        }

        private void CreateShader()
        {
            string path = "Toys.Resourses.shaders.";
            string vs = ShaderManager.ReadFromAssetStream(path + "PM.vsh");
            string fs = ShaderManager.ReadFromAssetStream(path + "PM.fsh");
            shaderProgram = ShaderConstructor.CreateShader(vs, fs);
            CreateShader(shaderProgram);
        }

        public override void ApplyMaterial()
        {
            base.ApplyMaterial();
            //foreach (var uni in UniManager.uniforms)
            //    uni.Assign();
        }


        public override Material Clone()
        {
            var material = new MaterialPM(ShaderSettings, RenderDirrectives,shaderProgram);
            foreach (var texture in textures)
                material.SetTexture(texture.Value, texture.Key);

            return material;
        }
    }
}
