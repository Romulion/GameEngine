using System;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace Toys
{
	public class Outline
	{
		Shader outline;
		public float size;
		public Vector3 colour;
		public bool active;

		public Outline()
		{
			outline = ShaderManager.GetInstance.GetShader("outline");
		}

		public void Draw(Model model, OpenTK.Matrix4 pvm)
		{
			GL.CullFace(CullFaceMode.Front);
			GL.Enable(EnableCap.CullFace);
			outline.ApplyShader();
			outline.SetUniform(pvm, "pvm");
			outline.SetUniform(size, "outline_scale");
			model.DrawOutline();
			GL.Disable(EnableCap.CullFace);
		}
	}
}
