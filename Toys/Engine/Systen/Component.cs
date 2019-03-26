using System;
namespace Toys
{
	public abstract class Component : Resource
	{
		//internal Component(Type t, string id) : base (t,id){ }

		protected Component(Type t) : base(t) { }

		internal SceneNode node;
	}
}
