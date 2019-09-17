using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using System.Windows.Forms;
using System.Linq;

namespace Toys
{
	public abstract class Material
	{
		public ShaderSettings shdrSettings { get; set; }
		public RenderDirectives rndrDirrectives { get; set; }
		public Outline outln;

		public string Name { get; set; }
		public int offset { get; set; }
		public int count { get; set; }
        public ShaderUniform[] variables { get; private set; }
        public ShaderUniformManager UniManager { get; private set; }

        protected Dictionary<TextureType, Texture> textures;
		protected Shader shdr;
		
		public Material()
		{
			textures = new Dictionary<TextureType, Texture>();
            outln = new Outline();
		}

		protected void CreateShader(Shader shader)
		{
            shdr = shader;
            Texture txtr = Texture.LoadEmpty();
			TextureUnit unit = TextureUnit.Texture0;
            variables = shdr.uniforms;
            UniManager = new ShaderUniformManager(shdr.uniforms,this);

			shdr.ApplyShader();
            if (shdrSettings.TextureDiffuse)
            {
                textures.Add(TextureType.Diffuse, txtr);
                GL.ActiveTexture(unit + (int)TextureType.Diffuse);
                txtr.BindTexture();
            }

            if (shdrSettings.TextureSpecular)
            {
                textures.Add(TextureType.Specular, txtr);
                GL.ActiveTexture(unit + (int)TextureType.Specular);
                txtr.BindTexture();
            }

            if (shdrSettings.toonShadow)
            {
                textures.Add(TextureType.Toon, txtr);
                GL.ActiveTexture(unit + (int)TextureType.Toon);
                txtr.BindTexture();
            }

            if (shdrSettings.envType > 0)
            {
                textures.Add(TextureType.Sphere, txtr);
                GL.ActiveTexture(unit + (int)TextureType.Sphere);
                txtr.BindTexture();
            }
		}

		public virtual void SetTexture(Texture txtr, TextureType type)
		{
			if (textures.ContainsKey(type))
			{
				textures[type] = txtr;
			}
		}

		public virtual void UpdateMaterial()
		{
			shdr.DeleteShader();
		}

		public virtual void ApplyMaterial()
		{
			shdr.ApplyShader();
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
