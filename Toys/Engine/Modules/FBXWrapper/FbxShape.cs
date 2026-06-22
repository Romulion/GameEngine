using FbxTools.FbxInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL;

namespace FbxTools
{
    class FbxShape
    {
        [DllImport(FbxDll.DllName)]
        [return: MarshalAs(UnmanagedType.BStr)]
        private static extern string FbxSDKShapeGetName(IntPtr pShape);

        [DllImport(FbxDll.DllName)]
        private static extern int FbxSDKShapeGetControllPointsCount(IntPtr pShape);

        [DllImport(FbxDll.DllName)]
        private static extern bool FbxSDKShapeGetControllPoint(IntPtr pShape, [Out] Vector4[] pBuffer);

        [DllImport(FbxDll.DllName)]
        private static extern int FbxSDKShapeGetControllPointIndicesCount(IntPtr pShape);

        [DllImport(FbxDll.DllName)]
        private static extern bool FbxSDKShapeGetControllPointIndices(IntPtr pShape, [Out] int[] pBuffer);

        IntPtr pShape;
        public FbxShape(IntPtr poiner)
        {
            pShape = poiner;
        }

        public string GetName()
        {
            return FbxSDKShapeGetName(pShape);
        }


        public Tuple<int[], Vector4[]> GetVertices()
        {
            var count = FbxSDKShapeGetControllPointsCount(pShape);
            var vertices = new Vector4[count];
            FbxSDKShapeGetControllPoint(pShape, vertices);
            count = FbxSDKShapeGetControllPointIndicesCount(pShape);
            var indeces = new int[count];
            FbxSDKShapeGetControllPointIndices(pShape, indeces);
            return new Tuple<int[], Vector4[]>(indeces, vertices);
        }

    }
}
