using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;
using Toys;

namespace ModelViewer
{
    class NPCNavigationSystem
    {
        List<POIData> POIs = new List<POIData>();

        void Start()
        {
            InitializePOIList();
        }

        void InitializePOIList()
        {
            POIs.Add(new POIData("Sofa2", Vector3.UnitZ, new Vector3(-0.375f, 0, 0.4f), GetNodeByName("Furniture.Sofa2")));
            POIs.Add(new POIData("Toilet", Vector3.UnitZ, new Vector3(0f, 0, 0.35f), GetNodeByName("Furniture.Toilet")));
            POIs.Add(new POIData("ChairLiving", Vector3.UnitZ, new Vector3(0, 0, 0.4f), GetNodeByName("Furniture.ChairLiving")));
            POIs.Add(new POIData("ChairDining#1", Vector3.UnitX, new Vector3(-0.375f, 0, 0f), GetNodeByName("Furniture.ChairDining#1")));
        }


        public void AddPOI(string name, Vector3 dir, Vector3 pos, string obj)
        {
            POIs.Add(new POIData(name, dir, pos, GetNodeByName(obj)));
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
