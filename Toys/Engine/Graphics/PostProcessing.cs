using System;
namespace Toys
{
	public class PostProcessing
	{
		Shader shader;
		Texture screen;

		public PostProcessing(Texture texture)
		{
			ShaderManager shdrm = ShaderManager.GetInstance;
			screen = texture;
			shdrm.LoadShader("pp");
			shader = shdrm.GetShader("pp");
		}

		public void RenderScreen()
		{
			
		}
	}
}
