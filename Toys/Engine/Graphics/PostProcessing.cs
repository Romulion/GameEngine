using System;
namespace Toys
{
	public class PostProcessing
	{
		Shader shdr;
		Texture screen;

		public PostProcessing(Texture texture)
		{
			ShaderManager shdrm = ShaderManager.GetInstance;
			screen = texture;
			shdrm.LoadShader("pp");
			shdr = shdrm.GetShader("pp");
		}

		public void RenderScreen()
		{
			
		}
	}
}
