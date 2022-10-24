using System;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;

namespace Toys
{
    public class GLWindow : GameWindow
    {
        public CoreEngine Engine { get; private set; }
        internal static GLWindow gLWindow;
        public static KeyboardState Keyboard;
        bool pause = false;
        bool pauseKey = false;
        bool visibleKey = false;
        public GLWindow() : base(new GameWindowSettings(), new NativeWindowSettings() { NumberOfSamples = 4 })
        {
            base.Size = new OpenTK.Mathematics.Vector2i(640,480);
            gLWindow = this;
            Keyboard = gLWindow.KeyboardState;
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
            Load += OnStart;
            
            UpdateFrame += Update;
            RenderFrame += Render;
            Closed += () => Engine.Close();

        }

        void OnStart()
        {
            VSync = VSyncMode.On;

            Engine.OnLoad();
            Resize += (ev) =>
            {
                if (Size.X != 0 && Size.Y != 0)
                    Engine.Resize(Size.X, Size.Y);
            };
        }

        void Update(FrameEventArgs e)
        {
            if (IsFocused)
            {
                if (KeyboardState.IsKeyDown(Keys.F))
                {
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                }
                if (KeyboardState.IsKeyDown(Keys.L))
                {
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                }
                if (KeyboardState.IsKeyDown(Keys.P))
                {
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Point);
                }

                if (KeyboardState.IsKeyDown(Keys.O) && !pauseKey)
                {
                    pause = !pause;
                    pauseKey = true;
                }
                else if (!KeyboardState.IsKeyDown(Keys.O) && pauseKey)
                    pauseKey = false;

                if (KeyboardState.IsKeyDown(Keys.V) && !visibleKey)
                {
                    IsVisible = !IsVisible;
                    visibleKey = true;
                }
                else if (!KeyboardState.IsKeyDown(Keys.V) && visibleKey)
                    visibleKey = false;
            }

            if (!pause)
                Engine.Update();
        }



        void Render(FrameEventArgs e)
        {
            if (!pause)
                Engine.Render();
            SwapBuffers();

        }
    }
}
