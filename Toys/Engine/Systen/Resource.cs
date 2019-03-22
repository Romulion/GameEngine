using System;
namespace Toys
{
	public abstract class Resource
	{
		string Name { get; }
		string Id { get; }
		string GetType { get; }

		internal abstract void Unload();
	}
}
