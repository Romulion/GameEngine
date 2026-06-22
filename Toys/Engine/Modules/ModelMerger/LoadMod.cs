using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FbxTools;
using OpenTK.Mathematics;

namespace Toys.Mod3DMigotoReconstructor
{
    public class LoadMod
    {
        const string DefaultMeshName = "Body";
        //Mod to model material reference table according to Genshin naming
        Dictionary<string, string>  matTable = new Dictionary<string, string>
            {
                 { "Head", "Mat_Hair"},
                 { "Body", "Mat_Body" },
                 { "Face", "Mat_Face" },
                 { "Dress","Mat_Dress" }
                 //{"Extra", "" }
            };

        string MeshName;
        
        VertexRigged3D[] verts;
        List<Vector3> points = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector4> tangents = new List<Vector4>();
        List<Vector2> uvs = new List<Vector2>();
        List<Vector2> uvs2 = new List<Vector2>();
        List<Vector4> BoneWeigth = new List<Vector4>();
        List<Vector4i> BoneIndices = new List<Vector4i>();

        string _modDir;
        List<int> indexes = new List<int>();
        ModelDataStruct _modModelData;
        SceneNode[] nodes;

        public LoadMod(string iniFile)
        {            
            _modDir = Path.GetDirectoryName(iniFile) + @"\";
            ParceIni(iniFile);
            
            MeshName = DefaultMeshName;
            nodes = CoreEngine.MainScene.GetRootNodes();
        }

        public ModelDataStruct GetParcedIni()
        {
            return _modModelData;
        }

        public void DrawMod(bool splitSubmesh)
        {
            var meshNode = nodes.First((node) => node.Name == MeshName);
            var mesh = meshNode.GetComponent<MeshDrawer>();
            meshNode.RemoveComponent(mesh);
            var materials = mesh.Materials;

            //clean old mesh
            ResourcesManager.DeleteResource(mesh);

            //create modded mesh
            var vertices = new Vertex3D[verts.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = (Vertex3D)verts[i];
            }
            var meshModded = new Mesh(vertices, indexes.ToArray());

            //Update Materials
            var matModded = new List<Material>();
            foreach(var matMod in _modModelData.materials)
            {
                var material = materials.Find((mat) => mat.Name.EndsWith(matTable[matMod.Name]));

                material.Count = matMod.Count;
                material.Offset = matMod.Offset;
                if (matMod.TextureDiffuse != "")
                {
                    var texture = ResourcesManager.LoadAsset<Texture2D>(_modDir + matMod.TextureDiffuse);
                    material.SetTexture(texture, TextureType.Diffuse);
                }
                matModded.Add(material);
            }

            var meshDrawlr = new MeshDrawer(meshModded, matModded.ToArray());
            meshNode.AddComponent(meshDrawlr);

        }

        public void LoadMod2Memory()
        {
            foreach (var mat in _modModelData.materials)
            {
                if (mat.IndexBufferFile != "")
                {
                    mat.Offset = indexes.Count;
                    ReadIb(_modDir + mat.IndexBufferFile, "DXGI_FORMAT_R32_UINT");
                    mat.Count = indexes.Count - mat.Offset;
                }
            }
            ReadPosition(_modDir + _modModelData.PositionPath, 40);
            ReadBlend(_modDir + _modModelData.BlendPath, 32);
            ReadTextures(_modDir + _modModelData.TexcoordPath, _modModelData.TexCordStride);


            //convert left to right coordinates
            for (int i = 0; i < indexes.Count; i += 3)
            {
                int temp = indexes[i];
                indexes[i] = indexes[i + 2];
                indexes[i + 2] = temp;
            }
        }

