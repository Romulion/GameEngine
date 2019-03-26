using System;
using OpenTK;
using System.Threading.Tasks;
using Gtk;

namespace Toys
{
	public class MainProcess
	{

		public static void Main(params string[] args)
		{

			if (args.Length == 0)
				return;
			//Console.WriteLine(args[0]);
            CoreEngine core = new CoreEngine();

            var scene = core.mainScene;

			//string str = "";

			SceneNode node = ResourcesManager.LoadAsset<SceneNode>(args[0]);
			node.Name = "Model1";
			MeshDrawer md = (MeshDrawer)node.GetComponent(typeof(MeshDrawer));
			IMaterial[] mats = md.mats;
			//node.GetTransform.Position = new Vector3(1.0f, 0.0f, 0.0f);
			//node.phys.ReinstalizeBodys();

			scene.AddObject(node);

			var task = new Task(() =>
				{
				Application.Init();
				Window wndw = new Window(mats,md.morph,core);
				Application.Run(); 
			});
			task.Start();

			core.Run(60);

		}

   }

}
