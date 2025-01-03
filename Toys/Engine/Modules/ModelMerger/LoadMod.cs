using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IniParser;
using OpenTK.Core.Native;
using static System.Net.Mime.MediaTypeNames;
using Assimp;
using OpenTK.Mathematics;
using IniParser.Model;
using System.Numerics;
using Assimp.Unmanaged;
using NAudio.Midi;
using System.Text.RegularExpressions;

namespace Toys.Mod3DMigotoReconstructor
{
    public class LoadMod
    {
        //Mod to model material reference table according to Genshin naming
        Dictionary<string, string>  matTable = new Dictionary<string, string>
            {
                 { "Head", "Mat_Hair"},
                 { "Body", "Mat_Body" },
                 { "Face", "Mat_Face" },
                 //{"Dress", "" }
                 //{"Extra", "" }
            };

        VertexRigged3D[] verts;
        List<OpenTK.Mathematics.Vector3> points = new List<OpenTK.Mathematics.Vector3>();
        List<OpenTK.Mathematics.Vector3> normals = new List<OpenTK.Mathematics.Vector3>();
        List<OpenTK.Mathematics.Vector4> tangents = new List<OpenTK.Mathematics.Vector4>();
        List<OpenTK.Mathematics.Vector2> uvs = new List<OpenTK.Mathematics.Vector2>();
        List<OpenTK.Mathematics.Vector2> uvs2 = new List<OpenTK.Mathematics.Vector2>();
        List<OpenTK.Mathematics.Vector4> BoneWeigth = new List<OpenTK.Mathematics.Vector4>();
        List<IVector4> BoneIndices = new List<IVector4>();
        AssimpTools Tools = new AssimpTools();
        Assimp.Scene scene;
        string _modDir;
        string _workDir;
        Mesh mesh;
        List<int> indexes = new List<int>();
        List<Material> materials =  new List<Material>();
        List<Node> nodes = new List<Node>();
        ModelDataStruct _modModelData;


        public LoadMod(string Inifile, string workDir)
        {
            LoadIni(Inifile);
            _workDir = workDir;

            AssimpContext importer = new AssimpContext();
            scene = importer.ImportFile(@"D:\HomeShare\Tests\NPC_Avatar_Loli_Bow_Sigewinne\NPC_Avatar_Loli_Bow_Sigewinne.fbx", PostProcessSteps.ValidateDataStructure);
            UpdateTexturePath();
            LoadModModel();
            //mesh = new Mesh(verts, indexes.ToArray());
            MergeModels(scene);

            //ModelExport(scene);

            //TestSaver();
            //var exporter = new AssimpContext();
            // exporter.ExportFile(scene, @"D:\HomeShare\Tests\Output\Test.fbx", "fbx");
        }

        void LoadIni(string iniFile)
        {
            if (Path.GetExtension(iniFile) != ".ini")
                throw new Exception("wrong file");

            _modDir = Path.GetDirectoryName(iniFile) + @"\";

            var parcer = new ModConfigParcer(iniFile);
            _modModelData = parcer._model;
        }

