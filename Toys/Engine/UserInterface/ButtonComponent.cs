using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace Toys
{
    enum ButtonStates
    {
        Normal,
        Hover,
        Clicked,
    }
    public class ButtonComponent : VisualComponent
    {
        public Action OnClick;
        static Material defaultMaterial;
        ShaderUniform shaderUniform;
        ShaderUniform colorMask;
        public static Texture2D Texture;

        Vector3 color;
        internal ButtonStates State { get; private set;}
        public ButtonComponent() : base(typeof(ButtonComponent))
        {
            Material = defaultMaterial;
            shaderUniform = Material.UniManager.GetUniform("model");
            colorMask = Material.UniManager.GetUniform("color_mask");
            color = Vector3.One;
        }

        static ButtonComponent()
        {
            ShaderSettings ss = new ShaderSettings();
            RenderDirectives rd = new RenderDirectives();
            string path = "Toys.Resourses.shaders.";
            string vs = ShaderManager.ReadFromAssetStream(path + "TextureImage.vsh");
            string fs = ShaderManager.ReadFromAssetStream(path + "ButtonColor.fsh");
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

        internal override void Draw()
        {

            Material.ApplyMaterial();
            colorMask.SetValue(color);
            shaderUniform.SetValue(Node.GetTransform.GlobalTransform);
            if (Texture)
                Texture.BindTexture();
            base.Draw();
        }

        internal void CkickedState()
        {
            if (State == ButtonStates.Hover)
            {
                OnClick();
                State = ButtonStates.Clicked;
                color = new Vector3(0.5f,0.5f,0.5f);
            }
        }


        internal void Hover()
        {
            if (State == ButtonStates.Clicked || State == ButtonStates.Normal)
            {
                State = ButtonStates.Hover;
                color = new Vector3(0.8f, 1, 0.8f);
            }
        }


        internal void Normal()
        {
            if (State == ButtonStates.Clicked || State == ButtonStates.Hover)
            {
                State = ButtonStates.Normal;
                color = new Vector3(1, 1, 1);
            }
        }
    }
}
