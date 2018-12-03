using System;
using OpenTK;

namespace Toys
{
	public class MorphVertex : Morph
	{
		Vector4[] morph;
		int offset = 0;
        public MeshMorper meshMorpher { set; private get; }
        float degree = 0f;

		public MorphVertex(string Name, string NameEng, int count)
		{
			morph = new Vector4[count];
			base.Name = Name;
			base.NameEng = NameEng;
			type = MorphType.Vertex;
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

        public override float morphDegree {
            get
            {
                return degree;
            }
            set
            {
                if (value == degree)
                    return;

                PerformMorph(value);
                degree = value;
            }
        }

        private void PerformMorph(float degree)
        {
            if (meshMorpher == null)
                return;

            meshMorpher.Morph(morph,degree);
        }
    }
}