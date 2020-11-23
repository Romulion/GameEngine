using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toys {

    enum ButtonStates
    {
        Normal,
        Hover,
        Clicked,
        Unclicked,
    }
    /// <summary>
    /// Base class for Interactable Components
    /// </summary>
    public abstract class InteractableComponent : VisualComponent
    {
        protected InteractableComponent(Type type) : base(type) { }

        internal bool IsAllowDrag;

        //state change processors
        /// <summary>
        /// Preceed mouse click down state
        /// </summary>
        internal abstract void ClickDownState();
        /// <summary>
        /// Preceed mouse click up state
        /// </summary>
        internal abstract void ClickUpState();
        /// <summary>
        /// Preceed mouse hover state
        /// </summary>
        internal abstract void Hover();

        /// <summary>
        /// Return to normal state
        /// </summary>
        internal abstract void Normal();

        //for dragable components
        internal virtual void PositionUpdate(float x, float y)
        {

        }
    }
}
