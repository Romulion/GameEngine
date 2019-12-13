using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toys
{
    /// <summary>
    /// Scene analog for ui elements
    /// </summary>
    public class Canvas
    {
        List<UIElement> nodes = new List<UIElement>();
        public enum RenderMode
        {
            Overlay,
            ScreenSpace
        };

        public void AddObject(UIElement node)
        {
            nodes.Add(node);
        }

        public UIElement[] GetNodes()
        {
            return nodes.ToArray();
        }
    }
}
