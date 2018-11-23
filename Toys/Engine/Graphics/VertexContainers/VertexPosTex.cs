using System;
using OpenTK;

namespace Toys
{
	public struct VertexPosTex
	{
		/// <summary>
		/// 3D vertex
		/// position + textures
		/// </summary>
		public Vector4 position;


		public VertexPosTex(Vector3 pos)
		{
			position = new Vector4(pos, 1f);
		}
	}
}
