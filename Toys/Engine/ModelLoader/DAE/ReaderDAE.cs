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
		const float multiplier = 0.1f;

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
			// обход всех узлов в корневом элементе


			Console.WriteLine("start parsing");
			var meshreader = new DAEMeshLoader(xRoot);
			mesh = meshreader.LoadMesh();

			Console.WriteLine("mesh ok");
			//bones
			LoadBones(xRoot);
			Console.WriteLine("bones ok");
			//materials
			var daemats = new DAEMaterialReader(xRoot);
			mats = daemats.GetMaterials();
			Console.WriteLine("mats ok");
			for (int i = 0; i < mats.Length; i++)
			{
				var materialMesh = meshreader.dgc.Find( (mat) => mat.mat + "_mat" == mats[i].Name);
				mats[i].count = materialMesh.indeces.Length;
				mats[i].offset = materialMesh.offset;
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
				getBone(scene[0].FindNodes("node")[0],0);
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

				bones[i].childs = childs.ToArray();
			}

		}

		void getBone(XmlNode xmlnode, int index)
		{

			int parent = bones.Count;
			string name = xmlnode.Attributes.GetNamedItem("sid").Value;
			string matrtx = xmlnode.FindNodes("matrix")[0].InnerText;
			float[] fls = StringParser.readFloat(matrtx,multiplier);
			Matrix4 mat = Matrix4.Identity;
			mat.Row0 = new Vector4(fls[0], fls[1], fls[2], fls[3]);
			mat.Row1 = new Vector4(fls[4], fls[5], fls[6], fls[7]);
			mat.Row2 = new Vector4(fls[8], fls[9], fls[10], fls[11]);
			mat.Row3 = new Vector4(fls[12], fls[13], fls[14], fls[15]);

			Bone bone = new Bone(name, mat, index);
			bones.Add(bone);

			var bonesNodes = xmlnode.FindNodes("node");
			foreach (var node in bonesNodes)
				getBone(node,parent);
			
		}

		public Model GetModel
		{
			get
			{
				return null;
			}
		}

		public Model GetRiggedModel
		{
			get
			{
				
				MeshDrawer md = new MeshDrawer(mesh, mats);
				md.OutlineDrawing = true;
				return new Model(md, bones.ToArray()); 

			}
		}
	}
}
