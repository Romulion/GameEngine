using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;
using System.Text.Json.Serialization;
using Toys;

namespace ModelViewer
{
    public class Save
    {
        [JsonConverter(typeof(Vector3JSONConverter))]
        public Vector3 playerPos { get; set; }
        [JsonConverter(typeof(QuaternionJSONConverter))]
        public Quaternion playerRot { get; set; }
        [JsonConverter(typeof(Vector3JSONConverter))]
        public Vector3 charPos { get; set; }
        [JsonConverter(typeof(QuaternionJSONConverter))]
        public Quaternion charRot { get; set; }

        public Save()
        {
            playerPos = Vector3.Zero;
            playerRot = Quaternion.Identity;
            charPos = Vector3.Zero;
            charRot = Quaternion.Identity;
        }
    }
}
