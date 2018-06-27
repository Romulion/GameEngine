using System;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using System.Collections;

namespace Toys
{
	public class MaterialPMX : IMaterial
	{
		public Texture[] textures;


		public string NameEng;
		public Vector4 DiffuseColor = Vector4.One;
		public Vector3 SpecularColour;
		public float Specular;
		public Vector3 AmbientColour;
		public Vector4 EdgeColour;
		public float EdgeScaler;
		public bool dontDraw { get; set; }
		public string Name { get; set; }
		public Outline outline { get; set; }

		//flags
		public bool drawShadow { get; set; }
		public bool noCull;
		public bool groundShadow;
		public bool receiveShadow;
		public bool hasEdge;
		public bool vertexColour;
		public bool pointDrawing;
		public bool lineDrawing;

		Shader program;
		Shader outlin;

		public MaterialPMX()
		{
			program = ShaderManager.GetInstance.GetShader("pmx");
			outlin = ShaderManager.GetInstance.GetShader("outline");
		}

		public int offset { get; set; }
		public int count { get; set; }

		public byte SetFlags 
		{
			set {
				BitArray flags = new BitArray(new byte[] { value });
				noCull = flags[0];
				groundShadow = flags[1];
				drawShadow = flags[2];
				receiveShadow = flags[3];
				hasEdge = flags[4];
				vertexColour = flags[5];
				pointDrawing = flags[6];
				lineDrawing = flags[7];
			}
		}


		public Shader GetShader
		{
			get { return program;}
		}



		public void ApplyMaterial() 
		{
			program.ApplyShader();
			if (textures != null)
			{
				TextureUnit unit = TextureUnit.Texture0;
				foreach (Texture txtr in textures)
				{
					GL.ActiveTexture(unit);
					txtr.BindTexture();
					unit += 1;      
    			}
			}
		}


		public void ApplyOutline()
		{
			outlin.SetUniform(EdgeColour, "EdgeColor");
			outlin.SetUniform(EdgeScaler, "EdgeScaler");
		}



	}
}
