using System;
namespace Toys
{
	public interface IMaterial
	{
		int count { get; }
		int offset{ get; }
		bool dontDraw { get; set; }
		void ApplyMaterial();
	}
}
