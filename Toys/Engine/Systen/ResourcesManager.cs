using System;
using System.Collections.Generic;
using System.Linq;

namespace Toys
{
	public class ResourcesManager
	{
		List<Resource> resources = new List<Resource>();

		CoreEngine ce;

		public ResourcesManager(CoreEngine core)
		{
			ce = core;
		}

	}
}
