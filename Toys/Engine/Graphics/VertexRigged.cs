using System;
using OpenTK;

namespace Toys
{
	public struct VertexRigged
	{

		public Vector3 position;
		public Vector3 normal;
		public Vector2 uvtex;
		public IVector4 boneIndexes;
		public Vector4 weigth;


		public VertexRigged(Vector3 pos, Vector3 norm, Vector2 tex, IVector4 indexes, Vector4 weigth)
		{
			position = pos;
			normal = norm;
			uvtex = tex;
			boneIndexes = indexes;
			this.weigth = weigth;
		}
	
	}

	public struct IVector4
	{
		public int bone1;
		public int bone2;
		public int bone3;
		public int bone4;

		public IVector4(int[] bones)
		{
			bone1 = bones[0];
			bone2 = bones[1];
			bone3 = bones[2];
			bone4 = bones[3];
		}

		public override string ToString()
		{
			return string.Format("({0},{1},{2},{3})",bone1,bone2,bone3,bone4);
		}
	}
}
