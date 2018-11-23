using System;
using OpenTK;

namespace Toys
{
	public struct VertexPos3D
	{
		/// <summary>
		/// 3D vertex
		/// position only
		/// </summary>
		public Vector4 position;

		public VertexPos3D(Vector3 pos)
		{
			position = new Vector4(pos, 1f);
		}
	}
}
