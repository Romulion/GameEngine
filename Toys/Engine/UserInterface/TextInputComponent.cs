using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

namespace Toys
{
    public class TextInputComponent : InteractiveComponent
    {
        static Texture2D Texture;
        ShaderUniform shaderUniform;
        ShaderUniform colorMask;
        Vector4 color;
        int caretPadding = 2;
        internal Vector2 textPadding = new Vector2(2,5);
        internal readonly TextBox Text = new TextBox();

        /// <summary>
        /// Size of input caret
        /// </summary>
        public Vector2 CaretSize = new Vector2(10,2);

        /// <summary>
        /// Event triggered if Input unfocused
        /// </summary>
        public Action OnChange;

        /// <summary>
        /// Active for keyboard input
        /// </summary>
        public bool IsFocused { get; private set; }
        internal ButtonStates State { get; private set; }
        public TextInputComponent() : base(typeof(CheckboxComponent))
        {
            shaderUniform = Material.UniformManager.GetUniform("model");
            colorMask = Material.UniformManager.GetUniform("color_mask");
            color = Vector4.One;
            Text.textCanvas.colour = Vector3.Zero;
            Text.textCanvas.alignHorizontal = TextAlignHorizontal.Left;
            Text.textCanvas.alignVertical = TextAlignVertical.Bottom;
            Texture = null;
        }

        static TextInputComponent()
        {
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

        internal override void Draw(Matrix4 worldTransform)
        {
            Material.ApplyMaterial();
            var trans = Node.GetTransform.GlobalTransform;
            shaderUniform.SetValue(trans * worldTransform);
            colorMask.SetValue(color);

            //draw box
            Texture?.BindTexture();
            base.Draw(worldTransform);

            //draw caret
            if (IsFocused)
            {
                trans.M41 += Text.textCanvas.Width + caretPadding;
                trans.M11 = CaretSize.X;
                trans.M22 = CaretSize.Y;
                shaderUniform.SetValue(trans * worldTransform);
                colorMask.SetValue(new Vector4(Vector3.Zero, 0.5f));
                base.Draw(worldTransform);
            }
        }

        internal void Deactivate()
        {
            IsFocused = false;
            OnChange?.Invoke();
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
