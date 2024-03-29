﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

namespace Toys
{
    public class CheckboxComponent : InteractiveComponent
    {
        //Graphics data
        static Texture2D Texture;
        static Texture2D chekMarkDefault;
        ShaderUniform shaderUniform;
        ShaderUniform colorMask;
        Vector4 color;

        /// <summary>
        /// Checkbox size in pixels
        /// </summary>
        public float BoxSize = 20;

        /// <summary>
        /// Event triggered when checkbox clicked
        /// </summary>
        public Action OnChange;

        /// <summary>
        /// Current state of checkbox
        /// </summary>
        public bool IsOn { get; set; }
        internal ButtonStates State { get; private set; }
        public CheckboxComponent() : base(typeof(CheckboxComponent))
        {
            //Material = defaultMaterial;
            shaderUniform = Material.UniformManager.GetUniform("model");
            colorMask = Material.UniformManager.GetUniform("color_mask");
            color = Vector4.One;
        }

        //Load Default Data
        static CheckboxComponent()
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
            Texture = Texture2D.LoadEmpty();
            var assembly = System.Reflection.IntrospectionExtensions.GetTypeInfo(typeof(Texture2D)).Assembly;
            using (var pic = new System.Drawing.Bitmap(assembly.GetManifestResourceStream("Toys.Resourses.textures.checkmark-24-512.png")))
                chekMarkDefault = new Texture2D(pic, TextureType.Toon, "Toys.Resourses.textures.checkmark-24-512.png");
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
            IsOn = !IsOn;
            OnChange?.Invoke();
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

        protected override void Unload()
        {
            base.Unload();
        }

        internal override void Draw(Matrix4 worldTransform)
        {
            Material.ApplyMaterial();

            //place button in right corner
            var trans = Node.GetTransform.GlobalTransform;
            trans.M41 += trans.M11 - BoxSize;
            trans.M11 = trans.M22  = BoxSize;

            shaderUniform.SetValue(trans * worldTransform);
            colorMask.SetValue(color);

            //draw box
            Texture?.BindTexture();
            base.Draw(worldTransform);

            //draw mark
            if (IsOn)
            {
                colorMask.SetValue(Vector4.One);
                chekMarkDefault?.BindTexture();
                base.Draw(worldTransform);
            }
        }

        public override VisualComponent Clone()
        {
            var checkbox = new CheckboxComponent();
            checkbox.OnChange = OnChange;
            checkbox.Material = Material;
            checkbox.color = color;
            checkbox.IsOn = IsOn;
            return checkbox;
        }
    }
}
