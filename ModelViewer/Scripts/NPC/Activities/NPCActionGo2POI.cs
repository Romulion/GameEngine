using System;
using System.Collections.Generic;
using System.Text;
using Toys;

namespace ModelViewer
{
    class NPCActionGo2POI: NPCBasicAction
    {
        NPCNavigationSystem navigationSystem;
        LocationState location;

        public NPCActionGo2POI(string name, string place) : base(name)
        {
            navigationSystem = CoreEngine.Shared.ScriptHolder.GetComponent<NPCNavigationSystem>();
            location = navigationSystem.GetLocationByName(place);
        }

        public override bool IsCompleted(NPCController controller)
        {
            return !controller.navigationController.IsWalking 
                && !controller.navigationController.IsRotating 
                && location.Ready(controller) ;
        }

        public override void Start(NPCController controller)
        {
            controller.Sheduler.Go2Location(location);
        }

        public override void Stop(NPCController controller)
        {
            if (!controller.navigationController.IsTaskCompleted)
                controller.navigationController.Stop();
        }
    }
}
