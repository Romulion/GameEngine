using System;
using OpenTK;
using System.Collections.Generic;

namespace Toys
{
	public class Scene
	{
        List<SceneNode> nodes = new List<SceneNode>();
		LightSource light;
        public Canvas canvas;
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
                nodes.Add(node);
                node.ParentScene = this;
            }
        }


		internal void Update(float time)
		{
			foreach (var node in nodes)
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
			return nodes.ToArray();
		}

        /// <summary>
        /// Find first node in scene using name
        /// </summary>
        /// <param name="name">Exact name of node</param>
        /// <returns></returns>
		public SceneNode FindByName(string name)
		{
			return nodes.Find( (node) => name == node.Name);
		}

        public void RemoveNode(SceneNode node)
        {
            nodes.Remove(node);
        }
	}
}
