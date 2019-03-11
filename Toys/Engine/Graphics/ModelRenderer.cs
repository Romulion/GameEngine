using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Toys
{
	public class ModelRenderer
	{
		
		public Matrix4 projection;
		public Matrix4 viev;

		Shader outline;
		UniformBufferSpace ubs;

		public ModelRenderer()
		{
			outline = ShaderManager.GetInstance.GetShader("outline");
			ubs = (UniformBufferSpace) UniformBufferManager.GetInstance.GetBuffer("space");
		}



		public void Render(SceneNode node) 
		{
			MeshDrawer msrd = node.model;

			Matrix4 pvm = node.GetTransform.globalTransform * viev * projection;
            Matrix4 norm = node.GetTransform.globalTransform.Inverted();
            norm.Transpose();

			ubs.SetNormalSpace(norm);
			ubs.SetPVMSpace(pvm);
			ubs.SetModelSpace(node.GetTransform.globalTransform);
			ubs.SetPvSpace(viev * projection);
            
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
