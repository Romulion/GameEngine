using System;
using OpenTK;
using System.Collections.Generic;

namespace Toys
{
	public class Scene
	{
        SceneNode root;
        //List<SceneNode> rootNodes = new List<SceneNode>();
		LightSource light;
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


		internal void Update(float time)
		{
			foreach (var node in root.Childs)
			{
                if (node.Active)
                    node.UpdateTransform();
                //todo: move this to animation system
                Animator an = node.GetComponent(typeof(Animator)) as Animator;
                if (an)
					an.Update(time);
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

        private List<SceneNode> TraverseNodeTree(Predicate<SceneNode> condition, SceneNode root)
        {
            List<SceneNode> nodes = new List<SceneNode>();

            foreach (var node in root.Childs)
            {
                if (condition(node))
                    nodes.Add(node);
                nodes.AddRange(TraverseNodeTree(condition, node));
            }
            
            return nodes;
        }
	}
}
