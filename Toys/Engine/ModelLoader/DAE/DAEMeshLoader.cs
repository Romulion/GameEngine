using System;
using System.Xml;
using OpenTK;
using System.Collections.Generic;
using System.Linq;

namespace Toys
{
	public class DAEMeshLoader
	{
		public List<DAEGeometryContainer> dgc;
		XmlNode xGeometry = null;
		const string nodeName = "library_geometries";

		float max = 0f;

		public DAEMeshLoader(XmlElement xRoot)
		{
			foreach (XmlNode xnode in xRoot)
			{
				if (xnode.Name == nodeName)
				{
					xGeometry = xnode;
					break;
				}
			}

			if (xGeometry == null)
				throw new Exception();

			dgc = new List<DAEGeometryContainer>(xGeometry.ChildNodes.Count);
			foreach (XmlNode mesh in xGeometry.ChildNodes)
			{
				ReadMesh(mesh);
			}

			foreach (XmlNode mesh in xRoot.FindNodes("library_controllers")[0].ChildNodes)
			{
			         ReadWeigth(mesh);
			}
			Console.WriteLine(max);
		}

		void ReadMesh(XmlNode geometry)
		{
			string vertID = "";
			string normID = "";
			string textID = "";
			string mat = "";
			XmlNode mesh = geometry.FirstChild;
			DAEGeometryContainer gc = null;

			//Console.WriteLine("{0}", mesh.Attributes.GetNamedItem("name").Value);

			//initializing mesh data
			foreach (XmlNode source in mesh.FindNodes("triangles"))
			{
				var cnt = source.Attributes.GetNamedItem("count").Value;
				int[] indexes = null;
				mat = source.Attributes.GetNamedItem("material").Value;


				foreach (XmlNode input in source.ChildNodes)
				{
					if (input.Name == "input")
					{
						string src = input.Attributes.GetNamedItem("source").Value;
						src = src.Replace("#","");
						switch (input.Attributes.GetNamedItem("semantic").Value)
						{
							case "VERTEX":
								vertID = src;
								break;
							case "NORMAL":
								normID = src;
								break;
							case "TEXCOORD":
								textID = src;
								break;
						}
					}
					else if (input.Name == "p")
						indexes = StringParser.readInt(input.InnerText);
				}

				gc = new DAEGeometryContainer((uint)indexes.Max() + 1);
				dgc.Add(gc);
				gc.indeces = indexes;

			}

			//
			gc.id = geometry.Attributes.GetNamedItem("id").Value;
			gc.name = geometry.Attributes.GetNamedItem("name").Value;
			gc.mat = mat;
			//vertices
			XmlNode vertS = mesh.FindId(vertID);
			var inpts = vertS.FindNodes("input");
			if (inpts[0].Attributes != null && inpts[0].Attributes.GetNamedItem("semantic") != null)
			{
				
				if (inpts[0].Attributes.GetNamedItem("semantic").Value == "POSITION")
				{
					string src = inpts[0].Attributes.GetNamedItem("source").Value;

					XmlNode vert = mesh.FindId(src.Replace("#",""));

					var flp = vert.FindNodes("float_array");

					float[] flsp = StringParser.readFloat(flp[0].InnerText, 0.01f);
					max = (flsp.Max() > max) ? flsp.Max() : max;
					int io = 0;
					for (int n = 0; n < flsp.Length && io < gc.position.Length; n += 3)
					{
						gc.position[io] = new Vector3(flsp[n], flsp[n + 1], flsp[n + 2]);
						io++;
					}

				}
				else
					Console.WriteLine("wrong position semantic type: {0}", inpts[0].Attributes.GetNamedItem("semantic").Value);

			}


			//get normals
			XmlNode norm = mesh.FindId(normID);
			var fl = norm.FindNodes("float_array");
			float[] fls = StringParser.readFloat(fl[0].InnerText);
			int i = 0;
			for (int n = 0; n < fls.Length && i < gc.normals.Length; n += 3)
			{
				gc.normals[i] = new Vector3(fls[n], fls[n + 1], fls[n + 2]);
				gc.normals[i].Normalize();
				i++;
			}

			//get texcoord
			XmlNode uvtex = mesh.FindId(textID);
			fl = norm.FindNodes("float_array");
			fls = StringParser.readFloat(fl[0].InnerText);
			i = 0;
			for (int n = 0; n < fls.Length && i < gc.uvcord.Length; n += 2)
			{
				//Console.WriteLine("{0}  {1}", fls[n], fls[n + 1]);
				gc.uvcord[i] = new Vector2(fls[n] , fls[n + 1]);
				i++;
			}
		}


