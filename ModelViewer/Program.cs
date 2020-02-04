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
            
            GLWindow window = new GLWindow();
            
            var scene = window.Engine.mainScene;

            //string str = "";
            if (args.Length != 0)
            {
                SceneNode modelNode = ResourcesManager.LoadAsset<SceneNode>(args[0]);
                //modelNode.Name = "model";
                scene.AddObject(modelNode);
            }
            var camera = new Camera();
            SceneNode cameraNode = new SceneNode();
            cameraNode.Name = "Camera";
            cameraNode.AddComponent(camera);
            cameraNode.AddComponent<CameraControllScript>();
            cameraNode.AddComponent<FrameTimeScript>();
            cameraNode.GetTransform.Position = new Vector3(0, 1f, 3);
            cameraNode.AddComponent<TestScript>();
            scene.AddObject(cameraNode);
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
            //var script = (DynamicFormStream)cameraNode.AddComponent<DynamicFormStream>();
            cameraNode.AddComponent<DynamicFormScript>();
            //var ISS = (ImageStreamerScript)cameraNode.AddComponent<ImageStreamerScript>();
            //ISS.SetDSS(script);

            var task = new Task(() =>
            {
                Application.Init();
                Window wndw = new Window(scene, window.Engine);
                //ISS.wndw = wndw;
                Application.Run();
            });
            task.Start();

            window.Title = "ModelViewer";
            window.Run(60);
        }
    }
}