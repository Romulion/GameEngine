using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace Toys
{
    public class TextInputComponent : InteractableComponent
    {
        static Texture2D Texture;
        static Texture2D chekMarkDefault;
        static Material defaultMaterial;
        ShaderUniform shaderUniform;
        ShaderUniform colorMask;
        Vector4 color;
        internal readonly TextBox Text = new TextBox();

        public Action OnChange;
        public bool IsFocused { get; private set; }
        internal ButtonStates State { get; private set; }
        public TextInputComponent() : base(typeof(CheckboxComponent))
        {
            Material = defaultMaterial;
            shaderUniform = Material.UniManager.GetUniform("model");
            colorMask = Material.UniManager.GetUniform("color_mask");
            color = Vector4.One;
            Text.textCanvas.colour = Vector3.Zero;
            Text.textCanvas.alignHorizontal = TextAlignHorizontal.Left;
            Text.textCanvas.alignVertical = TextAlignVertical.Bottom;
        }

        static TextInputComponent()
        {
            ShaderSettings ss = new ShaderSettings();
            RenderDirectives rd = new RenderDirectives();
            string path = "Toys.Resourses.shaders.";
            string vs = ShaderManager.ReadFromAssetStream(path + "UIElement.vsh");
            string fs = ShaderManager.ReadFromAssetStream(path + "UIElement.fsh");
            ss.TextureDiffuse = true;
            defaultMaterial = new MaterialCustom(ss, rd, vs, fs);
            defaultMaterial.Name = "Texture";
            Texture = Texture2D.LoadEmpty();
        }

        internal override void AddComponent(UIElement nod)
        {
            CoreEngine.gEngine.UIEngine.buttons.Add(this);
            base.AddComponent(nod);
            Text.AddComponent(nod);
        }

        internal override void RemoveComponent()
        {
            if (CoreEngine.iHandler.SetTextInputContext == this)
                CoreEngine.iHandler.SetTextInputContext = null;

            CoreEngine.gEngine.UIEngine.buttons.Remove(this);
            base.RemoveComponent();
            Text.RemoveComponent();
        }

        internal override void ClickDownState()
        {
            if (State == ButtonStates.Hover)
            {
                State = ButtonStates.Clicked;
                color = new Vector4(Vector3.One * 0.6f, 1);
            }
        }

        internal override void ClickUpState()
        {
            IsFocused = true;
            if (CoreEngine.iHandler.SetTextInputContext != this)
                CoreEngine.iHandler.SetTextInputContext = this;
            Normal();
        }

        internal override void Hover()
        {
            if (State != ButtonStates.Hover)
            {
                State = ButtonStates.Hover;
                color = new Vector4(Vector3.One * 0.9f, 1);
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

        internal override void Unload()
        {
            base.Unload();
        }

        internal override void Draw()
        {
            Material.ApplyMaterial();
            var trans = Node.GetTransform.GlobalTransform;

            shaderUniform.SetValue(trans);
            colorMask.SetValue(color);

            //draw box
            Texture?.BindTexture();
            base.Draw();

            //draw caret
            if (IsFocused)
            {
                trans.M41 += Text.textCanvas.Width / CoreEngine.gEngine.Width;
                trans.M11 = trans.M22 * 0.5f;
                trans.M22 *= 0.1f;
                shaderUniform.SetValue(trans);
                colorMask.SetValue(new Vector4(Vector3.Zero, 0.5f));
                chekMarkDefault?.BindTexture();
                base.Draw();
            }
        }

        internal void Deactivate()
        {
            IsFocused = false;
        }

        public override VisualComponent Clone()
        {
            var input = new TextInputComponent();
            input.OnChange = OnChange;
            input.Material = Material;
            input.color = color;
            return input;
        }
    }
}
