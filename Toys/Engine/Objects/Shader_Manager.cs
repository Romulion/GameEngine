using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

namespace Toys
{
	//singleton class
	public class ShaderManager
	{
		Dictionary<string, Shader> shaders;
		static ShaderManager shdrMgmr;
		string defPath = "Toys.Resourses.shaders.";
		
		private ShaderManager()
		{
			string frag = ReadFromAssetStream(defPath + "def.fsh");
			string vert = ReadFromAssetStream(defPath + "def.vsh");

			shaders = new Dictionary<string, Shader>();
			Shader def = new ShaderMain(vert, frag);
			shaders.Add("def", def);
		}

		//get singleton instance
		public static ShaderManager GetInstance
		{
			get
			{
				if (shdrMgmr == null)
					shdrMgmr = new ShaderManager();

				return shdrMgmr;
			}
		}

		public void LoadShader(string name)
		{
			
			if (shaders.ContainsKey(name))
				return;
			string file_path = defPath + name;
			string frag = ReadFromAssetStream(file_path + ".fsh");
			string vert = ReadFromAssetStream(file_path + ".vsh");
			LoadShader(name, vert, frag);
        }

		public void LoadShader(string name, string vert, string frag)
		{
			if (shaders.ContainsKey(name))
				return;
			Shader shdr = new ShaderMain(vert, frag);
			shdr.ApplyShader();
			shdr.SetUniform(0, "material.texture_difuse");
			shdr.SetUniform(1, "material.texture_toon");

			shdr.SetUniform(10, "shadowMap");

			shaders.Add(name, shdr);
		}

		public Shader LoadShader(string name, string compute)
		{
			if (shaders.ContainsKey(name))
				return shaders[name];
			string cmp = ReadFromAssetStream(defPath + compute);
			Shader shdr = new ShaderCompute(cmp);

			shaders.Add(name, shdr);
			return shdr;
		}

		public Shader GetShader(string name)
		{
			if (shaders.ContainsKey(name))
				return shaders[name];
			Console.WriteLine(shaders.Count);

			Console.WriteLine("shader {0} not found", name);
			return shaders["def"];
		}



		public void SetBinding(int index, string name)
		{
			foreach (var shader in shaders)
			{
				shader.Value.SetUBO(index, name);
			}
		}

		public static string ReadFromAssetStream(string path)
		{
            var assembly = IntrospectionExtensions.GetTypeInfo(typeof(ShaderManager)).Assembly;
            var stream = assembly.GetManifestResourceStream(path);

            string str = "";
			using (var reader = new StreamReader(stream)) {
				str = reader.ReadToEnd ();
			}
			return str;
		}

	}
}
