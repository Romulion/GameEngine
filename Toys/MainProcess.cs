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

           // string path = "D:/3d/3d/Models/Animu/BGHS onigiridojo/JC2_綿木ミシェルver3.2x/ミシェル制服ver3.pmx";
            CoreEngine core = new CoreEngine();

            var scene = core.mainScene;

            //string str = "";
			SceneNode node = ResourcesManager.LoadAsset<SceneNode>(args[0]);
			node.Name = "Model1";
			MeshDrawer md = (MeshDrawer)node.GetComponent(typeof(MeshDrawer));
            //node.GetTransform.Position = new Vector3(1.0f, 0.0f, 0.0f);
            //node.phys.ReinstalizeBodys();

            //need sync
            /*
            var tb = new TextBox();
            node.AddComponent(tb);
            tb.SetText("牡丹制服高校(アニメ版)ver3");
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
