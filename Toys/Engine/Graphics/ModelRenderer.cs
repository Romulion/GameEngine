using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Toys
{
	public class ModelRenderer
	{
		
		public Matrix4 projection;
		public Matrix4 viev;

		LightSource light;
		Camera camera;
		Shader outline;
		UniformBufferSpace ubs;

		public ModelRenderer(LightSource ls, Camera cam)
		{
			light = ls;
			camera = cam;
			outline = ShaderManager.GetInstance.GetShader("outline");
			ubs = (UniformBufferSpace) UniformBufferManager.GetInstance.GetBuffer("space");
		}



		public void Render(Model model) 
		{

			//model.anim.SkinMesh();
			MeshDrawer msrd = model.meshes;
			//Shader shader = msrd.GetShader;
			//shader.ApplyShader();
			//setting light
			//light.BindShadowMap();

			Matrix4 pvm = model.WorldSpace * viev * projection;
			Matrix4 norm = model.WorldSpace.Inverted();
			norm.Transpose();

			ubs.SetLightSpace(light.GetMat);
			ubs.SetNormalSpace(norm);
			ubs.SetPVMSpace(pvm);
			ubs.SetModelSpace(model.WorldSpace);

			msrd.Draw();

			if (msrd.OutlineDrawing)
			{
				GL.CullFace(CullFaceMode.Front);
				GL.Enable(EnableCap.CullFace);
				outline.ApplyShader();
				outline.SetUniform(pvm, "pvm");
				outline.SetUniform(0.03f, "outline_scale");
				msrd.DrawOutline();
				GL.Disable(EnableCap.CullFace);
			}
		}

	}
}
