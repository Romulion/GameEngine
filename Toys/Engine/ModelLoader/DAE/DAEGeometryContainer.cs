using System;
using OpenTK;

namespace Toys
{
	public class DAEGeometryContainer
	{
		public string id;
		public string name;
		public Vector3[] position;
		public Vector3[] normals;
		public Vector2[] uvcord;
		public Vector3[] colors;
		public Vector4[] weigth;
		public IVector4[] boneIndeces;
		public int[] indeces;
		public string mat;

		public DAEGeometryContainer(uint vertCount)
		{
			position = new Vector3[vertCount];
			normals = new Vector3[vertCount];
			uvcord = new Vector2[vertCount];
			weigth = new Vector4[vertCount];
			boneIndeces = new IVector4[vertCount];
		}
	}
}
