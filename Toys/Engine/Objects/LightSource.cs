using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;

namespace Toys
{
	public class LightSource
	{
		UniformBufferSkeleton ubo;
		UniformBufferLight ubl;
		UniformBufferSpace ubs;

		public Vector3 pos;
		Vector3 look;
		int shadowBuffer;
		//shadow texture resolution
		int width = 2048;
		int heigth = 2048;

		public Texture2D shadowMap { get; private set; }
		Shader shdr;
		Matrix4 direction;
		Matrix4 projection;

		public LightSource()
		{
			shadowBuffer = GL.GenFramebuffer();
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, shadowBuffer);
			shadowMap = Texture2D.CreateShadowMap(width,heigth);
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
			direction = Matrix4.LookAt(pos, look, new Vector3(0f, 1f, 0f));

			UniformBufferManager ubm = UniformBufferManager.GetInstance;
            ubo = (UniformBufferSkeleton) ubm.GetBuffer("skeleton");
			ubs = (UniformBufferSpace)ubm.GetBuffer("space");
            ubl = (UniformBufferLight)ubm.GetBuffer("light");
		}

		//shadow rendering
		public void RenderShadow(List<SceneNode> nodes)
		{
			SetLightVars();

			direction = Matrix4.LookAt(pos, look, new Vector3(0f, 1f, 0f));
			GL.Viewport(0, 0, width, heigth);
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, shadowBuffer);
			GL.Clear(ClearBufferMask.DepthBufferBit);
			shdr.ApplyShader();

			foreach (var node in nodes)
			{
				if (!node.Active)
					continue;
				MeshDrawer md = (MeshDrawer)node.GetComponent(typeof(MeshDrawer));
				if (md == null)
					continue;
				
				Matrix4 pvm = node.GetTransform.GlobalTransform * direction * projection;
				shdr.SetUniform(pvm, "pvm");
				md.DrawSimple();
			}
		}

		public void RenderShadow(MeshDrawer[] meshes)
		{
			SetLightVars();

			direction = Matrix4.LookAt(pos, look, new Vector3(0f, 1f, 0f));
			GL.Viewport(0, 0, width, heigth);
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, shadowBuffer);
			GL.Clear(ClearBufferMask.DepthBufferBit);
			shdr.ApplyShader();

			foreach (var mesh in meshes)
			{
				Matrix4 pvm = mesh.Node.GetTransform.GlobalTransform * direction * projection;
				shdr.SetUniform(pvm, "pvm");
				mesh.DrawSimple();
			}
		}

		public void BindShadowMap()
		{
			GL.ActiveTexture(TextureUnit.Texture0 + (int)TextureType.ShadowMap);
			shadowMap.BindTexture();
		}

		public void SetLightVars()
		{
			ubs.SetLightSpace(GetMat);
			ubl.SetLightPos(GetPos);
		}
		//depth matrix
		public Matrix4 GetMat
		{
			get { return direction * projection;}
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