        void LoadModModel()
        {
            foreach(var mat in _modModelData.materials)
            {
                if (mat.IndexBufferFile != "")
                {
                    mat.Offset = indexes.Count;
                    ReadIb(_modDir + mat.IndexBufferFile, "DXGI_FORMAT_R32_UINT");
                }
                ReadMaterials(_modDir + mat.TextureDiffuse, mat.Offset);
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

        void ModelExport(Assimp.Scene scene)
        {

            //Assimp exporter crash if blend shape exists
            foreach (var mesh in scene.Meshes)
            {
                mesh.MeshAnimationAttachments.Clear();
            }
            
            AssimpContext exporter = new AssimpContext();
            exporter.ExportFile(scene, @"D:\HomeShare\Tests\Test.fbx", "fbx");
        }

        void TraverseScene(Assimp.Node node)
        {
            nodes.Add(node);
            if (node.HasChildren)
            {
                foreach (var child in node.Children)
                {
                    TraverseScene(child);
                }
            }

        }

        void TestSaver()
        {
            Assimp.Scene s = new Assimp.Scene();
            s.RootNode = new Node();
            var assimpMaterial = new Assimp.Material();
            assimpMaterial.Name = "Test";
            s.Materials.Add(assimpMaterial);
            Assimp.Mesh meshNew = new Assimp.Mesh("MeshTest", Assimp.PrimitiveType.Triangle);

            //verts triangle
            var points = new System.Numerics.Vector3[] {
                new System.Numerics.Vector3(0.5f, 0, 0),
                new System.Numerics.Vector3(0, 1, 0),
                new System.Numerics.Vector3(-0.5f, 0, 0),
            };
            meshNew.Vertices.AddRange(points);

            meshNew.Faces.Add(new Face(new int[] { 0, 1, 2 }));

            meshNew.MaterialIndex = 0;

            var morph = new MeshAnimationAttachment();
            morph.Name = "Test morph";
            var morphPoints = new System.Numerics.Vector3[] {
                new System.Numerics.Vector3(0.5f, 1, 0),
                new System.Numerics.Vector3(0, 1, 0),
                new System.Numerics.Vector3(-0.5f, 1, 0),
            };
            morph.Vertices.AddRange(morphPoints);
            //meshNew.MeshAnimationAttachments.Add(morph);

            s.Meshes.Add(meshNew);

            Node nbase = new Node();
            nbase.Name = "Test Node";
            nbase.MeshIndices.Add(0);
            s.RootNode.Children.Add(nbase);

            AssimpContext exporter = new AssimpContext();
            exporter.ExportFile(s, @"D:\HomeShare\Tests\Test.fbx", "fbx", PostProcessSteps.FindInvalidData);
        }

        void MergeModels(Assimp.Scene scene)
        {


            foreach(var mat in _modModelData.materials)
            {
                if (matTable.ContainsKey(mat.Name))
                {
                    var material = scene.Materials.Find((matA) => matA.Name.EndsWith(matTable[mat.Name]));
                    if (mat.TextureDiffuse != "")
                    {
                        var tslot = material.TextureDiffuse;
                        tslot.FilePath = _modDir + mat.TextureDiffuse;
                        material.TextureDiffuse = tslot;
                    }
                    if (mat.TextureNormal != "")
                    {
                        var tslot = material.TextureNormal;
                        tslot.FilePath = _modDir + mat.TextureNormal;
                        material.TextureNormal = tslot;
                    }
                    if (mat.TextureLight != "")
                    {
                        var tslot = material.TextureLightMap;
                        tslot.FilePath = _modDir + mat.TextureLight;
                        material.TextureLightMap = tslot;
                    }
                }
                else
                {
                    Logger.Info("Unknown material: " + mat.Name);
                }
            }

            foreach (var meshOld in scene.Meshes)
                if (scene.Materials[meshOld.MaterialIndex].Name.EndsWith("Mat_Body"))
                {
                    MergeMeshMod(meshOld, scene);
                }
                else if (scene.Materials[meshOld.MaterialIndex].Name.EndsWith("Mat_Hair"))
                {
                    MergeMeshMod(meshOld, scene);
                }

            
        }

        void MergeMeshMod(Assimp.Mesh meshOld, Assimp.Scene scene)
        {
            int offset = materials[meshOld.MaterialIndex].Offset;
            int count = materials[meshOld.MaterialIndex].Count;

            //considering triangles arranged
            var indx = indexes.GetRange(offset, count);

            offset = indx.Min();
            //sine index start from 0 need +1 to get vertexes count
            count = indx.Max() - offset + 1;

            var verts = meshOld.Vertices;
            verts.Clear();
            verts.AddRange(Tools.Convert(points.GetRange(offset, count)));

            var norms = meshOld.Normals;
            norms.Clear();
            norms.AddRange(Tools.Convert(normals.GetRange(offset, count)));

            var uv = meshOld.TextureCoordinateChannels[0];
            uv.Clear();
            uv.AddRange(Tools.Convert(uvs.GetRange(offset, count)));

            var tang = meshOld.Tangents;
            tang.Clear();
            tang.AddRange(Tools.Convert43(tangents.GetRange(offset, count)));

            meshOld.SetIndices(indx.ConvertAll(a => a - offset).ToArray(), 3);

            //Mapping boneWeigth to Assimp structure
            var bones = Tools.GetArmaure(scene);
            var boneDict = new Dictionary<string, List<VertexWeight>>();
            for (int i = offset; i < offset + count; i++)
            {
                var vert = this.verts[i];
                for (int j = 0; j < 4; j++)
                {
                    if (vert.BoneIndices[j] >= bones.Count || vert.BoneIndices[j] < 0) {
                        Console.WriteLine(vert.BoneIndices[j]);
                        continue;
                    }

                    var bone = bones[vert.BoneIndices[j]];
                    if (!boneDict.ContainsKey(bone.Name))
                        boneDict.Add(bone.Name, new List<VertexWeight>());

                    if (vert.BoneWeigths[j] != 0)
                        boneDict[bone.Name].Add(new VertexWeight(i - offset, vert.BoneWeigths[j]));

                }
            }

            TraverseScene(scene.RootNode);


            
            //Clean all vertex weigth
            foreach (var bone in meshOld.Bones) {
                bone.VertexWeights.Clear();
            }
            //Update skinning data
            foreach (var bone in boneDict)
            {
                var oldBone = meshOld.Bones.Find(b => b.Name == bone.Key);
                if (oldBone == null)
                {
                    oldBone = new Assimp.Bone();
                    oldBone.Name = bone.Key;
                    oldBone.OffsetMatrix = Tools.Convert(bones.Find(n => n.Name == bone.Key).Parent2Local);
                    meshOld.Bones.Add(oldBone);
                }
                oldBone.VertexWeights.AddRange(bone.Value);
            }
        }

        void ReadMaterials(string diffuse, int offset)
        {
            var shdrst = new ShaderSettings();
            var rddir = new RenderDirectives();
            shdrst.HasSkeleton = true;
            shdrst.RecieveShadow = false;
            shdrst.AffectedByLight = true;
            rddir.CastShadow = false;
            rddir.HasEdges = true;
            shdrst.TextureDiffuse = true;
            shdrst.DiscardInvisible = true;
            shdrst.Ambient = true;


            string path = "Toys.Resourses.shaders.";
            string vs = ShaderManager.ReadFromAssetStream(path + "genshin.vsh");
            string fs = ShaderManager.ReadFromAssetStream(path + "genshin.fsh");
            var mat = new MaterialCustom(shdrst, rddir, vs, fs);

            if (diffuse != "")
            {
                //var textureDiffuse = ResourcesManager.LoadAsset<Texture2D>(diffuse);
                //mat.SetTexture(textureDiffuse, TextureType.Diffuse);
            }
            mat.Offset = offset;
            mat.Count = indexes.Count - offset;
            materials.Add(mat);


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
        }

        void ReadPosition(string file, int stride)
        {
            
            var read = new Reader(File.OpenRead(file));
            var count = read.BaseStream.Length / stride;
            verts = new VertexRigged3D[count];

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
            for (int i = 0; i < count; i++)
            {
                verts[i].BoneWeigths = read.readVector4();
                verts[i].BoneIndices = new IVector4(new int[]{read.ReadInt32(), read.ReadInt32(), read.ReadInt32(), read.ReadInt32()});

                BoneWeigth.Add(verts[i].BoneWeigths);
                BoneIndices.Add(verts[i].BoneIndices);
            }

            
            /*
            var indxs = new int[114];
            for (int i = 0; i < BoneIndices.Count; i++)
            {
                bool skip = true;
                for (int j = 0; j < 4; j++)
                    if (BoneWeigth[i][j] > 0 && BoneIndices[i][j] == 136)
                        skip = false;

                if (skip)
                    verts[i].Position = OpenTK.Mathematics.Vector3.Zero;
            }
            

            /*
            for (int i = 0; i < indxs.Length; i++)
            {
                Console.WriteLine("{0} ", indxs[i]);
            }
            */
            //Console.WriteLine(4444444444444);
            //    if (indxs[i] != 1)
            //        Console.WriteLine(i);

        }

        void UpdateTexturePath()
        {
            foreach (var mat in scene.Materials)
            {
                if (!mat.HasTextureDiffuse)
                    continue;
                var tex = mat.TextureDiffuse;
                tex.FilePath = _workDir  + tex.FilePath;
                mat.TextureDiffuse = tex;
            }
        }

        public ModelPrefab GetModel
        {
            get
            {
                /*
                List<Component> comps = new List<Component>();
                MeshDrawer md = new MeshDrawer(mesh, materials.ToArray());
                comps.Add(md);
                return new ModelPrefab(comps);
               */
                var model = new ReaderFBX(scene,"").GetModel;
                

                foreach (var comp in model.GetComponents)
                {
                    if (comp is MeshDrawerRigged)
                    {
                        UpdateMaterials((MeshDrawerRigged)comp);
                    }
                }
                
                return model;
                
            }
        }
        void UpdateMaterials(MeshDrawerRigged meshD)
        {
            //Due to assimp structure one mesh corresponds one structure
            var material = _modModelData.materials.Find(m => meshD.Materials[0].Name.EndsWith(matTable[m.Name]));
            if (material != null && material.MeshSubParts.Count > 0) 
            {
                int i = 0;
                var materialOld = meshD.Materials[0];
                meshD.Materials.Clear();
                foreach (var part in material.MeshSubParts)
                {
                    
                    var mat = materialOld.Clone();
                    mat.Offset = part.Offset;
                    mat.Count = part.Count;
                    mat.Name = i.ToString();
                    meshD.Materials.Add(mat);
                    i++;
                }
            }
        }
    }


}
