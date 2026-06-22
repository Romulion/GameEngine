using FbxTools.FbxInterop;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace FbxTools
{

    class FbxCluster
    {
        [DllImport(FbxDll.DllName)]
        private static extern void FbxSDKSkinnerGetClusterMatrix(IntPtr pCluster, out Matrix4 pBuffer);

        [DllImport(FbxDll.DllName)]
        private static extern void FbxSDKSkinnerGetClusterLinkMatrix(IntPtr pCluster, out Matrix4 pBuffer);

        [DllImport(FbxDll.DllName)]
        private static extern IntPtr FbxSDKSkinnerGetClusterBone(IntPtr pCluster);

        [DllImport(FbxDll.DllName)]
        private static extern int FbxSDKSkinnerGetClusterDataLength(IntPtr pCluster);

        [DllImport(FbxDll.DllName)]
        private static extern void FbxSDKSkinnerGetClusterIndeces(IntPtr pCluster, [Out] int[] pBuffer);

        [DllImport(FbxDll.DllName)]
        private static extern void FbxSDKSkinnerGetClusterIndecesWeight(IntPtr pCluster, [Out] float[] pBuffer);

        IntPtr pCluster;

        public FbxCluster(IntPtr pointer)
        {
            pCluster = pointer;
        }

        public FbxNode GetClusterBone()
        {
            return Fbx.GetNativeWrapper<FbxNode>(FbxSDKSkinnerGetClusterBone(pCluster));
        }

        public int GetClusterDataCount()
        {
            return FbxSDKSkinnerGetClusterDataLength(pCluster);
        }

        public Tuple<int[],float[]> GetVertWeigthMapping()
        {
            var count = GetClusterDataCount();
            var weigth = new float[count];
            FbxSDKSkinnerGetClusterIndecesWeight(pCluster, weigth);
            var indeces = new int[count];
            FbxSDKSkinnerGetClusterIndeces(pCluster, indeces);
            return new Tuple<int[], float[]>(indeces, weigth);
        }


        public Matrix4 GetClusterMatrix()
        {
            var transform = Matrix4.Zero;
            FbxSDKSkinnerGetClusterMatrix(pCluster, out transform);

            return transform;
        }


        public Matrix4 GetClusterLinkMatrix()
        {
            var transform = Matrix4.Zero;
            FbxSDKSkinnerGetClusterLinkMatrix(pCluster, out transform);

            return transform;
        }
        
    }
}
