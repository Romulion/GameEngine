using FbxTools.FbxInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;
using System.Reflection;

namespace FbxTools
{
    internal class FbxNode
    {
        [DllImport(FbxDll.DllName)]
        private static extern IntPtr FbxSDKGetNodeParent(IntPtr pNode);
        [DllImport(FbxDll.DllName)]
        private static extern int FbxSDKGetNodeChildsCount(IntPtr pNode);

        [DllImport(FbxDll.DllName)]
        private static extern IntPtr FbxSDKGetNodeAt(IntPtr pNode, int index);

        [DllImport(FbxDll.DllName)]
        [return: MarshalAs(UnmanagedType.BStr)]
        private static extern string FbxSDKGetNodeName(IntPtr pNode);

        [DllImport(FbxDll.DllName)]
        private static extern void FbxSDKGetNodeLocalTransform(IntPtr pNode, out Vector3 transVec3, out Vector3 rotVec3, out Vector3 scaleVec3);

        [DllImport(FbxDll.DllName)]
        private static extern int FbxSDKNodeGetType(IntPtr pNode);

        [DllImport(FbxDll.DllName)]
        private static extern IntPtr FbxSDKNodeGetMesh(IntPtr pNode);

        public enum FbxNodeType
        {
             UNIDENTIFIED,
             NULL,
             MARKER,
             SKELETON, 
             MESH, 
             NURB, 
             PATCH,
             CAMERA, 
             CAMERA_STEREO,
             CAMERA_SWITCHER,
             LIGHT,
             OPTICAL_REFERENCE,
             OPTICAL_MARKER,
             NURBS_CURVE,
             TRIM_NURBS_SURFACE,
             BOUNDARY,
             NURBS_SURFACE,
             SHAPE,
             LODGROUP,
             SUBDIV
        }

        IntPtr pNode;
        Vector3 translation = Vector3.Zero;
        Vector3 rotation = Vector3.Zero;
        Vector3 scale = Vector3.Zero;

        public FbxNode(IntPtr nodePointer)
        {
            pNode = nodePointer;
            GetLocalTransform();
        }

        void GetLocalTransform()
        {
            FbxSDKGetNodeLocalTransform(pNode, out translation, out rotation, out scale);
        }

        public string GetName()
        {
            return FbxSDKGetNodeName(pNode);
        }

        public FbxNode GetParent()
        {
            var pointer = FbxSDKGetNodeParent(pNode);
            if (pointer != IntPtr.Zero)
                return Fbx.GetNativeWrapper<FbxNode>(pointer);
            return null;
        }

        public int GetChildCount()
        {
            return FbxSDKGetNodeChildsCount(pNode);
        }

        public FbxNode GetChildAtID(int index)
        {
            if (index < FbxSDKGetNodeChildsCount(pNode) || index >= 0)
            {
                var pointer = FbxSDKGetNodeAt(pNode, index);
                return Fbx.GetNativeWrapper<FbxNode>(pointer);
            }

            return null;
        }

        public Vector3 GetRotaion
        {
            get { return rotation;  }
        }

        public Vector3 GetTranslation
        {
            get { return translation; }
        }

        public Vector3 GetScale
        {
            get
            {
                return scale;
            }
        }

        public FbxNodeType GetNodeType()
        {
            return (FbxNodeType) FbxSDKNodeGetType(pNode);
        }

        public FbxMesh GetFbxMesh()
        {
            return Fbx.GetNativeWrapper<FbxMesh>(FbxSDKNodeGetMesh(pNode));
        }

        internal IntPtr GetPointer
        {
            get
            {
                return pNode;
            }
        }
    }
}
