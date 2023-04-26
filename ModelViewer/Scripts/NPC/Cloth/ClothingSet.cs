using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace ModelViewer
{
    enum ClothingType
    {
        Top,
        Bottom,
        Bra,
        Pantie,
        Legs,
        Extra,
    }
    [Serializable]
    class ClothingSet
    {
        const int clothParts = 6;
        public string Name;
        public bool BottomOpen;
        public bool TopBottom;
        public ClothingPiece[] cloth;

        public ClothingSet()
        {
            cloth = new ClothingPiece[clothParts];
        }

        public void AddClothPiece(ClothingPiece piece)
        {
            cloth[(int)piece.Slot] = piece;
            if (piece.Slot == ClothingType.Top  && TopBottom)
                cloth[(int)ClothingType.Bottom] = piece;
        }

        
        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            var unsorted = cloth;
            cloth = new ClothingPiece[clothParts];
            foreach (var piece in unsorted)
                AddClothPiece(piece);
        }
    }
}
