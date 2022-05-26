#define PHYS

using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using OpenTK.Graphics;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading;

namespace Toys
{
	public class CoreEngine 
	{
        internal static InputHandler InptHandler { get; private set; }
		internal static GraphicsEngine GfxEngine { get; private set; }
        public static PhysicsEngine PhysEngine { get; private set; }
        internal static ScriptingEngine ScriptingEngine { get; private set; }
        public static CoreEngine ActiveCore { get; private set; }
        internal static SoundEngine AudioEngine { get; private set; }
        internal static AnimationEngine AnimEngine { get; private set; }
        public static InputSystem ISystem { get; private set; }
        public static VR.VRSystem VRSystem { get; private set; }
        public static Time Time { get; private set; }
        public static Time FrameTimer { get; private set; }
        List<Action> tasks;
        List<Action> tasksProcessing;

        public static Camera GetCamera
        {
            get
            {
                return GfxEngine.MainCamera;
            }
        }

		public static Scene MainScene { get; set; }
        public static ShareData Shared { get; set; }

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
                GfxEngine = new GraphicsEngine();
				PhysEngine = new PhysicsEngine();
                ScriptingEngine = new ScriptingEngine();
                InptHandler = new InputHandler();
                AudioEngine = new SoundEngine();
                ISystem = new InputSystem();
#if VR
                vrSystem = new VR.VRSystem();
                GLWindow.gLWindow.RenderFrequency = 80;
#endif
                AnimEngine = new AnimationEngine();
                Time = new Time();
                FrameTimer = new Time();
                FrameTimer.FrameTime = 0.01f;

                tasks = new List<Action>(20);
                tasksProcessing = new List<Action>(20);
                }
			catch (Exception e)
			{
				Logger.Critical(e.Message);
                throw;
			}
		}

        internal void Close()
        {
            PhysEngine.Dispose();
            ScriptingEngine.Destroy();
            VRSystem?.Exit();
        }

        //for Load event
        //
        internal void OnLoad()
		{
			GfxEngine.OnLoad();
		}

        internal void Resize(int width, int height)
        {
            GfxEngine.Resize(width, height);
        }

		internal void Update()
		{
            Time.FrameCount++;
            FrameTimer.FrameTime = (float)FrameTimer.Stop() * .001f;
            FrameTimer.Start();
            Time.Start();
            VRSystem?.Update();
            ISystem.Update();
            ScriptingEngine.Destroy();
            ScriptingEngine.Awake();
            ScriptingEngine.Start();
            PhysEngine.Scene2Body?.Invoke();
            ScriptingEngine.Update();
            InptHandler.Update();

            //thread unsafe methods
            if (tasks.Count > 0)
			{
                lock (tasks){
                    tasksProcessing.AddRange(tasks);
                    tasks.Clear();
                }

                for (int n = 0; n < tasksProcessing.Count; n++)
                {
                    tasksProcessing[n].Invoke();
                    tasksProcessing.Clear();
                }
            }

			MainScene.Update();
            AnimEngine.Upadate(FrameTimer.FrameTime);
            GfxEngine.UIEngine.UpdateUI();
            //physics
#if PHYS
            PhysEngine.Update(FrameTimer.FrameTime);
#endif
            AudioEngine.Update();
            ScriptingEngine.PreRender();
            PhysEngine?.Body2Scene?.Invoke();
            Time.UpdateTime = Time.Stop();
        }

        internal void Render ()
        {
            Time.Start();
            //render main scene
            GfxEngine.Render();
            //render physics
            //(pEngine.World.DebugDrawer as PhysicsDebugDraw).DrawDebugWorld();
            Time.RenderTime = Time.Stop();

            ScriptingEngine.PostRender();
        }

        /// <summary>
        /// Add task to process by graphics thread
        /// </summary>
		public Action AddTask
		{
			set
			{
                lock (tasks)
                {
                    tasks.Add(value);
                }
			}
		}

        /// <summary>
        /// Add task to process by graphics thread
        /// with completion notifier
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public ManualResetEvent AddNotyfyTask(Action task)
        {
            ManualResetEvent mre = new ManualResetEvent(false);
            AddTask = () =>
            {
                try
                {
                    task();
                }
                finally
                {
                    mre.Set();
                }
            };
            return mre;
        }


        /// <summary>
        /// Add syncronous task to graphics thread
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public void AddSyncTask(Action task)
        {
            AddNotyfyTask(task).WaitOne();
        }
    }
}
