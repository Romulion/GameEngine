using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toys.Mod3DMigotoReconstructor
{
    public class MaterialData
    {
        public string Name = "";
        public int Offset = 0;
        public string TextureDiffuse = "";
        public string TextureNormal = "";
        public string TextureLight = "";
        public string IndexBufferFile = "";
        public List<MeshSubPart> MeshSubParts = new List<MeshSubPart>();
    }
}
