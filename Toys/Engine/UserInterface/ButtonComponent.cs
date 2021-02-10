using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

namespace Toys
{
    public class ButtonComponent : InteractableComponent
    {
        /// <summary>
        /// Event proceeded when button clicked
        /// </summary>
        public Action OnClick;

        //Graphics data
        ShaderUniform shaderUniform;
        ShaderUniform colorMask;
        new static Texture2D defaultTexture;

        /// <summary>
        /// Button texture
        /// </summary>
        public Texture2D Texture;

        //color mask of element
        Vector4 color;
        internal ButtonStates State { get; private set;}
        public ButtonComponent() : base(typeof(ButtonComponent))
        {
            //Material = defaultMaterial;
            shaderUniform = Material.UniManager.GetUniform("model");
            colorMask = Material.UniManager.GetUniform("color_mask");
            color = Vector4.One;
            Texture = defaultTexture;
            base.IsAllowDrag = false;
        }

        //load default data
        static ButtonComponent()
        {
            /*
            ShaderSettings ss = new ShaderSettings();
            RenderDirectives rd = new RenderDirectives();
            string path = "Toys.Resourses.shaders.";
            string vs = ShaderManager.ReadFromAssetStream(path + "UIElement.vsh");
            string fs = ShaderManager.ReadFromAssetStream(path + "UIElement.fsh");
            ss.TextureDiffuse = true;
            defaultMaterial = new MaterialCustom(ss, rd, vs, fs);
            defaultMaterial.Name = "Texture";
            */
            var assembly = System.Reflection.IntrospectionExtensions.GetTypeInfo(typeof(Texture2D)).Assembly;
            using (var pic = new System.Drawing.Bitmap(assembly.GetManifestResourceStream("Toys.Resourses.textures.button2.png")))
            {
                defaultTexture = new Texture2D(pic, TextureType.Toon, "Toys.Resourses.textures.button2.png");
            }
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
            Texture?.BindTexture();
            base.Draw();
        }

        internal override void ClickDownState()
        {
            if (State == ButtonStates.Hover)
            {
                State = ButtonStates.Clicked;
                color = new Vector4(0.5f,0.5f,0.5f, 1);
            }
        }

        internal override void ClickUpState()
        {
            OnClick?.Invoke();
            Normal();
        }


        internal override void Hover()
        {
            if (State == ButtonStates.Clicked || State == ButtonStates.Normal)
            {
                State = ButtonStates.Hover;
                color = new Vector4(0.7f, 1, 0.7f, 1);
            }
        }

        internal override void Normal()
        {
            if (State == ButtonStates.Clicked || State == ButtonStates.Hover)
            {
                State = ButtonStates.Normal;
                color = new Vector4(1, 1, 1, 1);
            }
        }

        public override VisualComponent Clone()
        {
            var button = new ButtonComponent();
            button.OnClick = OnClick;
            button.Material = Material;
            button.Texture = Texture;
            button.color = color;

            return button;
        }
    }
}
