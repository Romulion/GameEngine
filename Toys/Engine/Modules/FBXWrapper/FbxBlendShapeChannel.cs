using FbxTools.FbxInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FbxTools
{
    class FbxBlendShapeChannel
    {
        [DllImport(FbxDll.DllName)]
        [return: MarshalAs(UnmanagedType.BStr)]
        private static extern string FbxSDKBlendShapeChannelGetName(IntPtr pBlendShapeCh);

        [DllImport(FbxDll.DllName)]
        private static extern int FbxSDKBlendShapeChannelGetShapeCount(IntPtr pBlendShapeCh);

        [DllImport(FbxDll.DllName)]
        private static extern IntPtr FbxSDKBlendShapeChannelGetShape(IntPtr pBlendShapeCh, int index);

        IntPtr pBlendShapeCh;
        public FbxBlendShapeChannel(IntPtr poiner)
        {
            pBlendShapeCh = poiner;
        }

        public string GetName()
        {
            return FbxSDKBlendShapeChannelGetName(pBlendShapeCh);
        }

        public int GetShapeCount()
        {
            return FbxSDKBlendShapeChannelGetShapeCount(pBlendShapeCh);
        }

        public FbxShape GetShapeAt(int index)
        {
            return Fbx.GetNativeWrapper<FbxShape>(FbxSDKBlendShapeChannelGetShape(pBlendShapeCh, index));
        }
    }
}
