using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using OpenTK.Mathematics;

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
        public MorphVertex[] morphs;
        float multipler = 0.1f;
        RigidContainer[] rigitBodies;
        JointContainer[] joints;
        int[] boneOrder;

        //Mesh mesh;
        Mesh meshRigged;

        string dir;

        public PmdReader(Stream fs, string path)
        {
            int indx = path.LastIndexOf('\\');
            if (indx >= 0)
                dir = path.Substring(0, indx) + '\\';
            else
                dir = "";
            empty = Texture2D.LoadEmpty();
            var reader = new Reader(fs);
            //JIS encoding
            reader.EncodingType = 200;
            //registed all availible encodings
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            ReadHeader(reader);
            ReadMesh(reader);
            ReadMaterial(reader);
            ReadBones(reader);                
            ReadMorhps(reader);
            ReadPanel(reader);
            if (reader.BaseStream.Position != reader.BaseStream.Length)
            {            
                ReadRigit(reader);
                ReadJoints(reader);
            }
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
                //Convert left to right coordinate system
                vertex.Position.Z = -vertex.Position.Z;
                vertex.Normal.Z = -vertex.Normal.Z;

                var weight = reader.ReadByte();
                if (weight > 100)
                    weight = 100;
                vertex.BoneWeigths[0] = (float)weight / 100;
                vertex.BoneWeigths[1] = 1 - vertex.BoneWeigths[0];
                //non edge flag
                reader.ReadByte();
                vertices[i] = vertex;
            }

            int faceCount = reader.ReadInt32();
            var indexes = new int[faceCount];
            /*
            for (int i = 0; i < faceCount; i++)
            {
                indexes[i] = reader.ReadUInt16();
            }
            */
            //invert triangles to convert left to right coordinates
            for (int i = 0; i < faceCount; i++)
            {
                var vertIndex = reader.ReadUInt16();
                int res = i % 3;
                if (res == 0)
                    indexes[i + 1] = vertIndex;
                else if (res == 1)
                    indexes[i - 1] = vertIndex;
                else
                    indexes[i] = vertIndex;
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
                        string path = dir + texturePath[n];

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
                else if (pathList.Length > 0)
                {
                    diffuseTexture = GetTexture(dir + pathList);
                }


                var outln = new Outline();
                outln.EdgeScaler = 0.1f;

                shdrs.HasSkeleton = true;
                shdrs.DiscardInvisible = true;
                shdrs.AffectedByLight = true;
                shdrs.DifuseColor = true;
                shdrs.Ambient = true;
                shdrs.SpecularColor = true;
                shdrs.TextureDiffuse = true;
                shdrs.RecieveShadow = true;
                shdrs.EnvType = envMode;
                shdrs.ToonShadow = true;

                var rndr = new RenderDirectives();
                rndr.HasEdges = hasEdge;
                var mat = new MaterialPMX(shdrs, rndr);
                mat.Name = string.Format("Material {0}",i);
                mat.Outline = outln;
                if (diffuseTexture)
                    mat.SetTexture(diffuseTexture, TextureType.Diffuse);
                //mat.SetTexture(toon, TextureType.Toon);
                if (sphereTexture)
                    mat.SetTexture(sphereTexture, TextureType.Sphere);
                mat.SpecularColour = specularColour;
                mat.Specular = specularPower;
                mat.DiffuseColor = difColor;
                mat.AmbientColour = ambientColour;
                mat.UniformManager.Set("specular_color", specularColour);
                mat.UniformManager.Set("ambient_color", ambientColour);
                mat.UniformManager.Set("specular_power", specularPower);
                mat.UniformManager.Set("diffuse_color", difColor);

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
                    
                    //was hardcoded in original
                    if (bones[boneIK.Links[n].Bone].Name == "左ひざ" || bones[boneIK.Links[n].Bone].Name == "右ひざ")
                    {
                        boneIK.Links[n].IsLimit = true;
                        boneIK.Links[n].LimitMin = new Vector3(-(float)Math.PI,0,0);
                        boneIK.Links[n].LimitMax = new Vector3(-(float)Math.PI / 180 * 5, 0, 0);
                    }
                    
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

                if (maxLevel < bones[i].Level)
                    maxLevel = bones[i].Level;
            }


            //convert position from model to parent bone
            for (int i = bones.Length - 1; i > -1; i--)
            {
                Bone bone = bones[i];
                if (bone.ParentIndex >= 0 && bone.ParentIndex < bones.Length)
                    bone.Parent2Local = Matrix4.CreateTranslation(bone.Position - bones[bone.ParentIndex].Position);
                else if (bone.ParentIndex == -1)
                    bone.Parent2Local = Matrix4.CreateTranslation(bone.Position);
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
            morphs = new MorphVertex[morphCount];

            for (int i = 0; i < morphCount; i++)
            {
                var name = reader.readStringLength(20);
                var vertCount = reader.ReadInt32();
                var category = reader.ReadByte();
                var morph = new MorphVertex(name, "", vertCount,meshRigged.GetMorpher);
                for (int n = 0; n < vertCount; n++)
                {
                    var index = reader.ReadInt32();
                    var pos = reader.readVector3() * multipler;
                    pos.Z = -pos.Z;
                    morph.AddVertex(pos, index);
                }
                morphs[i] = morph;
            }

            if (morphCount > 0)
            {
                MorphVertex baseMorph = morphs[0];
                //local to global index
                for (int i = 1; i < morphs.Length; i++)
                {
                    for (int n = 0; n < morphs[i].morph.Length; n++)
                        morphs[i].morph[n].ID = baseMorph.morph[morphs[i].morph[n].ID].ID;
                }
            }
        }

        void ReadPanel(Reader reader)
        {
            var expressionCount = reader.ReadByte();
            for (int i = 0; i < expressionCount; i++)
            {
                reader.ReadUInt16();
            }

            var  nodeCount = reader.ReadByte();
            for (int i = 0; i < nodeCount; i++)
            {
                //node names
                reader.readStringLength(50);
            }

            var boneNodesCount = reader.ReadUInt16();
            reader.ReadUInt16();
            for (int i = 0; i < boneNodesCount; i++)
            {
                //bone id
                reader.ReadUInt16();
                //node id
                reader.ReadByte();
            }

            if (reader.BaseStream.Length == reader.BaseStream.Position)
                return;

            //english names
            bool eng = reader.ReadByte() != 0;
            if (eng)
            {
                header.NameEng = reader.readStringLength(20);
                header.CommentEng = reader.readStringLength(256);
                //fill english bones names
                for (int i = 0; i < bones.Length; i++)
                {
                    bones[i].NameEng = reader.readStringLength(20);
                }
                //fill english morphs names
                for (int i = 0; i < morphs.Length - 1; i++)
                {
                    morphs[i].NameEng = reader.readStringLength(20);
                }
                //fill english nodes names
                for (int i = 0; i < nodeCount; i++)
                {
                    reader.readStringLength(50);
                }
            }

            //load toon textures
            var toonData = new Texture2D[10];
            for (int i = 0; i < 10; i++)
            {
                var toonName = reader.readStringLength(100);
                //detect internal texture
                if (toonName.StartsWith("toon"))
                {
                    var assembly = System.Reflection.IntrospectionExtensions.GetTypeInfo(typeof(Texture2D)).Assembly;
                    var pictureStream = assembly.GetManifestResourceStream("Toys.Resourses.textures.PMX." + toonName);
                    //check if exists in standart toon library
                    if (pictureStream != null)
                        using (var pic = new System.Drawing.Bitmap(pictureStream))
                        {
                            toonData[i] = new Texture2D(pic, TextureType.Toon, toonName);
                        }
                }

                if (toonData[i] == null)
                {
                    toonData[i] = ResourcesManager.LoadAsset<Texture2D>(dir + toonName);
                }
            }

            //assign toon textures to materials
            for (int i = 0; i < mats.Length; i++)
            {
                int toonID = materialToonTable[i];
                if (toonID < toonData.Length)
                    mats[i].SetTexture(toonData[materialToonTable[i]], TextureType.Toon);
            }

        }

        void ReadRigit(Reader reader)
        {
            var bodyCount = reader.ReadInt32();
            rigitBodies = new RigidContainer[bodyCount];
            for (int i = 0; i < bodyCount; i++)
            {
                var rigit = new RigidContainer();
                rigit.Name = reader.readStringLength(20);
                rigit.BoneIndex = reader.ReadInt16();
                rigit.GroupId = (byte)(16 + reader.ReadByte());
                rigit.NonCollisionGroup = (int)reader.ReadUInt16() << 16;
                rigit.PrimitiveType = (PhysPrimitiveType)reader.ReadByte();
                rigit.Size = reader.readVector3() * multipler;
                rigit.Position = reader.readVector3() * multipler;
                rigit.Rotation = reader.readVector3();
                rigit.Mass = reader.ReadSingle();
                rigit.MassAttenuation = reader.ReadSingle();
                rigit.RotationDamping = reader.ReadSingle();
                rigit.Restitution = reader.ReadSingle();
                rigit.Friction = reader.ReadSingle();
                rigit.Phys = (PhysType)reader.ReadByte();

                //convert coordinates from rigit2bone to rigit2world
                if (rigit.BoneIndex > -1)
                    rigit.Position += bones[rigit.BoneIndex].Position;

                rigitBodies[i] = rigit;
            }
        }

        void ReadJoints(Reader reader)
        {
            int jointCount = reader.ReadInt32();
            joints = new JointContainer[jointCount];

            for (int i = 0; i < jointCount; i++)
            {
                JointContainer joint = new JointContainer();
                joint.Name = reader.readStringLength(20);
                joint.Type = JointType.SpringSixDOF;
                joint.RigitBody1 = reader.ReadInt32();
                joint.RigitBody2 = reader.ReadInt32();
                joint.Position = reader.readVector3() * multipler;
                joint.Rotation = reader.readVector3();
                joint.PosMin = reader.readVector3() * multipler;
                joint.PosMax = reader.readVector3() * multipler;
                joint.RotMin = reader.readVector3();
                joint.RotMax = reader.readVector3();
                joint.PosSpring = reader.readVector3() * multipler;
                joint.RotSpring = reader.readVector3();

                joints[i] = joint;
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
                result.WrapMode = TextureWrapMode.Repeat;
                textures.Add(path, result);
            }

            return result;
        }

        public ModelPrefab GetModel
        {
            get
            {
                List<Component> comps = new List<Component>();

                MeshDrawerRigged md = new MeshDrawerRigged(meshRigged, mats, new BoneController(bones, boneOrder), morphs.ToList<Morph>());
                md.OutlineDrawing = true;
                comps.Add(md);
                comps.Add(new Animator(md.skeleton, md.Morphes));
                if (rigitBodies != null)
                    comps.Add(new PhysicsManager(rigitBodies, joints, md.skeleton));
                return new ModelPrefab(comps);
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
