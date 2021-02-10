using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toys.Engine.UserInterface
{
    /// <summary>
    /// Class for quick common UI element creation
    /// </summary>
    public static class UIHelper
    {
        public static UIElement GetImage()
        {
            var el = new UIElement();
            el.AddComponent<RawImage>();

            return el;
        }

        public static UIElement GetButton()
        {
            var el = new UIElement();
            el.AddComponent<RawImage>();

            return el;
        }
    }
}
