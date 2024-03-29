﻿using System;
namespace Toys
{
	public abstract class Component : Resource
	{
		//internal Component(Type t, string id) : base (t,id){ }

		protected Component(bool threadSafe = true) : base(threadSafe) { }

        public SceneNode Node { get; protected set; }

        //system defined interpritation
        internal abstract void AddComponent(SceneNode nod);
        internal abstract void RemoveComponent();

        internal virtual Component Clone()
        {
            return this;
        }
    }
}
