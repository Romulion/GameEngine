using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FbxTools.FbxInterop;
using static System.Net.Mime.MediaTypeNames;
using OpenTK.Mathematics;
using Valve.VR;

namespace FbxTools
{
    public class Tests
    {

        //[DllImport("PInvokeCpp.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern int TestArrayOfInts(int[] array, int size);

        //[DllImport("FBXWrapperNative.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern void Test(int n);

        [DllImport(FbxDll.DllName)]
        //[return: MarshalAs(UnmanagedType.BStr)]
        public static extern void Test(int[] n);


        [DllImport(FbxDll.DllName)]
        private static extern void AsUtilQuaternionToEuler(float qx, float qy, float qz, float qw, out float vx, out float vy, out float vz);


        static void main()
        {
            //var arr = new int[10];
            //TestArrayOfInts(arr, arr.Length);
            //foreach (var item in arr)
            //{
            //    Console.WriteLine(item);
            //}
            //Console.WriteLine(Fbx.QuaternionToEuler(new Quaternion(0, 0.5f, 0, 0)));
            //var arr = new Vector3[10];
            var arr = new int[10];
            Test(arr);
            Console.WriteLine(arr[0]);
            //Console.WriteLine(QuaternionToEuler(new Quaternion(0,0,0,0)));
            //Console.WriteLine(Fbx.QuaternionToEuler(new Quaternion(0, 0.5f, 0, 0)));

            /*
            TestArrayOfInts(arr, arr.Length);
            foreach (var item in arr)
            {
                Console.WriteLine(item);
            }

            var vec = new Vector3(1, 0, 2);
            TestVector(in vec);
            Console.WriteLine(vec);
            */
        }


        public static Vector3 QuaternionToEuler(Quaternion q)
        {
            AsUtilQuaternionToEuler(q.X, q.Y, q.Z, q.W, out var x, out var y, out var z);
            return new Vector3(x, y, z);
        }

        private Tests() { }

        public static void InitializeImport(string filePath)
        {
            //var wrapper = new FbxWrapper(filePath);
            //wrapper.DrawSceneSimple();
            //wrapper.ExportFile(@"D:\3d\3d\Models\Pronama\FBX\Pronama-chan2.fbx");
            //main();

            
            var exportOptions = new Fbx.ExportOptions();
            var context = new FbxWrapperContext(exportOptions);
            context.Initialize(true);
            context.ImportFromFile(filePath);
            var root = context.GetRootNode();
            //Console.WriteLine(root.GetName());
            //Console.WriteLine(root.GetTranslation());
            var body = TraverceScene(root, "Face");
           // Console.WriteLine("{0} {1}", body.GetName(), body.GetNodeType());
            //var mesh = body.GetFbxMesh();
            //CheckMaterials(mesh);
            //CheckMeshData(mesh);
            //Console.WriteLine(44444);
            //CheckSaveFile(context,mesh);
            //CheckBlendShapes(mesh);
            //CheckSkinData(mesh);
            //CheckIdecesData(mesh);
            //context.SaveFileTo(@"D:\3d\3d\Models\Pronama\FBX\Pronama-chan2.fbx");
            //context.SaveFileTo(@"D:\HomeShare\Tests\NPC_Avatar_Loli_Bow_Sigewinne\NPC_Avatar_Loli_Bow_Sigewinne2.fbx");
            


        }

        static void CheckSkinData(FbxMesh mesh)
        {
            var count = mesh.GetSkinnerCount();
            for (int i = 0; i < count; i++)
            {
                var skinner = mesh.GetSkinnerAt(i);
                var cCount = skinner.GetClusterCount();
                Console.WriteLine("New Skinner");
                for (int n = 0; n < cCount; n++)
                {
                    var cluster = skinner.GetClusterAt(n);
                    
                    var node = cluster.GetClusterBone();
                    Console.WriteLine(node.GetName());
                    Console.WriteLine(cluster.GetClusterMatrix());
                    var map = cluster.GetVertWeigthMapping();
                    Console.WriteLine($"counts is {map.Item1.Length} {map.Item2.Length}");
                    for (int k = 0; k < map.Item1.Length; k++)
                        Console.Write($"{map.Item1[k]} {map.Item2[k]} ");
                    Console.WriteLine();
                }
            }
        }

