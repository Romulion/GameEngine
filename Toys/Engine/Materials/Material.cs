using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using System.Linq;

namespace Toys
{
	public abstract class Material : Resource
	{
		internal ShaderSettings ShaderSettings { get; set; }
        public RenderDirectives RenderDirrectives { get; set; }
		public Outline Outline;

		string _name;
		public string Name { get { return _name; }
			set { _name = value; SetName(value); } }
		public int Offset { get; set; }
		public int Count { get; set; }
        public ShaderUniformManager UniformManager { get; protected set; }

        protected Dictionary<TextureType, Texture> textures;
		protected Shader shaderProgram;
		
		public Material() : base(false)
		{
			textures = new Dictionary<TextureType, Texture>();
            Outline = new Outline();
		}

		void CreateShaderThreadUnsafe(string vs, string fs)
        {
			shaderProgram = ShaderConstructor.CreateShader(vs, fs);
			Texture2D txtr = Texture2D.LoadEmpty();
			TextureUnit unit = TextureUnit.Texture0;
			UniformManager = new ShaderUniformManager(shaderProgram.GetUniforms, this);

			shaderProgram.ApplyShader();
			if (ShaderSettings.TextureDiffuse)
			{
				textures.Add(TextureType.Diffuse, txtr);
				GL.ActiveTexture(unit + (int)TextureType.Diffuse);
				txtr.BindTexture();
			}

			if (ShaderSettings.TextureSpecular)
			{
				textures.Add(TextureType.Specular, txtr);
				GL.ActiveTexture(unit + (int)TextureType.Specular);
				txtr.BindTexture();
			}

			if (ShaderSettings.ToonShadow)
			{
				textures.Add(TextureType.Toon, txtr);
				GL.ActiveTexture(unit + (int)TextureType.Toon);
				txtr.BindTexture();
			}

			textures.Add(TextureType.Extra, txtr);
			GL.ActiveTexture(unit + (int)TextureType.Extra);
			txtr.BindTexture();

			if (ShaderSettings.EnvType > 0)
			{
				textures.Add(TextureType.Sphere, txtr);
				GL.ActiveTexture(unit + (int)TextureType.Sphere);
				txtr.BindTexture();
			}
		}

		protected void CreateShader(string vs, string fs)
		{
			if (GLWindow.gLWindow.CheckContext)
				CreateShaderThreadUnsafe(vs, fs);
			else
			{
				CoreEngine.ActiveCore.AddTask = () => CreateShaderThreadUnsafe(vs, fs);
			}
			
		}

		void SetName(string name)
        {
			if (name != null)
			shaderProgram.SetName(name);
		}

		public virtual void SetTexture(Texture txtr, TextureType type)
		{
			if (textures.ContainsKey(type) && txtr != null)
			{
				textures[type] = txtr;
			}
		}

		
		protected override void Unload()
		{
			shaderProgram.DeleteShader();
		}
		

		internal virtual void ApplyMaterial()
		{
			shaderProgram.ApplyShader();
			TextureUnit unit = TextureUnit.Texture0;
			foreach (var kv in textures)
			{
				GL.ActiveTexture(unit + (int) kv.Key);
				kv.Value.BindTexture();
			}
		}

        public abstract Material Clone();
	}
}
