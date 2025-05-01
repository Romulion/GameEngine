using FbxWrapper.FbxInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FbxWrapper
{
    class FbxSkin
    {

        [DllImport(FbxDll.DllName)]
        private static extern int FbxSDKSkinnerGetClusterCount(IntPtr pSkin);

        [DllImport(FbxDll.DllName)]
        private static extern IntPtr FbxSDKSkinnerGetClusterById(IntPtr pSkin, int index);

        IntPtr pSkin;

        public FbxSkin(IntPtr pointer)
        {
            pSkin = pointer;
        }

        public int GetClusterCount()
        {
            return FbxSDKSkinnerGetClusterCount(pSkin);
        }

        public FbxCluster GetClusterAt(int index)
        {
            if (index < FbxSDKSkinnerGetClusterCount(pSkin) && index >= 0)
            {
                return Fbx.GetNativeWrapper<FbxCluster>(FbxSDKSkinnerGetClusterById(pSkin, index));
            }
            return null;
        }
    }
}
