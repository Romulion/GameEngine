using FbxWrapper.FbxInterop;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using OpenTK.Mathematics;
using System.Collections.Specialized;

namespace FbxWrapper
{
    class FbxMesh
    {
        [DllImport(FbxDll.DllName)]
        private static extern int FbxSDKMeshGetVertexCount(IntPtr pMesh);

        [DllImport(FbxDll.DllName)]
        private static extern bool FbxSDKMeshGetVertices(IntPtr pMesh, [Out] Vector3[] pBuffer);

        [DllImport(FbxDll.DllName)]
        private static extern int FbxSDKMeshGetNormalsCount(IntPtr pMesh);

        [DllImport(FbxDll.DllName)]
        private static extern bool FbxSDKMeshGetNormals(IntPtr pMesh, [Out] Vector3[] pBuffer);

        [DllImport(FbxDll.DllName)]
        private static extern int FbxSDKMeshGetTangentsCount(IntPtr pMesh);

        [DllImport(FbxDll.DllName)]
        private static extern bool FbxSDKMeshGetTangents(IntPtr pMesh, [Out] Vector4[] pBuffer);

        [DllImport(FbxDll.DllName)]
        private static extern int FbxSDKMeshGetColorCount(IntPtr pMesh);

        [DllImport(FbxDll.DllName)]
        private static extern bool FbxSDKMeshGetColor(IntPtr pMesh, [Out] Color4[] pBuffer);

        [DllImport(FbxDll.DllName)]
        private static extern int FbxSDKMeshGetUVChannelCount(IntPtr pMesh);

        [DllImport(FbxDll.DllName)]
        private static extern int FbxSDKMeshGetUVDataCount(IntPtr pMesh, int channelId);

        [DllImport(FbxDll.DllName)]
        private static extern bool FbxSDKMeshGetUVData(IntPtr pMesh, [Out] Vector2[] pBuffer, int channelId);

        [DllImport(FbxDll.DllName)]
        private static extern bool FbxSDKMeshCheckMatMapping(IntPtr pMesh);

        [DllImport(FbxDll.DllName)]
        private static extern int FbxSDKMeshGetPolygonCount(IntPtr pMesh);

        [DllImport(FbxDll.DllName)]
        private static extern bool FbxSDKMeshGetPolygons(IntPtr pMesh, [Out] int[] pBuffer);

        [DllImport(FbxDll.DllName)]
        private static extern bool FbxSDKMeshGetMaterialIndeces(IntPtr pMesh, [Out] int[] pBuffer);

        [DllImport(FbxDll.DllName)]
        private static extern int FbxSDKMeshGetMaterialCount(IntPtr pMesh);

        [DllImport(FbxDll.DllName)]
        private static extern IntPtr FbxSDKMeshGetMaterial(IntPtr pMesh, int index);

        [DllImport(FbxDll.DllName)]
        private static extern int FbxSDKMeshGetSkinnerCount(IntPtr pMesh);

        [DllImport(FbxDll.DllName)]
        private static extern IntPtr FbxSDKMeshGetSkinner(IntPtr pMesh, int index);

        [DllImport(FbxDll.DllName)]
        private static extern int FbxSDKMeshBlendShapeGetDeformerCount(IntPtr pMesh);

        [DllImport(FbxDll.DllName)]
        private static extern IntPtr FbxSDKMeshGetBlendShape(IntPtr pMesh, int index);

        [DllImport(FbxDll.DllName)]
        private static extern void FbxSDKMeshDestroy(IntPtr pMesh);

        [DllImport(FbxDll.DllName)]
        private static extern void FbxSDKNodeClearMesh(IntPtr pNode, IntPtr pMesh);

        IntPtr pMesh;

        public FbxMesh (IntPtr pointer)
        {
            pMesh = pointer;
        }

        public Vector3[] GetVertices()
        {
            var count = FbxSDKMeshGetVertexCount(pMesh);
            var vertices = new Vector3[count];
            FbxSDKMeshGetVertices(pMesh, vertices);
            return vertices;
        }

