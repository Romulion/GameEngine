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
		public bool drawShadow { get; set; }
		public string Name { get; set; }
		public Vector4 DiffuseColor = Vector4.One;
		public Outline outline {get; set; }
		Shader program;

		public MaterialSimple()
		{
			dontDraw = false;
			program = ShaderManager.GetInstance.GetShader("def");
			offset = 0;
			count = 0;
		}

		public Shader GetShader
		{
			get
			{
				return program;
			}
		}

		public void ApplyMaterial()
		{

			program.SetUniform(DiffuseColor, "material.diffuse");
		}
	}
}
