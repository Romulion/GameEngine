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
			IModelLoader pmx = ModelLoader.Load(args[0]);
			//Console.ReadLine();
			//return;
			SceneNode node = pmx.GetRiggedModel;
			node.Name = "Model1";
			IMaterial[] mats = node.model.mats;
			node.GetTransform.Position = new Vector3(0.0f, 0.0f, 0.0f);
			scene.AddObject(node);

			var task = new Task(() =>
				{
				Application.Init();
				Window wndw = new Window(mats,node.morph,core);
				Application.Run(); 
			});
			task.Start();

			core.Run(60);
		}

   }

}
