using System;
using OpenTK;
using System.Collections.Generic;

namespace Toys
{
	public class Scene
	{
        List<SceneNode> nodes = new List<SceneNode>();
		LightSource light;

		public void OnLoad()
		{
            light = new LightSource();
        }

		public void AddObject(SceneNode node)
		{
            nodes.Add(node);
		}


		public void Update(float time)
		{
			
			List<Animator> anims = new List<Animator>();

			foreach (var node in nodes)
			{
				var an = (Animator)node.GetComponent(typeof(Animator));
				if (an)
					an.Update(time);
			}
        }

		public LightSource GetLight
		{
			get { return light; } 
		}

		public SceneNode[] GetNodes()
		{
			return nodes.ToArray();
		}

		public SceneNode FindByName(string name)
		{
			return nodes.Find( (node) => name == node.Name);
		}
	}
}
