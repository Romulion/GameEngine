using System;
using OpenTK;
using System.Collections.Generic;
namespace Toys
{
	/// <summary>
	/// standart vertex set position + normals + uv
	/// converting 2d to 3d
	/// </summary>
	public struct Vertex3D
	{
        public Vector3 position;
        public Vector3 normal;
        public Vector2 uvtex;
	
		public Vertex3D(Vector3 pos, Vector3 norm, Vector2 tex)
		{
            position = pos;
            normal = norm;
            uvtex = tex;
		}

		//pos only
		public Vertex3D(Vector3 pos)
		{
			position = pos;
			normal = Vector3.Zero;
			uvtex = Vector2.Zero;
		}

		public Vertex3D(Vector3 pos, Vector2 tex)
		{
			position = pos;
			normal = Vector3.Zero;
			uvtex = tex;
		}

		//for 2d coordinates
		public Vertex3D(Vector2 pos)
		{
			position = new Vector3(pos);
			normal = Vector3.Zero;
			uvtex = Vector2.Zero;
		}

		public Vertex3D(Vector2 pos, Vector2 tex)
		{
			position = new Vector3(pos);
			normal = Vector3.Zero;
			uvtex = tex;
		}
	}

}
