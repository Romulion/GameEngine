using System;
using System.Collections.Generic;
using System.Text;
using Toys;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ModelViewer
{
    [Serializable]
    class ClothingPiece
    {
        public Dictionary<string, bool> mateials = new Dictionary<string, bool>();
        public string Morph;
        [JsonConverter(typeof(StringEnumConverter))]
        public ClothingType Slot;
    }
}
