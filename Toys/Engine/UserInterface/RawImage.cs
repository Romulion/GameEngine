using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

namespace Toys
{
    public class RawImage : VisualComponent
    {
        ShaderUniform shaderUniform;
        /// <summary>
        /// Draw texture
        /// </summary>
        public Texture2D Texture;
        ShaderUniform colorMask;
        Vector4 color;

        static RawImage()
        {
        }
        public RawImage() : base(typeof(RawImage))
        {
            //Material = defaultMaterial;
            shaderUniform = Material.UniManager.GetUniform("model");
            colorMask = Material.UniManager.GetUniform("color_mask");
            color = Vector4.One;
            Material.ApplyMaterial();
            colorMask.SetValue(color);
        }

        internal override void Draw()
        {
            
            Material.ApplyMaterial();
            colorMask.SetValue(color);
            shaderUniform.SetValue(Node.GetTransform.GlobalTransform);
            if (Texture)
                Texture.BindTexture();
            base.Draw();
        }

        public override VisualComponent Clone()
        {
            var img = new RawImage();
            img.Material = Material;
            img.Texture = Texture;
            img.color = color;
            return img;
        }
    }
}
