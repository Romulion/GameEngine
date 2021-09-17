using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toys;
using OpenTK.Mathematics;
using System.Xml;


namespace ModelViewer
{
    class TestSceneLoader : ScriptingComponent
    {
        public TestScript Button;
        LoadUIScript uiScript;
        NPCNavigationController cc;
        Material[] Materials;
        BoneController bc;
        SceneNode canvas;
        private void Awake()
        {
            LoadModels();
            LoadDefault();

            CoreEngine.GetCamera.Node.AddComponent<Toys.VR.VRControllScript>();
        }

        void LoadDefault()
        {

            //SceneNode navmeshNode = new SceneNode();
            //navmeshNode.Name = "NavMesh";
            //var test = (TestScript)navmeshNode.AddComponent<TestScript>();
            //Scene.AddNode2Root(navmeshNode);
            //test.camera = camera;
            //Button = test;
            var cameraNode = CoreEngine.GetCamera.Node;
            var playerNode = new SceneNode();
            playerNode.Name = "Player";
            playerNode.GetTransform.Position = new Vector3(0, 0.5f, 0);
            cameraNode.GetTransform.Position = new Vector3(0, 2.14f, 0);
            CoreEngine.MainScene.AddNode2Root(playerNode);
            playerNode.AddComponent<CharacterControllPlayer>();

            cameraNode.SetParent(playerNode);

            cameraNode.AddComponent<CameraPOVScript>();

            var audioListener = AudioListener.GetListener();
            cameraNode.AddComponent(audioListener);
            //load ui
            uiScript = Node.AddComponent<LoadUIScript>();
            uiScript.cc = cc;


            /*
            var rhand = new SceneNode();
            rhand.Name = "Lhand";
            var size = new Vector3(0.23f, 0.23f, 0.25f);
            rhand.AddComponent(Toys.Debug.TestBox.CreateBox(size));
            rhand.GetTransform.Position = new Vector3(0, 2, -1f);
            var physBox = rhand.AddComponent<Toys.Physics.RigitBodyBox>();
            physBox.Size = size;
            //physBox.Mass = 1;
            physBox.IsKinematic = true;
            physBox.SetFlags(-1, -1);
            //CoreEngine.MainScene.AddNode2Root(rhand);
            rhand.SetParent(CoreEngine.GetCamera.Node.Parent);
            */
        }

