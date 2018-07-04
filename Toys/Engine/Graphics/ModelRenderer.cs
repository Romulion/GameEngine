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

		public ModelRenderer(LightSource ls, Camera cam)
		{
			light = ls;
			camera = cam;
			outline = ShaderManager.GetInstance.GetShader("outline");
		}



		public void Render(Model model) 
		{
			MeshDrawer msrd = model.meshes;
			Shader shader = msrd.GetShader;
			shader.ApplyShader();
			//setting light
			shader.SetUniform(light.GetMat, "lightSpacePos");
			shader.SetUniform(light.GetPos, "LightPos");
			light.BindShadowMap();

			Matrix4 pvm = model.WorldSpace * viev * projection;
			Matrix4 norm = model.WorldSpace.Inverted();

			shader.SetUniform(pvm, "pvm");
			norm.Transpose();
			shader.SetUniform(norm, "NormalMat");
			shader.SetUniform(model.WorldSpace, "model");
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
