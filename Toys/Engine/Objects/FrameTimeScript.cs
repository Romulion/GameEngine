using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        void Awake()
        {
            text = new TextBox();
        }

        void Update()
        {
            if (frames >= framesMax)
            {
                text.SetText((update / frames).ToString("C2") + "  " + (render / frames).ToString("C2"));
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
