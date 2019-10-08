using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace Toys
{
    class TextBox : Component
    {
        string Text;
        Vector2 pos = Vector2.Zero;
        float scale = 1;

        TextCanvas textCanvas;

        TextRenderer textRenderer;
    
        public TextBox() : base (typeof(TextBox))
        {
            textRenderer = GraphicsEngine.TextRender;
            textCanvas = textRenderer.CreateCanvas();
            textCanvas.Position = new Vector2(25);
        }

        public void SetText(string text)
        {
            textCanvas.Text = text;
            textRenderer.UpdateText(textCanvas);
        }

        internal override void Unload()
        {
            
        }


        internal override void AddComponent(SceneNode node)
        {
        }

        internal override void RemoveComponent()
        {
        }
    }
}
