using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using System.Windows.Forms;

namespace Toys
{
	public class Material : IMaterial
	{
		public ShaderSettings shdrSettings { get; set; }
		public RenderDirectives rndrDirrectives { get; set; }
		public Outline outln;

		public string Name { get; set; }
		public int offset { get; set; }
		public int count { get; set; }

		Dictionary<TextureType, Texture> textures;
		//Dictionary<string, float> uniforms;
		Shader shdr;

		
		public Material(ShaderSettings shdrsett, RenderDirectives rdir)
		{
			textures = new Dictionary<TextureType, Texture>();
			shdrSettings = shdrsett;
			rndrDirrectives = rdir;

			outln = new Outline();

			CreateShader();
		}


		void CreateShader()
		{
			Texture txtr = Texture.LoadEmpty();
			TextureUnit unit = TextureUnit.Texture0;

			shdr = ShaderConstructor.CreateShader(shdrSettings);
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

		public void SetTexture(Texture txtr, TextureType type)
		{


			//var type = txtr.GetTextureType;


			if (textures.ContainsKey(type))
			{
				textures[type] = txtr;
			}

		}

		public void UpdateMaterial()
		{
			shdr.DeleteShader();
			CreateShader();
		}

		public void ApplyMaterial()
		{
			shdr.ApplyShader();
			TextureUnit unit = TextureUnit.Texture0;
			foreach (var kv in textures)
			{
				GL.ActiveTexture(unit + (int) kv.Key);
				kv.Value.BindTexture();
			}
		}

		public Material Clone()
		{
			var material = new Material(shdrSettings, rndrDirrectives);
			foreach (var texture in textures)
				material.SetTexture(texture.Value,texture.Key);

			return material;
		}
	}
}
