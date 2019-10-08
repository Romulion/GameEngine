using System;
using OpenTK;

namespace Toys
{

	public class MorphUV : Morph
	{

		Vector3[] morph;
		int offset = 0;
		public MeshMorper MeshMorpher { set; private get; }
		float degree = 0f;

		public MorphUV(string name, string nameEng, int count)
		{
			morph = new Vector3[count];
			Name = name;
			NameEng = nameEng;
			Type = MorphType.Uv;
		}



		public void AddVertex(Vector2 pos, int index)
		{
			morph[offset] = new Vector3(pos.X, pos.Y, index);
			offset++;
		}

		public Vector3[] GetMorph
		{
			get { return morph; }
		}

		public override float MorphDegree
		{
			get
			{
				return degree;
			}
			set
			{
				PerformMorph(value - degree);
				degree = value;
			}
		}

		private void PerformMorph(float degree)
		{
			if (MeshMorpher == null)
				return;

			MeshMorpher.Morph(morph, degree);
		}
	}

}
