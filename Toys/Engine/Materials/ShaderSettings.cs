using System;
namespace Toys
{
	public enum EnvironmentMode {
		None = 0,
		Multiply = 1,
		Additive = 2,
		Subtract = 4,
	}

	public class ShaderSettings
	{
		public EnvironmentMode envType = EnvironmentMode.None;
		public bool recieveShadow = true;
		public bool toonShadow;
		public bool hasSkeleton;
		public bool affectedByLight = true;
		public bool TextureDiffuse = true;
		public bool TextureSpecular;
		public bool Ambient;
        public bool SpecularColor;
        public bool discardInvisible;
        public bool DifuseColor;
	}
}