        public void ExportMod(FbxDataScene scene)
        {
            //change textures
            foreach (var matMod in _modModelData.materials)
            {
                var material = FindFbxMaterial(matMod.Name, scene.MaterialList);
                if (material != null)
                {
                    var texture = material.Textures.Find(t => t.Type == FbxTextureType.Diffuse);
                    if (texture != null)
                    {
                        texture.Name = _modDir + matMod.TextureDiffuse;
                    }
                }
            }

            //update mesh
            var meshData = scene.MeshList.Find(node => node.Path.EndsWith(MeshName));
            if (meshData == null)
            {
                Logger.Error($"Exporting error. Mesh node {MeshName} not found in fbx file");
                return;
            }
            //Convert mesh data for exporting
            meshData.HasNormal = true;
            meshData.HasTangent = true;
            meshData.HasUV = new [] { true };
            meshData.UVType = new [] { 0 };
            meshData.HasColor = false;

            meshData.VertexList = new List<FbxDataVertex>();
            for (int i = 0; i < points.Count; i++)
            {
                var vert = new FbxDataVertex();
                vert.Position = points[i];
                vert.Normal = normals[i];
                vert.Tangent = tangents[i];
                vert.UV = new[] { uvs[i]};
                vert.Weights = new[] { BoneWeigth[i][0], BoneWeigth[i][1], BoneWeigth[i][2], BoneWeigth[i][3] };
                vert.BoneIndices = new [] { BoneIndices[i][0], BoneIndices[i][1], BoneIndices[i][2], BoneIndices[i][3] };
                meshData.VertexList.Add(vert);
            }

            //Add submesh and materials
            meshData.SubmeshList = new List<FbxDataSubMesh>();
            foreach (var modMat in _modModelData.materials)
            {
                var matFbx = FindFbxMaterial(modMat.Name, scene.MaterialList);
                if (matFbx != null)
                {
                    if (modMat.MeshSubParts.Count > 0)
                    {
                        foreach (var subPart in modMat.MeshSubParts)
                        {
                            var fdsm = new FbxDataSubMesh();
                            fdsm.Material = matFbx.Name;
                            fdsm.Indeces = new int[subPart.Count];
                            indexes.CopyTo(subPart.Offset, fdsm.Indeces, 0, fdsm.Indeces.Length);
                            meshData.SubmeshList.Add(fdsm);

                        }
                    }
                    else
                    {
                        var fdsm = new FbxDataSubMesh();
                        fdsm.Material = matFbx.Name;
                        fdsm.Indeces = new int[modMat.Count];
                        indexes.CopyTo(modMat.Offset, fdsm.Indeces, 0, modMat.Count);
                        meshData.SubmeshList.Add(fdsm);
                    }
                }
            }
            //add only used in skin bones
            //meshData.BoneList = new List<FbxDataBone>();
            var bones = scene.BonesOrdered;
            var bonesIdxs = new List<int>();
            foreach (var boneidV4 in BoneIndices)
            {
                for (int k = 0; k < 4; k++)
                    if (!bonesIdxs.Contains(boneidV4[k]))
                        bonesIdxs.Add(boneidV4[k]);
  
            }
            bonesIdxs.Sort();
            foreach (var indx in bonesIdxs)
                AddBone(indx);

            
            void AddBone(int id)
            {
                if (id >= bones.Count)
                {
                    Console.WriteLine($" {id}");
                    return;
                }

                return;
                var bone = bones[id];
                
                var node = meshData.BoneList.Find(b => b.Path == bone);
                if (node == null)
                {
                    var boneData = new FbxDataBone();
                    boneData.Path = bone;
                    boneData.Matrix = scene.BonesMatrices[id];
                    meshData.BoneList.Add(boneData);
                }
            }
        }

        FbxDataMaterial FindFbxMaterial(string name, List<FbxDataMaterial> materials)
        {
            FbxDataMaterial material = null;
            if (matTable.ContainsKey(name))
            {
                material = materials.Find((mat) => mat.Name.EndsWith(matTable[name]));
            }
            else
            {
                Logger.Info("Unknown material: " + name);
            }
            return material;
        }
        void ParceIni(string iniFile)
        {
            if (Path.GetExtension(iniFile) != ".ini")
                throw new Exception("wrong file");

            _modDir = Path.GetDirectoryName(iniFile) + @"\";

            var parcer = new ModConfigParcer(iniFile);
            _modModelData = parcer._model;
        }

        void ReadIb(string file, string type)
        {
            int offset = indexes.Count;
            //if (type == "DXGI_FORMAT_R32_UINT")
            var read = new Reader(File.OpenRead(file));
            while (read.BaseStream.Position < read.BaseStream.Length)
            {
                indexes.Add(read.ReadInt32());
            }

            if ((indexes.Count - offset) % 3 != 0)
                Logger.Warning($"Index buffer not by 3 in {file}");
        }

        void ReadPosition(string file, int stride)
        {
            
            var read = new Reader(File.OpenRead(file));
            var count = read.BaseStream.Length / stride;
            verts = new VertexRigged3D[count];

            if (read.BaseStream.Length % stride != 0)
            {
                Logger.Warning($"File {file} mot match byte stride {stride}");
            }

            for (int i = 0; i < count; i++)
            {
                var pos = read.readVector3();
                var norm = read.readVector3();
                var tang = read.readVector4();

                //convert left to right coordinates
                
                pos.X = - pos.X;
                norm.X = - norm.X;
                tang.X = - tang.X;
                /*
                pos.Z = -pos.Z;
                norm.Z = -norm.Z;
                tang.Z = -tang.Z;
                */
                verts[i] = new VertexRigged3D();
                verts[i].Position = pos;
              
                verts[i].Normal = norm;

                points.Add(pos);
                normals.Add(norm);
                tangents.Add(tang);
                //verts[i].Normal.X = -norm.X;
            }
        }

        void ReadTextures(string file, int stride)
        {
            var read = new Reader(File.OpenRead(file));
            var count = read.BaseStream.Length / stride;

            if (read.BaseStream.Length % stride != 0)
            {
                Logger.Warning($"File {file} mot match byte stride {stride}");
            }

            for (int i = 0; i < count; i++)
            {
                verts[i].UV = read.readVector3().Yz;
                uvs.Add(verts[i].UV);
                if (stride == 20)
                {
                    var temp = read.readVector2();
                    uvs2.Add(temp);
                }
            }
        }
        void ReadBlend(string file, int stride)
        {
            var read = new Reader(File.OpenRead(file));
            var count = read.BaseStream.Length / stride;

            if (read.BaseStream.Length % stride != 0)
            {
                Logger.Warning($"File {file} mot match byte stride {stride}");
            }

            for (int i = 0; i < count; i++)
            {
                verts[i].BoneWeigths = read.readVector4();
                verts[i].BoneIndices = new Vector4i(read.ReadInt32(), read.ReadInt32(), read.ReadInt32(), read.ReadInt32());

                BoneWeigth.Add(verts[i].BoneWeigths);
                BoneIndices.Add(verts[i].BoneIndices);
            }
        }
    }
}
