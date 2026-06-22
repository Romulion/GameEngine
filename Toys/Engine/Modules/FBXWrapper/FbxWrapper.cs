/// Original code from https://github.com/RazTools/Studio/
/// 

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Toys;
using FbxTools.FbxInterop;
using OpenTK.Mathematics;
using System.Drawing.Drawing2D;
using OpenTK.Windowing.Common.Input;
using System.ComponentModel.DataAnnotations;
using Valve.VR;

namespace FbxTools
{
    public class FbxWrapper : IDisposable
    {

        private FbxWrapperContext _context;

        private string _importedFile;
        //private readonly IImported _imported;
        private Fbx.ExportOptions _exportOptions;

        readonly List<FbxNode> nodes = new List<FbxNode>();
        readonly List<FbxMesh> meshes = new List<FbxMesh>();
        readonly List<FbxNode> meshNodes = new List<FbxNode>();
        readonly List<FbxNode> bonesSorted = new List<FbxNode>();
        readonly List<FbxNode> bonesUnsorted = new List<FbxNode>();
        readonly List<Matrix4> bonesMatrices = new List<Matrix4>();
        readonly Dictionary<string, FbxNode> boneMap = new Dictionary<string, FbxNode>();
        public string WorkingDir { get; private set; }
        private FbxDataScene dataScene;
        public FbxWrapper(string fileName)
        {
            var exportOptions = new Fbx.ExportOptions();
            _context = new FbxWrapperContext(exportOptions);
            _exportOptions = exportOptions;
            _context.Initialize(true);


            _importedFile = fileName;
            WorkingDir = Path.GetDirectoryName(fileName) + @"\";
            if (!_context.ImportFromFile(fileName))
            {
                throw new Exception(_context.GetLastError());
            }
            MapScene();
        }


        ~FbxWrapper()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public bool IsDisposed { get; private set; }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }

