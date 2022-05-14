using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Toys
{
	public class SceneNode : Resource, ISave
    {
        readonly Transform transform;
        List<Component> components;

        /// <summary>
        /// Node first generation childs
        /// </summary>
        List<SceneNode> childs;

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

		public SceneNode()
        {
            childs = new List<SceneNode>();
			components = new List<Component>();
            Parent = null;
            ParentScene = null;
            transform = new Transform();
            AddComponent(transform);
        }

        public Transform GetTransform
        {
            get
            {
                return transform;
            }
        }

        public bool IsInSave { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

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
            //UpdateTransform();
        }

        /// <summary>
        /// update self and trigger update for childs
        /// </summary>
        public void UpdateTransform()
        {
            transform.UpdateGlobalTransform();
            foreach (var child in childs)
            {
                if (child.Active)
                    child.UpdateTransform();
            }
        }

        internal void RemoveChild(SceneNode node)
        {
            childs.Remove(node);
        }

        private void AddChilld(SceneNode node)
        {
            childs.Add(node);
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
        /// Add initialized component
        /// </summary>
        /// <param name="comp"></param>
        public void RemoveComponent(Component comp)
        {
            comp.RemoveComponent();
            components.Remove(comp);
        }

        /// <summary>
        /// get copy list of node childs
        /// </summary>
        /// <returns></returns>
        public SceneNode[] GetChilds()
        {
            var list = new SceneNode[childs.Count];
            for (int i = 0; i < childs.Count; i++)
                list[i] = childs[i];
            return list;
        }

        /// <summary>
        /// Initialize and add component of selected type
        /// </summary>
        /// <returns>new component</returns>
        public T AddComponent<T>() where T : Component
        {
            try
            {
                
                var comp = Activator.CreateInstance<T>();
                comp.AddComponent(this);
                components.Add(comp);
                return comp;
            }
            catch (Exception e)
            {
                Logger.Error(e.Message,e.StackTrace);
            }
                
            return null;
        }

        /// <summary>
        /// Search component of selected type.
        /// Returns first found.
        /// </summary>
        public T GetComponent<T>() where T : Component
        {
            Type t = typeof(T); 
            return (T)GetComponent(t);
        }

        /// <summary>
        /// Return all attached components
        /// </summary>
        /// <returns></returns>
        public Component[] GetAllComponents()
        {
            return components.ToArray();
        }

        /// <summary>
        /// Search component of selected type.
        /// Returns first found.
        /// </summary>
        public Component GetComponent(Type ctype)
		{
            Component result = null;
            for (int i = 0; i < components.Count; i++) {
                if (components[i].GetType() == ctype)
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
        public T[] GetComponents<T>() where T : Component
        {
            Type t = typeof(T);
            var array = GetComponents(t);
            return Array.ConvertAll(array, item => (T)item); ;
        }

        /// <summary>
        /// Search components of selected type.
        /// </summary>
        public Component[] GetComponents(Type ctype)
		{
			var result = from comp in components
						 where comp.GetType() == ctype
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

        protected override void Unload()
		{
            
            foreach (var comp in components)
            {
                comp.RemoveComponent();
                Destroy(comp);
            }
            components.Clear();

            foreach (var child in GetChilds())
                child.Unload();
        }

        //for model instancing
        internal SceneNode Clone()
        {
            var result = new SceneNode();

            foreach (var cpmnt in GetComponents())
            {
                if (cpmnt.GetType() != typeof(Transform))
                    result.AddComponent(cpmnt.Clone());
            }
            return result;
        }
        #region Save System preps
        public Dictionary<string,string> SaveSequence(bool extended = false)
        {
            var savedata = new Dictionary<string, string>();
            savedata.Add("Name", Name);
            savedata.Add("Position", transform.Position.X + "," + transform.Position.Y + "," + transform.Position.Z);
            savedata.Add("Rotation", transform.Rotation.X + "," + transform.Rotation.Y + "," + transform.Rotation.Z);
            savedata.Add("Scale", transform.Scale.X + "," + transform.Scale.Y + "," + transform.Scale.Z);
            foreach (var component in components)
            {
                //check if class is saveble
                if (typeof(ISave).IsAssignableFrom(component.GetType()))
                {
                    //var forSave = (ISave)component;
                    //if (forSave.IsInSave)
                    //    savedata.AddRange(forSave.SaveSequence(extended));
                }
            }
                
            return savedata;
        }

        public void LoadSequence(Dictionary<string, string> saveData, bool extended = false)
        {
            Name = saveData["Name"];
            transform.Position = ParceString(saveData["Position"]);
            transform.Rotation = ParceString(saveData["Rotation"]);
            transform.Scale = ParceString(saveData["Scale"]);
        }

        private OpenTK.Mathematics.Vector3 ParceString(string vector)
        {
            var vec3 = OpenTK.Mathematics.Vector3.Zero;
            try
            {
                var values = vector.Split(',');
                vec3.X = float.Parse(values[0]);
                vec3.Y = float.Parse(values[1]);
                vec3.Z = float.Parse(values[2]);
            }
            catch (Exception e) { }

            return vec3;
        }
        #endregion

    }
}
