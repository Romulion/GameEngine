using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

namespace Toys
{
    /// <summary>
    /// Slider Component
    /// filling from left to right
    /// </summary>
    public class SliderCompoent : InteractiveComponent
    {
        public Action OnValueChanged;
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
        public float Value {
            get {
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
        /// Slider Box Heigth
        /// </summary>
        public float SliderBoxSize = 7;
        /// <summary>
        /// Slider Box Fill Area Heigth
        /// </summary>
        public float SliderFillSize = 5;

        public SliderCompoent() : base(typeof(SliderCompoent))
        {
            //Material = defaultMaterial;
            shaderUniform = Material.UniformManager.GetUniform("model");
            colorMask = Material.UniformManager.GetUniform("color_mask");
            color = Vector4.One;
            bgTexture = null;
            fillTexture = null;
            base.IsAllowDrag = true;
        }

        //Load Default Data
        static SliderCompoent()
        {
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

        internal override void Draw(Matrix4 worldTransform)
        {
           
            Material.ApplyMaterial();
            //draw bg
            var trans = Node.GetTransform.GlobalTransform;
            trans.M42 += (trans.M22 - SliderBoxSize) * 0.5f;
            trans.M22 = SliderBoxSize;

            bgTexture?.BindTexture();
            colorMask.SetValue(new Vector4(Vector3.Zero,1));
            shaderUniform.SetValue(trans * worldTransform);
            base.Draw(worldTransform);

            //draw fill gauge
            trans.M11 *= Value;
            trans.M42 += (trans.M22 - SliderFillSize) * 0.5f;
            trans.M22 = SliderFillSize;

            fillTexture?.BindTexture();
            colorMask.SetValue(Vector4.One);
            shaderUniform.SetValue(trans * worldTransform);
            base.Draw(worldTransform);

            //draw slider button
            trans = Node.GetTransform.GlobalTransform;
            trans.M41 += trans.M11 * Value - ButtonSize * 0.5f;
            trans.M11 = trans.M22 =  ButtonSize;
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
                color = new Vector4(Vector3.One * 0.9f,1);
            }
        }

        internal override void ClickDownState()
        {
            if (State == ButtonStates.Normal)
            {
                State = ButtonStates.Clicked;
                color = new Vector4(Vector3.One * 0.6f,1);
            }

        }

        internal override void Normal()
        {
            if (State != ButtonStates.Normal)
            {
                State = ButtonStates.Normal;
                color = new Vector4(Vector3.One * 0.9f,1);
            }
        }

        internal override void Unload()
        {
            base.Unload();
        }

        internal override void PositionUpdate(float x, float y)
        {

            var oldValue = Value;
            var trans = Node.GetTransform.GlobalRect;

            Value = (x - trans.Left) / trans.Width;

            if (oldValue != Value)
                OnValueChanged?.Invoke();
        }

        public override VisualComponent Clone()
        {
            var slider = new SliderCompoent();
            slider.Material = Material;
            slider.color = color;

            return slider;
        }
    }
    
}
