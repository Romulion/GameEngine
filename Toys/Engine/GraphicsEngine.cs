using System;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using System.Linq;
using System.Collections.Generic;

namespace Toys
{
    class GraphicsEngine
    {
        int Width, Height;
        int FBO;

		MainRenderer mainRender;
		internal static TextRenderer textRender;
		//test
		int VBO, VAO;
		Shader sh;

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
			textRender = new TextRenderer();
			renderScene.GetLight.BindShadowMap();
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
			//get render object list
			MeshDrawer[] meshes = GetRenderObjects();
            //shadow pass
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
            GL.Disable(EnableCap.Multisample);

			//renderScene.GetLight.RenderShadow(renderScene.GetNodes());
			renderScene.GetLight.RenderShadow(meshes);

            GL.Enable(EnableCap.Multisample);
            GL.Disable(EnableCap.CullFace);
			GL.Viewport(0, 0, Width, Height);
                       
			//render scene to primary buffer
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.ClearColor(0.0f, 0.1f, 0.1f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			mainRender.Render(meshes);

			textRender.RenderText();

			/*
			//test
			sh.ApplyShader();
			GL.BindVertexArray(VAO);
			GL.DrawArrays(PrimitiveType.Triangles, 0, 3);
			*/
        }

        public void Resize(int newWidth, int newHeight)
        {
            Width = newWidth;
            Height = newHeight;
            GL.Viewport(0,0,Width,Height);
			renderScene.camera.projection =  Matrix4.CreatePerspectiveFieldOfView((float)Math.PI * (30 / 180f), Width / (float)Height, 0.1f, 10.0f);
			mainRender.Resize();
			textRender.Resize(newWidth, newHeight);
        }

		MeshDrawer[] GetRenderObjects()
		{
			List<MeshDrawer> meshes = new List<MeshDrawer>();
			foreach (var node in renderScene.GetNodes())
			{
				if (!node.Active)
					continue;
				
				meshes.Add((MeshDrawer) node.GetComponent(typeof(MeshDrawer)));
			}

			return meshes.ToArray();
		}
    }
}
