using System;
using OpenTK.Mathematics;

namespace Toys
{
	public class MorphVertex : Morph
	{
		internal VertMorphData[] morph;
		int offset = 0;
        public MeshMorper MeshMorpher { private set; get; }

		public MorphVertex(string name, string nameEng, int count, MeshMorper meshMorper)
		{
            MeshMorpher = meshMorper;
            morph = new VertMorphData[count];
			Name = name;
			NameEng = nameEng;
			Type = MorphType.Vertex;
		}



		public void AddVertex(Vector3 pos, int index)
		{
            morph[offset] = new VertMorphData() { Pos = pos, ID = index};
			offset++;
		}

		public VertMorphData[] GetMorph
		{
			get { return morph; }
		}

        public override float MorphDegree {
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

            MeshMorpher.Morph(morph,degree);
        }
    }
}