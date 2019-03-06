using System;
using OpenTK;

namespace Toys
{

	public class MorphUV : Morph
	{

		Vector3[] morph;
		int offset = 0;
		public MeshMorper meshMorpher { set; private get; }
		float degree = 0f;

		public MorphUV(string Name, string NameEng, int count)
		{
			morph = new Vector3[count];
			base.Name = Name;
			base.NameEng = NameEng;
			type = MorphType.Uv;
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

		public override float morphDegree
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
			if (meshMorpher == null)
				return;

			meshMorpher.Morph(morph, degree);
		}
	}

}
