using FbxTools.FbxInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using OpenTK.Mathematics;

namespace FbxTools
{
    public enum FbxTextureType
    {
        Diffuse,
        NormalMap,
        Specular,
        Bump,
        Ambient,
        Emissive,
        Reflection,
    }
    class FbxTexture
    {

        [DllImport(FbxDll.DllName)]
        [return: MarshalAs(UnmanagedType.BStr)]
        private static extern string FbxSDKTextureGetName(IntPtr pTexture);

        [DllImport(FbxDll.DllName)]
        private static extern bool FbxSDKTextureSetName(IntPtr pTexture, [MarshalAs(UnmanagedType.LPUTF8Str)] string pName);

        [DllImport(FbxDll.DllName)]
        private static extern bool FbxSDKTextureGetTransform(IntPtr pTexture, out Vector2 translation, out Vector2 scale);
        IntPtr pTexture;

        public Vector2 Offset { get; private set; }
        public Vector2 Scale { get; private set; }

        public FbxTexture(IntPtr pointerTexture)
        {
            pTexture = pointerTexture;
            ReadTransform();
        }

        public string GetName()
        {
            return FbxSDKTextureGetName(pTexture);
        }

        public void SetName(string name)
        {
            FbxSDKTextureSetName(pTexture, name);
        }

        void ReadTransform()
        {
            Vector2 offset, scale;
            FbxSDKTextureGetTransform(pTexture, out offset, out scale);
            Offset = offset;
            Scale = scale;
        }
    }
}
