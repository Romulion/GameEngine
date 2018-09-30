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
        //vector4 aligment
        //public Vector3 position;
        //public Vector3 normal;
        //public Vector2 uvtex;
        public Vector4 position;
        public Vector4 normal;
        public Vector4 uvtex;
	
		public Vertex(Vector3 pos, Vector3 norm, Vector2 tex)
		{
            position = new Vector4(pos, 1.0f);
            normal = new Vector4(norm, 1.0f);
            //uvtex = tex;
            uvtex = new Vector4(tex);
		}


		public static explicit operator Vertex(VertexRigged vr)
		{
			return new Vertex(vr.position.Xyz, vr.normal.Xyz, vr.uvtex.Xy);
		}

	}

}
