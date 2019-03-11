using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using OpenTK.Graphics;
using System.Diagnostics;

namespace Toys
{
	public delegate void queue();
	public class CoreEngine : GameWindow
    {

        GraphicsEngine gEngine;
        public static PhysicsEngine pEngine;
        queue task;

        //time controll
        Stopwatch stopwatch;
        float elapsed = 0.01f;

        public Scene mainScene;

		public CoreEngine() : base (640,480, new GraphicsMode(32,8,8,4))
        {
            Instalize();
            stopwatch = new Stopwatch();
        }

        void Instalize()
        {
            
            Title = "MMD Test";
            try
            {
                mainScene = new Scene();
                gEngine = new GraphicsEngine(mainScene);
                pEngine = new PhysicsEngine();

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

			pEngine.World.DebugDrawer = new PhysicsDebugDraw(pEngine.World);
        }



        //
        void Update(object sender, FrameEventArgs e)
        {
            stopwatch.Start();
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
            
            //test physics
            foreach (var node in mainScene.GetNodes())
            {
                node.phys.Update();
            }
            pEngine.Update(elapsed);
            foreach (var node in mainScene.GetNodes())
            {
                node.phys.PostUpdate();
            }
            
        }


        void OnRender (object sender, FrameEventArgs e)
        {
			//render main scene
			gEngine.Render();
			//render physics
			(pEngine.World.DebugDrawer as PhysicsDebugDraw).DrawDebugWorld();

			SwapBuffers();

            stopwatch.Stop();
            elapsed = stopwatch.ElapsedMilliseconds;
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
