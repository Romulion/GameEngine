using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FbxWrapper
{
    class FbxContainerBase
    {
        protected IntPtr pNative;

        internal FbxContainerBase(IntPtr poiner)
        {
            pNative = poiner;
        }
    }
}
