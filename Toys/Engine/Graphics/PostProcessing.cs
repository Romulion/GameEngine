using System;
namespace Toys
{
	public class PostProcessing
	{
		Shader shader;
		Texture2D screen;

		public PostProcessing(Texture2D texture)
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
