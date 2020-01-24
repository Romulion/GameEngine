using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace Toys
{
    public class UIMaskComponent : VisualComponent
    {
        static Material defaultMaterial;
        ShaderUniform shaderUniform;
        public static Texture2D texture;
        ShaderUniform colorMask;
        Vector4 color;
        internal int MaskValue;

        public UIMaskComponent() : base(typeof(UIMaskComponent))
        {
            Material = defaultMaterial;
            shaderUniform = Material.UniManager.GetUniform("model");
            colorMask = Material.UniManager.GetUniform("color_mask");
            color = Vector4.One * 0.1f;
            AllowMultiple = false;
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

        internal override void AddComponent(UIElement nod)
        {
            nod.IsMask = true;
            CoreEngine.gEngine.UIEngine.visualComponents.Add(this);
            base.AddComponent(nod);
        }

        internal override void RemoveComponent()
        {
            CoreEngine.gEngine.UIEngine.visualComponents.Remove(this);
            base.RemoveComponent();
        }

        public override VisualComponent Clone()
        {
            var uimask = new UIMaskComponent();
            uimask.color = color;
            return uimask;
        }
    }
}
