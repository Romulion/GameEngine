using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Toys
{
	public class SceneNode : Resource
    {
        List<SceneNode> childs;
        public SceneNode parent;
        Transformation transform;
		public string Name;
		public bool Active = true;
		List<Component> components;

		public SceneNode() : base (typeof(SceneNode))
        {
            childs = new List<SceneNode>();
			components = new List<Component>();
            parent = null;
            transform = new Transformation(this);
        }

        public Transformation GetTransform
        {
            get
            {
                return transform;
            }
        }

        public void SetParent(SceneNode node)
        {
            if (parent != null)
                parent.RemoveChild(this);

            parent = node;

            UpdateTransform();
        }

        public void UpdateTransform()
        {
            transform.UpdateGlobalTransform();
            foreach (var child in childs)
            {
                child.UpdateTransform();
            }
        }

        private void RemoveChild(SceneNode node)
        {
            childs.Remove(node);
        }


		//component framework
		public void AddComponent(Component comp)
		{
            //if (components.Exists((Component c) => c is ))
            comp.AddComponent(this);
            components.Add(comp);
		}

        public Component AddComponent<T>() where T : Component
        {
            Type t = typeof(T);
            try
            {
                Component comp = (Component)(t.GetConstructors()[0]).Invoke(new object[] { });
                comp.type = t;
                comp.AddComponent(this);
                components.Add(comp);
                return comp;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
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
			//if (components.Exists((Component c) => c is ))
			var result = from comp in components
						 where comp.type == ctype
						 select comp;

			if (result.Count() == 0)
				return null;
			
			return result.First();
		}

		public Component[] GetComponents(Type ctype)
		{
			//if (components.Exists((Component c) => c is ))
			var result = from comp in components
						 where comp.type == ctype
						 select comp;

			return result.ToArray();
		}

		internal override void Unload()
		{
			foreach (var comp in components)
				comp.Unload();
		}
    }
}
