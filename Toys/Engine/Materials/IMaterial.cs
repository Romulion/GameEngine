using System;
namespace Toys
{
	public interface IMaterial
	{
		//indexed range
		int count { get; set;}
		int offset{ get;}

		ShaderSettings shdrSettings { get; set; }
		RenderDirectives rndrDirrectives { get; set; }
		//bool drawShadow { get; set; }
		//bool dontDraw { get; set; }
		void ApplyMaterial();
		//Shader GetShader { get; }
		string Name { get; set; }
	}
}
