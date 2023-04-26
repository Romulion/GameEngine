using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;
using Toys;

namespace ModelViewer
{
    class ChairSeatState : LocationState
    {
        public ChairSeatState(string name, SceneNode obj, POIData[] locations): base(name, obj, locations)
        {
            Type = StateType.Seat;
        }

        public override void FreeOccupant(int slotID)
        {
            if (ocupants[slotID] != null)
            {
                ocupants[slotID].navigationController.AnimController.SetBool("sit", false);
                ocupants[slotID].navigationController.CurrentLocation = null;
                ocupants[slotID] = null;
            }
        }

        public override void FreeOccupant(NPCController controller)
        {
            for (int i = 0; i < ocupants.Length; i++)
                if (ocupants[i] == controller)
                    FreeOccupant(i);
        }

        public override void SetOccupant(NPCController controller, int slotID)
        {
            if (ocupants[slotID] == null && Ready(controller, slotID))
            {
                ocupants[slotID] = controller;
                
                controller.navigationController.AnimController.SetBool("sit", true);
                controller.navigationController.AnimController.AnimationChangeCalback += SeatCallback;
            }
            else
            {
                //Logger.
            }

            void SeatCallback(AnimationNode anim)
            {
                if (anim.Name == "Sit")
                {
                    controller.navigationController.CurrentLocation = this;
                    controller.navigationController.AnimController.AnimationChangeCalback -= SeatCallback;
                }
            }
        }

        
    }
}
