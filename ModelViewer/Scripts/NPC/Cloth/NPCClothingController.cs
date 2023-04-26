using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Toys;
using Newtonsoft.Json;
using System.IO;

namespace ModelViewer
{
    class NPCClothingController
    {
        ClothingPiece[] clothing = new ClothingPiece[6];

        ClothingSet set;
        //on, morph
        ClothPieceWrapper[] wrappers = new ClothPieceWrapper[6];
        MeshDrawer mesh;
        public NPCClothingController(MeshDrawer drawer)
        {
            wrappers = new ClothPieceWrapper[6];
            mesh = drawer;
        }

        public void LoadFromFile(string path)
        {
            var set = JsonConvert.DeserializeObject<ClothingSet>(File.ReadAllText(path));
            SetCloth(set);
        }

        public void SetCloth(ClothingSet cloth)
        {
            set = cloth;
            for (int i = 0; i < 6; i++)
            {
                if (set.cloth[i] != null)
                {
                    clothing[i] = set.cloth[i];
                    wrappers[i] = new ClothPieceWrapper(mesh, clothing[i]);
                }
            }
        }

        public bool GetBottomOpen()
        {
            return !IsOn(ClothingType.Bottom) || !IsOn(ClothingType.Pantie);
        }

        public bool GetBottomSafe()
        {
            return IsOn(ClothingType.Bottom) || IsOn(ClothingType.Pantie);
        }

        public bool IsSkirt
        {
            get
            {
                if (set != null)
                    return set.BottomOpen;
                else return true;
            }
        }

        public bool IsOn(ClothingType type)
        {
            int c = (int)type;
            return wrappers[c] != null && wrappers[c].IsOn && !wrappers[c].IsShifted;
        }

        public void ShiftCloth(ClothingType type)
        {
            int t = (int)type;
            if (wrappers[t] != null && wrappers[t].HasMorph)
            {
                wrappers[t].Shift();
            }
        }

        public void ShiftBackCloth(ClothingType type)
        {
            int t = (int)type;
            if (wrappers[t] != null && wrappers[t].HasMorph)
            {
                wrappers[t].UnShift();
            }
        }

        public void RemoveCloth(ClothingType type)
        {
            int t = (int)type;
            if (wrappers[t] != null)
            {
                wrappers[t].Remove();
            }
        }

        public void PutOn(ClothingType type)
        {
            int t = (int)type;
            if (wrappers[t] != null)
            {
                wrappers[t].PutOn();
            }
        }
    }
}
