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


		public void Update()
		{

            //light.pos = new Vector3(2 * (float)Math.Cos(radians(i)), 1.5f, 2 * (float)Math.Sin(radians(i)));
			//i++;
			//models[0].morph[20].morphDegree = Math.Abs((float)Math.Sin(radians(i * 2)));
            /*
			if (models.Count > 0)
			{
				float angle = (float)Math.Sin(radians(i * 3)) * radians(40) - radians(20);
				models[0].anim.Rotate("EndHips", new Quaternion(0f, angle, 0f) );
				//models[0].anim.Rotate("上半身", new Quaternion(0f, angle, 0f) );
				i++;
			}
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
