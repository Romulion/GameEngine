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

            
            SceneNode cameraNode = new SceneNode();
            cameraNode.Name = "Camera";
            var camera = (Camera)cameraNode.AddComponent<Camera>();
            
            cameraNode.GetTransform.Position = new OpenTK.Vector3(0, 2f, 0);
            camera.Background = new BackgroundSkybox();
            scene.AddNode2Root(cameraNode);

            CoreEngine.Shared.ScriptHolder.AddComponent<FrameTimeScript>();

            //character view mode
            if (args.Length != 0)
            {
                cameraNode.AddComponent<DynamicFormScript>();

                //Remote Image Streaming
                //var script = (DynamicFormStream)cameraNode.AddComponent<DynamicFormStream>();
                //var ISS = (ImageStreamerScript)cameraNode.AddComponent<ImageStreamerScript>();
                //ISS.SetDSS(script);
                
                SceneNode modelNode = ResourcesManager.LoadAsset<SceneNode>(args[0]);
                //var md =  modelNode.GetComponent<MeshDrawer>();
                //for(int i = 0; i < md.Materials.Length; i++)
                //   md.Materials[i] = 
                if (modelNode)
                {
                    modelNode.AddComponent<TestScript>();
                    modelNode.Name = "model";
                    scene.AddNode2Root(modelNode);
                    cameraNode.AddComponent<CameraControllOrbitScript>();
                }
            }
            //scene mode
            else
            {
                var testScene = (TestSceneLoader)CoreEngine.Shared.ScriptHolder.AddComponent<TestSceneLoader>();
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