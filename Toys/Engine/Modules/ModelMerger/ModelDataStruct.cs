using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toys.Mod3DMigotoReconstructor
{
    public class ModelDataStruct
    {
        public int TexCordStride = 0;
        public string TexcoordPath = "";
        public string BlendPath = "";
        public string PositionPath = "";
        public List<MaterialData> materials = new List<MaterialData>();
    }
}
