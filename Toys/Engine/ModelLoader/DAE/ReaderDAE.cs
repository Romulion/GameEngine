using System;
using System.Xml;
using System.Collections.Generic;
using OpenTK.Mathematics;

namespace Toys
{
    public class ReaderDAE : IModelLoader
    {
        string file;
        XmlDocument xDoc;
        string dir;
        Mesh[] mesh;
        Material[][] mats;
        List<Bone> bones = new List<Bone>();
        const float multiplier = 0.01f;
        DAEMeshLoader meshreader;

        public ReaderDAE(string filename)
        {
            file = filename;
            int indx = filename.LastIndexOf('\\');
            if (indx >= 0)
                dir = filename.Substring(0, indx) + '\\';
            else
                dir = "";
            xDoc = new XmlDocument();
            xDoc.Load(filename);
            LoadLibraries();
        }

        void LoadLibraries()
        {
            XmlElement xRoot = xDoc.DocumentElement;
            meshreader = new DAEMeshLoader(xRoot);
            mesh = meshreader.LoadMesh();
            //bones
            LoadBones(xRoot);
            //materials
            var daemats = new DAEMaterialReader(xRoot, dir);
            var matsList = daemats.GetMaterials();
            mats = new Material[meshreader.DAEGeometry.Count][];
            for (int i = 0; i < mesh.Length; i++)
            {
                var meshItem = meshreader.DAEGeometry[i];
                var materials = meshItem.MaterialIndexTable;
                mats[i] = new Material[materials.Count];
                
                for (int n = 0; n < materials.Count; n++)
                {
                    if (materials[n].Name == "")
                    {
                        //mats[i][n] = Material("default");
                        continue;
                    }
                        

                    var matname = daemats.materialIDReference[materials[n].Name];
                    var matTemplate = matsList.Find((obj) => obj.Name == matname);

                    var mat = matTemplate.Clone();
                    mat.Name = matname;
                    mat.UniformManager.Set("ambient_color", Vector3.One);
                    mat.Count = materials[n].Count;
                    mat.Offset = materials[n].Offset;
                    mats[i][n] = mat;                    
                }
            }
        }

        void LoadBones(XmlElement xRoot)
        {
            string node = "library_visual_scenes";
            List<Bone> bons = new List<Bone>();
            XmlNode xBones = null;
            foreach (XmlNode xnode in xRoot)
            {
                if (xnode.Name == node)
                {
                    xBones = xnode;
                    break;
                }
            }


            var scene = xBones.FindNodes("visual_scene");

            if (scene.Length > 0)
            {
                getBone(scene[0].FindAttrib("JOINT", "type"), -1);
            }

            //set childs
            for (int i = 0; i < bones.Count; i++)
            {
                List<int> childs = new List<int>();
                for (int n = 0; n < bones.Count; n++)
                    if (bones[n].ParentIndex == i)
                    {
                        childs.Add(n);
                    }

                bones[i].Childs = childs.ToArray();
            }
        }

        //Obsolette
        public Dictionary<string, MeshData> GetMeshes()
        {
            var meshes = new Dictionary<string, MeshData>();
            for (int i = 0; i< mesh.Length; i++)
            {
                List<Vertex3D> verts = new List<Vertex3D>();
                List<int> indeces = new List<int>();
                var gc = meshreader.DAEGeometry[i];
                for (int n = 0; n < gc.Positions.Length; n++)
                {
                    verts.Add(new Vertex3D(gc.Positions[n], Vector3.Zero, Vector2.Zero));
                }
                for (int m = 0; m < gc.MaterialIndexTable[0].Indexes.Length; m +=3)
                {
                    indeces.Add(gc.MaterialIndexTable[0].Indexes[m]);
                }
                var mesh = new MeshData();
                mesh.vertices = verts.ToArray();
                mesh.indeces = indeces.ToArray();
                meshes.Add(gc.Name, mesh);
            }
            return meshes;
        }


		void getBone(XmlNode xmlNode, int parent)
		{
            if (xmlNode == null)
                return;

            int index = bones.Count;
            string name = xmlNode.Attributes.GetNamedItem("name").Value;
            var matrtx = xmlNode.FindNodes("matrix");
            Matrix4 mat = Matrix4.Identity;
            if (matrtx.Length != 0)
            {
                float[] fls = StringParser.readFloatArray(matrtx[0].InnerText);
                
                mat.Row0 = new Vector4(fls[0], fls[1], fls[2], fls[3] * multiplier);
                mat.Row1 = new Vector4(fls[4], fls[5], fls[6], fls[7] * multiplier);
                mat.Row2 = new Vector4(fls[8], fls[9], fls[10], fls[11] * multiplier);
                mat.Row3 = new Vector4(fls[12], fls[13], fls[14], fls[15]);
                mat.Transpose();
            }
            else if (xmlNode.FindNodes("skew").Length != 0)
                throw new Exception("node skew transform not implemented");
            else if (xmlNode.FindNodes("lookat").Length != 0)
                throw new Exception("node lookat transform not implemented");
            else
            {
                var rotate = xmlNode.FindNodes("rotate");
                var scale = xmlNode.FindNodes("scale");
                var translate = xmlNode.FindNodes("translate");
                if (rotate.Length != 0)
                {
                    var rot = StringParser.readFloatArray(rotate[0].InnerText);
                    mat *= Matrix4.CreateFromQuaternion(Quaternion.FromAxisAngle(new Vector3(rot[0], rot[1], rot[2]), (float) (rot[3] * Math.PI / 180)));
                }
                if (translate.Length != 0)
                {
                    var rot = StringParser.readFloatArray(translate[0].InnerText);
                    mat *= Matrix4.CreateTranslation(new Vector3(rot[0], rot[1], rot[2]));
                }
                if (scale.Length != 0)
                {
                    var rot = StringParser.readFloatArray(scale[0].InnerText);
                    mat *= Matrix4.CreateScale(new Vector3(rot[0], rot[1], rot[2]));
                }

            }

            Bone bone = new Bone(name, mat, parent);
            bone.Index = index;
            bones.Add(bone);
            var bonesNodes = xmlNode.FindNodes("node");
			foreach (var node in bonesNodes)
				getBone(node,index);

        }

        /*
        void SetGlobalLocalSpace(Bone bone, Matrix4 parentalSpace)
        {
           
             = bone.Parent2Local * parentalSpace;

            foreach (int child in bone.childs)
                SetGlobalLocalSpace(bones[child], bone.localSpace);

        }
        */

		public SceneNode GetModel
		{
            get
            {
                var node = new SceneNode();
                for (int i = 0; i < mesh.Length; i++)
                    if (bones.Count > 0)
                    {
                        MeshDrawerRigged md = new MeshDrawerRigged(mesh[i], mats[i], new BoneController(bones.ToArray(), true));
                        md.OutlineDrawing = true;
                        node.AddComponent(md);
                        node.AddComponent(new Animator(md.skeleton));
                    }
                    else
                    {
                        MeshDrawer md = new MeshDrawer(mesh[i], mats[i]);
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
                for (int i = 0; i < mesh.Length; i++)
                {
                    MeshDrawerRigged md = new MeshDrawerRigged(mesh[i], mats[i], new BoneController(bones.ToArray(), true));
                    md.OutlineDrawing = true;
                    node.AddComponent(md);
                    node.AddComponent(new Animator(md.skeleton));
                }
				return node; 

			}
		}

        //no morphes support
        public Morph[] GetMorphes
        {
            get
            {
                return new Morph[0];
            }
        }

    }
}