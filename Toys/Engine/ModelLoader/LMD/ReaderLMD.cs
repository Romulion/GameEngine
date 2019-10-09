using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.IO;
using OpenTK;

namespace Toys
{
    class ReaderLMD : IModelLoader
    {
        BinaryReader file;
        Reader reader;
        Dictionary<string, Material> materialTable = new Dictionary<string, Material>();
        Mesh[] meshRigged = new Mesh[0];
        MeshDrawer[] mesDrawlers = new MeshDrawer[0];
        Bone[] bones = null;
        BoneController boneControll = null;
        Dictionary<string, Texture2D> texturesDict = new Dictionary<string, Texture2D>();

        public ReaderLMD(string path)
        {
            Stream fs = File.OpenRead(path);
            file = new BinaryReader(fs);
            reader = new Reader(file);
            reader.Encoding = 1;
            StartRead();
        }

        void StartRead()
        {
            ReadBones();
            ReadMaterials();

            file.BaseStream.Position = 0x48;
            int meshCount = file.ReadInt32();

            int[] meshPos = new int[meshCount];
            for (int i = 0; i < meshCount; i++)
            {
                meshPos[i] = (int)file.BaseStream.Position + file.ReadInt32();
            }

            mesDrawlers = new MeshDrawer[meshCount];
            for (int i = 0; i < meshCount; i++)
            {
                mesDrawlers[i] = ReadMeshChunk(meshPos[i]);
                mesDrawlers[i].OutlineDrawing = true;
            }
        }

