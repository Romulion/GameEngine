using System;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace Toys
{
	//default outline class
	public class Outline : IOutline
	{
		Shader outlineShader;
		public Vector4 EdgeColour { get; set; }
		public float EdgeScaler { get; set; }
		public bool HasEdge { get; set; }

		public Outline()
		{
			outlineShader = ShaderManager.GetInstance.GetShader("outline");
			HasEdge = true;
			EdgeScaler = 0.2f;
			EdgeColour = new Vector4(Vector3.Zero, 1f);
		}

		public void ApplyOutline()
		{
			outlineShader.SetUniform(EdgeColour, "EdgeColor");
			outlineShader.SetUniform(EdgeScaler, "EdgeScaler");
		}
	}
}
