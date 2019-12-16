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
    public class Canvas : Component
    {
        List<UIElement> nodes = new List<UIElement>();
        public UIElement Root;

        public Canvas() : base(typeof(Canvas)) {}

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

        internal override void AddComponent(SceneNode nod)
        {
            Node = nod;
            CoreEngine.gEngine.UIEngine.canvases.Add(this);
        }

        internal override void RemoveComponent()
        {
            Node = null;
            CoreEngine.gEngine.UIEngine.canvases.Remove(this);
        }

        internal override void Unload()
        {

        }
    }
}
