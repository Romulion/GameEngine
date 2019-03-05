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
		public bool recieveShadow;
		public bool toonShadow;
		public bool hasSkeleton;
		public bool affectedByLight;
		public bool TextureDiffuse;
		public bool TextureSpecular;
		public bool TextureAmbient;
		public bool discardInvisible;
        public bool DifuseColor;
	}
}
