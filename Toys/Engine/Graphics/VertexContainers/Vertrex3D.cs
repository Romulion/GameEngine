using System;
using OpenTK;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Toys
{
    /// <summary>
    /// standart vertex set position + normals + uv
    /// converting 2d to 3d
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 48)]
    public struct Vertex3D
	{
        [FieldOffset(0)]
        public Vector3 position;
        [FieldOffset(16)]
        public Vector3 normal;
        [FieldOffset(32)]
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
