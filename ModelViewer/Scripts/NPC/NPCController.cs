using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toys;

namespace ModelViewer
{
    class NPCController : ScriptingComponent
    {
        public NpcAI AI { get; private set; }
        public NPCNavigationController navigationController { get; private set; }
        PhysicsManager pManager;
        MeshDrawer meshDrawer;
        public NPCSheduler Sheduler { get; private set; }
        NPCPerseptionState contact;
        NPCAICharacter chara;
        //NPCReactionState reaction;
        public Zone CurrentZone { get; internal set; }
        public NPCClothingController ClothingController { get; private set; }
        void Awake()
        {
            AI = new NpcAI(Node);
            navigationController = new NPCNavigationController(Node);
            pManager = Node.GetComponent<PhysicsManager>();
            Sheduler = new NPCSheduler(this);
            contact = new NPCPerseptionState(Node);
            chara = new NPCAICharacter();
            //reaction = new NPCReactionState(Node);
            meshDrawer = Node.GetComponent<MeshDrawerRigged>();
            ClothingController = new NPCClothingController(meshDrawer);
        }

        void Update()
        {
            AI.Update();
            Sheduler.Update();
            navigationController.Update();
            contact.Update();
            chara.Update();
           // reaction.Update();
        }

        protected override void Destroy()
        {
            AI.Destroy();
        }

        public void Teleport(OpenTK.Mathematics.Vector3 location)
        {
            Node.GetTransform.Position = location;
            pManager.ReinstalizeBodys();
        }
    }
}
