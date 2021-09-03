using System;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System.Linq;
using System.Collections.Generic;

namespace Toys
{
    enum FaceCullMode
    {
        Disable,
        Front,
        Back,
    }

    class GraphicsEngine
    {
        public int Width { get; private set; }
        public int Height { get; private set; }

        int outputBuffer = 0;

        internal static MainRenderer MainRender;
		internal TextRenderer TextRender;
        internal UIEngine UIEngine;
		//test
		int VBO, VAO, FBO;
		Shader sh;
        bool faceCullEnable;
        bool faceCullFront;
        UniformBufferSystem system;

        //internal List<Camera> cameras = new List<Camera>();
        internal Camera MainCamera;
        internal List<MeshDrawer> meshes = new List<MeshDrawer>();

		public GraphicsEngine()
        {
            Instalize();
        }

		public void OnLoad()
		{
			CoreEngine.MainScene.OnLoad();
			MainRender = new MainRenderer();
			TextRender = new TextRenderer();
            CoreEngine.MainScene.GetLight.BindShadowMap();

            UniformBufferManager ubm = UniformBufferManager.GetInstance;
            system = (UniformBufferSystem)ubm.GetBuffer("system");
            //TestTriangle();
        }

		void TestTriangle()
		{
			/*
			Vertex3D[] vertices = {
				new Vertex3D(new Vector3( -0.5f, -0.5f, 0.0f),Vector3.Zero,Vector2.Zero),
				new Vertex3D(new Vector3( 0.5f, -0.5f, 0.0f),Vector3.Zero,Vector2.Zero),
				new Vertex3D(new Vector3( 0.0f,  0.5f, 0.0f),Vector3.Zero,Vector2.Zero),
			};
			ms = new Mesh(vertices, new int[] { 0, 1, 2 });
			*/
			float[] vertices = {
				-1.0f, -0.0f, 0.0f,
				 0.0f, -1.0f, 0.0f,
				 0.0f,  0.0f, 0.0f
			};
			VBO = GL.GenBuffer();
			VAO = GL.GenVertexArray();
			GL.BindVertexArray(VAO);
			GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
			GL.BufferData(BufferTarget.ArrayBuffer, 4 * vertices.Length, vertices, BufferUsageHint.StaticDraw);

			GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * 4, 0);
			GL.EnableVertexAttribArray(0);
			ShaderManager shdmMgmt = ShaderManager.GetInstance;
			shdmMgmt.LoadShader("defscreen");
			sh = shdmMgmt.GetShader("defscreen");

		}

        void Instalize()
        {
            UIEngine = new UIEngine();
            var settings = Settings.GetInstance();
			Width = settings.Graphics.Width;
			Height = settings.Graphics.Height;
            try
            {

                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                //setting aditional buffer
                FBO = GL.GenFramebuffer();
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);
                //Texture texture = Texture.LoadFrameBufer(Width, Height, "postprocess");
                //screen = new Model(texture,pp);
                //Console.WriteLine(GL.GetInteger(GetPName.MaxComputeImageUniforms));
                //allocation custom framebuffer buffers
                int RBO = GL.GenRenderbuffer();
                GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, RBO);
                //GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth24Stencil8, Width, Height);
                GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
                GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, RBO);
                //Console.WriteLine(GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer));
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

                //enable blending
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
				//GL.BlendFunc(BlendingFactor.Src1Alpha,BlendingFactor.OneMinusSrcAlpha);
                
                //enable depth test
                GL.Enable(EnableCap.DepthTest);
                GL.DepthFunc(DepthFunction.Less);

                //enable stencil 
                GL.Enable(EnableCap.StencilTest);
                GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Replace);
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
				shdmMgmt.LoadShader("shadow");
				shdmMgmt.LoadShader("outline");
            }
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}
        }

        internal void Render()
        {
            //update buffers
            system.SetScreenSpace(new Vector4(Width, Height, 1 / (float)Width, 1 / (float)Height));
            //get render object list
            MeshDrawer[] meshes = this.meshes.ToArray();

            //preparing models to rendering
            foreach (var mesh in meshes)
               mesh.Prepare();

            //shadow pass
            SetCullMode(FaceCullMode.Back);
            //GL.Disable(EnableCap.Multisample);
            CoreEngine.MainScene.GetLight.RenderShadow(meshes);

            //clear display buffer
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, outputBuffer);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
            //GL.Enable(EnableCap.Multisample);
            if (MainCamera != null)
            {
                MainCamera.CalcLook();
                GL.Viewport(0, 0, MainCamera.Width, MainCamera.Height);

                //render scene to primary buffer
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, MainCamera.RenderBuffer);
                SetCullMode(FaceCullMode.Disable);
                GL.ClearColor(MainCamera.ClearColor);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                //render background first due to model transperancy
                if (MainCamera.Background != null)
                    MainCamera.Background.DrawBackground(MainCamera);

                MainRender.Render(meshes.ToArray(), MainCamera);
            }

            //render ui
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, outputBuffer);
            GL.Disable(EnableCap.DepthTest);
            GL.Enable(EnableCap.StencilTest);
            SetCullMode(FaceCullMode.Disable);
            UIEngine.DrawUI(MainCamera);
            SetCullMode(FaceCullMode.Front);
            //TextRender.RenderText();
            GL.Disable(EnableCap.StencilTest);
            GL.Enable(EnableCap.DepthTest);
        }

        internal void Resize(int newWidth, int newHeight)
        {
            Width = newWidth;
            Height = newHeight;
            MainCamera.Width = newWidth;
            MainCamera.Height = newHeight;
            MainCamera.CalcProjection();
        }

        internal void SetCullMode(FaceCullMode cullMode)
        {
            if (cullMode == FaceCullMode.Disable && faceCullEnable)
            {
                if (faceCullEnable)
                {
                    faceCullEnable = false;
                    GL.Disable(EnableCap.CullFace);
                }
            }
            else if (cullMode == FaceCullMode.Back)
            {
                if (faceCullFront)
                { 
                    GL.CullFace(CullFaceMode.Back);
                    faceCullFront = false;
                }
                if (!faceCullEnable)
                {
                    GL.Enable(EnableCap.CullFace);
                    faceCullEnable = true;
                }
            }
            else if (cullMode == FaceCullMode.Front)
            {
                if (!faceCullFront)
                {
                    GL.CullFace(CullFaceMode.Front);
                    faceCullFront = true;
                }
                if (!faceCullEnable)
                {
                    GL.Enable(EnableCap.CullFace);
                    faceCullEnable = true;
                }
            }
        }

		MeshDrawer[] GetRenderObjects()
		{
			List<MeshDrawer> meshes = new List<MeshDrawer>();
			foreach (var node in CoreEngine.MainScene.GetNodes())
			{
				if (!node.Active)
					continue;
				
				meshes.Add((MeshDrawer) node.GetComponent(typeof(MeshDrawer)));
			}

			return meshes.ToArray();
		}
    }
}