        public Vector3[] GetNormals()
        {
            var count = FbxSDKMeshGetNormalsCount(pMesh);
            var vertices = new Vector3[count];
            FbxSDKMeshGetNormals(pMesh, vertices);
            return vertices;
        }

        public Vector4[] GetTangents()
        {
            var count = FbxSDKMeshGetTangentsCount(pMesh);
            var vertices = new Vector4[count];
            FbxSDKMeshGetTangents(pMesh, vertices);
            return vertices;
        }

        public Color4[] GetColors()
        {
            var count = FbxSDKMeshGetColorCount(pMesh);
            var vertices = new Color4[count];
            FbxSDKMeshGetColor(pMesh, vertices);
            return vertices;
        }

        public int GetUVChannelCount()
        {
            return FbxSDKMeshGetUVChannelCount(pMesh);
        }

        public Vector2[] GetUVFromChannel(int index)
        {
            if (index < FbxSDKMeshGetUVChannelCount(pMesh) && index >= 0)
            {
                var count = FbxSDKMeshGetUVDataCount(pMesh, index);
                var vertices = new Vector2[count];
                FbxSDKMeshGetUVData(pMesh, vertices, index);
                return vertices;
            }
            return null;
        }

        public Dictionary<int, List<int>> GetIndecesByMaterial()
        {
            if (!FbxSDKMeshCheckMatMapping(pMesh))
                return null;


            var matMapping = new Dictionary<int, List<int>>();
            var count = FbxSDKMeshGetPolygonCount(pMesh);
            var indeces = new int[count * 3];
            FbxSDKMeshGetPolygons(pMesh, indeces);
            var mats = new int[count];
            FbxSDKMeshGetMaterialIndeces(pMesh, mats);

            for (int i = 0; i < count; i++)
            {
                List<int> indecesList;
                if (matMapping.ContainsKey(mats[i]))
                {
                    indecesList = matMapping[mats[i]];
                }
                else
                {
                    indecesList = new List<int>();
                    matMapping.Add(mats[i], indecesList);
                }

                var offset = i * 3;
                indecesList.Add(indeces[offset]);
                indecesList.Add(indeces[offset + 1]);
                indecesList.Add(indeces[offset + 2]);
            }

            return matMapping;
        }

        public int GetMaterialCount()
        {
            return FbxSDKMeshGetMaterialCount(pMesh);
        }

        public FbxMaterial GetMaterialAt(int index)
        {
            if (index < FbxSDKMeshGetMaterialCount(pMesh) && index >= 0)
            {
                return Fbx.GetNativeWrapper<FbxMaterial>(FbxSDKMeshGetMaterial(pMesh, index));
            }
            return null;
        }

        public int GetSkinnerCount()
        {
            return FbxSDKMeshGetSkinnerCount(pMesh);
        }


        public FbxSkin GetSkinnerAt(int index)
        {
            if (index < FbxSDKMeshGetSkinnerCount(pMesh) && index >= 0)
            {
                return Fbx.GetNativeWrapper<FbxSkin>(FbxSDKMeshGetSkinner(pMesh, index));
            }

            return null;
        }

        public int GetBlendShapeDeformerCount() {
            return FbxSDKMeshBlendShapeGetDeformerCount(pMesh);
        }

        public FbxBlendShape GetBlendShapeDeformer(int index)
        {
            if (index < FbxSDKMeshBlendShapeGetDeformerCount(pMesh) && index >= 0)
            {
                return Fbx.GetNativeWrapper<FbxBlendShape>(FbxSDKMeshGetBlendShape(pMesh, index));
            }

            return null;
        }

        public void Destroy(FbxNode parentNode)
        {
            Fbx.ClearNativeObject(pMesh);
            FbxSDKNodeClearMesh(parentNode.GetPointer, pMesh);
        }
    }
}
