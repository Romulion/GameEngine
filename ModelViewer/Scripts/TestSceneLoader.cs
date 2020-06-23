using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toys;


namespace ModelViewer
{
    class TestSceneLoader : ScriptingComponent
    {
        public Scene Scene;
        public TestScript Button;
        private void Start()
        {
            LoadModels();
        }

        void LoadModels()
        {
            var model1 = ResourcesManager.LoadAsset<SceneNode>(@"Assets\Models\Michelle\Seifuku.pmx");
            if (model1)
            {
                model1.Name = "Michelle.Seifuku";
                model1.GetTransform.Position = OpenTK.Vector3.UnitZ * 2;
                Scene.AddNode2Root(model1);
                var manager = (PhysicsManager)model1.GetComponent<PhysicsManager>();
                //Button.image1.OnClick += () => manager?.ReinstalizeBodys();

            }

            var model2 = ResourcesManager.LoadAsset<SceneNode>(@"Assets\Models\Hinata\Seifuku.pmx");
            if (model2)
            {
                model2.Name = "Hinata.Seifuku";
                model2.GetTransform.Position = -OpenTK.Vector3.UnitZ * 2;
                Scene.AddNode2Root(model2);
            }
        }


        void LoadEnviropment()
        {

        }
    }
}
