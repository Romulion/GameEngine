﻿using System;
using OpenTK.Graphics.OpenGL;
using OpenTK;
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
        int Width, Height;

        internal static MainRenderer mainRender;
		internal static TextRenderer textRender;
		//test
		int VBO, VAO, FBO;
		Shader sh;
        bool faceCullEnable;
        bool faceCullFront;

        internal List<Camera> cameras = new List<Camera>();
        internal List<MeshDrawer> meshes = new List<MeshDrawer>();

        public Scene renderScene { get; private set; }

		public GraphicsEngine(Scene scene)
        {
			renderScene = scene;
            Instalize();
        }

		public void OnLoad()
		{
			renderScene.OnLoad();
			mainRender = new MainRenderer(renderScene);
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

        void Instalize()
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
                //GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth24Stencil8, Width, Height);
                GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
                GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, RBO);
                //Console.WriteLine(GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer));
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
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
			//get render object list
			MeshDrawer[] meshes = this.meshes.ToArray();

            //preparing models to rendering
            foreach (var mesh in meshes)
               mesh.Prepare();

            //shadow pass
            SetCullMode(FaceCullMode.Back);
            //GL.Enable(EnableCap.CullFace);
            //GL.CullFace(CullFaceMode.Back);
            GL.Disable(EnableCap.Multisample);

			//renderScene.GetLight.RenderShadow(renderScene.GetNodes());
			renderScene.GetLight.RenderShadow(meshes);

            foreach (var cam in cameras)
            {
                cam.CalcLook();
                GL.Enable(EnableCap.Multisample);
                //GL.Disable(EnableCap.CullFace);
                SetCullMode(FaceCullMode.Disable);
                GL.Viewport(0, 0, cam.Width, cam.Height);

                //render scene to primary buffer
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                GL.ClearColor(0.0f, 0.1f, 0.1f, 1.0f);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                mainRender.Render(meshes.ToArray(), cam);
            }
			textRender.RenderText();

			/*
			//test
			sh.ApplyShader();
			GL.BindVertexArray(VAO);
			GL.DrawArrays(PrimitiveType.Triangles, 0, 3);
			*/
        }

        internal void Resize(int newWidth, int newHeight)
        {
            Width = newWidth;
            Height = newHeight;
            foreach (var cam in cameras)
            {
                if (cam.Main)
                {  
                    cam.Width = newWidth;
                    cam.Height = newHeight;
                    cam.CalcProjection();
                }
            }
			textRender.Resize(newWidth, newHeight);
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
