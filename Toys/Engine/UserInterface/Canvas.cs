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

        internal List<VisualComponent> activeComponents = new List<VisualComponent>();
        internal List<InteractiveComponent> activeButtons = new List<InteractiveComponent>();
        internal List<UIMaskComponent> masks = new List<UIMaskComponent>();

        List<UIElement> rootNodes = new List<UIElement>();
        //public UIElement Root;
        public RenderMode Mode { get; set; }
        public bool IsActive = true;
        public float Canvas2WorldScale { get; set; }
        public float CanvasScale { get; set; }
        public Canvas() { 
            Canvas2WorldScale = 1;
            CanvasScale = 1;
        }

        public enum RenderMode
        {
            Overlay,
            ScreenSpace,
            WorldSpace,
        };

        public void Add2Root(UIElement node)
        {
            rootNodes.Add(node);
        }

        public UIElement[] GetNodes()
        {
            return rootNodes.ToArray();
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
            foreach (var root in rootNodes)
                UnloadTree(root);
        }


        private void UnloadTree(UIElement uie)
        {
            foreach(var child in uie.Childs)
            {
                UnloadTree(child);
            }
            uie.Unload();
        }
        
    }
}
