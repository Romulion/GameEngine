using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;

namespace Toys
{
    public class GLWindow : GameWindow
    {
        public CoreEngine Engine { get; private set; }
        internal static GLWindow gLWindow;
        bool pause = false;
        bool pauseKey = false;
        bool visibleKey = false;
        public GLWindow() : base(640, 480, new GraphicsMode(32, 8, 8, 4))
        {
            gLWindow = this;
            Initialize();
        }

        public bool CheckContext{
            get
            {
                return Context.IsCurrent;
            }
        }

        void Initialize()
        {
            Engine = new CoreEngine();
            Load += OnLoad;
            UpdateFrame += Update;
            RenderFrame += Render;
            Closing += (s, ev) => Engine.Close();
        }


        void OnLoad(object sender, EventArgs e)
        {
            VSync = VSyncMode.On;
            Engine.OnLoad();
            Resize += (s, ev) =>
            {
                if (Width != 0 && Height != 0)
                    Engine.Resize(Width, Height);
            };
        }

        void Update(object sender, EventArgs e)
        {
            if (Focused)
            {
                var keystate = Keyboard.GetState();
                if (keystate[Key.Escape])
                {
                    Exit();
                }
                if (keystate[Key.F])
                {
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                }
                if (keystate[Key.L])
                {
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                }
                if (keystate[Key.P])
                {
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Point);
                }

                if (keystate[Key.O] && !pauseKey)
                {
                    pause = !pause;
                    pauseKey = true;
                }
                else if (!keystate[Key.O] && pauseKey)
                    pauseKey = false;

                if (keystate[Key.V] && !visibleKey)
                {
                    Visible = !Visible;
                    visibleKey = true;
                }
                else if (!keystate[Key.V] && visibleKey)
                    visibleKey = false;
            }

            if (!pause)
                Engine.Update();
        }

        void Render(object sender, EventArgs e)
        {
            if (!pause)
                Engine.Render();
            SwapBuffers();
        }
    }
}
