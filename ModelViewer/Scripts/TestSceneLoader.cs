using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toys;
using OpenTK.Mathematics;


namespace ModelViewer
{
    class TestSceneLoader : ScriptingComponent
    {
        public TestScript Button;
        LoadUIScript uiScript;
        CharControll cc;
        Material[] Materials;
        BoneController bc;
        private void Start()
        {
            LoadModels();
            LoadDefault();
            
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
            cameraNode.AddComponent<CharacterControllPlayer>();
            cameraNode.AddComponent<CameraPOVScript>();

            var audioListener = AudioListener.GetListener();
            cameraNode.AddComponent(audioListener);

            //load ui
            uiScript = (LoadUIScript) Node.AddComponent<LoadUIScript>();
            uiScript.cc = cc;
        }

        void LoadModels()
        {

            MeshData[] meshData;
            var build = ResourcesManager.LoadAsset<SceneNode>(@"..\Assets\Models\School1F\School.pmx");
            if (build)
            {
                build.Name = "School1F";
                CoreEngine.MainScene.AddNode2Root(build);
                var serviceData = new ReaderDAE(@"..\Assets\Models\School1F\School.dae");
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
                Vector3[] verts = new Vector3[meshData[1].vertices.Length];
                for (int i = 0; i < verts.Length; i++)
                {
                    verts[i] = meshData[1].vertices[i].Position;
                }
                CoreEngine.pEngine.SetScene(verts, meshData[1].indeces, build.GetTransform.GlobalTransform);
                
            }
            
            var model1 = ResourcesManager.LoadAsset<SceneNode>(@"..\Assets\Models\Michelle\Seifuku.pmx");
            if (model1)
            {
                model1.Name = "Michelle.Seifuku";
                model1.GetTransform.Position = Vector3.UnitZ * 2;
                model1.GetTransform.RotationQuaternion = new Quaternion(0,MathF.PI,0);
                CoreEngine.MainScene.AddNode2Root(model1);
                //var manager = model1.GetComponent<PhysicsManager>();
                model1.AddComponent<NpcAI>();
                cc = (CharControll)model1.AddComponent<CharControll>();
                cc.Materials = Materials;

                var anim = model1.GetComponent<Animator>() as Animator;
                bc = anim.BoneController;

                //Console.WriteLine(bc.GetBone("頭").World2BoneInitial);
                //Button.image1.OnClick += () => manager?.ReinstalizeBodys();

                //var src = ResourcesManager.LoadAsset<AudioSource>(@"Assets\Sound\mumi.mp3");
                //model1.AddComponent(src);
                //src.Play();
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
        }

        /*
        void Update()
        {
            bc.GetBone("頭").SetTransform(new Vector3(0, 0, 0));
        }
        */

        AnimationController CreateAnimationController()
        {
            var controller = new AnimationController();

            var idle = new AnimationNode();
            idle.MainAnimation = ResourcesManager.LoadAsset<Animation>("");
            var walk = new AnimationNode();
            walk.MainAnimation = ResourcesManager.LoadAsset<Animation>("");

            var idleWalkTransit = new AnimationTransition((anim) => anim.GetFloat("speed") > 0, walk);
            var walkIdleTransit = new AnimationTransition((anim) => anim.GetFloat("speed") == 0, idle);

            idle.Transitions.Add(idleWalkTransit);
            walk.Transitions.Add(walkIdleTransit);

            return controller;
        }

        void LoadEnviropment()
        {

        }

        void LoadNavMap()
        {

        }
    }
}
