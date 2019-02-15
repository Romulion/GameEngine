using System;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace Toys
{
    class GraphicsEngine
    {
        int Width, Height;
        int FBO;

		MainRenderer mainRender;

		public Scene renderScene { get; private set; }

		public GraphicsEngine(Scene scene)
        {
			renderScene = scene;
            Instalize();
        }

		public void OnLoad()
		{
			renderScene.OnLoad();
			mainRender = new MainRenderer(renderScene.camera, renderScene);
			renderScene.GetLight.BindShadowMap();
		}

        public void Instalize()
        {
			var settings = Settings.GetInstance();
			Width = settings.Graphics.Width;
			Height = settings.Graphics.Height;
            try
            {

                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

                //ShaderManager mgr = ShaderManager.GetInstance;
                //mgr.LoadShader("pp");
                //pp = mgr.GetShader("pp");

                //setting aditional buffer
                FBO = GL.GenFramebuffer();
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);
                //Texture texture = Texture.LoadFrameBufer(Width, Height, "postprocess");
                //screen = new Model(texture,pp);
                //Console.WriteLine(GL.GetInteger(GetPName.MaxComputeImageUniforms));
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
				//GL.BlendFunc(BlendingFactor.Src1Alpha,BlendingFactor.OneMinusSrcAlpha);

                GL.Enable(EnableCap.DepthTest);
                GL.DepthFunc(DepthFunction.Less);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.ReadKey();
            }

			//Loading essential shaders
			ShaderManager shdmMgmt = ShaderManager.GetInstance;
			try
			{
				shdmMgmt.LoadShader("pmx");
				shdmMgmt.LoadShader("shadow");
				shdmMgmt.LoadShader("outline");
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}
        }

        


        public void Render()
        {
            //shadow pass
            
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
            GL.Disable(EnableCap.Multisample);

			renderScene.GetLight.RenderShadow(renderScene.GetNodes());

            GL.Enable(EnableCap.Multisample);
            GL.Disable(EnableCap.CullFace);
			GL.Viewport(0, 0, Width, Height);
                       
			//render scene to primary buffer
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.ClearColor(0.0f, 0.1f, 0.1f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			mainRender.Render();
        }

        public void Resize(int newWidth, int newHeight)
        {
            Width = newWidth;
            Height = newHeight;
            GL.Viewport(0,0,Width,Height);
			renderScene.camera.projection =  Matrix4.CreatePerspectiveFieldOfView((float)Math.PI * (30 / 180f), Width / (float)Height, 0.1f, 10.0f);
			mainRender.Resize();
			//renderScene.Resize(Width, Height);

        }
    }
}
