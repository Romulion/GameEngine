using System;
using System.Text;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace Toys
{
	public class ShaderMain : Shader
	{
		
		
		public ShaderMain(string vertex, string fragment)
		{
			int vertexShader, fragmentShader;

			vertexShader = CompileShader(vertex, ShaderType.VertexShader);
			fragmentShader = CompileShader(fragment, ShaderType.FragmentShader);

			shaderProgram = GL.CreateProgram();
			GL.AttachShader(shaderProgram, vertexShader);
			GL.AttachShader(shaderProgram, fragmentShader);
			GL.LinkProgram(shaderProgram);
			GL.DeleteShader(vertexShader);
			GL.DeleteShader(fragmentShader);


		}


		void GetTextureNames()
		{
			int m, length, size;
			StringBuilder name = new StringBuilder(16);
			ActiveUniformType type;
			GL.GetProgram(shaderProgram, GetProgramParameterName.ActiveUniforms, out m);
			for (int i = 0; i < m; i++)
			{
				GL.GetActiveUniform(shaderProgram, i, 16, out length, out size, out type, name);
				Console.WriteLine("{0}  {1}",type, name);
			}
		}




	}
}
