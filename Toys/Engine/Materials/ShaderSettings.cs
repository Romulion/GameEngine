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
		public EnvironmentMode EnvType = EnvironmentMode.None;
		public bool RecieveShadow = true;
		public bool ToonShadow;
		public bool HasSkeleton;
		public bool AffectedByLight = true;
		public bool TextureDiffuse = true;
		public bool TextureSpecular;
		public bool Ambient;
        public bool SpecularColor;
        public bool DiscardInvisible;
        public bool DifuseColor;

        public ShaderSettings Clone()
        {
            return (ShaderSettings)this.MemberwiseClone();
        }
    }
}
