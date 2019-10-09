using System;
using System.Xml;
using System.Collections.Generic;

using OpenTK;

namespace Toys
{
	public class ReaderDAE : IModelLoader
	{
		string file;
		XmlDocument xDoc;
		string dir;
		Mesh mesh;
		Material[] mats;
		List<Bone> bones = new List<Bone>();
		const float multiplier = 0.01f;
        int bonesCounter = 0;

		public ReaderDAE(string filename)
		{
			file = filename;
			int indx = filename.LastIndexOf('/');
            if (indx >= 0)
                dir = filename.Substring(0, indx) + '/';
            else
                dir = "";

			xDoc = new XmlDocument();
			xDoc.Load(filename);
			LoadLibraries();
		}

		void LoadLibraries()
		{
			XmlElement xRoot = xDoc.DocumentElement;

			var meshreader = new DAEMeshLoader(xRoot);
			mesh = meshreader.LoadMesh();

			//bones
			LoadBones(xRoot);
			//materials
			var daemats = new DAEMaterialReader(xRoot);
			var matsList = daemats.GetMaterials();

			mats = new Material[meshreader.DAEGeometry.Count];

			for (int i = 0; i < meshreader.DAEGeometry.Count; i++)
			{
				var meshItem = meshreader.DAEGeometry[i];
				var matTemplate = matsList.Find((obj) => obj.Name == meshItem.MaterialName + "_mat" );
				mats[i] = matTemplate.Clone();
				mats[i].Name = meshItem.Name;
				mats[i].Count = meshItem.Indeces.Length;
				mats[i].Offset = meshItem.Offset;
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
				getBone(scene[0].FindNodes("node")[0],-1);
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
            //set local to global space
            //SetGlobalLocalSpace(bones[0], Matrix4.Identity);

        }

		void getBone(XmlNode xmlNode, int parent)
		{

			int index = bones.Count;
			string name = xmlNode.Attributes.GetNamedItem("sid").Value;
			string matrtx = xmlNode.FindNodes("matrix")[0].InnerText;
			float[] fls = StringParser.readFloat(matrtx);
			Matrix4 mat = Matrix4.Identity;
			mat.Row0 = new Vector4(fls[0], fls[1], fls[2], fls[3] * multiplier);
			mat.Row1 = new Vector4(fls[4], fls[5], fls[6], fls[7] * multiplier);
			mat.Row2 = new Vector4(fls[8], fls[9], fls[10], fls[11] * multiplier);
			mat.Row3 = new Vector4(fls[12], fls[13], fls[14], fls[15]);

			mat.Transpose();
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
				return null;
			}
		}


		public SceneNode GetRiggedModel
		{
			get
			{
				
				MeshDrawerRigged md = new MeshDrawerRigged(mesh, mats,new BoneController(bones.ToArray(),true));
				md.OutlineDrawing = true;
				var node = new SceneNode();
				node.AddComponent(md);
				node.AddComponent(new Animator(md.skeleton));
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