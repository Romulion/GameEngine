using System;
using OpenTK;
using System.Collections.Generic;
namespace Toys
{
	/// <summary>
	/// /size 48 bits
	/// </summary>
	public struct Vertex
	{
		public Vector3 position;
		public Vector3 normal;
		//public Vector2 uvtex;
		public Vector3 uvtex;
	
		public Vertex(Vector3 pos, Vector3 norm, Vector2 tex)
		{
			position = pos;
			normal = norm;
			//uvtex = tex;
			uvtex = new Vector3(tex);
		}


		public static explicit operator Vertex(VertexRigged vr)
		{
			return new Vertex(vr.position.Xyz, vr.normal.Xyz, vr.uvtex.Xy);
		}

	}

}
