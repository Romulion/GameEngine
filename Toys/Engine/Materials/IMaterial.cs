using System;
namespace Toys
{
	public interface IMaterial
	{
		//indexed range
		int count { get;}
		int offset{ get;}

		bool drawShadow { get; set; }
		bool dontDraw { get; set; }
		void ApplyMaterial();
		Shader GetShader { get; }
		string Name { get; set; }
	}
}
