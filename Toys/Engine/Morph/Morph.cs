using System;
namespace Toys
{
	//base class for morphs
	public enum MorphType
	{
		Group = 1,
		Vertex,
		Bone,
		Uv,
		UvEx1,
		UvEx2,
		UvEx3,
		UvEx4,
		Material,
		Flip,
		Impulse
	}

	public class Morph
	{
		public string Name;
		public string NameEng;
		public MorphType type;
		public int count;

	}
}
