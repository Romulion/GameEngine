using System;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using System.Collections;

namespace Toys
{
	public class MaterialPMX : Material
	{
		public Vector4 DiffuseColor = Vector4.One;
		public Vector3 SpecularColour;
		public float Specular;
		public Vector3 AmbientColour;

		//IOutline
		public Vector4 EdgeColour{ get; set;}
		public float EdgeScaler { get; set;}

		//IMateria


		//flags
		public bool noCull;
		public bool groundShadow;
		public bool receiveShadow;
		public bool vertexColour;
		public bool pointDrawing;
		public bool lineDrawing;

        public MaterialPMX(ShaderSettings shdrsett, RenderDirectives rdir) :base()
		{
            shdrSettings = shdrsett;
            rndrDirrectives = rdir;
            CreateShader();
        }
        /*
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
        */

        private void CreateShader()
        {
            shdr = ShaderConstructor.CreateShader(shdrSettings);
            CreateShader(shdr);
        }

        public override Material Clone()
        {
            var material = new MaterialPMX(shdrSettings, rndrDirrectives);
            foreach (var texture in textures)
                material.SetTexture(texture.Value, texture.Key);

            return material;
        }

    }
}
