using Toys;
using System.Threading.Tasks;
using Gtk;
using OpenTK;
using System.IO;
using System;

namespace ModelViewer
{
    class Program
    {
        static void Main(params string[] args)
        {
            if (args.Length == 0)
                return;
            GLWindow window = new GLWindow();
            
            var scene = window.Engine.mainScene;

            //string str = "";
            SceneNode node = ResourcesManager.LoadAsset<SceneNode>(args[0]);
            node.Name = "model";
            var camera = new Camera();
            SceneNode sceneNode = new SceneNode();
            sceneNode.Name = "Camera";
            sceneNode.AddComponent(camera);
            sceneNode.AddComponent<CameraControllScript>();
            sceneNode.GetTransform.Position = new Vector3(0, 1, 3);
            var script = (DynamicFormStream)sceneNode.AddComponent<DynamicFormStream>();
            //sceneNode.AddComponent<DynamicFormScript>();
            scene.AddObject(sceneNode);
            camera.Background = new BackgroundSkybox();
            //node.Name = "Model1";
            //var loader = new ReaderLMD(args[0]);
            //SceneNode node = loader.GetModel;
            //node.GetTransform.Rotation = new Vector3(0,(float)Math.PI / 2,0);
            //MeshDrawer md = (MeshDrawer)node.GetComponent(typeof(MeshDrawer));
            //node.GetTransform.Position = new Vector3(0f, 1.0f, 0.0f);
            //node.phys.ReinstalizeBodys();
            //window.Visible = false;
            //need sync
            scene.AddObject(node);
            TestScript ts = (TestScript)node.AddComponent<TestScript>();
            FrameTimeScript ft = (FrameTimeScript)node.AddComponent<FrameTimeScript>();
            Window wndw = null;
            var ISS = (ImageStreamerScript)node.AddComponent<ImageStreamerScript>();
            ISS.SetDSS(script);
            

            var task = new Task(() =>
            {
                Application.Init();
                wndw = new Window(scene, window.Engine);
                ISS.wndw = wndw;
                Application.Run();
            });
            task.Start();

            window.Title = "ModelViewer";
            window.Run(60);
        }
    }
}