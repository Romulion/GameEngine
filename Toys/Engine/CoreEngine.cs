#define PHYS

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
        public static CoreEngine ActiveCore { get; private set; }
        internal static SoundEngine aEngine { get; private set; }
        internal static AnimationEngine animEngine { get; private set; }
        public static VR.VRSystem vrSystem { get; private set; }
        public static Time time { get; private set; }
        public static Time frameTimer { get; private set; }
        Action task;

        public static Camera GetCamera
        {
            get
            {
                return gEngine.MainCamera;
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
                gEngine = new GraphicsEngine();
				pEngine = new PhysicsEngine();
                sEngine = new ScriptingEngine();
                iHandler = new InputHandler();
                aEngine = new SoundEngine();
#if VR
                vrSystem = new VR.VRSystem();
                GLWindow.gLWindow.RenderFrequency = 80;
#endif
                animEngine = new AnimationEngine();
                time = new Time();
                frameTimer = new Time();
                frameTimer.FrameTime = 0.01f;
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
            vrSystem?.Exit();
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
            time.FrameCount++;
            frameTimer.FrameTime = (float)frameTimer.Stop() * .001f;
            frameTimer.Start();
            time.Start();
            vrSystem?.Update();
            sEngine.Awake();
            sEngine.Start();
            pEngine.Scene2Body?.Invoke();
            sEngine.Update();
            iHandler.Update();
            //mesh morpher
            if (task != null)
			{
				task();
				task = null;
			}
			MainScene.Update();
            animEngine.Upadate(frameTimer.FrameTime);
            gEngine.UIEngine.UpdateUI();
            //physics
#if PHYS
            pEngine.Update(frameTimer.FrameTime);
#endif
            aEngine.Update();
            sEngine.PreRender();
            pEngine?.Body2Scene?.Invoke();
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

        /// <summary>
        /// Add task to process by graphics thread
        /// </summary>
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
                task();
                mre.Set();
            };
            return mre;
        }
    }
}
