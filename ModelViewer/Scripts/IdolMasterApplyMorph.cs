using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FbxWrapper;
using Toys;
using OpenTK.Mathematics;
using Toys.UnityAnimReader;
using System.IO;
using System.Text.RegularExpressions;

namespace ModelViewer
{
    class IdolMasterApplyMorph
    {
        FbxDataScene sceneData;
        FbxDataMesh mesh;
        BoneController bController;
        Dictionary<string, ModToolBone> bones;
        ModToolBone rootBone;
        List<ModToolBone> bonesList;
        FbxDataMorph morphObject;

        public IdolMasterApplyMorph(FbxDataScene dataScene, string meshName)
        {
            sceneData = dataScene;
            mesh = sceneData.MeshList.Find(m => m.Path.EndsWith(meshName));
            MapSkinningData();

            morphObject = sceneData.MorphList.Find(m => m.Path == mesh.Path);
            if (morphObject == null)
            {
                morphObject = new FbxDataMorph();
                morphObject.Path = mesh.Path;
                morphObject.Channels = new List<FbxDataMorphChannel>();
                sceneData.MorphList.Add(morphObject);
            }
        }

        void MapSkinningData()
        {
            var fbxBones = mesh.BoneList;
            bones = new Dictionary<string, ModToolBone>();
            bonesList = new List<ModToolBone>();
            rootBone = TraverceSceneBones(sceneData.RootNode, null);

            var tempArr = new ModToolBone[bonesList.Count];
            int nextBoneIndex = fbxBones.Count;
            foreach (var boneElem in bonesList)
            {
                boneElem.CalcPath();
                bones.Add(boneElem.Path, boneElem);

                var index = fbxBones.FindIndex(b => b.Path == boneElem.Path);

                if (index < 0)
                {
                    tempArr[nextBoneIndex++] = boneElem;
                }
                else
                {
                    tempArr[index] = boneElem;
                    boneElem.Inverse = fbxBones[index].Matrix.Inverted();
                }
            }
            bonesList = tempArr.ToList();

            rootBone.CalcGlobalTransform();
            for(int i = fbxBones.Count; i < bonesList.Count; i++)
            {
                bonesList[i].Inverse = bonesList[i].GlobalTransform.Inverted();
            }
            
            foreach(var bone in bonesList)
            {
                //Console.WriteLine(bone.Path);
                /*
                var res = bone.Inverse * bone.GlobalTransform;
                for (int n = 0; n < 4; n++)
                    for (int m = 0; m < 4; m++)
                        if (res[n, m] < 0.0000001f)
                            res[n, m] = 0;
                Console.WriteLine(res);
                */
            }
            
            SetupSkinner();
        }

        void SetupSkinner()
        {
            var boneArray = new List<Bone>();
            foreach(var boneTemp in bonesList)
            {
                boneArray.Add(new Bone(boneTemp.Path, boneTemp.BaseLocalTransform, bonesList.IndexOf(boneTemp.Parent)));
            }
            bController = new BoneController(boneArray.ToArray(), true);
        }

        ModToolBone TraverceSceneBones(FbxDataNode node, ModToolBone parent)
        {
            var mat = Matrix4.CreateScale(node.LocalScale);
            mat *= Matrix4.CreateFromQuaternion(Quaternion.FromEulerAngles(node.LocalRotation));
            mat *= Matrix4.CreateTranslation(node.LocalPosition);
            var bone = new ModToolBone(node.Name, mat);
            foreach (var child in node.Children)
            {
                var childNew = TraverceSceneBones(child, parent);
                childNew.SetParent(bone);
                bonesList.Add(childNew);
            }

            return bone;
        }

        public void LoadDirectoryAnims(string directory)
        {
            foreach(var file in Directory.GetFiles(directory, "*.anim"))
            {
                var fileName = Path.GetFileName(file);
                var match = Regex.Match(fileName, "_([^_]+_[^_]+_[^_]+).anim$");
                var anim = new YamlAnimReader(file);
                anim.Name = match.Groups[1].Value;
                AddAnimAsMorph(anim);
                bonesList.ForEach(b => b.LocalTransform = Matrix4.Identity);
            }
        }

