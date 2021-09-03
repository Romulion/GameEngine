using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using System.Linq;

namespace Toys
{
	public abstract class Material
	{
		internal ShaderSettings ShaderSettings { get; set; }
        public RenderDirectives RenderDirrectives { get; set; }
		public Outline Outline;

		public string Name { get; set; }
		public int Offset { get; set; }
		public int Count { get; set; }
        public ShaderUniformManager UniformManager { get; protected set; }

        protected Dictionary<TextureType, Texture2D> textures;
		protected Shader shaderProgram;
		
		public Material()
		{
			textures = new Dictionary<TextureType, Texture2D>();
            Outline = new Outline();
		}

		protected void CreateShader(Shader shader)
		{
            shaderProgram = shader;
            Texture2D txtr = Texture2D.LoadEmpty();
			TextureUnit unit = TextureUnit.Texture0;
            UniformManager = new ShaderUniformManager(shaderProgram.GetUniforms,this);

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

            if (ShaderSettings.EnvType > 0)
            {
                textures.Add(TextureType.Sphere, txtr);
                GL.ActiveTexture(unit + (int)TextureType.Sphere);
                txtr.BindTexture();
            }
		}

		public virtual void SetTexture(Texture2D txtr, TextureType type)
		{
			if (textures.ContainsKey(type) && txtr != null)
			{
				textures[type] = txtr;
			}
		}

		internal virtual void UpdateMaterial()
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
