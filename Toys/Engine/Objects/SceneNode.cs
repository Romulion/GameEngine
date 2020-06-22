using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Toys
{
	public class SceneNode : Resource
    {
        Logger logger = new Logger("SceneNode");
        readonly Transform transform;
        List<Component> components;

        /// <summary>
        /// Node first generation childs
        /// </summary>
        public List<SceneNode> Childs { get; private set; }

        /// <summary>
        /// Parent of node
        /// </summary>
        public SceneNode Parent { get; private set; }
        
        /// <summary>
        /// Node name
        /// </summary>
		public string Name = "Node";

        /// <summary>
        /// Node activity in hierarchy
        /// </summary>
		public bool Active = true;

        public Scene ParentScene { get; internal set; }

		public SceneNode() : base (typeof(SceneNode))
        {
            Childs = new List<SceneNode>();
			components = new List<Component>();
            Parent = null;
            ParentScene = null;
            transform = new Transform(this);
        }

        public Transform GetTransform
        {
            get
            {
                return transform;
            }
        }

        /// <summary>
        /// Set parent to node
        /// node can have only one parent
        /// </summary>
        /// <param name="node">parent node</param>
        public void SetParent(SceneNode node)
        {
            if (!node)
                return;
            if (Parent)
                Parent.RemoveChild(this);
            ParentScene = node.ParentScene;
            Parent = node;
            Parent.AddChilld(this);
            UpdateTransform();
        }

        public void UpdateTransform()
        {
            transform.UpdateGlobalTransform();
            foreach (var child in Childs)
            {
                if (child.Active)
                    child.UpdateTransform();
            }
        }

        internal void RemoveChild(SceneNode node)
        {
            Childs.Remove(node);
        }

        private void AddChilld(SceneNode node)
        {
            Childs.Add(node);
        }

        /// <summary>
        /// Add initialized component
        /// </summary>
        /// <param name="comp"></param>
        public void AddComponent(Component comp)
		{
            comp.AddComponent(this);
            components.Add(comp);
		}

        /// <summary>
        /// Initialize and add component of selected type
        /// </summary>
        /// <returns>new component</returns>
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

        /// <summary>
        /// Search component of selected type.
        /// Returns first found.
        /// </summary>
        public Component GetComponent<T>() where T : Component
        {
            Type t = typeof(T); 
            return GetComponent(t);
        }

        /// <summary>
        /// Search component of selected type.
        /// Returns first found.
        /// </summary>
        public Component GetComponent(Type ctype)
		{
            Component result = null;
            for (int i = 0; i < components.Count; i++) {
                if (components[i].Type == ctype)
                {
                    result = components[i];
                    break;
                }
            }
            return result;
		}

        /// <summary>
        /// Search components of selected type.
        /// </summary>
        public Component[] GetComponents<T>() where T : Component
        {
            Type t = typeof(T);
            return GetComponents(t);
        }

        /// <summary>
        /// Search components of selected type.
        /// </summary>
        public Component[] GetComponents(Type ctype)
		{
			var result = from comp in components
						 where comp.Type == ctype
						 select comp;

			return result.ToArray();
		}

        /// <summary>
        /// Get all components
        /// </summary>
        /// <returns></returns>
        public Component[] GetComponents()
        {
            return components.ToArray();
        }

        internal override void Unload()
		{
            foreach (var comp in components)
            {
                comp.RemoveComponent();
                comp.Unload();
            }
		}
    }
}
