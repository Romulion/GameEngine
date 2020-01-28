﻿using System;
using System.Collections.Generic;

namespace Toys
{
	public class UniformBufferManager
	{
		private static UniformBufferManager _uniformManager;

		Dictionary<string, UniformBuffer> _buffers = new Dictionary<string, UniformBuffer>();

		public enum Target
		{
			Skeleton = 0,
			ModelCoord = 1,
			Scene = 2,
		}

		UniformBufferManager()
		{
			//add default buffers
			_buffers.Add("skeleton", new UniformBufferSkeleton());
			_buffers.Add("space", new UniformBufferSpace());
			_buffers.Add("light", new UniformBufferLight());
            _buffers.Add("system", new UniformBufferSystem());
		}


		public static UniformBufferManager GetInstance
		{
			get
			{
				if (_uniformManager == null)
					_uniformManager = new UniformBufferManager();

				return _uniformManager;

			}
		}

		public UniformBuffer GetBuffer(string name)
		{
			if (_buffers.ContainsKey(name))
				return _buffers[name];

			Console.WriteLine("shader {0} not found", name);
			return null;
		}
	}
}
