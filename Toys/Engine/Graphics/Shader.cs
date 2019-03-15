using System;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace Toys
{
	public class Shader
	{
		protected int shaderProgram;

        public ShaderUniform[] uniforms { get; protected set; }

		public Shader()
		{
		}

        public void ApplyShader()
		{
			GL.UseProgram(shaderProgram);
		}

		//by string
		public void SetUniform(float value, string name)
		{
			GL.Uniform1(GL.GetUniformLocation(shaderProgram, name), value);
		}

		public void SetUniform(int value, string name)
		{
			GL.Uniform1(GL.GetUniformLocation(shaderProgram, name), value);
		}

		public void SetUniform(Matrix4 value, string name)
		{
			GL.UniformMatrix4(GL.GetUniformLocation(shaderProgram, name), false, ref value);
		}

		public void SetUniform(Vector3 value, string name)
		{
			GL.Uniform3(GL.GetUniformLocation(shaderProgram, name), ref value);
		}

		public void SetUniform(Vector4 value, string name)
		{
			GL.Uniform4(GL.GetUniformLocation(shaderProgram, name), ref value);
		}

		public void SetUniform(Vector4[] value, string name)
		{
			for (int i = 0; i < value.Length; i++)
			{
				GL.Uniform4(GL.GetUniformLocation(shaderProgram, name + "[" + i + "]"), ref value[i]);
			}
		}

		public void SetUniform(Matrix4[] value, string name)
		{
			for (int i = 0; i < value.Length; i++)
			{
				GL.UniformMatrix4(GL.GetUniformLocation(shaderProgram, name + "[" + i + "]"), false, ref value[i]);
			}
		}

		//by index
		public void SetUniform(float value, int id)
		{
			GL.Uniform1(id, value);
		}

		public void SetUniform(int value, int id)
		{
			GL.Uniform1(id, value);
		}

		public void SetUniform(Matrix4 value, int id)
		{
			GL.UniformMatrix4(id, false, ref value);
		}

		public void SetUniform(Vector3 value, int id)
		{
			GL.Uniform3(id, ref value);
		}

		public void SetUniform(Vector4 value, int id)
		{
			GL.Uniform4(id, ref value);
		}


		protected int CompileShader(string source, ShaderType st)
		{

			int compShader = GL.CreateShader(st);
			GL.ShaderSource(compShader, source);
			GL.CompileShader(compShader);
			string log;
			GL.GetShaderInfoLog(compShader, out log);

			if (log != "")
			{
				Console.WriteLine(st);
				Console.WriteLine(log);
				throw new Exception();
			}

			return compShader;
		}


		public void SetUBO(int index, string name)
		{
			int indx = GL.GetUniformBlockIndex(shaderProgram, name);
			if (indx == -1)
			{
				//Console.WriteLine("uniform buffer '{0}' not found", name);
				return;
			}
			GL.UniformBlockBinding(shaderProgram, indx, index);
		}

		public void DeleteShader()
		{
			GL.DeleteProgram(shaderProgram);
		}

	}
}
