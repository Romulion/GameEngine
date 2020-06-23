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
            var scene = CoreEngine.MainScene;
            //Console.WindowHeight = 700;
            //string str = "";
            var camera = new Camera();
            SceneNode cameraNode = new SceneNode();
            cameraNode.Name = "Camera";
            cameraNode.AddComponent(camera);
            cameraNode.AddComponent<CameraControllScript>();
            cameraNode.AddComponent<FrameTimeScript>();
            cameraNode.AddComponent<CharacterControllPlayer>();
            cameraNode.GetTransform.Position = new Vector3(0, 3f, 3);
            camera.Background = new BackgroundSkybox();
            scene.AddNode2Root(cameraNode);
            SceneNode navmeshNode = new SceneNode();
            navmeshNode.Name = "NavMesh";
            var test = (TestScript)navmeshNode.AddComponent<TestScript>();
            scene.AddNode2Root(navmeshNode);
            test.camera = camera;
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

            if (args.Length != 0)
            {
                SceneNode modelNode = ResourcesManager.LoadAsset<SceneNode>(args[0]);
                modelNode.Name = "model";
                scene.AddNode2Root(modelNode);
                test.cc = (CharControll)modelNode.AddComponent<CharControll>();
            }
            else
            {
                var testScene = (TestSceneLoader)CoreEngine.Shared.ScriptHolder.AddComponent<TestSceneLoader>();
                testScene.Scene = scene;
                testScene.Button = test;
            }

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