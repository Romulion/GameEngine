using System;
using OpenTK;

namespace Toys
{
	public class MorphVertex : Morph
	{
		Vector4[] morph;
		int offset = 0;
        public MeshMorper MeshMorpher { set; private get; }
        float degree = 0f;

		public MorphVertex(string name, string nameEng, int count)
		{
			morph = new Vector4[count];
			Name = name;
			NameEng = nameEng;
			Type = MorphType.Vertex;
		}



		public void AddVertex(Vector3 pos, int index)
		{
			morph[offset] = new Vector4(pos, index);
			offset++;
		}

		public Vector4[] GetMorph
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