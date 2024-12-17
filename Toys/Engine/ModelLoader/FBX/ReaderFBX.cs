using BulletSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assimp;
using System.Runtime.CompilerServices;
using System.IO;
using static System.Net.Mime.MediaTypeNames;
using OpenTK.Mathematics;
using System.Drawing;
using System.Xml.Linq;

namespace Toys
{
    public class ReaderFBX : IModelLoader
    {
        List<Mesh> meshes = new List<Mesh>();
        List<Node> sceneNodes = new List<Node>();
        List<Bone> bones = new List<Bone>();
        Material[] materials;
        MeshDrawerRigged[] MeshDrawer = null;
        BoneController boneControll;
        string dir = "";
        float sizeMultiplier = 1f;
        public ReaderFBX(string path) 
        { 
            AssimpContext importer = new AssimpContext();
            var scene = importer.ImportFile(path);

            TraverseScene(scene.RootNode);
            LoadMeshes(scene.Meshes);
        }

        public ReaderFBX(Stream fs, string path)
        {
            int indx = path.LastIndexOf('\\');
            if (indx >= 0)
                dir = path.Substring(0, indx) + '\\';
            else
                dir = "";

            AssimpContext importer = new AssimpContext();
            var scene = importer.ImportFileFromStream(fs, "fbx");
            TraverseScene(scene.RootNode);
            LoadMaterial(scene);
            LoadMeshes(scene.Meshes);
        }

        void TraverseScene(Assimp.Node node)
        {
            //Console.WriteLine(node.Name);
            //foreach (var mesh in node.Metadata) 
            //    Console.WriteLine("{0} {1}", mesh.Key, mesh.Value);
            int parent = -1;
            if (node.Parent != null)
                parent = sceneNodes.IndexOf(node.Parent);

            var bone = new Bone(node.Name, Convert(node.Transform), parent);
            bones.Add(bone);
            sceneNodes.Add(node);

            //if (node.Name == "Bip001 Head")
            //    Console.WriteLine(node.Transform);

            if (node.HasChildren)
            {
                foreach (var child in node.Children)
                {
                    TraverseScene(child);
                }   
            }

        }

        void LoadMaterial(Assimp.Scene scene)
        {
            materials = new Material[scene.MaterialCount];
            for (int i = 0; i < scene.MaterialCount; i++)
            {
                Texture2D textureDiffuse = Texture2D.LoadEmpty();
                var mat = scene.Materials[i];
                if (mat.HasTextureDiffuse)
                {
                    var fullPath = dir + mat.TextureDiffuse.FilePath;
                    if (File.Exists(fullPath))
                    {
                        textureDiffuse = ResourcesManager.LoadAsset<Texture2D>(fullPath);
                        textureDiffuse.WrapModeU = Convert(mat.TextureDiffuse.WrapModeU);
                        textureDiffuse.WrapModeV = Convert(mat.TextureDiffuse.WrapModeV);
                    }
                }

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

                var mater = new MaterialCustom(shdrst, rddir, vs, fs);
                //var mater = new MaterialPMX(shdrst, rddir);
                mater.Name = mat.Name;

                
                if (mat.HasColorAmbient)
                    mater.UniformManager.Set("ambient_color", Convert(mat.ColorAmbient).Xyz);
                if (mat.HasColorDiffuse)
                    mater.UniformManager.Set("diffuse_color", Convert(mat.ColorDiffuse));
                if (mat.HasColorSpecular)
                {
                    mater.UniformManager.Set("specular_color", Convert(mat.ColorSpecular).Xyz);
                    mater.UniformManager.Set("specular_power", Convert(mat.ColorSpecular).W);
                }
                
                
                mater.SetTexture(textureDiffuse, TextureType.Diffuse);

                materials[i] = mater;
            }
        }

