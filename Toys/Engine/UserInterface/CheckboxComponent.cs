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
    class CheckboxComponent : InteractableComponent
    {
        static Material defaultMaterial;
        ShaderUniform shaderUniform;
        ShaderUniform colorMask;
        public static Texture2D Texture;

        Vector4 color;
        Vector2 cursorPrev;

        public ScrollMode ScrollDirection { get; set; }
        internal ButtonStates State { get; private set; }
        public CheckboxComponent() : base(typeof(CheckboxComponent))
        {
            Material = defaultMaterial;
            shaderUniform = Material.UniManager.GetUniform("model");
            colorMask = Material.UniManager.GetUniform("color_mask");
            color = Vector4.One;
            ScrollDirection = ScrollMode.Vertical;
        }

        static CheckboxComponent()
        {
            ShaderSettings ss = new ShaderSettings();
            RenderDirectives rd = new RenderDirectives();
            string path = "Toys.Resourses.shaders.";
            string vs = ShaderManager.ReadFromAssetStream(path + "UIElement.vsh");
            string fs = ShaderManager.ReadFromAssetStream(path + "UIElement.fsh");
            ss.TextureDiffuse = true;
            defaultMaterial = new MaterialCustom(ss, rd, vs, fs);
            defaultMaterial.Name = "Texture";
            var assembly = System.Reflection.IntrospectionExtensions.GetTypeInfo(typeof(Texture2D)).Assembly;
            var pic = new System.Drawing.Bitmap(assembly.GetManifestResourceStream("Toys.Resourses.textures.button2.png"));
            Texture = new Texture2D(pic, TextureType.Toon, "def");
        }

        internal override void AddComponent(UIElement nod)
        {
            CoreEngine.gEngine.UIEngine.buttons.Add(this);
            base.AddComponent(nod);
        }

        internal override void RemoveComponent()
        {
            CoreEngine.gEngine.UIEngine.buttons.Remove(this);
            base.RemoveComponent();
        }

        internal override void ClickDownState()
        {

        }

        internal override void ClickUpState()
        {

        }

        internal override void Hover()
        {

        }


        internal override void Normal()
        {

        }

        internal override void Unload()
        {
            base.Unload();
        }

        internal override void Draw()
        {

            base.Draw();
        }

        public override VisualComponent Clone()
        {
            throw new NotImplementedException();
        }
    }
}
