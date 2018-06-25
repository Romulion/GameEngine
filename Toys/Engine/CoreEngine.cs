using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using OpenTK.Graphics;

namespace Toys
{
    class CoreEngine : GameWindow
    {

		SceneManager Scene;
		Shader pp;
		//Model screen;
		bool trigger = false;

		int FBO;

		public CoreEngine() : base (640,480, new GraphicsMode(32,8,8,4))
        {
            Instalize();
			Scene = SceneManager.GetInstance;

        }

        void Instalize()
        {
            Title = "MMD Test";

            try
            {
				
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

				ShaderManager mgr = ShaderManager.GetInstance;
				mgr.LoadShader("pp");
				pp = mgr.GetShader("pp");


				//setting aditional buffer
                FBO = GL.GenFramebuffer();
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);
				Texture texture = Texture.LoadFrameBufer(Width, Height, "postprocess");
				//screen = new Model(texture,pp);

				//allocation custom framebuffer buffers
                int RBO = GL.GenRenderbuffer();
                GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, RBO);
                GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth24Stencil8, Width, Height);
                GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
				GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, RBO);
				//Console.WriteLine(GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer));
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

				GL.Enable(EnableCap.Blend);
				GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);


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
			Scene.camera = new Camera(this);
            VSync = VSyncMode.On;

            Resize += (s, ev) => {
				
				GL.Viewport(Size);
				Scene.Resize(Width,Height);
            };

			GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);
            
        }

        //
        void Update(object sender, FrameEventArgs e)
        {
			Scene.Update();
            if (Keyboard[Key.Escape])
            {
                Exit();
            }
            if (Keyboard[Key.F])
            {
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            }
            if (Keyboard[Key.L])
            {
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            }
            if (Keyboard[Key.P])
            {
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Point);
            }
        }


        void OnRender (object sender, FrameEventArgs e)
        {
			// render graphics
			//GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);
			//	GL.Enable(EnableCap.DepthTest);

			//reducing draw calls
			//if (!trigger)
			//{
				//drawing shadow
				GL.Enable(EnableCap.CullFace);
				GL.CullFace(CullFaceMode.Back);
				GL.Disable(EnableCap.Multisample);

				Scene.GetLight.RenderShadow();
				
				GL.Enable(EnableCap.Multisample);
				GL.Disable(EnableCap.CullFace);
				//resize viev to normal size
				GL.Viewport(0, 0, Width, Height);
			//}
			//trigger = !trigger;

			//render scene to buffer
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.ClearColor(0.0f, 0f, 0.0f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit |  ClearBufferMask.DepthBufferBit);
			Scene.Render();

			/*
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
			GL.Clear(ClearBufferMask.ColorBufferBit);
			pp.ApplyShader();
			screen.Draw();
*/
			//		pp.ApplyShader();
			//pp.SetUniform(0);
			//			screen.Draw();
			//shdr1.ApplyShader();
			//plane.Draw(shdr1);



            SwapBuffers();
        }


    }
}
