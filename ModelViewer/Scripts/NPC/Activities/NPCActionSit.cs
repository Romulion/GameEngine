using System;
using System.Collections.Generic;
using System.Text;
using Toys;

namespace ModelViewer
{
    internal class NPCActionSit : NPCBasicAction
    {
        NPCNavigationSystem navigationSystem;
        LocationState location;
        public NPCActionSit(string name, string place) : base(name)
        {
            navigationSystem = CoreEngine.Shared.ScriptHolder.GetComponent<NPCNavigationSystem>();
            location = navigationSystem.GetLocationByName(place);
        }

        public override bool IsCompleted(NPCController controller)
        {
            return controller.navigationController.CurrentLocation == location;
        }

        public override void Start(NPCController controller)
        {          
            controller.Sheduler.OcupyLocation(location);
        }

        public override void Stop(NPCController controller)
        {
            if (controller.navigationController.CurrentLocation != null)
                controller.navigationController.CurrentLocation.FreeOccupant(controller);
            else if (!controller.navigationController.IsTaskCompleted)
                controller.navigationController.Stop();
        }
    }
}
