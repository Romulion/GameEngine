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
	public class CoreEngine 
	{

		internal static GraphicsEngine gEngine { get; private set; }
        public static PhysicsEngine pEngine { get; private set; }
        internal static ScriptingEngine sEngine { get; private set; }
        internal static CoreEngine ActiveCore { get; private set; }
        public static Time time { get; private set; }
        public static Time frameTimer { get; private set; }
        queue task;

		float elapsed = 0.01f;

		public Scene mainScene;

		public CoreEngine()
		{
			Instalize();
			ActiveCore = this;
		}

		void Instalize()
		{

			try
			{
				mainScene = new Scene();
				gEngine = new GraphicsEngine(mainScene);
				pEngine = new PhysicsEngine();
                sEngine = new ScriptingEngine();
                time = new Time();
                frameTimer = new Time();
                frameTimer.Start();
            }
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				Console.ReadKey();
			}
		}

        internal void Close()
        {
            pEngine.Dispose();
            sEngine.Destroy();
        }

        //for Load event
        //
        internal void OnLoad()
		{
			gEngine.OnLoad();
		}

        internal void Resize(int width, int height)
        {
            gEngine.Resize(width, height);
        }

		internal void Update()
		{
            elapsed = (float)frameTimer.Stop();
            elapsed *= .001f;
            frameTimer.Start();
            time.Start();
            
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
            gEngine.UIEngine.UpdateUI();
            gEngine.UIEngine.CheckMouse();
            //physics
#if PHYS
            pEngine.Update(elapsed);
#endif
            sEngine.PreRender();
            time.UpdateTime = time.Stop();
        }


        internal void Render ()
        {
            time.Start();
            //render main scene
            gEngine.Render();
            //render physics
            //(pEngine.World.DebugDrawer as PhysicsDebugDraw).DrawDebugWorld();
            time.RenderTime = time.Stop();

            sEngine.PostRender();
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
