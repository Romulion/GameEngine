using System;
using OpenTK;

namespace Toys
{
	public class MorphVertex : Morph
	{
		Vector4[] morph;
		int offset = 0;

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
	}
}