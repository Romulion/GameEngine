using FbxTools.FbxInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FbxTools
{
    class FbxBlendShape
    {
        [DllImport(FbxDll.DllName)]
        private static extern int FbxSDKBlendShapeGetChannelCount(IntPtr pBlendShape);

        [DllImport(FbxDll.DllName)]
        private static extern IntPtr FbxSDKBlendShapeGetChannel(IntPtr pBlendShape, int index);


        IntPtr pBlendShape;
        public FbxBlendShape(IntPtr poiner)
        {
            pBlendShape = poiner;
        }

        public int GetChannelCount()
        {
            return FbxSDKBlendShapeGetChannelCount(pBlendShape);
        }

        public FbxBlendShapeChannel GetChannelAt(int index)
        {
            return Fbx.GetNativeWrapper<FbxBlendShapeChannel>(FbxSDKBlendShapeGetChannel(pBlendShape, index));
        }
    }
}
