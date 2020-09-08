using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using OpenTK;

namespace Toys
{
    class PmdReader : IModelLoader
    {
        enum BoneType
        {
            Rotate,
            RotateMove,
            IK,
            Unknown,
            IKLink,
            RotateEffect,
            IKTo,
            Unvisible,
            Twist,
            RotateRatio
        }

        Header header;
        Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
        int[] materialToonTable;
        Texture2D empty;
        Material[] mats;
        Bone[] bones;
        public Morph[] morphs;
        float multipler = 0.1f;
        RigidContainer[] rigitBodies;
        JointContainer[] joints;
        int[] boneOrder;

        Mesh mesh;
        Mesh meshRigged;

        string dir;

        public PmdReader(string path)
        {
            int indx = path.LastIndexOf('\\');
            if (indx >= 0)
                dir = path.Substring(0, indx) + '\\';
            else
                dir = "";
            empty = Texture2D.LoadEmpty();
            Stream fs = File.OpenRead(path);
            var reader = new Reader(fs);
            //JIS encoding
            reader.EncodingType = 200;
            ReadHeader(reader);
            ReadMesh(reader);
            ReadMaterial(reader);
            ReadBones(reader);
            ReadMorhps(reader);
            //ReadPanel();
            //ReadRigit();
            //ReadJoints();

            reader.Close();
        }

        void ReadHeader(Reader reader)
        {
            header = new Header();
            //Pmd
            string signature = new String(reader.ReadChars(3));
            header.Version = reader.ReadSingle();
            header.Name = reader.readStringLength(20);
            header.Comment = reader.readStringLength(256);
        }

        void ReadMesh(Reader reader)
        {
            int vertexCount = reader.ReadInt32();
            var vertices = new VertexRigged3D[vertexCount];
            
            for (int i = 0; i < vertexCount; i++)
            {
                var vertex = new VertexRigged3D();
                vertex.Position = reader.readVector3() * multipler;
                vertex.Normal = reader.readVector3();
                vertex.UV = reader.readVector2();
                vertex.BoneIndices.bone1 = reader.ReadInt16();
                vertex.BoneIndices.bone2 = reader.ReadInt16();
                vertex.BoneWeigths[0] = (float)reader.ReadByte() / byte.MaxValue;
                vertex.BoneWeigths[1] = 1 - vertex.BoneWeigths[0];
                //non edge flag
                reader.ReadByte();
                vertices[i] = vertex;
            }

            int faceCount = reader.ReadInt32();
            var indexes = new int[faceCount];
            for (int i = 0; i < faceCount; i++)
            {
                indexes[i] = reader.ReadUInt16();
            }

            meshRigged = new Mesh(vertices, indexes);
        }

        void ReadMaterial(Reader reader)
        {
            int materialCount = reader.ReadInt32();
            materialToonTable = new int[materialCount];
            mats = new Material[materialCount];
            int offset = 0;
            for (int i = 0; i < materialCount; i++)
            {
                var shdrs = new ShaderSettings();
                var difColor = reader.readVector4();
                var specularPower = reader.ReadSingle();
                var specularColour = reader.readVector3();
                var ambientColour = reader.readVector3();
                materialToonTable[i] = reader.ReadByte();
                bool hasEdge = (reader.ReadByte() > 0);
                int vertCount = reader.ReadInt32();
                EnvironmentMode envMode = EnvironmentMode.None;
                Texture2D diffuseTexture = null;
                Texture2D sphereTexture = null;
                var pathList = reader.readStringLength(20);
                if (pathList.Contains('*'))
                {
                    var texturePath = pathList.Split('*');
                    shdrs.EnvType = EnvironmentMode.None;

                    for (int n = 0; n < texturePath.Length; n++)
                    {
                        string path = texturePath[n];

                        EnvironmentMode sphereMode = GetEnvType(path);
                        if (sphereMode != 0)
                        {
                            sphereTexture = GetTexture(path);
                            envMode = sphereMode;
                        }
                        else
                        {
                            diffuseTexture = GetTexture(path);
                        }
                    }
                }

                var outln = new Outline();
                outln.EdgeScaler = 0.4f;

                shdrs.HasSkeleton = true;
                shdrs.DiscardInvisible = true;
                shdrs.AffectedByLight = true;
                shdrs.DifuseColor = true;
                shdrs.Ambient = true;
                shdrs.SpecularColor = true;
                shdrs.TextureDiffuse = true;
                shdrs.RecieveShadow = true;
                shdrs.EnvType = envMode;

                var rndr = new RenderDirectives();
                rndr.HasEdges = hasEdge;
                var mat = new MaterialPMX(shdrs, rndr);
                mat.Name = "";
                mat.Outline = outln;
                mat.SetTexture(diffuseTexture, TextureType.Diffuse);
                //mat.SetTexture(toon, TextureType.Toon);
                mat.SetTexture(sphereTexture, TextureType.Sphere);
                mat.SpecularColour = specularColour;
                mat.Specular = specularPower;
                mat.DiffuseColor = difColor;
                mat.AmbientColour = ambientColour;
                mat.UniManager.Set("specular_color", specularColour);
                mat.UniManager.Set("ambient_color", ambientColour);
                mat.UniManager.Set("specular_power", specularPower);
                mat.UniManager.Set("diffuse_color", difColor);

                if (mat.DiffuseColor.W < 0.001f)
                    mat.RenderDirrectives.IsRendered = false;
                //skip empty materials
                if (vertCount == 0)
                    mat.RenderDirrectives.IsRendered = false;
                mat.Offset = offset;
                mat.Count = vertCount;
                mats[i] = mat;
                offset += vertCount;
            }

        }

