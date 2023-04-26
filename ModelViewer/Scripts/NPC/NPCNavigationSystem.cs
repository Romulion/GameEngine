using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;
using Toys;

namespace ModelViewer
{
    class NPCNavigationSystem : ScriptingComponent
    {
        List<LocationState> locations = new List<LocationState>();
        public LocationState Current;

        void Awake()
        {
            InitializeLocationsList();
        }

        void InitializeLocationsList()
        {
            AddLocation("Sofa2", GetNodeByName("Furniture.Sofa2"), new POIData[] { new POIData("Seat1", Vector3.UnitZ, new Vector3(-0.375f, 0, 0.4f)) });
            AddLocation("Toilet", GetNodeByName("Furniture.Toilet"), new POIData[] { new POIData("Seat1", Vector3.UnitZ, new Vector3(0f, 0, 0.35f)) });
            AddLocation("ChairLiving", GetNodeByName("Furniture.ChairLiving"), new POIData[] { new POIData("Seat1", Vector3.UnitZ, new Vector3(0, 0, 0.4f)) });
            AddLocation("Bench", GetNodeByName("Furniture.Bench"), new POIData[] { new POIData("Seat1", Vector3.UnitZ, new Vector3(-0.375f, 0, 0.4f)) });
        }


        public void AddLocation(string name, SceneNode node, POIData[] pois)
        {
            if (node != null)
            {
                locations.Add(new ChairSeatState(name, node, pois));
            }
        }

        public LocationState GetLocationByName(string name)
        {
            return locations.Find((e) => e.Name == name);
        }

        public LocationState GetClosest(SceneNode npcNode, LocationState.StateType type)
        {
            LocationState loc = null;
            float lastDistance = 0;            
            foreach (var location in locations.FindAll((m) => (m.Type & type) > 0))
            {
                float distance = (npcNode.GetTransform.Position - location.SceneObject.GetTransform.Position).LengthSquared;
                if (loc == null || distance < lastDistance)
                {
                    loc = location;
                    lastDistance = distance;
                }
            }
            
            return loc;
        }

        SceneNode GetNodeByName(string name)
        {
            var nodes = CoreEngine.MainScene.FindByName(name);
            if (nodes.Length < 1)
                return null;
            return nodes[0];
        }

    }
}
