using System;
using System.Collections.Generic;

namespace Toys
{
	public class ResourcesManager
	{
		static Dictionary<string,Resource> resources = new Dictionary<string,Resource>();

		CoreEngine ce;

		public ResourcesManager(CoreEngine core)
		{
			ce = core;
		}

		public static T LoadAsset<T>(string path) where T : Resource
		{

			if (resources.ContainsKey(path))
				return resources[path] as T;

			Type tp = typeof(T);
			Resource asset = null;
			if (tp == typeof(MeshDrawer))
			{
				//IModelLoader model = ModelLoader.Load(path);
				//asset = model.model;
				//resources.Add(path, asset);
			}
			else if (tp == typeof(Texture))
			{
				asset = new Texture(path);
				resources.Add(path, asset);
			}
			else if (tp == typeof(SceneNode))
			{
				/*
				IModelLoader model = ModelLoader.Load(path);
				asset = model.GetRiggedModel;
				resources.Add(path, asset);
				*/
				IModelLoader model = ModelLoader.Load(path);
				asset = model.GetRiggedModel;
				resources.Add(path, asset);
			}

			if (asset)
			{
				asset.Id = path;
				asset.type = typeof(T);
			}
			return asset as T;
		}

		public static T[] GetResourses<T>() where T : Resource
		{
			List<T> result = new List<T>();
			foreach (var val in resources.Values)
				if (val is T)
					result.Add((T)val);

			return result.ToArray();
		}

		public static T[] GetComponents<T>() where T : Component
		{
			List<T> result = new List<T>();
			foreach (var val in resources.Values)
			{
				if (val is T && ((Component)val).node.Active)
					result.Add((T)val);
			}

			return result.ToArray();
		}
	}
}
