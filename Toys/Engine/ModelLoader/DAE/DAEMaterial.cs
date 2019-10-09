using System;
using OpenTK;

namespace Toys
{
	public class DAEMaterial
	{
		public string Name;
		public string ID;
		public Texture2D DiffuseTexture;
		public Vector4 Emission;
		public Vector4 Ambient;
		public Vector4 Specular;
		public string TextureName;
	}
}