        void ReadBones(Reader reader)
        {
            int bonesCount = reader.ReadUInt16();
            bones = new Bone[bonesCount];
            var boneTypes = new BoneType[bonesCount];
            var temp = new int[bonesCount, 2];
            int maxLevel = 0;
            for (int i = 0; i < bonesCount; i++)
            {
                string name = reader.readStringLength(20);

                int parent = reader.ReadInt16();
                int to = reader.ReadInt16();
                boneTypes[i] = (BoneType)reader.ReadByte();
                int ikNum = reader.ReadInt16();
                var position = reader.readVector3() * multipler;

                Bone bone = new Bone(name, "", position, parent, new byte[16]);
                bone.tail = true;
                temp[i, 0] = to;
                temp[i, 1] = ikNum;
                bones[i] = bone;
            }

            int ikCount = reader.ReadUInt16();
            for (int i = 0; i < ikCount; i++)
            {
                int bone = reader.ReadUInt16();
                BoneIK boneIK = new BoneIK();
                boneIK.Target = reader.ReadUInt16();
                int linkCount = reader.ReadByte();
                boneIK.LoopCount = reader.ReadUInt16();
                boneIK.AngleLimit = reader.ReadSingle();
                boneIK.Links = new IKLink[linkCount];
                
                for (int n = 0; n < linkCount; n++)
                {
                    boneIK.Links[n] = new IKLink();
                    boneIK.Links[n].Bone = reader.ReadUInt16();
                }

                bones[bone].IKData = boneIK;
                bones[bone].IK = true;
                bones[bone].Translatable = true;
                bones[bone].Level = 1;
            }

            for (int i = 0; i < bonesCount; i++)
            {
                bones[i].Rotatable = true;
                bones[i].IsVisible = true;
                bones[i].Enabled = true;
                switch (boneTypes[i])
                {
                    case BoneType.RotateMove:
                        bones[i].Translatable = true;
                        break;
                    case BoneType.IK:
                        bones[i].IK = true;
                        break;
                    case BoneType.RotateEffect:
                        bones[i].InheritRotation = true;
                        bones[i].ParentInfluence = 1f;
                        bones[i].Level = 2;
                        bones[0].ParentInheritIndex = temp[i, 1];
                        break;
                    case BoneType.IKTo:
                        bones[i].IsVisible = false;
                        break;
                    case BoneType.Unvisible:
                        bones[i].IsVisible = false;
                        break;
                    case BoneType.Twist:
                        bones[i].FixedAxis = true;
                        break;
                    case BoneType.RotateRatio:
                        bones[i].InheritRotation = true;
                        bones[i].tail = false;
                        bones[i].IsVisible = false;
                        bones[0].ParentInheritIndex = temp[i, 0];
                        bones[0].ParentInfluence = (float)temp[i, 1] * 0.01f;
                        break;
                }
            }


            //convert position from model to parent bone
            for (int i = bones.Length - 1; i > -1; i--)
            {
                Bone bone = bones[i];
                if (bone.ParentIndex >= 0 && bone.ParentIndex < bones.Length)
                {
                    bone.Parent2Local = Matrix4.CreateTranslation(bone.Position - bones[bone.ParentIndex].Position);
                }
            }
            boneOrder = new int[bones.Length];
            int m = 0;
            for (int n = 0; n <= maxLevel; n++)
            {
                for (int i = 0; i < bones.Length; i++)
                {
                    if (bones[i].Level == n)
                    {
                        boneOrder[m] = i;
                        m++;
                    }
                }
            }
        }

        void ReadMorhps(Reader reader)
        {
            int morphCount = reader.ReadUInt16();
            morphs = new Morph[morphCount];
            for (int i = 0; i < morphCount; i++)
            {
                var name = reader.readStringLength(20);
                var vertCount = reader.ReadInt32();
                var category = reader.ReadByte();
                var morph = new MorphVertex(name,"", vertCount);
                for (int n = 0; n < vertCount; n++)
                {
                    var index = reader.ReadInt32();
                    var pos = reader.readVector3() * multipler;
                    morph.AddVertex(pos, index);
                }
            }
        }


        EnvironmentMode GetEnvType(string path)
        {

            EnvironmentMode result = EnvironmentMode.None;
            if (string.IsNullOrEmpty(path))
            {
                return result;
            }

            string a = Path.GetExtension(path).ToLower();
            if (a == ".sph")
            {
                result = EnvironmentMode.Multiply;
            }
            else if (a == ".spa")
            {
                result = EnvironmentMode.Additive;
            }
            return result;
        }

        Texture2D GetTexture(string path)
        {
            Texture2D result = null;
            if (textures.ContainsKey(path))
            {
                result = textures[path];
            }
            else
            {
                result = ResourcesManager.LoadAsset<Texture2D>(path);
                textures.Add(path, result);
            }

            return result;
        }

        public SceneNode GetModel
        {
            get
            {
                MeshDrawer md = new MeshDrawer(mesh, mats);
                md.OutlineDrawing = true;
                var node = new SceneNode();
                node.AddComponent(md);

                return node;
            }
        }

        public SceneNode GetRiggedModel
        {
            get
            {
                MeshDrawerRigged md = new MeshDrawerRigged(meshRigged, mats, new BoneController(bones, boneOrder), morphs);
                md.OutlineDrawing = true;

                var node = new SceneNode();
                node.AddComponent(md);
                node.AddComponent(new Animator(md.skeleton));
                //node.AddComponent(new PhysicsManager(rigitBodies, joints, md.skeleton));
                return node;
            }
        }

        public Morph[] GetMorphes
        {
            get
            {
                return morphs;
            }
        }
    }
}
