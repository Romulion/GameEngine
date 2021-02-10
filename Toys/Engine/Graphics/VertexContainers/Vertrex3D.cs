using System;
using OpenTK.Mathematics;
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
        public Vector3 Position;
        [FieldOffset(16)]
        public Vector3 Normal;
        [FieldOffset(32)]
        public Vector2 UV;
	
		public Vertex3D(Vector3 position, Vector3 normal, Vector2 uv)
		{
            Position = position;
            Normal = normal;
            UV = uv;
		}

		//pos only
		public Vertex3D(Vector3 position)
		{
			Position = position;
			Normal = Vector3.Zero;
			UV = Vector2.Zero;
		}

		public Vertex3D(Vector3 position, Vector2 uv)
		{
			Position = position;
			Normal = Vector3.Zero;
			UV = uv;
		}

		//for 2d coordinates
		public Vertex3D(Vector2 position)
		{
			Position = new Vector3(position);
			Normal = Vector3.Zero;
			UV = Vector2.Zero;
		}

		public Vertex3D(Vector2 position, Vector2 uv)
		{
			Position = new Vector3(position);
			Normal = Vector3.Zero;
			UV = uv;
		}
	}

}