        void ReadMaterials()
        {
            file.BaseStream.Position = 0x38;
            int matOffset = (int)file.BaseStream.Position + file.ReadInt32();
            
            file.BaseStream.Position = matOffset + 4;

            file.BaseStream.Position += file.ReadInt32();
            int materialCount = file.ReadInt32();

            int[] offsets = new int[materialCount];
            for (int i = 0; i < materialCount; i++)
                offsets[i] = (int)file.BaseStream.Position + file.ReadInt32();

            foreach(var moffs in offsets)
            {
                file.BaseStream.Position = moffs + 4;
                int MaterialNameTextOffset = (int)file.BaseStream.Position + file.ReadInt32();
                file.BaseStream.Position = MaterialNameTextOffset;
                string MaterialName = reader.readString();
                file.BaseStream.Position = moffs + 0x48;
                string MaterialFileName = reader.readString();
                ShaderSettings sdrs = new ShaderSettings();
                RenderDirectives rndd = new RenderDirectives();
                sdrs.HasSkeleton = true;
                sdrs.TextureDiffuse = true;
                sdrs.TextureSpecular = true;
                sdrs.RecieveShadow = false;
                rndd.HasEdges = false;
                Material mat = new MaterialPM(sdrs, rndd);
                mat.Name = MaterialName;
                mat.Outline.EdgeScaler = 0.2f;
                materialTable.Add(MaterialName, mat);
                mat.UniManager.Set("uv_scale", Vector4.One);
                //scale face shadow texture
                if (MaterialName.Contains("face"))
                    mat.UniManager.Set("uv_scale", new Vector4(1,1,4,4));
                try
                {
                    ReadTexturesFromMaterial(MaterialName, mat);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

        }

        MeshDrawer ReadMeshChunk(int streamOffset)
        {
            //mesh name
            file.BaseStream.Position = streamOffset + 0x7;
            int chunkSize = file.ReadByte();
            int meshNamePos = (int)file.BaseStream.Position + file.ReadInt32();
            file.BaseStream.Position = meshNamePos;
            string meshName = reader.readString();
            //material reference
            file.BaseStream.Position = streamOffset + 0x14;
            int matNamePos = (int)file.BaseStream.Position + file.ReadInt32();
            file.BaseStream.Position = matNamePos + 0x8;
            string matName = reader.readString();

            //bones reference
            file.BaseStream.Position = streamOffset + 0x58;
            int weightBoneNameTableStart = (int)file.BaseStream.Position + file.ReadInt32();
            file.BaseStream.Position = streamOffset + 0x5c;
            int WeightBoneTableStart = (int)file.BaseStream.Position + file.ReadInt32();

            file.BaseStream.Position = streamOffset + 0x78;
            int facesCount = file.ReadInt32();
            file.BaseStream.Position = streamOffset + 0x84;
            int verticesCount = file.ReadInt32();
            byte size = 4;
            int SizeTest = verticesCount * chunkSize;
            if (SizeTest < 0x100)
                size = 1;
            else if (SizeTest < 0x10000)
                size = 2;

            file.BaseStream.Position += 8;
            int vertSize = reader.readVal(size);
            int VertOffset = (int)file.BaseStream.Position;

            List<VertexRigged3D> vertices = new List<VertexRigged3D>();
            for (int i = 0; i < verticesCount; i++)
            {
                VertexRigged3D vertice = new VertexRigged3D();
                vertice.Position = reader.readVector3();
                //vertex color unsupported
                file.BaseStream.Position += 4;
                //if (chunkSize == 0x24 || chunkSize == 0x28)
                if (chunkSize == 0x24)
                {
                    file.BaseStream.Position += 4;
                }
                else if (chunkSize == 0x30)
                {
                    file.BaseStream.Position += 4;
                    file.BaseStream.Position += 0xc;
                }
                vertice.UV = new Vector2();
                vertice.UV.X = Half.FromBytes(file.ReadBytes(2), 0);
                vertice.UV.Y = Half.FromBytes(file.ReadBytes(2), 0);
                /*
                if (chunkSize == 0x28)
                    file.BaseStream.Position += 4;
                    */
                vertice.BoneIndices = new IVector4(new int[] { file.ReadByte(), file.ReadByte(), file.ReadByte(), file.ReadByte() });
                if (chunkSize == 0x28)
                {
                    vertice.BoneWeigths = reader.readVector4();
                }
                else
                {
                    vertice.BoneWeigths = new Vector4(file.ReadUInt16(), file.ReadUInt16(), file.ReadUInt16(), file.ReadUInt16());
                    vertice.BoneWeigths /= 65535f;
                }
                vertices.Add(vertice);
            }

            int unknownSize = 4;
            if (size == 1)
                unknownSize = 2;

            file.BaseStream.Position = VertOffset + vertSize + size + unknownSize;
            int unknownCount = file.ReadInt32();
            file.BaseStream.Position += 0x10 * unknownCount;
            file.ReadInt32();
            //Read Faces
            size = 4;
            if (facesCount < 0x100)
                size = 1;
            else if (facesCount < 0x10000)
                size = 2;

            int faceSize = reader.readVal(size);
            byte indexValueSize = 4;
            if (verticesCount < 0x100)
                indexValueSize = 1;
            else if (verticesCount < 0x10000)
                indexValueSize = 2;

            int faceOffset = (int)file.BaseStream.Position;
            int[] indexes = new int[facesCount];
            for (int i = 0; i < facesCount; i++)
                indexes[i] = reader.readVal(indexValueSize);

            //Read Weigth Bones dictionary
            file.BaseStream.Position = weightBoneNameTableStart;
            int weightBoneCount = file.ReadInt32();
            int[] boneIdDict = new int[weightBoneCount];
            for (int i = 0; i < weightBoneCount; i++)
            {
                file.BaseStream.Position = weightBoneNameTableStart + i * 4 + 4;
                file.BaseStream.Position += file.ReadInt32();
                string weightBoneName = reader.readString();
                boneIdDict[i] = Array.FindIndex(bones, (b) => b.Name == weightBoneName);
            }

            var vv = vertices.ToArray();
            //set bone id to global
            for (int i = 0; i < vv.Length; i++)
            {
                IVector4 boneIndexes = vv[i].BoneIndices;
                boneIndexes.bone1 = boneIdDict[boneIndexes.bone1];
                boneIndexes.bone2 = boneIdDict[boneIndexes.bone2];
                boneIndexes.bone3 = boneIdDict[boneIndexes.bone3];
                boneIndexes.bone4 = boneIdDict[boneIndexes.bone4];
                vv[i].BoneIndices = boneIndexes;
            }

            CalculateVertNormals.CalculateNormals(vv, indexes);
            var mesh = new Mesh(vv, indexes);
            Material mat;
            if (materialTable.TryGetValue(matName, out mat))
            {
                return new MeshDrawerRigged(mesh, new Material[] { mat }, boneControll);
            }
            else
                return new MeshDrawerRigged(mesh, boneControll);
        }
        void ReadBones()
        {
            file.BaseStream.Position = 0x34;
            int boneOffset = (int)file.BaseStream.Position + file.ReadInt32();

            file.BaseStream.Position = boneOffset + 0x8;
            int boneCount = file.ReadInt32();

            int[] boneOffsets = new int[boneCount];
            for (int i = 0; i < boneCount; i++)
            {
                boneOffsets[i] = (int)file.BaseStream.Position + file.ReadInt32();
            }

            bones = new Bone[boneCount];
            string[] parents = new string[boneCount];
            for (int i = 0; i < boneCount; i++)
            {
                
                file.BaseStream.Position = boneOffsets[i];
                int magicNumber = file.ReadInt32();
                file.BaseStream.Position = boneOffsets[i] + 4;
                int nameOffset = (int)file.BaseStream.Position + file.ReadInt32();
                file.BaseStream.Position = nameOffset;
                string boneName = reader.readString();

                file.BaseStream.Position = boneOffsets[i] + 8;
                Matrix4 boneMatrix = new Matrix4(
                    reader.readVector4(),
                    reader.readVector4(),
                    reader.readVector4(),
                    reader.readVector4()
                );
                file.BaseStream.Position = boneOffsets[i] + 0x38;
                Vector3 bonePosition = reader.readVector3();
                file.BaseStream.Position = boneOffsets[i] + 0x48;
                int boneParentOffset = (int)file.BaseStream.Position + file.ReadInt32();
                file.BaseStream.Position = boneParentOffset;
                parents[i] = reader.readString();
                Bone bone = new Bone(boneName, boneMatrix, -1);
                bone.Index = i;
                bone.Position = bonePosition;
                bones[i] = bone;
                if (magicNumber < 0x5000)
                    continue;
            }

            for (int i = 0; i < boneCount; i++)
            {
                bones[i].ParentIndex = Array.FindIndex(bones, (element) => element.Name == parents[i]);
            }
            boneControll = new BoneController(bones,true);
        }

        public SceneNode GetModel
        {
            get
            {
                var node = new SceneNode();
                foreach (Mesh mesh in meshRigged)
                {
                    MeshDrawer md = new MeshDrawer(new Mesh(mesh.Vertices, mesh.Indices));
                    node.AddComponent(md);
                }
                return node;
            }
        }

        public SceneNode GetRiggedModel
        {
            get
            {
                var node = new SceneNode();
                foreach (MeshDrawerRigged mesh in mesDrawlers)
                {
                    node.AddComponent(mesh);
                    node.AddComponent(new Animator(mesh.skeleton));
                }
                node.AddComponent<ScriptChangeExpression>();
                return node;
            }
        }

        public Morph[] GetMorphes
        {
            get
            {
                return null;
            }
        }

        delegate bool Check();
        void ReadTexturesFromMaterial(string name, Material mat)
        {
            string path = "Materials/" + name + ".material";
            Stream fs = File.OpenRead(path);
            var file = new BinaryReader(fs);
            var reader = new Reader(file);
            reader.Encoding = 1;

            byte[] buffer = new byte[3];
            while (file.BaseStream.Read(buffer, 2, 1) > 0)
            {
                //looking for sequence "00 0A 75" 0A - 10 chars string; 0x75 - u char
                if (buffer[0] == 0 && buffer[2] == 'u' && buffer[1] == 10)
                {
                    file.BaseStream.Position -= 2;
                    string textureId = reader.readStringB();
                    string textPath = "";
                    TextureType type = TextureType.Diffuse;

                    switch (textureId)
                    {
                        case "u_texture0":
                            file.BaseStream.Position += 1;
                            textPath = reader.readStringB();
                            type = TextureType.Diffuse;
                            break;
                        case "u_texture1":
                            file.BaseStream.Position += 1;
                            textPath = reader.readStringB();
                            type = TextureType.Specular;
                            break;
                        case "u_texture2":
                            file.BaseStream.Position += 1;
                            textPath = reader.readStringB();
                            type = TextureType.Toon;
                            break;
                    }
                    if (textPath != "")
                    {
                        textPath = textPath.Substring(textPath.LastIndexOf("Models/") + 7);
                        if (texturesDict.ContainsKey(textPath))
                            mat.SetTexture(texturesDict[textPath], type);
                        else
                        {
                            var text = new Texture2D(textPath);
                            texturesDict.Add(textPath, text);
                            mat.SetTexture(text, type);
                        }
                    }
                }

                buffer[0] = buffer[1];
                buffer[1] = buffer[2];
            }
        }
    }
}
