﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace Toys
{
    public class TextBox : VisualComponent
    {
        public string Text { get; private set; }
        Vector2 pos = Vector2.Zero;
        float scale = 1;

        public TextCanvas textCanvas { get; private set; }

        TextRenderer textRenderer;
    
        public TextBox() : base (typeof(TextBox))
        {
            textRenderer = CoreEngine.gEngine.TextRender;
            textCanvas = textRenderer.CreateCanvas();
            textCanvas.alignHorizontal = TextAlignHorizontal.Left;
            textCanvas.alignVertical = TextAlignVertical.Center;
        }

        public void SetText(string text)
        {
            Text = text;
            textCanvas.Text = text;
            textRenderer.UpdateText(textCanvas);
        }

        public void SetScale(float scale)
        {
            textCanvas.Scale = scale;
            if (textCanvas.Text != "")
                textRenderer.UpdateText(textCanvas);
        }

        internal override void Unload()
        {
        }


        internal override void AddComponent(UIElement node)
        {
            CoreEngine.gEngine.TextRender.textBoxes.Add(this);
            Node = node;
        }

        internal override void RemoveComponent()
        {
            if (Node)
                Node = null;
            CoreEngine.gEngine.TextRender.textBoxes.Remove(this);
        }

        internal override void Draw()
        {
        }

        public override VisualComponent Clone()
        {
            var text = new TextBox();
            text.Text = Text;
            text.pos = pos;
            text.scale = scale;
            textCanvas.CloneTo(text.textCanvas);
            textRenderer.UpdateText(text.textCanvas);
            return text;
        }
    }
}