		void ReadWeigth(XmlNode controller)
		{
			XmlNode skin = controller.FirstChild;

			string geometryID = skin.Attributes.GetNamedItem("source").Value;
			var geometry = dgc.Find(inst => inst.id == geometryID.Replace("#","") );

			var weighth = skin.FindNodes("vertex_weights");
			var inputs = weighth[0].FindNodes("input");


			int[] bindindCount = StringParser.readInt(weighth[0].FindNodes("vcount")[0].InnerText);

			int[] bindedBones = StringParser.readInt(weighth[0].FindNodes("v")[0].InnerText);

			string weigthID = "";
			foreach (XmlNode inpt in inputs)
			{
				if (inpt.Attributes.GetNamedItem("semantic").Value == "WEIGHT")
					weigthID = inpt.Attributes.GetNamedItem("source").Value;
			}

			XmlNode srcs = skin.FindId(weigthID.Replace("#",""));


			if (srcs == null)
				Console.WriteLine("not found");
			
			float[] weigths = StringParser.readFloat(srcs.FindNodes("float_array")[0].InnerText);

			//assigning
			int offset = 0;
			for (int i = 0; i < bindindCount.Length; i++)
			{
				int count = bindindCount[i];
				switch (count)
				{
					case 1:
						geometry.boneIndeces[i] = new IVector4(new int[] { bindedBones[offset],0,0,0 });
						geometry.weigth[i] = new Vector4( weigths[bindedBones[offset + 1]], 0f, 0f, 0f);
						break;
					case 2:
						geometry.boneIndeces[i] = new IVector4(new int[] { bindedBones[offset],bindedBones[offset + 2],0,0 });
						geometry.weigth[i] = new Vector4(weigths[bindedBones[offset + 1]], weigths[bindedBones[offset + 3]], 0f, 0f);
						break;
					case 3:
						geometry.boneIndeces[i] = new IVector4(new int[] { bindedBones[offset],bindedBones[offset + 2],bindedBones[offset + 4],0 });
						geometry.weigth[i] = new Vector4(weigths[bindedBones[offset + 1]], weigths[bindedBones[offset + 3]], weigths[bindedBones[offset + 5]], 0f);
						break;
					case 4:
						geometry.boneIndeces[i] = new IVector4(new int[] { bindedBones[offset],bindedBones[offset + 2],bindedBones[offset + 4],bindedBones[offset + 6]});
						geometry.weigth[i] = new Vector4(weigths[bindedBones[offset + 1]], weigths[bindedBones[offset + 3]],weigths[bindedBones[offset + 5]], weigths[bindedBones[offset + 7]]);
						break;
				}
				geometry.weigth[i].Normalize();
				offset += count * 2;
			}


		}


		public Mesh LoadMesh()
		{
			List<VertexRigged> verts = new List<VertexRigged>();
			List<int> indeces = new List<int>();

			int offset = 0;
			foreach (var gc in dgc)
			{
				for (int n = 0; n < gc.position.Length; n++)
				{
					verts.Add(new VertexRigged(gc.position[n], gc.normals[n], gc.uvcord[n], gc.boneIndeces[n], gc.weigth[n]));
					Console.WriteLine(gc.uvcord[n]);
				}
				foreach (int index in gc.indeces)
				{
					indeces.Add(index + offset);
				}


				offset += gc.position.Length;
			}


			Mesh mesh = new Mesh(verts.ToArray(), indeces.ToArray());
			return mesh;
		}



	}
}
