using System;
using OpenTK;

namespace Toys
{
	//outline render variables
	public interface IOutline
	{
		float EdgeScaler { get; }
		Vector4 EdgeColour { get; }
		bool HasEdge { get; set;}
		void ApplyOutline();
	}
}
