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
            shaderUniform = Material.UniformManager.GetUniform("model");
            colorMask = Material.UniformManager.GetUniform("color_mask");
            color = Vector4.One;
            Material.ApplyMaterial();
            colorMask.SetValue(color);
        }

        internal override void Draw(Matrix4 worldTransform)
        {
            
            Material.ApplyMaterial();
            colorMask.SetValue(color);
            shaderUniform.SetValue(Node.GetTransform.GlobalTransform * worldTransform);
            /*
            Console.WriteLine(1111);
            Console.WriteLine(Node.GetTransform.GlobalTransform);
            Console.WriteLine(Node.GetTransform.GlobalTransform * worldTransform);
            Console.WriteLine(new Vector4(1,1,0,1) * Node.GetTransform.GlobalTransform * worldTransform);
            Console.ReadLine();
            */
            if (Texture)
                Texture.BindTexture();
            base.Draw(worldTransform);
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
