using System;
namespace Toys
{
	//base class for morphs
	public enum MorphType
	{
		Group = 0,
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

	public abstract class Morph
	{
		protected float degree = 0f;
		public string Name;
		public string NameEng;
		public MorphType Type;
		public int Count;
        public abstract float MorphDegree {  get; set; }
	}
}
