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
        ShaderUniform colorUniform;
        Vector3 color;
        internal ButtonStates State { get; private set;}
        public ButtonComponent() : base(typeof(ButtonComponent))
        {
            Material = defaultMaterial;
            shaderUniform = Material.UniManager.GetUniform("model");
            colorUniform = Material.UniManager.GetUniform("col");
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
            colorUniform.SetValue(color);
            shaderUniform.SetValue(Node.GetTransform.GlobalTransform);
            base.Draw();
        }

        internal void CkickedState()
        {
            if (State == ButtonStates.Hover || State == ButtonStates.Normal)
            {
                OnClick();
                State = ButtonStates.Clicked;
                color = new Vector3(1,0,0);
            }
        }


        internal void Hover()
        {
            if (State == ButtonStates.Clicked || State == ButtonStates.Normal)
            {
                State = ButtonStates.Hover;
                color = new Vector3(0, 1, 0);
            }
        }


        internal void Normal()
        {
            if (State == ButtonStates.Clicked || State == ButtonStates.Hover)
            {
                State = ButtonStates.Normal;
                color = new Vector3(0, 0, 1);
            }
        }
    }
}
