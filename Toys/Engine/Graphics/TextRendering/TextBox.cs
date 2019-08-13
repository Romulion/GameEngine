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

        TextCanvas textobj;

        TextRenderer tr;
    
        public TextBox() : base (typeof(TextBox))
        {
            tr = GraphicsEngine.textRender;
            textobj = tr.CreateCanvas();
            textobj.pos = new Vector2(25);
        }

        public void SetText(string text)
        {
            textobj.text = text;
            tr.AddText(textobj);
        }

        internal override void Unload()
        {
            
        }
    }
}
