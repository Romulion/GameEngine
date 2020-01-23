using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace Toys
{
    /// <summary>
    /// test prototype
    /// </summary>
    class UIMaskComponent : VisualComponent
    {
        static Material defaultMaterial;
        ShaderUniform shaderUniform;
        public static Texture2D texture;
        ShaderUniform colorMask;
        Vector4 color;

        public UIMaskComponent() : base(typeof(UIMaskComponent))
        {
            Material = defaultMaterial;
            shaderUniform = Material.UniManager.GetUniform("model");
            colorMask = Material.UniManager.GetUniform("color_mask");
            color = Vector4.One * 0.1f;
        }

        static UIMaskComponent()
        {
            ShaderSettings ss = new ShaderSettings();
            RenderDirectives rd = new RenderDirectives();
            string path = "Toys.Resourses.shaders.";
            string vs = ShaderManager.ReadFromAssetStream(path + "UIElement.vsh");
            string fs = ShaderManager.ReadFromAssetStream(path + "UIElement.fsh");
            ss.TextureDiffuse = true;
            defaultMaterial = new MaterialCustom(ss, rd, vs, fs);
            defaultMaterial.Name = "Mask";
            texture = Texture2D.LoadEmpty();
        }

        internal override void Draw()
        {
            Material.ApplyMaterial();
            shaderUniform.SetValue(Node.GetTransform.GlobalTransform);
            texture?.BindTexture();
            base.Draw();
        }


        public override VisualComponent Clone()
        {
            throw new NotImplementedException();
        }
    }
}
