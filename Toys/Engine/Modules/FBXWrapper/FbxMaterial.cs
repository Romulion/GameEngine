using FbxTools.FbxInterop;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FbxTools
{
    class FbxMaterial
    {
        //[DllImport(FbxDll.DllName)]
        //[return: MarshalAs(UnmanagedType.BStr)]
        //private static extern string FbxSDKMaterialGetDiffuseTexture(IntPtr pMat);

        //[DllImport(FbxDll.DllName)]
        //private static extern bool FbxSDKMaterialSetDiffuseTexture(IntPtr pMat, [MarshalAs(UnmanagedType.LPUTF8Str)] string pName);

        [DllImport(FbxDll.DllName)]
        private static extern IntPtr FbxSDKMaterialGetDiffuseTexture(IntPtr pMat);

        [DllImport(FbxDll.DllName)]
        [return: MarshalAs(UnmanagedType.BStr)]
        private static extern string FbxSDKMaterialGetName(IntPtr pMat);

        [DllImport(FbxDll.DllName)]
        private static extern bool FbxSDKMaterialGetProperties(IntPtr pMat, out Vector3 diffuse, out Vector3 ambient, out Vector3 emissive, out Vector3 specular, out Vector3 reflect, out float shininess, out float transparency);

        IntPtr pMat;

        public Vector3 Diffuse { get; private set; }
        public Vector3 Ambient { get; private set; }
        public Vector3 Emissive { get; private set; }
        public Vector3 Specular { get; private set; }
        public Vector3 Reflect { get; private set; }
        public float Shininess { get; private set; }
        public float Transparency { get; private set; }

        public string Name { get; private set; }
        public FbxMaterial(IntPtr pointerMaterial)
        {
            pMat = pointerMaterial;
            GetName();
            ReadParameters();
        }

        public FbxTexture GetDiffuseTexture()
        {
            var pointer = FbxSDKMaterialGetDiffuseTexture(pMat);
            if (pointer == IntPtr.Zero)
                return null;

            return Fbx.GetNativeWrapper<FbxTexture>(pointer);
        }

        /*
        //Set Absolute path only
        public void SetDiffuseTexture(string tPath)
        {
            FbxSDKMaterialSetDiffuseTexture(pMat, tPath);
        }
        */

        void GetName()
        {
            Name = FbxSDKMaterialGetName(pMat);
        }

        void ReadParameters()
        {
            Vector3 diffuse, ambient, emissive, specular, reflect;
            float shininess, transparency;
            FbxSDKMaterialGetProperties(pMat, out diffuse, out ambient, out emissive, out specular, out reflect, out shininess, out transparency);
            Diffuse = diffuse;
            Ambient = ambient;
            Emissive = emissive;
            Specular = specular;
            Reflect = reflect;
            Shininess = shininess;
            Transparency = transparency;
        }

        internal IntPtr GetPointer { get { return pMat; } }
    }
}
