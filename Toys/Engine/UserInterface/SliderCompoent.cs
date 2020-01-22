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
        Vector3 color;
        internal ButtonStates State { get; private set; }

        /// <summary>
        /// from 0 to 1
        /// </summary>
        float value = 0.7f;

        public SliderCompoent() : base(typeof(ButtonComponent))
        {
            Material = defaultMaterial;
            shaderUniform = Material.UniManager.GetUniform("model");
            colorMask = Material.UniManager.GetUniform("color_mask");
            color = Vector3.One;
        }

        static SliderCompoent()
        {
            ShaderSettings ss = new ShaderSettings();
            RenderDirectives rd = new RenderDirectives();
            string path = "Toys.Resourses.shaders.";
            string vs = ShaderManager.ReadFromAssetStream(path + "UIElement.vsh");
            string fs = ShaderManager.ReadFromAssetStream(path + "UIElement.fsh");
            ss.TextureDiffuse = true;
            defaultMaterial = new MaterialCustom(ss, rd, vs, fs);
            defaultMaterial.Name = "Texture";
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
            trans.M42 += trans.M22 * 0.30f;
            trans.M22 *= 0.4f;
            bgTexture?.BindTexture();
            colorMask.SetValue(Vector3.Zero);
            shaderUniform.SetValue(trans);
            base.Draw();

            //draw fill gauge
            trans = Node.GetTransform.GlobalTransform;
            trans.M11 *= value;
            trans.M41 = Node.GetTransform.GlobalRect.Left * 2 - (1 - trans.M11);
            trans.M42 += trans.M22 * 0.24f;
            trans.M22 *= 0.43f;
            fillTexture?.BindTexture();
            colorMask.SetValue(Vector3.One);
            shaderUniform.SetValue(trans);
            base.Draw();

            //draw slider button
            /*
            bgTexture?.BindTexture();
            colorMask.SetValue(color);
            //colorMask.SetValue(color);
            shaderUniform.SetValue(Node.GetTransform.GlobalTransform);
            
            base.Draw();
            */
        }

        internal override void Hover()
        {

        }

        internal override void ClickUpState()
        {
            if (State == ButtonStates.Clicked)
                State = ButtonStates.Normal;
        }

        internal override void ClickDownState()
        {
            if (State == ButtonStates.Normal)
                State = ButtonStates.Clicked;

        }

        internal override void Normal()
        {

        }

        internal override void Unload()
        {
            base.Unload();
        }

        internal override void PositionUpdate(float x, float y)
        {
            
            var oldValue = value;
            var trans = Node.GetTransform.GlobalRect;
            if (x <= trans.Left)
                value = 0;
            else if (x >= trans.Right)
                value = 1;
            else
                value = (x - trans.Left) / trans.Width;
            if (oldValue != value)
                OnValueChanged?.Invoke();
        }
    }
    
}
