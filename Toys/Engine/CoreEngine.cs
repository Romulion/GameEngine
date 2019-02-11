using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using OpenTK.Graphics;

namespace Toys
{
	public delegate void queue();
	public class CoreEngine : GameWindow
    {

        GraphicsEngine gEngine;
        queue task;

        public Scene mainScene;

		public CoreEngine() : base (640,480, new GraphicsMode(32,8,8,4))
        {
            Instalize();
        }

        void Instalize()
        {
            Title = "MMD Test";
            try
            {
                mainScene = new Scene();
                gEngine = new GraphicsEngine(mainScene);

                Load += OnLoad;
                UpdateFrame += Update;
                RenderFrame += OnRender;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
				Console.ReadKey();
            }
        }

        //for Load event
        //
        void OnLoad(object sender, EventArgs e)
        {
            VSync = VSyncMode.On;
			gEngine.OnLoad();
			mainScene.camera.Control(this);
            Resize += (s, ev) => {
                gEngine.Resize(Width,Height);
            };
            
        }

        //
        void Update(object sender, FrameEventArgs e)
        {
			//mesh morpher
			if (task != null)
			{
				task();
				task = null;
			}
            mainScene.Update();
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
        }


        void OnRender (object sender, FrameEventArgs e)
        {
			gEngine.Render();
			SwapBuffers();
		}


		public queue addTask
		{
			set
			{
				if (task == null)
					task = value;
				else
					task += value;
			}
		}
    }
}
