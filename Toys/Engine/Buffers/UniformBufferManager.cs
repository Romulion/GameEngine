using System;
using System.Collections.Generic;

namespace Toys
{
	public class UniformBufferManager
	{
		private static UniformBufferManager unfrmMgmr;

		Dictionary<string, UniformBuffer> buffers = new Dictionary<string, UniformBuffer>();
		int currIndex = 3;

		public enum Target
		{
			Skeleton = 0,
			ModelCoord = 1,
			Scene = 2,
		}

		UniformBufferManager()
		{
			//add default buffers
			buffers.Add("skeleton", new UniformBufferSkeleton());
			buffers.Add("space", new UniformBufferSpace());
			buffers.Add("light", new UniformBufferLight());
		}


		public static UniformBufferManager GetInstance
		{
			get
			{
				if (unfrmMgmr == null)
					unfrmMgmr = new UniformBufferManager();

				return unfrmMgmr;

			}
		}

		public UniformBuffer GetBuffer(string name)
		{
			if (buffers.ContainsKey(name))
				return buffers[name];

			Console.WriteLine("shader {0} not found", name);
			return null;
		}
	}
}
