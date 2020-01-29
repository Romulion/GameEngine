using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace Toys
{
    /// <summary>
    /// filling from left to right
    /// 
    /// </summary>
    public class SliderCompoent : InteractableComponent
    {
        public Action OnValueChanged;
        static Material defaultMaterial;
        ShaderUniform shaderUniform;
        ShaderUniform colorMask;
        static Texture2D bgTexture;
        static Texture2D fillTexture;
        static Texture2D padTexture;
        Vector4 color;
        internal ButtonStates State { get; private set; }

        /// <summary>
        /// from 0 to 1
        /// </summary>
        public float Value = 0.7f;
        public float ButtonSize = 20;
        public float SliderBoxSize = 7;
        public float SliderFillSize = 5;

        public SliderCompoent() : base(typeof(SliderCompoent))
        {
            //Material = defaultMaterial;
            shaderUniform = Material.UniManager.GetUniform("model");
            colorMask = Material.UniManager.GetUniform("color_mask");
            color = Vector4.One;
        }

        static SliderCompoent()
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
            //var assembly = System.Reflection.IntrospectionExtensions.GetTypeInfo(typeof(Texture2D)).Assembly;
            //var pic = new System.Drawing.Bitmap(assembly.GetManifestResourceStream("Toys.Resourses.textures.button2.png"));
            //bgTexture =  new Texture2D(pic, TextureType.Toon, "def");
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
            //draw bg
            //shink heigth by 60%
            var trans = Node.GetTransform.GlobalTransform;
            //trans.M42 += trans.M22 * 0.31f;
            //trans.M22 *= 0.38f;
            trans.M42 += (trans.M22 - SliderBoxSize) * 0.5f;
            trans.M22 = SliderBoxSize;

            bgTexture?.BindTexture();
            colorMask.SetValue(new Vector4(Vector3.Zero,1));
            shaderUniform.SetValue(trans);
            base.Draw();

            //draw fill gauge
            //trans = Node.GetTransform.GlobalTransform;
            trans.M11 *= Value;
            //trans.M41 = Node.GetTransform.GlobalRect.Left * 2 - (1 - trans.M11);
            //trans.M42 += trans.M22 * 0.3f;
            //trans.M22 *= 0.4f;
            trans.M42 += (trans.M22 - SliderFillSize) * 0.5f;
            trans.M22 = SliderFillSize;

            fillTexture?.BindTexture();
            colorMask.SetValue(Vector4.One);
            shaderUniform.SetValue(trans);
            base.Draw();

            //draw slider button
            trans = Node.GetTransform.GlobalTransform;
            trans.M41 += trans.M11 * Value - ButtonSize * 0.5f;
            trans.M11 = trans.M22 =  ButtonSize;
            bgTexture?.BindTexture();
            colorMask.SetValue(color);
            shaderUniform.SetValue(trans);
            base.Draw();
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
            if (x <= trans.Left)
                Value = 0;
            else if (x >= trans.Right)
                Value = 1;
            else
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
