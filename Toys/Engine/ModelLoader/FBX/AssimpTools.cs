using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;
using Assimp;
using NAudio.CoreAudioApi.Interfaces;

namespace Toys
{
    internal class AssimpTools
    {
        List<Node> sceneNodes = new List<Node>();
        List<Bone> bones = new List<Bone>();
        List<string> boneEntrys = new List<string>();
        List<Assimp.Bone> bonesBase = new List<Assimp.Bone>();
        List<AssimpNodeTransform> node2BoneConvertedTransform = new List<AssimpNodeTransform>();
        void TraverseScene(Assimp.Node node, bool isArmature = false)
        {

            int parent = -1;
            if (node.Parent != null) { 
                parent = sceneNodes.IndexOf(node.Parent);
                }
            
            //Check node names dublicate and rename in blender style
            //Suggesting bone cant have dublicate names
            if (sceneNodes.FindIndex(n => n.Name == node.Name) != -1)
            {
                var startId = 1;
                for (int i = 0; i < 1000; i++) 
                {
                    var name = String.Format("{0}.{1}", node.Name, startId.ToString().PadLeft(3, '0'));
                    if (sceneNodes.FindIndex(n => n.Name == name) == -1)
                    {
                        node.Name = name;
                        break;
                    }
                }
            }
            
            sceneNodes.Add(node);

            //Fill transfomr table
            var transform = Convert(node.Transform);
            if (parent > -1)
                transform = transform * node2BoneConvertedTransform[parent].World2Bone;
            node2BoneConvertedTransform.Add(new AssimpNodeTransform() { Name = node.Name, World2Bone = transform, Parent2Bone = Convert(node.Transform), IsBone = boneEntrys.Contains(node.Name) });
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
            bonesBase.Clear();
            boneEntrys.Clear();
            node2BoneConvertedTransform.Clear();
            foreach (var mesh in scene.Meshes)
            {
                foreach (var boneTemp in mesh.Bones)
                {
                    if (!boneEntrys.Contains(boneTemp.Name))
                        boneEntrys.Add(boneTemp.Name);

                    if (bonesBase.FindIndex(b => b.Name == boneTemp.Name) == -1)
                        bonesBase.Add(boneTemp);
                }
            }

            //Get Armature Nodes
            sceneNodes.Clear();
            bones.Clear();
            TraverseScene(scene.RootNode);
            
            //Sort bones to alphabet
            bones.Sort((a, b) => a.Name.CompareTo(b.Name));
            //add skipped scene nodes
            foreach (var node in sceneNodes)
            {
                if (!boneEntrys.Contains(node.Name))
                {
                    int parent = -1;
                    if (node.Parent != null)
                        parent = sceneNodes.IndexOf(node.Parent);

                    var b = new Bone(node.Name, Convert(node.Transform), parent);
                    bones.Add(b);
                }
            }
            int id = 0;
            UpdateBonesData(scene.RootNode, Matrix4.Identity, ref id);

            //Sorting bones according to their position in model data
            var bonesOrdered = new List<Bone>(bones.Count);
            foreach (var mesh in scene.Meshes)
            {
                foreach (var boneTemp in mesh.Bones)
                {
                    var bone = bones.Find(b => b.Name == boneTemp.Name);
                    if (!bonesOrdered.Contains(bone))
                    {
                        bonesOrdered.Add(bone);
                    }
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

        //Since some models can be not in default "zero" pose,
        //we get default pose from AssimpBone offsetMatrix and calculate bone transfome from node matrix
        void UpdateBonesData(Assimp.Node node, Matrix4 world2Parent, ref int id)
        {
            var nodeData = node2BoneConvertedTransform[id];

            if (nodeData.IsBone)
            {
                var boneOld = bonesBase.Find(b => b.Name == node.Name);
                //try find parent
                var parent = sceneNodes[id].Parent;
                //if parent is a bone
                if (boneEntrys.Contains(parent.Name))
                {
                    var pBone = bonesBase.Find(b => b.Name == parent.Name);
                    nodeData.Parent2Bone = Convert(boneOld.OffsetMatrix).Inverted() * Convert(pBone.OffsetMatrix);
                    //Calculate skeleton default pose
                    nodeData.LocalTransform = nodeData.Parent2Bone * Convert(sceneNodes[id].Transform).Inverted();
                }
                //if parent is regular node
                else
                {
                    nodeData.Parent2Bone = Convert(boneOld.OffsetMatrix).Inverted() * world2Parent.Inverted();
                }
                //Fix bone default transform
                bones.Find(b => b.Name == node.Name).Parent2Local = nodeData.Parent2Bone;

            }

            //update own world to bone matrix
            nodeData.World2Bone = nodeData.Parent2Bone * world2Parent;
            id++;

            //repeat for children
            foreach (var child in node.Children)
                UpdateBonesData(child, nodeData.World2Bone, ref id);
        }
        public OpenTK.Mathematics.Vector3 Convert(System.Numerics.Vector3 vector)
        {
            return new OpenTK.Mathematics.Vector3(vector.X, vector.Y, vector.Z);
        }

        public OpenTK.Mathematics.Vector4 Convert(System.Numerics.Vector4 vector)
        {
            return new OpenTK.Mathematics.Vector4(vector.X, vector.Y, vector.Z, vector.W);
        }

        bool IsMatrixIndentity(Matrix4 matrx)
        {
            bool result = true;
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    result &= ((i == j) && matrx[i, j] == 1) || Math.Abs(matrx[i, j]) < 0.00001f;
                }
            }
            return result;
        }
        
        public OpenTK.Mathematics.Matrix4 Convert(System.Numerics.Matrix4x4 vector)
        {
            return new OpenTK.Mathematics.Matrix4(
                vector.M11, vector.M21, vector.M31, vector.M41,
                vector.M12, vector.M22, vector.M32, vector.M42,
                vector.M13, vector.M23, vector.M33, vector.M43,
                vector.M14, vector.M24, vector.M34, vector.M44
                );
        }

        public System.Numerics.Matrix4x4 Convert(OpenTK.Mathematics.Matrix4 vector)
        {
            return new System.Numerics.Matrix4x4(
                vector.M11, vector.M21, vector.M31, vector.M41,
                vector.M12, vector.M22, vector.M32, vector.M42,
                vector.M13, vector.M23, vector.M33, vector.M43,
                vector.M14, vector.M24, vector.M34, vector.M44
                );
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
