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
            SceneNode cameraNode = new SceneNode();
            cameraNode.Name = "Camera";
            var camera = (Camera)cameraNode.AddComponent<Camera>();
            cameraNode.AddComponent<DynamicFormScript>();
            cameraNode.GetTransform.Position = new Vector3(0, 2f, 0);
            camera.Background = new BackgroundSkybox();
            scene.AddNode2Root(cameraNode);

            CoreEngine.Shared.ScriptHolder.AddComponent<FrameTimeScript>();
            
            SceneNode navmeshNode = new SceneNode();
            navmeshNode.Name = "NavMesh";
            var test = (TestScript)navmeshNode.AddComponent<TestScript>();
            scene.AddNode2Root(navmeshNode);
            test.camera = camera;

            //Remote Image Streaming
            //var script = (DynamicFormStream)cameraNode.AddComponent<DynamicFormStream>();
            //var ISS = (ImageStreamerScript)cameraNode.AddComponent<ImageStreamerScript>();
            //ISS.SetDSS(script);

            if (args.Length != 0)
            {
                SceneNode modelNode = ResourcesManager.LoadAsset<SceneNode>(args[0]);
                modelNode.Name = "model";
                scene.AddNode2Root(modelNode);
                test.cc = (CharControll)modelNode.AddComponent<CharControll>();
                cameraNode.AddComponent<CameraControllOrbitScript>();
            }
            else
            {
                var testScene = (TestSceneLoader)CoreEngine.Shared.ScriptHolder.AddComponent<TestSceneLoader>();
                testScene.Scene = scene;
                testScene.Button = test;
                cameraNode.AddComponent<CharacterControllPlayer>();
                cameraNode.AddComponent<CameraPOVScript>();
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