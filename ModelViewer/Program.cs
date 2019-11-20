using System;
using Toys;
using OpenTK;
using System.Threading.Tasks;
using Gtk;

namespace ModelViewer
{
    class Program
    {
        static void Main(params string[] args)
        {
            if (args.Length == 0)
                return;
            CoreEngine core = new CoreEngine();
            
            var scene = core.mainScene;

            //string str = "";
            SceneNode node = ResourcesManager.LoadAsset<SceneNode>(args[0]);
            
            var camera = new Camera();
            SceneNode sceneNode = new SceneNode();
            sceneNode.AddComponent(camera);
            sceneNode.AddComponent<CameraControllScript>();
            sceneNode.GetTransform.Position = new Vector3(0, 1, 3);
            sceneNode.AddComponent<DynamicFormScript>();
            scene.AddObject(sceneNode);
            camera.Background = new BackgroundSkybox();

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
            FrameTimeScript ft = (FrameTimeScript)node.AddComponent<FrameTimeScript>();

            var task = new Task(() =>
            {
                Application.Init();
                Window wndw = new Window(node, core);
                Application.Run();
            });
            task.Start();

            core.Run(60);
        }
    }
}