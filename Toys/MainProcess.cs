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
            //node.Name = "Model1";
            //var loader = new ReaderLMD(args[0]);
            //SceneNode node = loader.GetModel;
            //node.GetTransform.Rotation = new Vector3(0,(float)Math.PI / 2,0);
            //MeshDrawer md = (MeshDrawer)node.GetComponent(typeof(MeshDrawer));
            //node.GetTransform.Position = new Vector3(0f, 1.0f, 0.0f);
            //node.phys.ReinstalizeBodys();

            //need sync
            /*
            var tb = new TextBox();
            node.AddComponent(tb);
            tb.SetText("牡丹制服高校(アニメ版)ver3");
            */
            scene.AddObject(node);
            TestScript ts = (TestScript)node.AddComponent<TestScript>();
            
            
            TextBox text = null;
            long frames = 1;
            long framesMax = 60;
            double update = 0, render = 0;
            core.Load += (s, e) => { text = new TextBox(); node.AddComponent(text);};
            core.UpdateFrame += (s, e) => {

                if (frames >= framesMax)
                {
                    text.SetText((update / frames).ToString("C2") + "  " + (render / frames).ToString("C2"));
                    frames = 0;
                    update = 0;
                    render = 0;
                }
                update += CoreEngine.time.updagteTime;
                render += CoreEngine.time.renderTime;
                frames++;
            };
            
            //
            //
            
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
