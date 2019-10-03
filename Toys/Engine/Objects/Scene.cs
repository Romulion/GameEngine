using System;
using OpenTK;
using System.Collections.Generic;

namespace Toys
{
	public class Scene
	{
        List<SceneNode> nodes = new List<SceneNode>();
		LightSource light;

        int i = 0;

		public void OnLoad()
		{
            var camera = new Camera();
            SceneNode node = new SceneNode();
            node.AddComponent(camera);
            node.AddComponent<CameraControllScript>();
            node.GetTransform.Position = new Vector3(0, 1, 3);
            nodes.Add(node);
            node.AddComponent<DynamicFormScript>();
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
