using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toys {
    public abstract class InteractableComponent : VisualComponent
    {
        protected InteractableComponent(Type type) : base(type) { }

        internal abstract void ClickDownState();
        internal abstract void ClickUpState();
        internal abstract void Hover();
        internal abstract void Normal();

        internal virtual void PositionUpdate(float x, float y)
        {

        }
    }
}
