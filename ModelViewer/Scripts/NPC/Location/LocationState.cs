using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;
using Toys;


namespace ModelViewer
{
    abstract class LocationState
    {
        [Flags]
        public enum StateType {
            Seat = 1,
            Lay = 2,
            Crouch = 4,
            Stand = 8,
        }
        POIData[] slots;
        protected NPCController[] ocupants;
        public SceneNode SceneObject { get; private set; }
        public string Name;
        public StateType Type { get; private protected set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">Location Name</param>
        /// <param name="obj">Scnene object to attach</param>
        /// <param name="locations">Relative to object location</param>
        public LocationState(string name, SceneNode obj, POIData[] locations)
        {
            Name = name;
            SceneObject = obj;
            slots = new POIData[locations.Length];
            ocupants = new NPCController[locations.Length];
            for (int i = 0; i < locations.Length; i++)
                slots[i] = GetRelativePosRot(locations[i]);
        }

        public NPCController GetOcupant(int slotID)
        {
            return null;
        }

        public bool IsOccupied(int slotID)
        {
            return ocupants[slotID] != null;
        }

        public int GetSlotCount()
        {
            return slots.Length;
        }

        protected POIData GetRelativePosRot(POIData location)
        {
            var loc = SceneObject.GetTransform.GlobalTransform;
            var pos = (new Vector4(location.Position, 1) * loc).Xyz;
            loc = loc.ClearTranslation();
            var dir = (new Vector4(location.Direction) * loc).Xyz;

            return new POIData(location.Name, dir, pos);
        }

        public POIData GetSlotPOI(int slotID)
        {
            return slots[slotID];
        }

        public int GetFreeSlotID()
        {
            for(int i = 0; i < slots.Length; i++)
            {
                if (!IsOccupied(i))
                    return i;
            }
            return -1;
        }

        public bool Ready(NPCController controller, int slotID)
        {
            float bias = 0.01f;
            var slot = slots[slotID];
            return (controller.Node.GetTransform.Position.Xz - slot.Position.Xz).LengthFast <= bias;
        }
        public bool Ready(NPCController controller)
        {
            float bias = 0.01f;
            for (int slotID = 0; slotID < slots.Length; slotID++)
            {
                var slot = slots[slotID];
                if ((controller.Node.GetTransform.Position.Xz - slot.Position.Xz).LengthFast <= bias)
                    return true;
            }
            return false;
        }
        public abstract void SetOccupant(NPCController controller, int slotID);
        public abstract void FreeOccupant(int slotID);
        public abstract void FreeOccupant(NPCController controller);
    }
}
