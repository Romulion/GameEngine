using System;
using OpenTK;
using System.Threading.Tasks;
using Gtk;

namespace Toys
{
	public class EmptyClass
	{

		public static void Main(params string[] args)
		{

			if (args.Length == 0)
				return;
			//Console.WriteLine(args[0]);
            CoreEngine core = new CoreEngine();
			var scene = SceneManager.GetInstance;
			//string str = "";
			PmxReader pmx = new PmxReader(args[0]);
			Model model = pmx.GetRiggedModel;
			IMaterial[] mats = model.GetMaterials;
			model.WorldSpace = Matrix4.CreateTranslation(0.0f, 0.0f, 0.0f);

			scene.AddModel(model);

			var task = new Task(() =>
				{
				Application.Init();
				Window wndw = new Window(mats);
				Application.Run(); 
			});
			task.Start();

			core.Run(60);
		}

   }

}
