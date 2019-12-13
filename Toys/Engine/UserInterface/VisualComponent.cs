using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toys
{
    public abstract class VisualComponent : Resource
    {
        //internal Component(Type t, string id) : base (t,id){ }

        protected VisualComponent(Type t) : base(t) { }

        public UIElement Node { get; protected set; }

        //system defined interpritation
        internal abstract void AddComponent(UIElement nod);
        internal abstract void RemoveComponent();
    }
}
