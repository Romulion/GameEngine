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
        AssimpTools Tools = new AssimpTools();

        Material[] materials;
        MeshDrawerRigged[] MeshDrawer = null;
        BoneController boneControll;
        string dir = "";
        float sizeMultiplier = 1f;
        public ReaderFBX(string path) 
        { 
            AssimpContext importer = new AssimpContext();
            var scene = importer.ImportFile(path);

            PrepareArmaure(scene);
            LoadMaterial(scene);
            LoadMeshes(scene.Meshes);
        }

        public ReaderFBX(Assimp.Scene scene, string path)
        {
            dir = path;
            PrepareArmaure(scene);
            LoadMaterial(scene);
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
            PrepareArmaure(scene);
            LoadMaterial(scene);
            LoadMeshes(scene.Meshes);
        }

        void PrepareArmaure(Assimp.Scene scene)
        {
            //Get Armature Nodes
            bones = Tools.GetArmaure(scene);
            boneControll = new BoneController(bones.ToArray(), true);
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
                mater.Name = mat.Name;
                
                if (mat.HasColorAmbient)
                    mater.UniformManager.Set("ambient_color", Tools.Convert(mat.ColorAmbient).Xyz);
                if (mat.HasColorDiffuse)
                    mater.UniformManager.Set("diffuse_color", Tools.Convert(mat.ColorDiffuse));
                if (mat.HasColorSpecular)
                {
                    mater.UniformManager.Set("specular_color", Tools.Convert(mat.ColorSpecular).Xyz);
                    mater.UniformManager.Set("specular_power", Tools.Convert(mat.ColorSpecular).W);
                }       
                
                mater.SetTexture(textureDiffuse, TextureType.Diffuse);

                materials[i] = mater;
            }
        }

        void LoadMeshes(List<Assimp.Mesh> meshes)
        {

            MeshDrawer = new MeshDrawerRigged[meshes.Count];
            int m = 0;
            foreach (var meshData in meshes)
            {
                var vertexRigged3D = new VertexRigged3D[meshData.VertexCount];
                var boneweight = new int[meshData.VertexCount];
                var indexes = meshData.GetIndices().ToArray();
                var vertexes = meshData.Vertices.ToArray();

                for (int i = 0; i < meshData.VertexCount; i++)
                    vertexRigged3D[i].Position = Tools.Convert(vertexes[i]) * sizeMultiplier;

                if (meshData.HasNormals)
                {
                    var normals = meshData.Normals;
                    for (int i = 0; i < meshData.VertexCount; i++)
                        vertexRigged3D[i].Normal = Tools.Convert(normals[i]);
                }

                if (meshData.HasTextureCoords(0))
                {
                    var uvs = meshData.TextureCoordinateChannels[0];
                    for (int i = 0; i < meshData.VertexCount; i++)
                    {
                        vertexRigged3D[i].UV = Tools.Convert(uvs[i]).Xy;
                        vertexRigged3D[i].UV.Y = 1 - vertexRigged3D[i].UV.Y;
                    }
                }

                foreach (var bone in meshData.Bones)
                {
                    var index = bones.FindIndex((bon) => bon.Name == bone.Name);
                    foreach (var vertW in bone.VertexWeights)
                    {
                        if (boneweight.Length < vertW.VertexID) {
                            //Console.WriteLine($"{materials[meshData.MaterialIndex].Name} {bone.Name} {vertW.VertexID}");
                            continue; 
                        }

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
                morphs[i] = new MorphVertex(anim.Name, "", anim.VertexCount, mesh.GetMorpher);

                //considering same vertex order
                for (int j = 0; j < anim.Vertices.Count; j++)
                {
                    var vert = Tools.Convert(anim.Vertices[j]);
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
