using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;

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
			else if (tp == typeof(Texture2D))
			{
				asset = new Texture2D(path);
			}
			else if (tp == typeof(SceneNode))
			{
				IModelLoader model = ModelLoader.Load(path);
				asset = model?.GetRiggedModel;
			}
            else if (tp == typeof(AudioSource))
            {
                var audio = new NAudio.Wave.AudioFileReader(path);
                asset = new AudioSource(audio);
            }
			if (asset)
			{
				asset.Id = path;
				asset.Type = typeof(T);
                resources.Add(path, asset);
            }
            return asset as T;
		}

        internal static void AddAsset(Resource asset, string name)
        {
            resources.Add(name, asset);
        }

        public static T[] GetResourses<T>() where T : Resource
		{
			List<T> result = new List<T>();
			foreach (var val in resources.Values)
				if (val is T)
					result.Add((T)val);

			return result.ToArray();
		}

        public static T GetResourse<T>(string name) where T : Resource
        {
            if (resources.ContainsKey(name))
                return resources[name] as T;
            return null;
        }

        public static T[] GetComponents<T>() where T : Component
		{
			List<T> result = new List<T>();
			foreach (var val in resources.Values)
			{
				if (val is T && ((Component)val).Node.Active)
					result.Add((T)val);
			}

			return result.ToArray();
		}

        public static string ReadFromInternalResource(string path)
        {
            var stream = ReadFromInternalResourceStream(path);

            string str = "";
            using (var reader = new StreamReader(stream))
            {
                str = reader.ReadToEnd();
            }
            return str;
        }

        public static Stream ReadFromInternalResourceStream(string path)
        {
            string defPath = "Toys.Resourses.";
            var assembly = IntrospectionExtensions.GetTypeInfo(typeof(ShaderManager)).Assembly;
            var stream = assembly.GetManifestResourceStream(defPath + path);
            return stream;
        }

        public static bool DeleteResource(Resource resource)
        {
            string name = resources.FirstOrDefault(x => x.Value == resource).Key;
            if (name != default(string))
                return DeleteResource(name);
            else
            {
                CoreEngine.ActiveCore.AddTask = () => resource.Unload();
                return true;
            }
        }

        public static bool DeleteResource(string res)
        {
            Resource resource;
            if (resources.TryGetValue(res, out resource))
            {
                CoreEngine.ActiveCore.AddTask = () => resource.Unload();
                resources.Remove(res);
                return true;
            }

            return false;
        }
    }
}
