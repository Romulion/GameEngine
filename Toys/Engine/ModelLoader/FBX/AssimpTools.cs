using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;
using Assimp;

namespace Toys
{
    internal class AssimpTools
    {
        List<Node> sceneNodes = new List<Node>();
        List<Bone> bones = new List<Bone>();
        List<string> boneEntrys = new List<string>();

        void TraverseScene(Assimp.Node node, bool isArmature = false)
        {

            int parent = -1;
            if (node.Parent != null)
                parent = sceneNodes.IndexOf(node.Parent);

            sceneNodes.Add(node);

            //Find bone that has mash assigned with all childrens
            if (!isArmature && boneEntrys.Contains(node.Name))
            {
                isArmature = true;
            }

            if (isArmature)
            {
                var bone = new Bone(node.Name, Convert(node.Transform), parent);
                bones.Add(bone);
            }
            
            if (node.HasChildren)
            {
                foreach (var child in node.Children)
                {
                    TraverseScene(child, isArmature);
                }
            }

        }

        public List<Bone> GetArmaure(Assimp.Scene scene)
        {
            foreach (var mesh in scene.Meshes)
            {
                foreach (var boneTemp in mesh.Bones)
                {
                    if (!boneEntrys.Contains(boneTemp.Name))
                        boneEntrys.Add(boneTemp.Name);
                }
            }
            

            //Get Armature Nodes
            TraverseScene(scene.RootNode);

            //Sort bones to alphabet
            bones.Sort((a, b) => a.Name.CompareTo(b.Name));


            //Sorting bones according to their position in model data
            var bonesOrdered = new List<Bone>(bones.Count);
            foreach (var mesh in scene.Meshes)
            {
                foreach (var boneTemp in mesh.Bones)
                {
                    var bone = bones.Find(b => b.Name == boneTemp.Name);
                    if (!bonesOrdered.Contains(bone))
                        bonesOrdered.Add(bone);
                }
            }
            //Add bones that dont attached to mesh
            foreach (var bone in bones)
            {
                if (!bonesOrdered.Contains(bone))
                    bonesOrdered.Add(bone);
            }

            
            //Remap bone parent indexes
            for (int i = 0; i < bonesOrdered.Count; i++)
            {
                
                var bone = bonesOrdered[i];
                
                if (bone.ParentIndex >= 0)
                {
                    bone.ParentIndex = bonesOrdered.FindIndex(b =>  b.Name == sceneNodes[bone.ParentIndex].Name);
                }
            }
            return bonesOrdered;
        }

        public OpenTK.Mathematics.Vector3 Convert(System.Numerics.Vector3 vector)
        {
            return new OpenTK.Mathematics.Vector3(vector.X, vector.Y, vector.Z);
        }

        public OpenTK.Mathematics.Vector4 Convert(System.Numerics.Vector4 vector)
        {
            return new OpenTK.Mathematics.Vector4(vector.X, vector.Y, vector.Z, vector.W);
        }

        public OpenTK.Mathematics.Matrix4 Convert(System.Numerics.Matrix4x4 vector)
        {
            return new OpenTK.Mathematics.Matrix4(vector.M11, vector.M12, vector.M13, vector.M14,
                vector.M21, vector.M22, vector.M23, vector.M24,
                vector.M31, vector.M32, vector.M33, vector.M34,
                vector.M41, vector.M42, vector.M43, vector.M44).Transposed();
        }


        public List<System.Numerics.Vector3> Convert43(List<Vector4> list)
        {
            return list.ConvertAll<System.Numerics.Vector3>(el => Convert43(el));
        }
        public List<System.Numerics.Vector3> Convert(List<Vector3> list)
        {
            return list.ConvertAll<System.Numerics.Vector3>(el => Convert(el));
        }

        public List<System.Numerics.Vector3> Convert(List<Vector2> list)
        {
            return list.ConvertAll<System.Numerics.Vector3>(el => Convert(el));
        }

        public System.Numerics.Vector3 Convert(OpenTK.Mathematics.Vector3 vector)
        {
            return new System.Numerics.Vector3(vector.X, vector.Y, vector.Z);
        }

        public System.Numerics.Vector3 Convert(OpenTK.Mathematics.Vector2 vector)
        {
            return new System.Numerics.Vector3(vector.X, vector.Y, 0);
        }

        public System.Numerics.Vector4 Convert(OpenTK.Mathematics.Vector4 vector)
        {
            return new System.Numerics.Vector4(vector.X, vector.Y, vector.Z, vector.W);
        }

        public System.Numerics.Vector3 Convert43(OpenTK.Mathematics.Vector4 vector)
        {
            return new System.Numerics.Vector3(vector.X, vector.Y, vector.Z);
        }
    }
}
