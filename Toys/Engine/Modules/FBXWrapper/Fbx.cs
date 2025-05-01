/// Original code from https://github.com/RazTools/Studio/
/// 

using FbxWrapper.FbxInterop;
using FbxWrapper.PInvoke;
using System.IO;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace FbxWrapper
{
    public static partial class Fbx
    {
        private static Dictionary<IntPtr, object> refDict = new Dictionary<nint, object>();

        static Fbx()
        {
            DllLoader.PreloadDll(FbxDll.DllName);
        }

        public static Vector3 QuaternionToEuler(Quaternion q)
        {
            AsUtilQuaternionToEuler(q.X, q.Y, q.Z, q.W, out var x, out var y, out var z);
            return new Vector3(x, y, z);
        }

        public static Color4 VectorToColor(Vector3 vec)
        {
            return new Color4(vec.X, vec.Y, vec.Z, 1);
        }

        public static Quaternion EulerToQuaternion(Vector3 v)
        {
            AsUtilEulerToQuaternion(v.X, v.Y, v.Z, out var x, out var y, out var z, out var w);
            return new Quaternion(x, y, z, w);
        }

        //To avoid dublicating objects on single native object
        internal static T GetNativeWrapper<T>(IntPtr pointer)
        {
            T result;
            if (refDict.ContainsKey(pointer))
            {
                result  = (T)refDict[pointer];
            }
            else
            {
                BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
                //result = (T)Activator.CreateInstance(typeof(T), flags, null, new object[] { pointer }, null);
                result = (T)Activator.CreateInstance(typeof(T), new object[] { pointer });
                refDict.Add(pointer, result);
            }

            return result;
        }

        internal static void ClearNativeObject(IntPtr pointer)
        {
            if (refDict.ContainsKey(pointer))
                refDict.Remove(pointer);
        }

        public static class Exporter
        {
            /*
            public static void Export(string path, IImported imported, ExportOptions exportOptions)
            {
                var file = new FileInfo(path);
                var dir = file.Directory;

                if (!dir.Exists)
                {
                    dir.Create();
                }

                var currentDir = Directory.GetCurrentDirectory();
                Directory.SetCurrentDirectory(dir.FullName);

                var name = Path.GetFileName(path);

                using (var exporter = new FbxWrapper(name, imported, exportOptions))
                {
                    exporter.Initialize();
                    exporter.ExportAll();
                }
                
                Directory.SetCurrentDirectory(currentDir);
            }
            */
        }

        public record ExportOptions
        {
            public bool eulerFilter;
            public float filterPrecision;
            public bool exportAllNodes;
            public bool exportSkins;
            public bool exportAnimations;
            public bool exportBlendShape;
            public bool castToBone;
            public int boneSize;
            public float scaleFactor;
            public int fbxVersion;
            public int fbxFormat;
        }
    }
}