            IsDisposed = true;
        }


        void MapScene()
        {
            var root = _context.GetRootNode();
            TraverceScene(root);
            GetBonesSorted();
        }

        void TraverceScene(FbxNode node)
        {
            nodes.Add(node);

            var type = node.GetNodeType();
            switch (type)
            {
                case FbxNode.FbxNodeType.MESH:
                    meshes.Add(node.GetFbxMesh());
                    meshNodes.Add(node);
                    break;
                case FbxNode.FbxNodeType.SKELETON:
                    bonesUnsorted.Add(node);
                    break;
            }

            for (int i = 0; i < node.GetChildCount(); i++)
            {
                TraverceScene(node.GetChildAtID(i));
            }
        }

        void GetBonesSorted()
        {
            //add binded to mesh bones
            foreach (var mesh in meshes)
            {
                var skinners = mesh.GetSkinnerCount();
                if (skinners < 1)
                    continue;

                var skinner = mesh.GetSkinnerAt(0);
                var count = skinner.GetClusterCount();
                for (int i = 0; i < count; i++)
                {
                    var cluster = skinner.GetClusterAt(i);
                    var bone = cluster.GetClusterBone();
                    if (!bonesSorted.Contains(bone))
                    {
                        bonesSorted.Add(bone);
                        bonesMatrices.Add(cluster.GetClusterLinkMatrix());
                    }
                }
            }
            //add rest bones
            foreach (var bone in bonesUnsorted)
            {
                if (!bonesSorted.Contains(bone))
                {
                    bonesSorted.Add(bone);
                    //TODO: calculate matrix
                    bonesMatrices.Add(Matrix4.Identity);

                }
            }

            
            foreach (var bone in bonesSorted)
            {
                boneMap.Add(GetNodePath(bone), bone);
            }
        }


        //create model without skinning
        internal MeshDrawer ConvertMesh(FbxMesh fbxMesh)
        {
            var verts = fbxMesh.GetVertices();
            var normals = fbxMesh.GetNormals();
            var indeces = fbxMesh.GetIndecesByMaterial();
            var uvs = fbxMesh.GetUVFromChannel(0);
            
            //fill mesh data
            var meshData = new Vertex3D[verts.Length];
            for (int i = 0; i < meshData.Length; i++)
            {
                uvs[i].Y = 1 - uvs[i].Y;
                meshData[i] = new Vertex3D(verts[i], normals[i], uvs[i]);
            }
            var indecesMerged = new List<int>();
            foreach (var elem in indeces)
                indecesMerged.AddRange(elem.Value);

            var mesh = new Mesh(meshData, indecesMerged.ToArray());
            var materials = new Material[indeces.Keys.Count];
            var matInds = indeces.Keys.ToArray();
            int offset = 0;
            for (int n = 0; n < materials.Length; n++)
            {
                var index = matInds[n];
                var mat = fbxMesh.GetMaterialAt(index);
                if (mat != null)
                {
                    var texture = mat.GetDiffuseTexture();
                    materials[n] = CreateMaterial(mat.Name, offset, indeces[index].Count, texture.GetName());
                    
                }
                else
                    materials[n] = CreateMaterial("empty", offset, indeces[index].Count, "");
                offset += indeces[index].Count;
            }

            var drawler = new MeshDrawer(mesh, materials);
            return drawler;
        }

        Material CreateMaterial(string Name, int offset, int count, string fileTexture, FbxDataMaterial mat = null) {
            Texture2D textureDiffuse = Texture2D.LoadEmpty();

            //fileTexture = WorkingDir + fileTexture;
            if (File.Exists(fileTexture))
            {
                textureDiffuse = ResourcesManager.LoadAsset<Texture2D>(fileTexture);
                
                //textureDiffuse.WrapModeU = Convert(mat.TextureDiffuse.WrapModeU);
                //textureDiffuse.WrapModeV = Convert(mat.TextureDiffuse.WrapModeV);
            }

            var shdrst = new ShaderSettings();
            var rddir = new RenderDirectives();
            shdrst.HasSkeleton = false;
            shdrst.RecieveShadow = true;
            shdrst.AffectedByLight = true;
            rddir.CastShadow = true;
            rddir.HasEdges = true;
            shdrst.TextureDiffuse = true;
            shdrst.DiscardInvisible = true;
            shdrst.Ambient = true;


            string path = "Toys.Resourses.shaders.";
            string vs = ShaderManager.ReadFromAssetStream(path + "genshin.vsh");
            string fs = ShaderManager.ReadFromAssetStream(path + "genshin.fsh");

            var mater = new MaterialCustom(shdrst, rddir, vs, fs);
            mater.Name = Name;

            if (mat != null)
            {
                mater.UniformManager.Set("ambient_color", ConvertColor(mat.Ambient));
                mater.UniformManager.Set("diffuse_color", ConvertColor(mat.Diffuse));
                mater.UniformManager.Set("specular_color", ConvertColor(mat.Specular));
                mater.UniformManager.Set("specular_power", mat.Shininess);
            }
            /*
            if (mat.HasColorAmbient)
                mater.UniformManager.Set("ambient_color", Tools.Convert(mat.ColorAmbient).Xyz);
            if (mat.HasColorDiffuse)
                mater.UniformManager.Set("diffuse_color", Tools.Convert(mat.ColorDiffuse));
            if (mat.HasColorSpecular)
            {
                mater.UniformManager.Set("specular_color", Tools.Convert(mat.ColorSpecular).Xyz);
                mater.UniformManager.Set("specular_power", Tools.Convert(mat.ColorSpecular).W);
            }
            */

            mater.SetTexture(textureDiffuse, TextureType.Diffuse);
            mater.Offset = offset;
            mater.Count = count;
            return mater;


            Vector3 ConvertColor(Color4 color)
            {
                return new Vector3(color.R, color.G, color.B);
            }
        }

        public void DrawSceneSimple()
        {
            foreach (var node in meshNodes)
            {
                var sNode = new SceneNode();
                sNode.Name = node.GetName();
                var mesh = ConvertMesh(node.GetFbxMesh());
                sNode.AddComponent(mesh);
                var trans = sNode.GetTransform;
                trans.Scale = node.GetScale;
                trans.Rotation = node.GetRotaion;
                trans.Position = node.GetTranslation;

                CoreEngine.MainScene.AddNode2Root(sNode);
            }
        }

        public void DrawScene(FbxDataScene scene)
        {
            //preapare root node
            SceneNode root = null;
            var rootList = CoreEngine.MainScene.FindByName("Root");
            if (rootList.Length == 0)
            {
                root = new SceneNode();
                root.Name = "Root";
                CoreEngine.MainScene.AddNode2Root(root);
            }
            else
            {
                //Clear root node from old model
                root = rootList[0];
                foreach (var child in root.GetChilds())
                {
                    root.RemoveChild(child);
                }
            }

            foreach (var node in scene.MeshList)
            {
                var sNode = new SceneNode();
                sNode.Name = node.Path;
                var mesh = ConvertMesh(node, scene);
                sNode.AddComponent(mesh);
                sNode.SetParent(root);
            }
        }


        MeshDrawer ConvertMesh(FbxDataMesh dataMesh, FbxDataScene scene)
        {
            var meshData = new Vertex3D[dataMesh.VertexList.Count];
            for (int i = 0; i < meshData.Length; i++)
            {
                var vert = dataMesh.VertexList[i];
                Vector2 uv = Vector2.Zero;
                if (dataMesh.HasUV[0])
                {
                    uv = vert.UV[0];
                    uv.Y = 1 - uv.Y;
                }
                meshData[i] = new Vertex3D(vert.Position, vert.Normal, uv);
            }
            var indecesMerged = new List<int>();
            foreach (var elem in dataMesh.SubmeshList)
                indecesMerged.AddRange(elem.Indeces);

            var mesh = new Mesh(meshData, indecesMerged.ToArray());
            var materials = new Material[dataMesh.SubmeshList.Count];
            int offset = 0;
            for (int n = 0; n < materials.Length; n++)
            {
                var subMesh = dataMesh.SubmeshList[n];
                var mat = scene.MaterialList.Find(m => m.Name == subMesh.Material);
                if (mat != null)
                {
                    var tex = mat.Textures.Find(t => t.Type == FbxTextureType.Diffuse);
                    string texturePath = "";
                    if (tex != null)
                        texturePath = tex.Name;
                    materials[n] = CreateMaterial(mat.Name, offset, subMesh.Indeces.Length, texturePath, mat);
                    var material = (MaterialCustom)materials[n];
                    material.UniformManager.Set("ambient_color", Color2Vec3(mat.Ambient));
                    material.UniformManager.Set("diffuse_color", Color2Vec3(mat.Diffuse));
                    material.UniformManager.Set("specular_color", Color2Vec3(mat.Specular));
                    material.UniformManager.Set("specular_power", mat.Shininess);
                }
                else
                    materials[n] = CreateMaterial("empty", offset, subMesh.Indeces.Length, "");
                offset += subMesh.Indeces.Length;
            }

            List<Morph> morphList = null;
            var morphesData = scene.MorphList.Find(ml => ml.Path == dataMesh.Path);
            if (morphesData != null)
            {
                morphList = new List<Morph>();
                foreach (var channel in morphesData.Channels)
                {
                    var keyframe = channel.KeyframeList[0];
                    var morph = new MorphVertex(channel.Name, channel.Name, keyframe.VertexList.Count, mesh.GetMorpher);
                    //Console.WriteLine($"{channel.Name} {keyframe.VertexList.Count}");
                    for(int i = 0; i < keyframe.VertexList.Count; i++)
                    {
                        var vert = keyframe.VertexList[i];

                        var vertPos = dataMesh.VertexList[(int)vert.Index];
                        morph.AddVertex(vert.Vertex.Position - vertPos.Position, (int)vert.Index);

                        //if (channel.Name == "Mouth_Open01")
                        //    Console.WriteLine($"{vert.Vertex.Position - vertPos.Position} {(int)vert.Index}");
                    }
                        morphList.Add(morph);
                }
                
            }


            var drawler = new MeshDrawer(mesh, materials, morphList);
            return drawler;


            Vector3 Color2Vec3(Color4 color)
            {
                return new Vector3(color.R, color.G, color.B);
            }
        }

        MeshDrawerRigged ConvertMeshRigged(FbxDataMesh dataMesh, FbxDataScene scene, BoneController bController)
        {
            var meshData = new VertexRigged3D[dataMesh.VertexList.Count];
            for (int i = 0; i < meshData.Length; i++)
            {
                var vert = dataMesh.VertexList[i];
                Vector2 uv = Vector2.Zero;
                if (dataMesh.HasUV[0])
                {
                    uv = vert.UV[0];
                    uv.Y = 1 - uv.Y;
                }
                var weight = Vector4.Zero;
                for (int j = 0; j < vert.Weights.Length; j++)
                {
                    weight[j] = vert.Weights[j];
                }
                var boneindeces = Vector4i.Zero;
                for (int j = 0; j < vert.BoneIndices.Length; j++)
                {
                    boneindeces[j] = vert.BoneIndices[j];
                } 
                meshData[i] = new VertexRigged3D(vert.Position, vert.Normal, uv, boneindeces, weight);
            }
            var indecesMerged = new List<int>();
            foreach (var elem in dataMesh.SubmeshList)
                indecesMerged.AddRange(elem.Indeces);

            var mesh = new Mesh(meshData, indecesMerged.ToArray());
            var materials = new Material[dataMesh.SubmeshList.Count];
            int offset = 0;
            for (int n = 0; n < materials.Length; n++)
            {
                var subMesh = dataMesh.SubmeshList[n];
                var mat = scene.MaterialList.Find(m => m.Name == subMesh.Material);
                if (mat != null)
                {
                    var tex = mat.Textures.Find(t => t.Type == FbxTextureType.Diffuse);
                    string texturePath = "";
                    if (tex != null)
                        texturePath = tex.Name;
                    materials[n] = CreateMaterial(mat.Name, offset, subMesh.Indeces.Length, texturePath, mat);
                    var material = (MaterialCustom)materials[n];
                    material.UniformManager.Set("ambient_color", Color2Vec3(mat.Ambient));
                    material.UniformManager.Set("diffuse_color", Color2Vec3(mat.Diffuse));
                    material.UniformManager.Set("specular_color", Color2Vec3(mat.Specular));
                    material.UniformManager.Set("specular_power", mat.Shininess);
                }
                else
                    materials[n] = CreateMaterial("empty", offset, subMesh.Indeces.Length, "");
                offset += subMesh.Indeces.Length;
            }

            List<Morph> morphList = null;
            var morphesData = scene.MorphList.Find(ml => ml.Path == dataMesh.Path);
            if (morphesData != null)
            {
                morphList = new List<Morph>();
                foreach (var channel in morphesData.Channels)
                {
                    var keyframe = channel.KeyframeList[0];
                    var morph = new MorphVertex(channel.Name, channel.Name, keyframe.VertexList.Count, mesh.GetMorpher);
                    //Console.WriteLine($"{channel.Name} {keyframe.VertexList.Count}");
                    for (int i = 0; i < keyframe.VertexList.Count; i++)
                    {
                        var vert = keyframe.VertexList[i];

                        var vertPos = dataMesh.VertexList[(int)vert.Index];
                        morph.AddVertex(vert.Vertex.Position - vertPos.Position, (int)vert.Index);

                        //if (channel.Name == "Mouth_Open01")
                        //    Console.WriteLine($"{vert.Vertex.Position - vertPos.Position} {(int)vert.Index}");
                    }
                    morphList.Add(morph);
                }

            }

            

            var drawler = new MeshDrawerRigged(mesh, materials, bController, morphList);
            return drawler;


            Vector3 Color2Vec3(Color4 color)
            {
                return new Vector3(color.R, color.G, color.B);
            }
        }

        BoneController CreateBoneController(FbxDataScene scene)
        {
            var bones = new List<Bone>();
            for (int i = 0; i < scene.BonesOrdered.Count; i++)
            {
                if (!boneMap.TryGetValue(scene.BonesOrdered[i], out var boneNode))
                    continue;
                var parIndex = bonesSorted.IndexOf(boneNode.GetParent());

                Matrix4 boneLocal2Parent = new Matrix4();
                if (parIndex >= 0)
                    boneLocal2Parent = scene.BonesMatrices[i] * scene.BonesMatrices[parIndex].Inverted();
                else
                    boneLocal2Parent = scene.BonesMatrices[i];
                var bone = new Bone(boneNode.GetName(), boneLocal2Parent, parIndex);
                bones.Add(bone);
                
            }
            return new BoneController(bones.ToArray(), true);
        }

        public List<SceneNode> GetSceneNodes()
        {
            var bController = CreateBoneController(dataScene);
            var nodes = new List<SceneNode>();
            foreach (var node in meshNodes)
            { 
                var sNode = new SceneNode();
                sNode.Name = node.GetName();
                sNode.AddComponent(new Animator(bController));
                var path = GetNodePath(node);
                var mesh = ConvertMeshRigged(dataScene.MeshList.FirstOrDefault(m => m.Path == path), dataScene, bController);
                sNode.AddComponent(mesh);
                var trans = sNode.GetTransform;
                trans.Scale = node.GetScale;
                trans.Rotation = node.GetRotaion;
                trans.Position = node.GetTranslation;

                nodes.Add(sNode);
            }

            return nodes;
        }

        internal FbxNode[] GetMeshesNodes()
        {
            return nodes.ToArray();
        }

        internal FbxNode[] BonesOrdered
        {
           get { return bonesSorted.ToArray(); }
        }

        internal void ExportFile(string fileName)
        {
            _context.ExportToFile(fileName);
        }


        public FbxDataScene ReadImportedScene()
        {
            dataScene = new FbxDataScene();
            var root = _context.GetRootNode();
            dataScene.RootNode = FillNodeData(root);

            //reduce root node
            var rootNode = dataScene.RootNode;
            dataScene.RootNode = rootNode.GetFirstChild;
            rootNode.Remove(dataScene.RootNode);

            dataScene.MeshList = new List<FbxDataMesh>();
            dataScene.MaterialList = new List<FbxDataMaterial>();
            dataScene.TextureList = new List<FbxDataTexture>();
            dataScene.MorphList = new List<FbxDataMorph>();
            dataScene.BonesOrdered = new List<string>();
            foreach (var meshNode in meshNodes)
            {
                var fbxMesh = meshNode.GetFbxMesh();
                var path = GetNodePath(meshNode);
                dataScene.MeshList.Add(ReadMesh(fbxMesh, path));
                dataScene.MaterialList.AddRange(GetMaterials(fbxMesh));
                dataScene.MorphList.AddRange(GetMorphs(fbxMesh, path));
            }

            //read scene bone list
            foreach (var boneNode in BonesOrdered)
                dataScene.BonesOrdered.Add(GetNodePath(boneNode));

            dataScene.BonesMatrices = new List<Matrix4>();
            foreach (var boneMat in bonesMatrices)
                dataScene.BonesMatrices.Add(boneMat);

            return dataScene;
        }

        private List<FbxDataMorph> GetMorphs(FbxMesh fbxMesh, string path)
        {
            var morphList = new List<FbxDataMorph>();
            if (fbxMesh.GetBlendShapeDeformerCount() < 1)
                return morphList;
            var morph = new FbxDataMorph();
            morph.Path = path;
            morph.Channels = new List<FbxDataMorphChannel>();
            var deformer = fbxMesh.GetBlendShapeDeformer(0);
            for (int i = 0; i < deformer.GetChannelCount(); i++)
            {
                var channel = new FbxDataMorphChannel();
                var fbxChannel = deformer.GetChannelAt(i);
                channel.Name = fbxChannel.GetName();
                channel.KeyframeList = new List<FbxDataMorphKeyframe>();
                for (int k = 0; k < fbxChannel.GetShapeCount(); k++)
                {
                    var frame = new FbxDataMorphKeyframe();
                    frame.VertexList = new List<FbxDataMorphVertex>();
                    var shape = fbxChannel.GetShapeAt(k);
                    
                    var tmp = shape.GetVertices();

                    for (int n = 0; n < tmp.Item1.Length; n++)
                    {
                        var vert = new FbxDataMorphVertex();
                        vert.Index = (uint)tmp.Item1[n];
                        vert.Vertex = new FbxDataVertex();
                        vert.Vertex.Position = tmp.Item2[vert.Index].Xyz;
                        frame.VertexList.Add(vert);
                    }
                    channel.KeyframeList.Add(frame);
                }
                morph.Channels.Add(channel);
            }
            morphList.Add(morph);

            return morphList;
        }

        private List<FbxDataMaterial> GetMaterials(FbxMesh fbxMesh)
        {
            var matList = new List<FbxDataMaterial>();
            for(int i = 0; i < fbxMesh.GetMaterialCount(); i++)
            {
                var mat = new FbxDataMaterial();
                var fbxMat = fbxMesh.GetMaterialAt(i);
                mat.Name = fbxMat.Name;
                mat.Transparency = fbxMat.Transparency;
                mat.Shininess = fbxMat.Shininess;
                mat.Ambient = Fbx.VectorToColor(fbxMat.Ambient);
                mat.Diffuse = Fbx.VectorToColor(fbxMat.Diffuse);
                mat.Emissive = Fbx.VectorToColor(fbxMat.Emissive);
                mat.Reflection = Fbx.VectorToColor(fbxMat.Reflect);
                mat.Specular = Fbx.VectorToColor(fbxMat.Specular);

                mat.Textures = new List<FbxDataTexture>();
                var fbxTexture = fbxMat.GetDiffuseTexture();
                if (fbxTexture != null)
                {
                    var path = fbxTexture.GetName();
                    var texture = dataScene.TextureList.Find(t => t.Name == path);
                    if (texture == null)
                    {
                        texture = new FbxDataTexture();
                        texture.Name = WorkingDir + path;
                        texture.Type = FbxTextureType.Diffuse;
                        texture.Offset = fbxTexture.Offset;
                        texture.Scale = fbxTexture.Scale;
                        dataScene.TextureList.Add(texture);
                    }
                    mat.Textures.Add(texture);
                }
                matList.Add(mat);
            }

            return matList;
        }

        private FbxDataNode FillNodeData(FbxNode node)
        {
            var resNode = new FbxDataNode();
            resNode.Name = node.GetName();
            resNode.LocalPosition = node.GetTranslation;
            resNode.LocalRotation = node.GetRotaion;
            resNode.LocalScale = node.GetScale;
            //extra node patrametres 

            switch (node.GetNodeType())
            {
                case FbxNode.FbxNodeType.MESH:
                    //ReadMesh(node, resNode.Path);
                    break;
                case FbxNode.FbxNodeType.SKELETON:
                    break;
                default:
                    break;

            }

            for (int i = 0; i < node.GetChildCount(); i++)
            {
                var childNode = FillNodeData(node.GetChildAtID(i));
                resNode.AddChild(childNode);
            }

            return resNode;
        }

        FbxDataMesh ReadMesh(FbxMesh fbxMesh, string path)
        {
            var mesh = new FbxDataMesh();
            mesh.Path = path;
            mesh.VertexList = new List<FbxDataVertex>();
            var verts = fbxMesh.GetVertices();
            var normals = fbxMesh.GetNormals();
            var indeces = fbxMesh.GetIndecesByMaterial();
            var colors = fbxMesh.GetColors();
            var tangents = fbxMesh.GetTangents();
            var uvs = fbxMesh.GetUVFromChannel(0);
            
            var boneWeigths = new Vector4[verts.Length];
            var boneIndeces = new Vector4i[verts.Length];
            var hasBones = fbxMesh.GetSkinnerCount() > 0;
            if (hasBones)
            {
                mesh.BoneList = ParseSkinner(fbxMesh.GetSkinnerAt(0), boneWeigths, boneIndeces);
            }
            //fill mesh data

            if (normals.Length == verts.Length)
                mesh.HasNormal = true;
            if (uvs.Length == verts.Length)
            {
                mesh.HasUV = [true];
                mesh.UVType = [0];
            }
            else
                mesh.HasUV = [false];
            if (tangents.Length == verts.Length)
                mesh.HasTangent = true;
            if (colors.Length == verts.Length)
                mesh.HasColor = true;

            
            for (int i = 0; i < verts.Length; i++)
            {
                var vertData = new FbxDataVertex();
                vertData.Position = verts[i];
                if (mesh.HasNormal)
                    vertData.Normal = normals[i];
                if (mesh.HasUV[0])
                    vertData.UV = new Vector2[] { uvs[i] };
                if (mesh.HasTangent)
                    vertData.Tangent = tangents[i];
                if (mesh.HasColor)
                    vertData.Color = colors[i];
                if (hasBones)
                {
                    vertData.Weights = new[] { boneWeigths[i][0], boneWeigths[i][1], boneWeigths[i][2], boneWeigths[i][3] };
                    vertData.BoneIndices = [boneIndeces[i][0], boneIndeces[i][1], boneIndeces[i][2], boneIndeces[i][3]];
                }
                mesh.VertexList.Add(vertData);
            }

            //Fill indeces
            mesh.SubmeshList = new List<FbxDataSubMesh>();
            foreach (var elem in indeces) {
                var subMesh = new FbxDataSubMesh();
                var mat = fbxMesh.GetMaterialAt(elem.Key);
                if (mat != null)
                    subMesh.Material = mat.Name;
                subMesh.Indeces = elem.Value.ToArray();
                mesh.SubmeshList.Add(subMesh);
            }
            
            return mesh;
        }


        List<FbxDataBone> ParseSkinner(FbxSkin skinner, Vector4[] boneWeigth,  Vector4i[] boneIndeces)
        {
            var boneNodeList = new List<FbxNode>();
            var boneList = new List<FbxDataBone>();
            //initialize array
            var IndxCount = new int[boneWeigth.Length];
            for (int n = 0; n < boneWeigth.Length; n++) {
                boneWeigth[n] = new Vector4();
                boneIndeces[n] = new Vector4i();
                IndxCount[n] = 0;
            }
            for (int i = 0; i < skinner.GetClusterCount(); i++)
            {
                var cluster = skinner.GetClusterAt(i);
                var bone = cluster.GetClusterBone();
                var boneIndex = bonesSorted.IndexOf(bone);
                if (boneIndex > -1)
                {
                    var map = cluster.GetVertWeigthMapping();

                    for (int m = 0; m < map.Item1.Length; m++)
                    {
                        var vertId = map.Item1[m];
                        var carriage = IndxCount[vertId]++;
                        if (carriage > 3)
                        {
                            Logger.Error("Vertex bones overflow");
                            continue;
                        }
                        boneWeigth[vertId][carriage] = map.Item2[m];
                        boneIndeces[vertId][carriage] = i;
                    }


                    var boneData = new FbxDataBone();
                    boneData.Path = GetNodePath(bone);
                    boneData.Matrix = cluster.GetClusterLinkMatrix();

                    //Console.WriteLine(boneData.Path);
                    //Console.WriteLine(boneData.Matrix);
                    boneList.Add(boneData);
                }
            }

            return boneList;
        }

        string GetNodePath(FbxNode node)
        {
            var frame = node;
            var path = frame.GetName();
            while (frame.GetParent() != null)
            {
                frame = frame.GetParent();
                path = frame.GetName() + "/" + path;
            }
            //Reduce root node in path
            path = path.Remove(0, path.IndexOf('/') + 1);
            return path;
        }

        public void ExportScene(FbxDataScene dataScene, string filePath)
        {
            var contextBackup = _context;
            var exportBackup = _exportOptions;
            var exportOptions = new Fbx.ExportOptions();
            _context = new FbxWrapperContext(exportOptions);
            _context.Initialize(true);

            exportOptions.exportSkins = true;
            exportOptions.exportBlendShape = true;
            exportOptions.exportAllNodes = true;
            exportOptions.castToBone = false;
            exportOptions.scaleFactor = 100;
            _exportOptions = exportOptions;

            CopyTextures(dataScene, filePath);

            var meshFrames = new List<FbxDataNode>();

            ExportRootFrame(meshFrames, dataScene);

            
            if (dataScene.MeshList != null)
            {
                SetJointsFromImportedMeshes(dataScene);
                
                PrepareMaterials(dataScene);
                
                ExportMeshFrames(dataScene.RootNode, meshFrames, dataScene);
            }
            else
            {
                SetJointsNode(dataScene.RootNode, null, true);
            }

            

            if (_exportOptions.exportBlendShape)
            {
                ExportMorphs(dataScene);
            }

            if (_exportOptions.exportAnimations)
            {
                ExportAnimations(dataScene);
            }

            //Tests.TraverceScene(_context.GetRootNode(), "");
            _context.ExportToFile(filePath);
            _context.Dispose();

            _exportOptions = exportBackup;
            _context = contextBackup;
        }

        void CopyTextures(FbxDataScene scene, string path)
        {
            var directory = Path.GetFullPath(path);
            foreach (var texture in scene.TextureList)
            {
                var oldPath = texture.Name;
                var newPath = directory + Path.GetFileName(oldPath);
                if (!File.Exists(newPath) && File.Exists(oldPath))
                {
                    File.Copy(oldPath, newPath);
                }
                texture.Name = newPath;
            }
        }

        /*
        internal void Initialize()
        {
            var is60Fps = _imported.AnimationList.Count > 0 && _imported.AnimationList[0].SampleRate.Equals(60.0f);

            _context.Initialize(_fileName, is60Fps);

            if (!_exportOptions.exportAllNodes)
            {
                var framePaths = SearchHierarchy();

                _context.SetFramePaths(framePaths);
            }
        }
        */


        private void ExportMorphs(FbxDataScene scene)
        {
            _context.ExportMorphs(scene.RootNode, scene.MorphList);
        }

        private void ExportAnimations(FbxDataScene scene)
        {
            _context.ExportAnimations(scene.RootNode, scene.AnimationList);
        }

        private void ExportRootFrame(List<FbxDataNode> meshFrames, FbxDataScene scene)
        {
            _context.ExportFrame(scene.MeshList, meshFrames, scene.RootNode);
        }

        private void SetJointsFromImportedMeshes(FbxDataScene scene)
        {
            if (!_exportOptions.exportSkins)
            {
                return;
            }

            Debug.Assert(scene.MeshList != null);

            var bonePaths = new HashSet<string>();

            foreach (var mesh in scene.MeshList)
            {
                var boneList = mesh.BoneList;

                if (boneList != null)
                {
                    foreach (var bone in boneList)
                    {
                        bonePaths.Add(bone.Path);
                    }
                }
            }
            SetJointsNode(scene.RootNode, bonePaths, _exportOptions.castToBone);
        }

        private void SetJointsNode(FbxDataNode rootFrame, HashSet<string> bonePaths, bool castToBone)
        {
            _context.SetJointsNode(rootFrame, bonePaths, castToBone);
        }

        private void PrepareMaterials(FbxDataScene scene)
        {
            _context.PrepareMaterials(scene.MaterialList.Count, scene.TextureList.Count);
        }

        private void ExportMeshFrames(FbxDataNode rootFrame, List<FbxDataNode> meshFrames, FbxDataScene scene)
        {
            foreach (var meshFrame in meshFrames)
            {
                _context.ExportMeshFromFrame(rootFrame, meshFrame, scene.MeshList, scene.MaterialList, scene.TextureList);
            }
        }

        private HashSet<string> SearchHierarchy(FbxDataScene scene)
        {
            if (scene.MeshList == null || scene.MeshList.Count == 0)
            {
                return null;
            }

            var exportFrames = new HashSet<string>();

            SearchHierarchy(scene.RootNode, scene.MeshList, exportFrames);

            return exportFrames;
        }

        private static void SearchHierarchy(FbxDataNode rootFrame, List<FbxDataMesh> meshList, HashSet<string> exportFrames)
        {
            var frameStack = new Stack<FbxDataNode>();

            frameStack.Push(rootFrame);

            while (frameStack.Count > 0)
            {
                var frame = frameStack.Pop();

                var meshListSome = FbxDataHelpers.FindMesh(frame.Path, meshList);

                if (meshListSome != null)
                {
                    var parent = frame;

                    while (parent != null)
                    {
                        exportFrames.Add(parent.Path);
                        parent = parent.Parent;
                    }

                    var boneList = meshListSome.BoneList;

                    if (boneList != null)
                    {
                        foreach (var bone in boneList)
                        {
                            if (!exportFrames.Contains(bone.Path))
                            {
                                var boneParent = rootFrame.FindFrameByPath(bone.Path);

                                while (boneParent != null)
                                {
                                    exportFrames.Add(boneParent.Path);
                                    boneParent = boneParent.Parent;
                                }
                            }
                        }
                    }
                }

                for (var i = frame.Count - 1; i >= 0; i -= 1)
                {
                    frameStack.Push(frame[i]);
                }
            }
        }
    }
}
