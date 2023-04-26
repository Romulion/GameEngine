using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toys;
using OpenTK.Mathematics;

namespace ModelViewer
{
    class SceneTime : ScriptingComponent
    {
        double refTime;
        Canvas canvas;
        TextBox text;
        DateTime time;
        public float timeFloat { get; private set; }
        bool pause;

        public int Hour { get
            {
                return (int)timeFloat / 60;
            }
        }

        public int Minute
        {
            get
            {
                return (int)timeFloat % 60;
            }
        }

        void Awake()
        {
            refTime = CoreEngine.Time.TimeFromStart;
            canvas = Node.AddComponent<Canvas>();
            canvas.Mode = Canvas.RenderMode.Overlay;
            var root = new UIElement();
            //img = (RawImage)root.AddComponent<RawImage>();
            text = (TextBox)root.AddComponent<TextBox>();
            var rect = root.GetTransform;
            text.textCanvas.colour = Vector3.Zero;
            text.textCanvas.alignVertical = TextAlignVertical.Center;
            text.Scale = 0.7f;
            rect.anchorMax = new Vector2(1, 1);
            rect.anchorMin = new Vector2(1, 1);
            rect.offsetMin = new Vector2(-70, -32);
            rect.offsetMax = new Vector2(-20, -8);

            canvas.Add2Root(root);
            pause = false;
            time = DateTime.Parse("8:00");
            timeFloat = 60 * 8;
        }


        void Update()
        {
            if (pause)
                return;

            time = time.AddMinutes((CoreEngine.Time.TimeFromStart - refTime) / 1000);
            text.Text = time.ToShortTimeString();
            timeFloat += (float)(CoreEngine.Time.TimeFromStart - refTime) / 1000;

            refTime = CoreEngine.Time.TimeFromStart;

            //time rotation
            if (timeFloat > 24 * 60)
                timeFloat -= 24 * 60;
        }

        void OnPause()
        {
            canvas.IsActive = false;
            pause = true;
            
        }

        void OnResume()
        {
            canvas.IsActive = true;
            pause = false;
        }
    }
}
