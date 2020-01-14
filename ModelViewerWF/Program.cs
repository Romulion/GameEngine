using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Toys;
using OpenTK;

namespace ModelViewerWF
{
    static class Program
    {
        static void Main(params string[]  args)
        {

            GLWindow window = new GLWindow();
            var scene = window.Engine.mainScene;
            if (args.Length != 0)
            {
                SceneNode modelNode = ResourcesManager.LoadAsset<SceneNode>(args[0]);
                modelNode.Name = "model";
                scene.AddObject(modelNode);
            }
            var camera = new Camera();
            SceneNode cameraNode = new SceneNode();
            cameraNode.Name = "Camera";
            cameraNode.AddComponent(camera);
            cameraNode.AddComponent<CameraControllScript>();
            //cameraNode.AddComponent<FrameTimeScript>();
            cameraNode.GetTransform.Position = new Vector3(0, 1f, 3);

            scene.AddObject(cameraNode);
            cameraNode.AddComponent<DynamicFormScript>();

            var task = new Task(() =>
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                var form = new Form1();
                form.scene = scene;
                form.FormClosed += (s,e) => window.Close();
                form.cam = camera;
                Application.Run(form);
            });
            task.Start();

            window.Title = "ModelViewer";
            window.Run(60);
        }
    }
}
