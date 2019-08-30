using System;
namespace Toys
{
	public abstract class Component : Resource
	{
		//internal Component(Type t, string id) : base (t,id){ }

		protected Component(Type t) : base(t) { }

        public SceneNode node { get; protected set; }

        //system defined interpritation
        internal abstract void AddComponent(SceneNode nod);
        internal abstract void RemoveComponent();
    }
}
