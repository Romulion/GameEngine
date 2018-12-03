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
			var scene = SceneManager.GetInstance;
			//string str = "";
			IModelLoader pmx = ModelLoader.Load(args[0]);
			//Console.ReadLine();
			//return;
			Model model = pmx.GetRiggedModel;

            if (model.morph != null)
                model.morph[0].morphDegree = 1f;

            //morph.Morph((MorphVertex)morphdata[146], 1f);
            // System.Windows.Forms.MessageBox.Show(morphdata[146].Name);


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
