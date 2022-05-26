using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

namespace Toys
{
    /// <summary>
    /// Scrollbar Component
    /// Set on scrollbox Parent and link scrollbox
    /// </summary>
    public class ScrollBarComponent : InteractiveComponent
    {
        public Action OnValueChanged;
        public ScrollBoxComponent ScrollBox
        {
            get; private set;
        }
        ShaderUniform shaderUniform;
        ShaderUniform colorMask;
        static Texture2D bgTexture;
        static Texture2D fillTexture;
        Vector4 color;
        internal ButtonStates State { get; private set; }
        private float value;
        /// <summary>
        /// Slider value
        /// from 0 to 1
        /// </summary>
        public float Value
        {
            get
            {
                return value;
            }
            set
            {
                if (value > 1)
                    this.value = 1;
                else if (value < 0)
                    this.value = 0;
                else
                    this.value = value;
            }
        }
        /// <summary>
        /// Knob size in pixels
        /// </summary>
        public float ButtonSize = 20;
        /// <summary>
        /// Scroll Bar Heigth
        /// </summary>
        public float ScrollBarSize = 20;

        public ScrollBarComponent() : base(typeof(ScrollBarComponent))
        {
            //Material = defaultMaterial;
            shaderUniform = Material.UniformManager.GetUniform("model");
            colorMask = Material.UniformManager.GetUniform("color_mask");
            color = Vector4.One;
            bgTexture = null;
            fillTexture = null;
            base.IsAllowDrag = true;
            Value = 1;
        }

        //Load Default Data
        static ScrollBarComponent()
        {
        }

        public void SetScrollBox(ScrollBoxComponent component)
        {
            ScrollBox = component;
            ScrollBox.AddScrollBar(this);
        }

        internal override void AddComponent(UIElement nod)
        {
            CoreEngine.GfxEngine.UIEngine.buttons.Add(this);
            base.AddComponent(nod);
        }

        internal override void RemoveComponent()
        {
            CoreEngine.GfxEngine.UIEngine.buttons.Remove(this);
            base.RemoveComponent();
        }

        internal override void Draw(Matrix4 worldTransform)
        {
            if (!ScrollBox)
                return;

            Material.ApplyMaterial();
            //draw bg
            var trans = Node.GetTransform.GlobalTransform;
            trans.M41 += trans.M11 - ScrollBarSize;
            trans.M11 = ScrollBarSize;

            bgTexture?.BindTexture();
            colorMask.SetValue(new Vector4(Vector3.Zero, 1));
            shaderUniform.SetValue(trans);
            base.Draw(worldTransform * worldTransform);

            //knob size
            var scrollSize = ScrollBox.Node.GetTransform.GlobalTransform.M22;
            var trackSize = Node.GetTransform.GlobalTransform.M22;
            var buttonSize = (float)(trackSize / scrollSize);
            buttonSize = (buttonSize > 1) ? 1 : buttonSize;
            buttonSize *= trackSize;
            ButtonSize = buttonSize;

            //draw slider button
            //trans = Node.GetTransform.GlobalTransform;
            trans.M42 += (trans.M22 - ButtonSize) * Value;
            trans.M22 = ButtonSize;
            bgTexture?.BindTexture();
            colorMask.SetValue(color);
            shaderUniform.SetValue(trans * worldTransform);
            base.Draw(worldTransform);
            
        }

        internal override void Hover()
        {

        }

        internal override void ClickUpState()
        {
            if (State == ButtonStates.Clicked)
            {
                State = ButtonStates.Normal;
                color = new Vector4(Vector3.One * 0.9f, 1);
            }
        }

        internal override void ClickDownState()
        {
            if (State == ButtonStates.Normal)
            {
                State = ButtonStates.Clicked;
                color = new Vector4(Vector3.One * 0.6f, 1);
            }

        }

        internal override void Normal()
        {
            if (State != ButtonStates.Normal)
            {
                State = ButtonStates.Normal;
                color = new Vector4(Vector3.One * 0.9f, 1);
            }
        }

        protected override void Unload()
        {
            base.Unload();
        }

        internal override void PositionUpdate(float x, float y)
        {
            if (!ScrollBox)
                return;

            y *= Node.ParentCanvas.CanvasScale;
            x *= Node.ParentCanvas.CanvasScale;

            var oldValue = Value;
            var trans = Node.GetTransform.GlobalRect; 
            var button = ButtonSize / 2;
            if (y >= trans.Bottom - button)
                Value = 1;
            else if (y <= trans.Top + button)
                Value = 0;
            else
                Value = (y - trans.Top - button) / (trans.Height - ButtonSize);

            if (oldValue != Value)
            {
                OnValueChanged?.Invoke();
                ScrollBox.UpdatePositionScrollbox(new Vector2(0, 1 - Value));
            }
        }

        public override VisualComponent Clone()
        {
            var scrollbar = new ScrollBarComponent();
            scrollbar.Material = Material;
            scrollbar.color = color;

            return scrollbar;
        }
    }

}
