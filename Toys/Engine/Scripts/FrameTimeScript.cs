using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

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
            canvas = Node.AddComponent<Canvas>();
            canvas.Mode = Canvas.RenderMode.Overlay;
            var root = new UIElement();
            img = (RawImage)root.AddComponent<RawImage>();
            text = (TextBox)root.AddComponent<TextBox>();
            rect = root.GetTransform;
            text.textCanvas.colour = Vector3.Zero;
            text.textCanvas.alignVertical = TextAlignVertical.Center;
            text.Scale = 0.7f;
            rect.anchorMax = new Vector2(0, 1);
            rect.anchorMin = new Vector2(0, 1);
            rect.offsetMin = new Vector2(20,-32);
            rect.offsetMax = new Vector2(160,-8);

            canvas.Add2Root(root);
        }

        void Update()
        {
            if (frames >= framesMax)
            {
                text.Text = (update / frames).ToString("C2") + " " + (render / frames).ToString("C2");
                frames = 0;
                update = 0;
                render = 0;
            }
            update += CoreEngine.Time.UpdateTime;
            render += CoreEngine.Time.RenderTime;
            frames++;
        }
    }
}
