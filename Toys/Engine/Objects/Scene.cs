using System;
using OpenTK;
using System.Collections.Generic;

namespace Toys
{
	public class Scene
	{
        //dummy root node - origin of scene
        SceneNode root;
        //TODO: move to light system
		LightSource light;
        //root ui canvas
        public Canvas canvas;

        public Scene()
        {
            root = new SceneNode();
            root.Name = "Root";
            root.ParentScene = this;
        }

		internal void OnLoad()
		{
            light = new LightSource();
        }

        /// <summary>
        /// Add node to scene root
        /// </summary>
        /// <param name="node">Node to add</param>
		public void AddNode2Root(SceneNode node)
		{
            if (node)
            {
                node.SetParent(root);
            }
        }


		internal void Update()
		{
			foreach (var node in root.GetChilds())
			{
                if (node.Active)
                    node.UpdateTransform();
			}
        }

        /// <summary>
        /// TODO remove this
        /// </summary>
		public LightSource GetLight
		{
			get { return light; } 
		}

        /// <summary>
        /// Get array of all scene nodes
        /// </summary>
        /// <returns></returns>
		public SceneNode[] GetNodes()
		{
            var test = TraverseNodeTree((node) => true, root).ToArray();
            return test;

        }

        /// <summary>
        /// Get array of root scene nodes
        /// </summary>
        /// <returns></returns>
        public SceneNode[] GetRootNodes()
        {
            return root.GetChilds();
        }

        /// <summary>
        /// Find first node in scene using name
        /// </summary>
        /// <param name="name">Exact name of node</param>
        /// <returns></returns>
		public SceneNode[] FindByName(string name)
		{
            return TraverseNodeTree((node) => name == node.Name, root).ToArray();
        }


        public void RemoveNode(SceneNode node)
        {
            if (node.Parent)
                node.Parent.RemoveChild(node);
        }

        /// <summary>
        /// Search all nodes that matches condition
        /// </summary>
        /// <param name="condition">search condition</param>
        /// <param name="root">node to start from</param>
        /// <returns></returns>
        private List<SceneNode> TraverseNodeTree(Predicate<SceneNode> condition, SceneNode root)
        {
            List<SceneNode> nodes = new List<SceneNode>();

            foreach (var node in root.GetChilds())
            {
                if (condition(node))
                    nodes.Add(node);
                nodes.AddRange(TraverseNodeTree(condition, node));
            }
            
            return nodes;
        }
	}
}
