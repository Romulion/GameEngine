﻿#define PHYS

using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using OpenTK.Graphics;
using System.Diagnostics;
using System.Threading;

namespace Toys
{
	public class CoreEngine 
	{
        internal static InputHandler iHandler { get; private set; }
		internal static GraphicsEngine gEngine { get; private set; }
        public static PhysicsEngine pEngine { get; private set; }
        internal static ScriptingEngine sEngine { get; private set; }
        internal static CoreEngine ActiveCore { get; private set; }
        internal static SoundEngine aEngine { get; private set; }
        public static Time time { get; private set; }
        public static Time frameTimer { get; private set; }
        Action task;

        public float elapsed { get; private set; }

		public static Scene MainScene;
        public static ShareData Shared;

		public CoreEngine()
		{
			Instalize();
			ActiveCore = this;
		}

		void Instalize()
		{
			try
			{
				MainScene = new Scene();
                Shared = new ShareData();
                gEngine = new GraphicsEngine(MainScene);
				pEngine = new PhysicsEngine();
                sEngine = new ScriptingEngine();
                iHandler = new InputHandler();
                aEngine = new SoundEngine();
                time = new Time();
                frameTimer = new Time();
                elapsed = 0.01f;
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
            iHandler.Update();
            //mesh morpher
            if (task != null)
			{
				task();
				task = null;
			}
			MainScene.Update(elapsed);
            gEngine.UIEngine.UpdateUI();
            gEngine.UIEngine.CheckMouse();
            //physics
#if PHYS
            pEngine.Update(elapsed);
#endif
            aEngine.Update();
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


		public Action AddTask
		{
			set
			{
				if (task == null)
					task = value;
				else
					task += value;
			}
		}

        public ManualResetEvent AddNotyfyTask(Action task)
        {
            ManualResetEvent mre = new ManualResetEvent(false);
            AddTask = () =>
            {
                task();
                mre.Set();
            };
            return mre;
        }
    }
}
