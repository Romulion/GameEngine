using System;
using OpenTK;

namespace Toys
{
	public class MaterialSimple: IMaterial
	{
		public Texture[] textures;
		public int offset { get; set; }
		public int count { get; set; }
		public bool dontDraw { get; set; }
		public Vector4 DiffuseColor = Vector4.One;
		Shader program;

		public MaterialSimple()
		{
			dontDraw = false;
			program = ShaderManager.GetInstance.GetShader("def");
			offset = 0;
			count = 0;
		}

		public void ApplyMaterial()
		{

			program.SetUniform(DiffuseColor, "material.diffuse");
		}
	}
}
