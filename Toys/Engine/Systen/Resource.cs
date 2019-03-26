using System;

namespace Toys
{
	public abstract class Resource
	{

		string Name { get; }
		internal string Id { get; set;}
		internal Type type;
		internal abstract void Unload();


		protected Resource(Type type)
		{
			this.type = type;
		}

		/*
		public static bool operator ==(Resource res1, Resource res2)
		{
			return res1.Id == res2.Id;
		}

		public static bool operator !=(Resource res1, Resource res2)
		{
			return res1.Id != res2.Id;
		}
*/
		public static bool operator true(Resource res)
		{
			return res != null;
		}

		public static bool operator false(Resource res)
		{
			return res == null;
		}

		public override bool Equals(object obj)
		{
			return base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}
