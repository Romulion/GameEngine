using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;

namespace Toys
{
	public class LightSource
	{
		List<Model> models;
		UniformBufferSkeleton ubo;
		public Vector3 pos;
		Vector3 look;
		int shadowBuffer;
		//shadow texture resolution
		int Width = 1024;
		int Heigth = 1024;

		Texture shadowMap;
		Shader shdr;
		Matrix4 lightdir;
		Matrix4 projection;
		int SWidth = 640;
		int SHeigth = 480;

		public LightSource(List<Model> models)
		{
			shadowBuffer = GL.GenFramebuffer();
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, shadowBuffer);
			shadowMap = Texture.CreateShadowMap(Width,Heigth);
			//shadowMap.BindTexture();
			GL.DrawBuffer(DrawBufferMode.None);
			GL.ReadBuffer(ReadBufferMode.None);
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

			//load shader
			shdr = ShaderManager.GetInstance.GetShader("shadow");

			//locate source
			pos = new Vector3(-1f, 2f, -1f);
			look = new Vector3(0f, 0f, 0f);


			projection = Matrix4.CreateOrthographic(5f, 5f, 0.1f, 5f);
			//projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI * (30 / 180f), Width / (float)Heigth, 1f, 100.0f);
			lightdir = Matrix4.LookAt(pos, look, new Vector3(0f, 1f, 0f));

            ubo = (UniformBufferSkeleton) UniformBufferManager.GetInstance.GetBuffer("skeleton");
			this.models = models;
		}

		//for resizing on runtime
		public void Resize(int Width, int Heigth)
		{
			SWidth = Width;
			SHeigth = Heigth;
		}

		//shadow rendering
		public void RenderShadow()
		{
			lightdir = Matrix4.LookAt(pos, look, new Vector3(0f, 1f, 0f));
			GL.Viewport(0, 0, Width, Heigth);
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, shadowBuffer);
			GL.Clear(ClearBufferMask.DepthBufferBit);
			shdr.ApplyShader();

			foreach (var model in models)
			{
				Matrix4 pvm = model.WorldSpace * lightdir * projection;
				ubo.SetBones(model.anim.GetSkeleton);
				shdr.SetUniform(pvm, "pvm");
				model.meshes.DrawSimple();
			}
			//GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
			//
		}

		public void BindShadowMap()
		{
			GL.ActiveTexture(TextureUnit.Texture10);
			shadowMap.BindTexture();
		}

		//depth matrix
		public Matrix4 GetMat
		{
			get { return lightdir * projection;}
		}

		//light position
		public Vector3 GetPos
		{
			get
			{
				return pos;
			}
		}
	}
}