        public void AddAnimAsMorph(YamlAnimReader anim)
        {
            //fill local transform data
            var data = anim.AnimationData;

            
            foreach (var boneData in data)
            {
                ModToolBone bone = bonesList.Find((b) => b.Path.EndsWith(boneData.Path));
                if (bone != null)
                {
                    var newTransform = Matrix4.CreateFromQuaternion(boneData.Rotation) * Matrix4.CreateTranslation(boneData.Position);
                    bone.LocalTransform = newTransform * bone.BaseLocalTransform.Inverted();
                }
                else
                    Logger.Warning($"Bone {boneData.Path} not found");
            }

            rootBone.CalcGlobalTransform();

            var morphed = new Vector3[mesh.VertexList.Count];
            var indecesList = new List<int>();
            var skeleton = bController.GetSkeleton;
            for (int i =0; i <  mesh.VertexList.Count; i++)
            {
                var vertex = mesh.VertexList[i];
                var transorm = Matrix4.Zero;
                for (int m = 0; m < 4; m++)
                {
                    var bone = bonesList[vertex.BoneIndices[m]];
                    transorm += bone.Inverse * bone.GlobalTransform * vertex.Weights[m];
                }
                morphed[i] = (new Vector4(vertex.Position, 1) * transorm).Xyz;
                if (Vector3.DistanceSquared(morphed[i], vertex.Position) > 0.000001f)
                    indecesList.Add(i);
            }


            var channel = new FbxDataMorphChannel();
            channel.Name = anim.Name;
            channel.KeyframeList = new List<FbxDataMorphKeyframe>();
            var keyframe = new FbxDataMorphKeyframe();
            channel.KeyframeList.Add(keyframe);
            keyframe.Weight = 1;
            keyframe.VertexList = new List<FbxDataMorphVertex>();
            foreach (var index in indecesList)
            {
                var morphVertex = new FbxDataMorphVertex();
                morphVertex.Index = (uint)index;
                var vertexData = new FbxDataVertex();
                vertexData.Position = morphed[index];
                morphVertex.Vertex = vertexData;
                keyframe.VertexList.Add(morphVertex);
            }

            morphObject.Channels.Add(channel);
        }

        public void MergeWithBody(FbxDataScene body)
        {
            var attachPoint = body.RootNode.FindChild("Neck");
            string entryPoint2 = body.RootNode.Path;
            string entryPoint = attachPoint.Path;
            //delete old head node
            attachPoint.Remove(attachPoint.GetFirstChild);

            //Add meshes nodes to new scene
            var root = sceneData.RootNode;
            foreach (var child in root.Children.ToArray())
            {
                if (!child.Path.EndsWith("Position"))
                {
                    body.RootNode.AddChild(child);
                }
            }

            var neck = root.FindChild("Neck");
            var oldPath = neck.Path;
            foreach(var child in neck.Children.ToArray())
                AddToSceneRecurcive(child, attachPoint);

            var transform = CalculateTransform(attachPoint);

            foreach (var mesh in sceneData.MeshList)
            {
                foreach (var bone in mesh.BoneList)
                {
                    bone.Path = bone.Path.Replace(oldPath, entryPoint);
                    bone.Matrix *= transform;
                }

                var meshName = mesh.Path.Substring(mesh.Path.LastIndexOf('/') + 1);
                mesh.Path = entryPoint2 + "/" + meshName;

                foreach (var vert in mesh.VertexList)
                {
                    vert.Position = (new Vector4(vert.Position, 1) * transform).Xyz;
                }

                body.MeshList.Add(mesh);
            }

            foreach(var blend in sceneData.MorphList)
            {
                var blendName = blend.Path.Substring(blend.Path.LastIndexOf('/') + 1);
                blend.Path = entryPoint2 + "/" + blendName;
                foreach (var channel in blend.Channels)
                    foreach(var key in channel.KeyframeList)
                        foreach(var vert in key.VertexList)
                            vert.Vertex.Position = (new Vector4(vert.Vertex.Position, 1) * transform).Xyz;
                body.MorphList.Add(blend);
            }

            foreach (var mat in sceneData.MaterialList)
                body.MaterialList.Add(mat);

            sceneData.TextureList.ForEach(t => body.TextureList.Add(t));
        }

        void AddToSceneRecurcive(FbxDataNode node, FbxDataNode parent)
        {
            parent.AddChild(node);
            foreach (var child in node.Children.ToArray())
                AddToSceneRecurcive(child, node);
        }

        Matrix4 CalculateTransform(FbxDataNode node)
        {
            var transform = Matrix4.CreateFromQuaternion(Quaternion.FromEulerAngles(node.LocalRotation)) * Matrix4.CreateTranslation(node.LocalPosition);
            if (node.Parent != null)
                transform *= CalculateTransform(node.Parent);
            return transform;
        }
    }

    
    class ModToolBone
    {
        public string Name { get; private set; }
        public string Path { get; private set; }

        public Matrix4 BaseLocalTransform;
        public Matrix4 LocalTransform;
        public Matrix4 GlobalTransform;
        public Matrix4 Inverse;

        public ModToolBone Parent { get; private set; }
        List<ModToolBone> childs = new List<ModToolBone>();

        public ModToolBone(string name, Matrix4 baseTransform)
        {
            Name = name;
            BaseLocalTransform = baseTransform;
            LocalTransform = Matrix4.Identity;
        }
        public void SetParent(ModToolBone parent)
        {
            Parent = parent;
            Parent.childs.Add(this);
        }

        public void CalcGlobalTransform()
        {
            Matrix4 parent = (Parent == null) ? Matrix4.Identity : Parent.GlobalTransform;
            GlobalTransform =  LocalTransform * BaseLocalTransform * parent;
            foreach (var child in childs)
                child.CalcGlobalTransform();
        }

        public void CalcPath()
        {
            //string pathStart = "Position";

            var frame = this;
            var path = frame.Name;
            while (frame.Parent != null)
            {
                frame = frame.Parent;
                path = frame.Name + "/" + path;
            }
            
            Path = path;
        }
    
    }
    
}
