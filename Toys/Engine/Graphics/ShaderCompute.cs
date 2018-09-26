using System;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using System.Text;

namespace Toys
{
	public class ShaderCompute: Shader
	{
		public ShaderCompute(string shader)
		{
			int computeShader = CompileShader(shader, ShaderType.ComputeShader);

			int[] tttt = new int[1];
			GL.GetShader(computeShader,ShaderParameter.CompileStatus, tttt);
			Console.WriteLine("compilation status: {0}", tttt[0]);

			shaderProgram = GL.CreateProgram();
			GL.AttachShader(shaderProgram, computeShader);
			GL.LinkProgram(shaderProgram);

			GL.ValidateProgram(shaderProgram);
			int[] info = new int[1];
			GL.GetProgram(shaderProgram, GetProgramParameterName.LinkStatus, info);
			Console.WriteLine("link status: {0}",info[0]);



			//string aaaa;
			//GL.GetProgramInfoLog(shaderProgram, out aaaa);
			//Console.WriteLine(GL.IsProgram(shaderProgram));

			//
			GL.UseProgram(shaderProgram);
			Console.WriteLine(GL.GetError());
			GL.DeleteShader(computeShader);
		}

		public void SetSSBO(int index, string name)
		{
			int indx = GL.GetUniformBlockIndex(shaderProgram, name);

			Console.WriteLine();
			if (indx == -1)
			{
				//Console.WriteLine("uniform buffer '{0}' not found", name);
				return;
			}

			GL.ShaderStorageBlockBinding(shaderProgram, indx, index);
		}
	}
}
