using System;
using OpenTK.Mathematics;

namespace Toys
{

	public class MorphUV : Morph
	{

		UVMorphData[] morph;
		int offset = 0;
		public MeshMorper MeshMorpher { private set; get; }

		public MorphUV(string name, string nameEng, int count, MeshMorper meshMorper)
		{
            MeshMorpher = meshMorper;
            morph = new UVMorphData[count];
			Name = name;
			NameEng = nameEng;
			Type = MorphType.Uv;
		}



		public void AddVertex(Vector2 pos, int index)
		{
			morph[offset] = new UVMorphData() {UV = pos, ID = index };
			offset++;
		}

		public UVMorphData[] GetMorph
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
