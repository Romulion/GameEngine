using System;
using OpenTK;
using System.Collections.Generic;

namespace Toys
{
	public class Scene
	{
        List<SceneNode> nodes = new List<SceneNode>();
        public Camera camera;
		LightSource light;

        int i = 0;

		public void OnLoad()
		{
			camera = new Camera();
			light = new LightSource();
        }

		public void AddObject(SceneNode node)
		{
            nodes.Add(node);
		}


		public void Update(float time)
		{
			/*
			List<Animator> anims = new List<Animator>();

			foreach (var node in nodes)
			{
				var an = (Animator)node.GetComponent(typeof(Animator));
				if (an)
					an.Update(time);
			}
*/
            //light.pos = new Vector3(2 * (float)Math.Cos(radians(i)), 1.5f, 2 * (float)Math.Sin(radians(i)));
            //i++;
            //nodes[0].morph[20].morphDegree = Math.Abs((float)Math.Sin(radians(i * 2)));
            /*
			float angle = (float)Math.Sin(radians(i * 10)) * radians(70) - radians(70);
			var md = (MeshDrawerRigged)nodes[0].GetComponent(typeof(MeshDrawer));
			md.skeleton.Rotate(46, new Quaternion(angle, 0, 0) );
			md.skeleton.Rotate(47, new Quaternion(angle, 0, 0) );
			md.skeleton.Rotate(48, new Quaternion(angle, 0, 0) );
			//nodes[0].GetTransform.Rotation = new Vector3(0f, angle, 0);
				//models[0].anim.Rotate("上半身", new Quaternion(0f, angle, 0f) );
			i++;
            */
        }


        public Camera GetCamera
		{
			get { return camera; }
		}

		public LightSource GetLight
		{
			get { return light; } 
		}

		float radians(float angle)
		{
			return (float)Math.PI * angle / 360.0f;
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
