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
			//node.GetTransform.Position = new Vector3(1.0f, 0.0f, 0.0f);
			//node.phys.ReinstalizeBodys();

			var tra = Matrix4.CreateTranslation(new Vector3 (3,0,0));
			var rot = Matrix4.CreateRotationY((float)Math.PI / 2);
			var pos = new Vector4(1, 0, 0, 1);

			//var test = rot * tra;
			//Console.WriteLine(test);
			//Console.WriteLine(pos * rot * tra);

			/*
			tra.Transpose();
			rot.Transpose();
			Console.WriteLine(tra * rot * pos);
			*/
			scene.AddObject(node);

			var task = new Task(() =>
				{
				Application.Init();
				Window wndw = new Window(node,core);
				Application.Run(); 
			});
			task.Start();

			core.Run(60);

		}

   }

}
