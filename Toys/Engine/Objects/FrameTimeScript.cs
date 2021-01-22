using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace Toys
{
    /// <summary>
    /// Script for displaing update and render times
    /// </summary>
    public class FrameTimeScript : ScriptingComponent
    {
        long frames = 1;
        long framesMax = 60;
        double update = 0, render = 0;
        TextBox text;
        Canvas canvas;
        RectTransform rect;
        RawImage img;
        void Awake()
        {
            canvas = (Canvas)Node.AddComponent<Canvas>();
            canvas.Root = new UIElement();
            img = (RawImage)canvas.Root.AddComponent<RawImage>();
            text = (TextBox)canvas.Root.AddComponent<TextBox>();
            rect = canvas.Root.GetTransform;
            text.textCanvas.colour = Vector3.Zero;
            text.textCanvas.alignVertical = TextAlignVertical.Center;
            rect.anchorMax = new Vector2(0, 1);
            rect.anchorMin = new Vector2(0, 1);
            rect.offsetMin = new Vector2(20,-32);
            rect.offsetMax = new Vector2(160,-8);
        }

        void Update()
        {
            if (frames >= framesMax)
            {
                text.SetText((update / frames).ToString("C2") + " " + (render / frames).ToString("C2"));
                frames = 0;
                update = 0;
                render = 0;
            }
            update += CoreEngine.time.UpdateTime;
            render += CoreEngine.time.RenderTime;
            frames++;
        }
    }
}
