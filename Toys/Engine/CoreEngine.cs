#define PHYS

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

		internal static GraphicsEngine gEngine;
		internal static PhysicsEngine pEngine;
        internal static ScriptingEngine sEngine;
        internal static CoreEngine core;
        public static Time time;
        queue task;

		float elapsed = 0.01f;

		public Scene mainScene;

		public CoreEngine() : base(640, 480, new GraphicsMode(32, 8, 8, 4))
		{
			Instalize();
			core = this;
		}

		void Instalize()
		{

			Title = "MMD Test";
			try
			{
				mainScene = new Scene();
				gEngine = new GraphicsEngine(mainScene);
				pEngine = new PhysicsEngine();
                sEngine = new ScriptingEngine();
                time = new Time();
                Load += OnLoad;
				UpdateFrame += Update;
				RenderFrame += OnRender;
                Closing += CoreEngine_Closing;
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				Console.ReadKey();
			}
		}

        private void CoreEngine_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            pEngine.Dispose();
        }

        //for Load event
        //
        void OnLoad(object sender, EventArgs e)
		{
			VSync = VSyncMode.On;
			gEngine.OnLoad();
			Resize += (s, ev) =>
			{
				gEngine.Resize(Width, Height);
			};
		}



		//
		void Update(object sender, FrameEventArgs e)
		{
            time.Start();
            elapsed = (float)(UpdateTime + RenderTime);
            sEngine.Awake();
            sEngine.Start();
            sEngine.Update();
            //mesh morpher
            if (task != null)
			{
				task();
				task = null;
			}
			mainScene.Update(elapsed);
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
            //physics
#if (PHYS)
            //stopwatch.Start();
            pEngine.Update(elapsed);
            //stopwatch.Stop();
            //Console.WriteLine(stopwatch.ElapsedTicks * ((double)1000 / Stopwatch.Frequency));
            //stopwatch.Reset();
#endif
            sEngine.PreRender();
            time.updagteTime = time.Stop();
        }


        void OnRender (object sender, FrameEventArgs e)
        {
            time.Start();
            //render main scene
            gEngine.Render();
            //render physics
            //(pEngine.World.DebugDrawer as PhysicsDebugDraw).DrawDebugWorld();
            time.renderTime = time.Stop();
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
