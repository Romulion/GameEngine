using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toys
{
    class TestScript : ScriptingComponent
    {
        long frames = 1;
        double update = 0, render = 0;
        TextBox text;

        void Awake()
        {
            //text = new TextBox();
            //node.AddComponent(text);
        }

        void Update()
        {
            //update++;
            //update += .UpdateTime * 1000;
            //render += core.RenderTime * 1000;
            //text.SetText((update / frames).ToString("C1") + "  " + (render / frames).ToString("C1"));
            //frames++;
        }
    }
}
