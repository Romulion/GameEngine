using System;
using OpenTK;

namespace Toys
{
	//outline render variables
	public interface IOutline
	{
		float EdgeScaler { get; }
		Vector4 EdgeColour { get; }
		bool hasEdge { get; set;}
		void ApplyOutline();
	}
}