        void LoadMeshes(List<Assimp.Mesh> meshes)
        {
            boneControll = new BoneController(bones.ToArray(), true);
            Console.WriteLine(boneControll.GetBone("Bip001 Head").World2BoneInitial);

            MeshDrawer = new MeshDrawerRigged[meshes.Count];
            int m = 0;
            foreach (var meshData in meshes)
            {

                var vertexRigged3D = new VertexRigged3D[meshData.VertexCount];
                var boneweight = new int[meshData.VertexCount];
                //Console.WriteLine(meshData.Name);
                var indexes = meshData.GetIndices().ToArray();
                var vertexes = meshData.Vertices.ToArray();

                //Console.WriteLine("{0} {1} {2}", indexes.Length, meshData.FaceCount,0);
                for (int i = 0; i < meshData.VertexCount; i++)
                    vertexRigged3D[i].Position = Convert(vertexes[i]) * sizeMultiplier;

                if (meshData.HasNormals)
                {
                    var normals = meshData.Normals;
                    for (int i = 0; i < meshData.VertexCount; i++)
                        vertexRigged3D[i].Normal = Convert(normals[i]);
                }

                if (meshData.HasTextureCoords(0))
                {
                    var uvs = meshData.TextureCoordinateChannels[0];
                    for (int i = 0; i < meshData.VertexCount; i++)
                    {
                        vertexRigged3D[i].UV = Convert(uvs[i]).Xy;
                        vertexRigged3D[i].UV.Y = 1 - vertexRigged3D[i].UV.Y;
                    }
                }


                foreach (var bone in meshData.Bones)
                {
                    var index = bones.FindIndex((bon) => bon.Name == bone.Name);
                    foreach (var vertW in bone.VertexWeights)
                    {
                        if (boneweight[vertW.VertexID] >= 4)
                        {
                            Logger.Error("Vertex bone weigth overflow");
                        }
                        else
                        {
                            vertexRigged3D[vertW.VertexID].BoneIndices[boneweight[vertW.VertexID]] = index;
                            vertexRigged3D[vertW.VertexID].BoneWeigths[boneweight[vertW.VertexID]] = vertW.Weight;
                            boneweight[vertW.VertexID]++;
                        }
                    }

                }

                //Normalizing bone weigth
                for (int i = 0; i < meshData.VertexCount; i++)
                {
                    if (vertexRigged3D[i].BoneWeigths == Vector4.Zero)
                        vertexRigged3D[i].BoneWeigths.X = 1;
                    else
                        vertexRigged3D[i].BoneWeigths /= vertexRigged3D[i].BoneWeigths.X + vertexRigged3D[i].BoneWeigths.Y + vertexRigged3D[i].BoneWeigths.Z + vertexRigged3D[i].BoneWeigths.W;
                }

                var mesh = new Mesh(vertexRigged3D, indexes);

                Morph[] morphs = null;
                if (meshData.HasMeshAnimationAttachments)
                {
                    morphs = ReadMorphs(meshData, mesh);
                }

                

                MeshDrawer[m] = new MeshDrawerRigged(mesh, new Material[] { materials[meshData.MaterialIndex] }, boneControll, morphs?.ToList());
                m++;
            }

        }

        Morph[] ReadMorphs(Assimp.Mesh meshData, Mesh mesh)
        {
            
            var morphs = new Morph[meshData.MeshAnimationAttachmentCount];
            for (int i = 0; i < morphs.Length; i++)
            {
                int k = 0;
                var anim = meshData.MeshAnimationAttachments[i];
                Console.WriteLine("{0} {1} {2} {3}", anim.Name, anim.Vertices.Count, anim.Weight, anim.VertexCount);
                morphs[i] = new MorphVertex(anim.Name, "", anim.VertexCount, mesh.GetMorpher);

                //considering same vertex order
                for (int j = 0; j < anim.Vertices.Count; j++)
                {
                    var vert = Convert(anim.Vertices[j]);
                    var vertChange = vert - mesh.Vertices[j].Position;
                    //skip unchanged vertices
                    if (vertChange.LengthSquared < 0.000000001f)
                        continue;

                    k++;
                    ((MorphVertex)morphs[i]).AddVertex(vertChange, j);
                }

                //Console.WriteLine("skipped {0} from {1}", k, anim.Vertices.Count);
            }
            return morphs;
        }

        public TextureWrapMode Convert(Assimp.TextureWrapMode wrapMode)
        {
            TextureWrapMode result;
            switch (wrapMode)
            {
                case Assimp.TextureWrapMode.Clamp:
                    result = TextureWrapMode.ClampToEdge; break;
                case Assimp.TextureWrapMode.Wrap:
                    result = TextureWrapMode.ClampToBorder; break;
                case Assimp.TextureWrapMode.Mirror:
                    result = TextureWrapMode.MirrorRepeat; break;
                case Assimp.TextureWrapMode.Decal:
                    result = TextureWrapMode.Repeat; break;
                default:
                    result = TextureWrapMode.ClampToEdge;
                    break;
            }
            return result;
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

        public ModelPrefab GetModel
        {
            get
            {
                List<Component> comps = new List<Component>();
                foreach (MeshDrawerRigged mesh in MeshDrawer)
                {
                    comps.Add(mesh);
                }
                comps.Add(new Animator(boneControll));
                return new ModelPrefab(comps);
            }
        }

        public Morph[] GetMorphes
        {
            get
            {
                return null;
            }
        }
    }
}