        static void CheckBlendShapes(FbxMesh mesh)
        {
            if (mesh.GetBlendShapeDeformerCount() < 1)
                return;

            var deformer = mesh.GetBlendShapeDeformer(0);
            for (int i = 0; i < deformer.GetChannelCount(); i++)
            {
                var channel = deformer.GetChannelAt(i);
                Console.WriteLine($"channel: {channel.GetName()}");
                for (int k = 0; k < channel.GetShapeCount(); k++)
                {
                    var shape = channel.GetShapeAt(k);
                    Console.WriteLine($"shape: {shape.GetName()}");
                    if (shape.GetName() == "Mouth_Open01")
                    {
                        var tmp = shape.GetVertices();
                        Console.WriteLine();
                    }
               
                    /*
                    var tmp = shape.GetVertices();
                    foreach (var elem in tmp.Item1)
                        Console.Write($"{elem} ");

                    Console.WriteLine();
                    foreach (var elem in tmp.Item2)
                        Console.Write($"{elem} ");
                    Console.WriteLine();
                    */
                }
            }
        }

        static void CheckMaterials(FbxMesh mesh)
        {
            for (int i = 0; i < mesh.GetMaterialCount(); i++)
            {
                var mat = mesh.GetMaterialAt(i);
                Console.WriteLine(mat.Name);
                Console.WriteLine(mat.GetDiffuseTexture());
                Console.WriteLine($"{mat.Diffuse} {mat.Specular} {mat.Emissive} {mat.Shininess} {mat.Ambient} {mat.Reflect} {mat.Transparency}");
            }
            
        }

        static void CheckSaveFile(FbxWrapperContext context, FbxMesh mesh)
        {
            var mat = mesh.GetMaterialAt(1);
            Console.WriteLine(mat.Name);
            mat.GetDiffuseTexture().SetName(@"D:\HomeShare\Tests\NPC_Avatar_Loli_Bow_Sigewinne\SigewinneBodyDiffuse1.png");
            Console.WriteLine(mat.GetDiffuseTexture());

            var res = context.ExportToFile(@"D:\HomeShare\Tests\NPC_Avatar_Loli_Bow_Sigewinne\NPC_Avatar_Loli_Bow_Sigewinne1.fbx");
            //if (!res)
            //{
            //    Console.WriteLine(context.GetLastError());
            //}
            
        }

        static void CheckMeshData(FbxMesh mesh)
        {
            var colors = mesh.GetColors();
            var vertices = mesh.GetVertices();
            var tangents = mesh.GetTangents();
            var uv = mesh.GetUVFromChannel(0);
            var normals = mesh.GetNormals();


            Console.WriteLine($" {vertices.Length} {normals.Length} {colors.Length} {tangents.Length}");
            //foreach (var elem in colors)
            //    if (elem != Vector4.Zero)
            //        Console.WriteLine(elem);
        }

        static void CheckIdecesData(FbxMesh mesh)
        {
            Console.WriteLine("By matID");
            var indeces = mesh.GetIndecesByMaterial();
            foreach (var elem in indeces)
            {
                Console.WriteLine(elem.Key);
                foreach (var id in elem.Value)
                    Console.Write(id + " ");
                Console.WriteLine();
            }
        }

        internal static FbxNode TraverceScene(FbxNode node, string meshName)
        {


            Console.WriteLine($"{node.GetName()} {node.GetNodeType()}");
            //if (node.GetName() == meshName && node.GetNodeType() == FbxNode.FbxNodeType.MESH)
            //if (node.GetName() == "Face" && node.GetNodeType() == FbxNode.FbxNodeType.MESH)
            //    return node;


            FbxNode res = null;
            for (int i = 0; i < node.GetChildCount(); i++)
            {
                res = TraverceScene(node.GetChildAtID(i), meshName);
                if (res != null)
                    break;
            }
            return res;
        }

    }
}
