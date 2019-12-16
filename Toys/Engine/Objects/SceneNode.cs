using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Toys
{
	public class SceneNode : Resource
    {
        Logger logger = new Logger("SceneNode");
        public List<SceneNode> Childs { get; private set; }
        public SceneNode Parent;
        Transform transform;
		public string Name;
		public bool Active = true;
		List<Component> components;

		public SceneNode() : base (typeof(SceneNode))
        {
            Childs = new List<SceneNode>();
			components = new List<Component>();
            Parent = null;
            transform = new Transform(this);
        }

        public Transform GetTransform
        {
            get
            {
                return transform;
            }
        }

        public void SetParent(SceneNode node)
        {
            if (Parent != null)
                Parent.RemoveChild(this);

            Parent = node;
            Parent.AddChilld(this);
            UpdateTransform();
        }

        public void UpdateTransform()
        {
            transform.UpdateGlobalTransform();
            foreach (var child in Childs)
            {
                child.UpdateTransform();
            }
        }

        private void RemoveChild(SceneNode node)
        {
            Childs.Remove(node);
        }

        internal void AddChilld(SceneNode node)
        {
            Childs.Add(node);
        }

        //component framework
        public void AddComponent(Component comp)
		{
            comp.AddComponent(this);
            components.Add(comp);
		}

        public Component AddComponent<T>() where T : Component
        {
            Type t = typeof(T);
            try
            {
                Component comp = (Component)(t.GetConstructors()[0]).Invoke(new object[] { });
                comp.Type = t;
                comp.AddComponent(this);
                components.Add(comp);
                return comp;
            }
            catch (Exception e)
            {
                logger.Error(e.Message,e.StackTrace);
            }
                
            return null;
        }

        public Component GetComponent<T>() where T : Component
        {
            Type t = typeof(T); 
            return GetComponent(t);
        }

        public Component GetComponent(Type ctype)
		{
			var result = from comp in components
						 where comp.Type == ctype
						 select comp;

			if (result.Count() == 0)
				return null;
			
			return result.First();
		}

		public Component[] GetComponents(Type ctype)
		{
			var result = from comp in components
						 where comp.Type == ctype
						 select comp;

			return result.ToArray();
		}

        public Component[] GetComponents()
        {
            return components.ToArray();
        }

        internal override void Unload()
		{
			foreach (var comp in components)
				comp.Unload();
		}
    }
}