        void LoadModels()
        {


            /*
            var build = ResourcesManager.LoadAsset<SceneNode>(@"Assets\Models\School1F\School.pmx");
            if (build)
            {
                build.Name = "School1F";
                CoreEngine.MainScene.AddNode2Root(build);
                var serviceData = new ReaderDAE(@"Assets\Models\School1F\School.dae");
                meshData = serviceData.GetMeshes();
                NavigationMesh.Instalize(meshData[0].vertices, meshData[0].indeces);

                /*
                //test NavMap draw
                Mesh mesh = new Mesh(meshData[0].vertices, meshData[0].indeces);
                Materials = new Material[meshData[0].indeces.Length / 3];
                var shdrst = new ShaderSettings();
                shdrst.Ambient = true;
                for (int i = 0; i < Materials.Length; i++)
                {
                    Materials[i] = new MaterialPMX(shdrst, new RenderDirectives());
                    Materials[i].Offset = i * 3;
                    Materials[i].Count = 3;
                }
                MeshDrawer md = new MeshDrawer(mesh, Materials);
                Node.AddComponent(md);
                */
            //Load Collision Mesh
            /*
            Vector3[] verts = new Vector3[meshData[1].vertices.Length];
            for (int i = 0; i < verts.Length; i++)
            {
                verts[i] = meshData[1].vertices[i].Position;
            }
            CoreEngine.pEngine.SetScene(verts, meshData[1].indeces, build.GetTransform.GlobalTransform);

        }
        */

            //var build = ResourcesManager.LoadAsset<SceneNode>(@"Assets\Models\Home\house.dae");
            //if (build)
            //{
            //  build.Name = "Home";
            // CoreEngine.MainScene.AddNode2Root(build);
            //var serviceData = new ReaderDAE(@"Assets\Models\School1F\School.dae");
            //meshData = serviceData.GetMeshes();
            //NavigationMesh.Instalize(meshData[0].vertices, meshData[0].indeces);

            /*
            //test NavMap draw
            Mesh mesh = new Mesh(meshData[0].vertices, meshData[0].indeces);
            Materials = new Material[meshData[0].indeces.Length / 3];
            var shdrst = new ShaderSettings();
            shdrst.Ambient = true;
            for (int i = 0; i < Materials.Length; i++)
            {
                Materials[i] = new MaterialPMX(shdrst, new RenderDirectives());
                Materials[i].Offset = i * 3;
                Materials[i].Count = 3;
            }
            MeshDrawer md = new MeshDrawer(mesh, Materials);
            Node.AddComponent(md);
            */
            //Load Collision Mesh
            /*
            Vector3[] verts = new Vector3[meshData[1].vertices.Length];
            for (int i = 0; i < verts.Length; i++)
            {
                verts[i] = meshData[1].vertices[i].Position;
            }
            CoreEngine.pEngine.SetScene(verts, meshData[1].indeces, build.GetTransform.GlobalTransform);
            */
            //}

            var model1 = ResourcesManager.LoadAsset<ModelPrefab>(@"Assets\Models\Michelle\Seifuku.pmx").CreateNode();
            if (model1)
            {
                model1.Name = "Michelle.Seifuku";
                model1.GetTransform.Position = Vector3.UnitZ * 0.5f;
                model1.GetTransform.RotationQuaternion = new Quaternion(0, MathF.PI,0);
                CoreEngine.MainScene.AddNode2Root(model1);
                //var manager = model1.GetComponent<PhysicsManager>();
                model1.AddComponent<NpcAI>();
                cc = model1.AddComponent<NPCNavigationController>();
                //cc.Materials = Materials;

                var anim = model1.GetComponent<Animator>();
                bc = anim.BoneController;
                
                /*
                root.UpdateTransform();
                var scale = Matrix4.CreateOrthographic(800,600, -1,1);
                var model = rect.GlobalTransform;// * Matrix4.CreateScale(1 / (float)CoreEngine.GetCamera.Width, 1 / (float)CoreEngine.GetCamera.Height, 1);

                
                Vector4 pos = model * new Vector4(1, 1, 0, 1);
                Console.WriteLine(model);
                //pos.X /= 800;
                //pos.Y /= 600;
                Console.WriteLine(scale * pos);
                
                Console.WriteLine(new Vector4(800, 300, 0, 1)* Matrix4.CreateTranslation(-400, -300, 0));
                */
            }

            var build = ResourcesManager.LoadAsset<ModelPrefab>(@"Assets\Models\Home\house.dae").CreateNode();
            if (build)
            {
                build.Name = "Home";
                CoreEngine.MainScene.AddNode2Root(build);

                var serviceData = new ReaderDAE(@"Assets\Models\Home\house_add.dae");
                var meshData = serviceData.GetMeshes();
                if (meshData.ContainsKey("NavMap"))
                    NavigationMesh.Instalize(meshData["NavMap"].vertices, meshData["NavMap"].indeces);

                if (meshData.ContainsKey("Phys"))
                {
                    Vector3[] verts = new Vector3[meshData["Phys"].vertices.Length];
                    for (int i = 0; i < verts.Length; i++)
                    {
                        verts[i] = meshData["Phys"].vertices[i].Position;
                    }
                    CoreEngine.pEngine.SetScene(verts, meshData["Phys"].indeces, build.GetTransform.GlobalTransform);
                }
                /*
                Mesh mesh = new Mesh(meshData[0].vertices, meshData[0].indeces);
                Materials = new Material[meshData[0].indeces.Length / 3];
                var shdrst = new ShaderSettings();
                shdrst.Ambient = true;
                for (int i = 0; i < Materials.Length; i++)
                {
                    Materials[i] = new MaterialPMX(shdrst, new RenderDirectives());
                    Materials[i].Offset = i * 3;
                    Materials[i].Count = 3;
                }
                MeshDrawer md = new MeshDrawer(mesh, Materials);
                Node.AddComponent(md);
                */

            }
            /*
            var model2 = ResourcesManager.LoadAsset<SceneNode>(@"Assets\Models\Hinata\Seifuku.pmx");
            if (model2)
            {
                model2.Name = "Hinata.Seifuku";
                model2.GetTransform.Position = -OpenTK.Vector3.UnitZ * 2;
                CoreEngine.MainScene.AddNode2Root(model2);
                model2.AddComponent<NpcAI>();
                //var src = ResourcesManager.LoadAsset<AudioSource>(@"Assets\Sound\hina.mp3");
                //model2.AddComponent(src);
                //src.Play();
            }
            */
            //SceneSaveLoadSystem.Save2File("");

            LoadAssetLocations(@"Assets\Models\Home\house_assets.dae",build);
        }


        void Update()
        {
            //bc.GetBone("頭").SetTransform(new Vector3(0, 0, 0));
            // canvas.GetTransform.LookAt(CoreEngine.GetCamera.GetPos);
        }
            

        void LoadAssetLocations(string file, SceneNode parent)
        {
            var xDoc = new XmlDocument();
            xDoc.Load(file);
            var xRoot = xDoc.DocumentElement;
            var visualScenes = xRoot.FindNodes("library_visual_scenes")[0];
            var visualScene = visualScenes.ChildNodes[0];
            var nodes = visualScene.ChildNodes;
            string instanceName = "";
            foreach (XmlNode node in nodes)
            {
                var name = node.Attributes.GetNamedItem("name").Value;
                if (name == null)
                    continue;
                //Console.WriteLine(name);
                var comps = name.Split(':');
                var objType = comps[0];
                SceneNode assetNode = null;
                if (objType == "Asset")
                {
                    var path = comps[1];
                    //remove index
                    var indx = path.IndexOf('#');
                    var suffix = "";
                    if (indx > 0)
                    {
                        suffix = path.Substring(indx);
                        path = path.Remove(indx);
                    }
                    instanceName = path;
                    path = @"Assets\Models\" + path.Replace('.', '\\') + ".dae";
                    assetNode = ResourcesManager.LoadAsset<ModelPrefab>(path).CreateNode();
                    assetNode.Name += suffix;
                }

                
                if (assetNode)
                {
                    assetNode.Name = instanceName;
                    assetNode.SetParent(parent);
                    //Location
                    var floatArray = StringParser.readFloatArray(node.ChildNodes[0].InnerText);
                    Matrix4 transform = new Matrix4(floatArray[0], floatArray[1], floatArray[2], floatArray[3],
                        floatArray[4], floatArray[5], floatArray[6], floatArray[7],
                        floatArray[8], floatArray[9], floatArray[10], floatArray[11],
                        floatArray[12], floatArray[13], floatArray[14], floatArray[15]);
                    transform.Transpose();
                    assetNode.GetTransform.Position = transform.ExtractTranslation();
                    assetNode.GetTransform.RotationQuaternion = transform.ExtractRotation();
                    assetNode.GetTransform.Scale = transform.ExtractScale();
                }
            }
        }

        void LoadNavMap()
        {

        }
    }
}
