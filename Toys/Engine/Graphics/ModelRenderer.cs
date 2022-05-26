using System;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL;

namespace Toys
{
	public class ModelRenderer
	{
		
		public Matrix4 Projection;
		public Matrix4 Viev;

		Shader outline;
		UniformBufferSpace ubs;

		public ModelRenderer()
		{
			outline = ShaderManager.GetInstance.GetShader("outline");
			ubs = (UniformBufferSpace) UniformBufferManager.GetInstance.GetBuffer("space");
		}

		public void Render(MeshDrawer msrd)
		{
			//var mesh = (MeshDrawer) node.GetComponent(typeof(MeshDrawer));
			SceneNode node = msrd.Node;
            
            Matrix4 pvm = node.GetTransform.GlobalTransform * Viev * Projection;
			Matrix4 norm = node.GetTransform.GlobalTransform.Inverted();
			norm.Transpose();
			ubs.SetNormalSpace(norm);
			ubs.SetPVMSpace(pvm);
			ubs.SetModelSpace(node.GetTransform.GlobalTransform);
            ubs.SetVievModelSpace(node.GetTransform.GlobalTransform * Viev);
            msrd.Draw();

			if (msrd.OutlineDrawing)
			{
                CoreEngine.GfxEngine.SetCullMode(FaceCullMode.Front);
				outline.ApplyShader();
				outline.SetUniform(pvm, "pvm");
				outline.SetUniform(0.03f, "outline_scale");
				msrd.DrawOutline();
                CoreEngine.GfxEngine.SetCullMode(FaceCullMode.Disable);
            }
		}
	}
}
