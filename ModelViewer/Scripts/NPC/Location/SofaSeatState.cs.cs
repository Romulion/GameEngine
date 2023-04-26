using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;
using Toys;

namespace ModelViewer
{
    class SofaSeatState : ChairSeatState
    {
        public SofaSeatState(string name, SceneNode obj, POIData[] locations) : base(name, obj, locations)
        {
            Type = StateType.Seat | StateType.Lay;
        }

        /*
        public override void FreeOccupant(int slotID)
        {
            if (ocupants[slotID] != null)
            {
                ocupants[slotID].navigationController.AnimController.SetBool("sit", false);
                ocupants[slotID].navigationController.CurrentLocation = null;
                ocupants[slotID] = null;
            }
        }

        public override void SetOccupant(NPCController controller, int slotID)
        {
            if (ocupants[slotID] == null && Ready(controller, slotID))
            {
                ocupants[slotID] = controller;
                controller.navigationController.CurrentLocation = this;
                controller.navigationController.AnimController.SetBool("sit", true);
            }
            else
            {
                //Logger.
            }
        }
        */
    }
}
