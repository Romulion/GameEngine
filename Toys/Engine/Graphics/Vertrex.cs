using System;
using OpenTK;
using System.Collections.Generic;
namespace Toys
{
	public struct Vertex
	{
		public Vector3 position;
		public Vector3 normal;
		public Vector2 uvtex;
	
		public Vertex(Vector3 pos, Vector3 norm, Vector2 tex)
		{
			position = pos;
			normal = norm;
			uvtex = tex;
		}

	}

}
