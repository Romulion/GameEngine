using System;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace Toys
{
	public class Shader
	{
		protected int shaderProgram;

        protected ShaderUniform[] uniforms;

        //clone uniforms for each copy of material
        public ShaderUniform[] GetUniforms {
            get
            {
                ShaderUniform[] vars = new ShaderUniform[uniforms.Length];
                for (int i = 0; i < vars.Length; i++)
                {
                    vars[i] = uniforms[i].Clone();
                }
                return vars;
            }
        }

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
            SetUniform(value, GL.GetUniformLocation(shaderProgram, name));
        }

		public void SetUniform(int value, string name)
		{
            SetUniform(value, GL.GetUniformLocation(shaderProgram, name));
        }

		public void SetUniform(Matrix4 value, string name)
		{
            SetUniform(value, GL.GetUniformLocation(shaderProgram, name));
        }

		public void SetUniform(Vector3 value, string name)
		{
            SetUniform(value, GL.GetUniformLocation(shaderProgram, name));
        }

        public void SetUniform(Vector2 value, string name)
        {
            SetUniform(value, GL.GetUniformLocation(shaderProgram, name));
        }

        public void SetUniform(Vector4 value, string name)
		{
            SetUniform(value,GL.GetUniformLocation(shaderProgram, name));
		}

		public void SetUniform(Vector4[] value, string name)
		{
			for (int i = 0; i < value.Length; i++)
			{
                SetUniform(value[i], GL.GetUniformLocation(shaderProgram, name + "[" + i + "]"));
			}
		}

		public void SetUniform(Matrix4[] value, string name)
		{
			for (int i = 0; i < value.Length; i++)
			{
                SetUniform(value[i], GL.GetUniformLocation(shaderProgram, name + "[" + i + "]"));
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
        public void SetUniform(Vector2 value, int id)
        {
            GL.Uniform2(id, ref value);
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

			int computeShader = GL.CreateShader(st);
			GL.ShaderSource(computeShader, source);
			GL.CompileShader(computeShader);
			string log;
			GL.GetShaderInfoLog(computeShader, out log);

			if (log != "")
			{
				Console.WriteLine(st);
				Console.WriteLine(log);
				throw new Exception();
			}

			return computeShader;
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
