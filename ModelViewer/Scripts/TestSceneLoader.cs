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

        private void Start()
        {
            LoadDefault();
            LoadModels();
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
            Node.AddComponent<LoadUIScript>();
        }

        void LoadModels()
        {
            var model1 = ResourcesManager.LoadAsset<SceneNode>(@"..\Assets\Models\Michelle\Seifuku.pmx");
            if (model1)
            {
                model1.Name = "Michelle.Seifuku";
                model1.GetTransform.Position = Vector3.UnitZ * 2;
                CoreEngine.MainScene.AddNode2Root(model1);
                //var manager = model1.GetComponent<PhysicsManager>();
                model1.AddComponent<NpcAI>();
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

        }

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
