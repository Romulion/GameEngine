using System;

namespace Toys
{
    public abstract class Resource
	{

		string Name { get; }
		internal string Id { get; set;}
		protected abstract void Unload();
		public bool isDestroyed;
		
		protected bool IsThreadSafe { get; private set; }

		protected Resource(bool threadSafe = true)
		{
			IsThreadSafe = threadSafe;
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
		public static void Destroy(Resource res)
        {
			if (!res.isDestroyed)
			{
				if (GLWindow.gLWindow.CheckContext || res.IsThreadSafe)
					res.Destroy();
				else
					CoreEngine.ActiveCore.AddTask = res.Destroy;
			}
        }

		private void Destroy()
        {
			Unload();
			isDestroyed = true;
		}

		public static bool operator true(Resource res)
		{
			return res != null;
		}

		public static bool operator false(Resource res)
		{
			return res == null;
		}

        public static bool operator !(Resource res)
        {
            return res == null;
        }

        public override bool Equals(object obj)
		{
			return base.Equals(obj);
		}

		//Clean "Lost" resources. Slow
        ~Resource()
        {			
			Destroy(this);
		}
	}
}
