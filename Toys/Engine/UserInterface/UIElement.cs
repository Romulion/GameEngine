using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toys
{
    public class UIElement : Resource
    {
        Logger logger = new Logger("SceneNode");
        public List<UIElement> Childs { get; private set; }
        public UIElement Parent;
        RectTransform transform;
        public string Name;
        public bool Active = true;
        List<VisualComponent> components;

        public UIElement() : base(typeof(UIElement))
        {
            Childs = new List<UIElement>();
            components = new List<VisualComponent>();
            Parent = null;
            transform = new RectTransform(this);
        }

        public RectTransform GetTransform
        {
            get
            {
                return transform;
            }
        }

        public void SetParent(UIElement node)
        {
            if (Parent != null)
                Parent.RemoveChild(this);

            Parent = node;
            Parent.AddChilld(this);
            //UpdateTransform();
        }

        public void UpdateTransform()
        {
            transform.UpdateGlobalPosition();
            //transform.UpdateGlobalPositionPix();
        }
        public void UpdateTransformRecursive()
        {
            transform.UpdateGlobalPosition();
            //transform.UpdateGlobalPositionPix();
            foreach (var child in Childs)
            {
                child.UpdateTransformRecursive();
            }
        }

        internal void RemoveChild(UIElement node)
        {
            Childs.Remove(node);
        }
        internal void AddChilld(UIElement node)
        {
            Childs.Add(node);
        }

        //component framework
        public void AddComponent(VisualComponent comp)
        {
            comp.AddComponent(this);
            components.Add(comp);
        }

        public VisualComponent AddComponent<T>() where T : VisualComponent
        {
            Type t = typeof(T);
            try
            {
                VisualComponent comp = (VisualComponent)(t.GetConstructors()[0]).Invoke(new object[] { });
                comp.Type = t;
                comp.AddComponent(this);
                components.Add(comp);
                return comp;
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e.StackTrace);
            }

            return null;
        }

        public VisualComponent GetComponent<T>() where T : VisualComponent
        {
            Type t = typeof(T);
            return GetComponent(t);
        }

        public VisualComponent GetComponent(Type ctype)
        {
            var result = from comp in components
                         where comp.Type == ctype
                         select comp;

            if (result.Count() == 0)
                return null;

            return result.First();
        }

        public VisualComponent[] GetComponents(Type ctype)
        {
            var result = from comp in components
                         where comp.Type == ctype
                         select comp;

            return result.ToArray();
        }

        public VisualComponent[] GetComponents()
        {
            return components.ToArray();
        }

        internal override void Unload()
        {
            foreach (var comp in components)
                comp.Unload();
        }

        public UIElement Clone()
        {
            var ui = new UIElement();
            ui.Name = Name;
            ui.transform.offsetMax = transform.offsetMax;
            ui.transform.offsetMin = transform.offsetMin;
            ui.transform.anchorMax = transform.anchorMax;
            ui.transform.anchorMin = transform.anchorMin;
            ui.components = new List<VisualComponent>(components.Count);
            for (int i = 0; i < components.Count; i++)
                ui.AddComponent(components[i].Clone());
        
            return ui;
        }
    }
}
