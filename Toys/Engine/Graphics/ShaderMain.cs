using System;
using System.Text;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;
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

            GetUniforms();
        }

        //retrieving active uniforms
		void GetUniforms()
		{
            List<ShaderUniform> uniforms = new List<ShaderUniform>();

			int m, length, size;
			StringBuilder name = new StringBuilder(40);
			ActiveUniformType type;
			GL.GetProgram(shaderProgram, GetProgramParameterName.ActiveUniforms, out m);
			for (int i = 0; i < m; i++)
			{
				GL.GetActiveUniform(shaderProgram, i, 40, out length, out size, out type, name);

                string group = "";
                string uname;
                string[] grouping = name.ToString().Split('.');
                if (grouping.Length == 2)
                {
                    group = grouping[0];
                    uname = grouping[1];
                }
                else
                    uname = grouping[0];


                switch (type)
                {
                    case ActiveUniformType.Float:
                        uniforms.Add(new ShaderUniformFloat(uname, group, this));
                        break;
                    case ActiveUniformType.Int:
                        uniforms.Add(new ShaderUniformInt(uname, group, this));
                        break;
                    case ActiveUniformType.FloatVec3:
                        uniforms.Add(new ShaderUniformVector3(uname, group, this));
                        break;
                    case ActiveUniformType.FloatVec4:
                        uniforms.Add(new ShaderUniformVector4(uname, group, this));
                        break;
                    case ActiveUniformType.FloatMat4:
						uniforms.Add(new ShaderUniformMatrix4(uname, group, this));
                        break;
                    default:
                        break;
                }
                
                //Console.WriteLine("{0}  {1}",type, name);
			}

            this.uniforms = uniforms.ToArray();

        }




	}
}
