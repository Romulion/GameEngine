using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

namespace Toys
{
    public class TextBox : VisualComponent
    {
        public string Text { get { return textCanvas.Text; }
            set
            {
                textCanvas.Text = value;
            }
        }
        Vector2 pos = Vector2.Zero;
        public float Scale
        {
            get { return textCanvas.Scale; }
            set
            {
                textCanvas.Scale = value;
            }
        }

        public TextCanvas textCanvas { get; private set; }

        TextRenderer textRenderer;
    
        public TextBox() : base (typeof(TextBox))
        {
            textRenderer = CoreEngine.GfxEngine.TextRender;
            textCanvas = textRenderer.CreateCanvas();
            textCanvas.alignHorizontal = TextAlignHorizontal.Left;
            textCanvas.alignVertical = TextAlignVertical.Center;
        }

        protected override void Unload()
        {
            textRenderer.UnloadText(textCanvas);
        }

        internal override void AddComponent(UIElement node)
        {
            
            CoreEngine.GfxEngine.TextRender.textBoxes.Add(this);
            base.AddComponent(node);
        }

        internal override void RemoveComponent()
        {
            CoreEngine.GfxEngine.TextRender.textBoxes.Remove(this);
            base.RemoveComponent();
        }

        internal override void Draw(Matrix4 worldTransform)
        {
            
            if (textCanvas.IsTextUpdated)
            {
                textRenderer.UpdateText(textCanvas);
                textCanvas.IsTextUpdated = false;
            }
            
            CoreEngine.GfxEngine.TextRender.Render(this, worldTransform);
        }

        public override VisualComponent Clone()
        {
            var text = new TextBox();
            text.pos = pos;
            textCanvas.CloneTo(text.textCanvas);
            textRenderer.UpdateText(text.textCanvas);
            return text;
        }
    }
}
