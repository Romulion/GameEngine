using System;
using System.Xml;
using OpenTK;
using System.Collections.Generic;
using System.Linq;

namespace Toys
{
	public class DAEMeshLoader
	{
		public List<DAEGeometryContainer> DAEGeometry;
		XmlNode xGeometry = null;
		const string nodeName = "library_geometries";
        Logger logger;
		float max = 0f;

		public DAEMeshLoader(XmlElement xRoot)
		{
            logger = new Logger("DAE mesh loader");
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

			DAEGeometry = new List<DAEGeometryContainer>(xGeometry.ChildNodes.Count);
			foreach (XmlNode mesh in xGeometry.ChildNodes)
			{
                try
                {
                    ReadMesh(mesh);
                }
                catch (Exception e)
                {
                    logger.Warning("Error Parsing Mesh", e.Source);
                }
			}

			foreach (XmlNode mesh in xRoot.FindNodes("library_controllers")[0].ChildNodes)
			{
			    ReadWeigth(mesh);
			}
		}

		void ReadMesh(XmlNode geometry)
		{
			string vertID = "";
			string normID = "";
			string textID = "";
			string mat = "";
			XmlNode mesh = geometry.FirstChild;
			DAEGeometryContainer gc = null;

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
				DAEGeometry.Add(gc);
				gc.Indeces = indexes;

			}

			//
			gc.ID = geometry.Attributes.GetNamedItem("id").Value;
			gc.Name = geometry.Attributes.GetNamedItem("name").Value;
			gc.MaterialName = mat;
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
					for (int n = 0; n < flsp.Length && io < gc.Positions.Length; n += 3)
					{
						gc.Positions[io] = new Vector3(flsp[n], flsp[n + 1], flsp[n + 2]);
						io++;
					}

				}
				else
					Console.WriteLine("wrong position semantic type: {0}", inpts[0].Attributes.GetNamedItem("semantic").Value);

			}

			int i = 0;
			//get normals
			if (normID != "")
			{
				XmlNode norm = mesh.FindId(normID);
				var fl = norm.FindNodes("float_array");

				float[] fls = StringParser.readFloat(fl[0].InnerText);

				for (int n = 0; n < fls.Length && i < gc.Normals.Length; n += 3)
				{
					gc.Normals[i] = new Vector3(fls[n], fls[n + 1], fls[n + 2]);
					gc.Normals[i].Normalize();
					i++;
				}
			}
			else 
			{
				for (int n = 0; i<gc.Normals.Length; n += 3)
				{
					gc.Normals[i] = Vector3.Zero;
					i++;
				}
			}

            //get texcoord
            if (textID != "")
            {
                XmlNode uvtex = mesh.FindId(textID);
                var fluv = uvtex.FindNodes("float_array");
                float[] fls1 = StringParser.readFloat(fluv[0].InnerText);
                i = 0;
                //Console.WriteLine(fls1.Length);

                for (int n = 0; n < fls1.Length && i < gc.UVs.Length; n += 2)
                {
                    gc.UVs[i] = new Vector2(fls1[n], fls1[n + 1]);
                    i++;
                }
            }
        }


		void ReadWeigth(XmlNode controller)
		{
			XmlNode skin = controller.FirstChild;

			string geometryID = skin.Attributes.GetNamedItem("source").Value;
			var geometry = DAEGeometry.Find(inst => inst.ID == geometryID.Replace("#","") );

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
						geometry.BoneIndeces[i] = new IVector4(new int[] { bindedBones[offset],0,0,0 });
						geometry.BoneWeigths[i] = new Vector4( weigths[bindedBones[offset + 1]], 0f, 0f, 0f);
						break;
					case 2:
						geometry.BoneIndeces[i] = new IVector4(new int[] { bindedBones[offset],bindedBones[offset + 2],0,0 });
						geometry.BoneWeigths[i] = new Vector4(weigths[bindedBones[offset + 1]], weigths[bindedBones[offset + 3]], 0f, 0f);
						break;
					case 3:
						geometry.BoneIndeces[i] = new IVector4(new int[] { bindedBones[offset],bindedBones[offset + 2],bindedBones[offset + 4],0 });
						geometry.BoneWeigths[i] = new Vector4(weigths[bindedBones[offset + 1]], weigths[bindedBones[offset + 3]], weigths[bindedBones[offset + 5]], 0f);
						break;
					case 4:
						geometry.BoneIndeces[i] = new IVector4(new int[] { bindedBones[offset],bindedBones[offset + 2],bindedBones[offset + 4],bindedBones[offset + 6]});
						geometry.BoneWeigths[i] = new Vector4(weigths[bindedBones[offset + 1]], weigths[bindedBones[offset + 3]],weigths[bindedBones[offset + 5]], weigths[bindedBones[offset + 7]]);
						break;
				}
				//geometry.BoneWeigths[i].Normalize();
				offset += count * 2;
			}


		}


		public Mesh LoadMesh()
		{
			List<VertexRigged3D> verts = new List<VertexRigged3D>();
			List<int> indeces = new List<int>();

			int offset = 0;
			int vertsOffset = 0;
			foreach (var gc in DAEGeometry)
			{
				for (int n = 0; n < gc.Positions.Length; n++)
				{
					verts.Add(new VertexRigged3D(gc.Positions[n], gc.Normals[n], gc.UVs[n], gc.BoneIndeces[n], gc.BoneWeigths[n]));
				}
				foreach (int index in gc.Indeces)
				{
					indeces.Add(index + vertsOffset);
				}

				gc.Offset = offset;
				vertsOffset += gc.Positions.Length;
				offset += gc.Indeces.Length;
			}

			Mesh mesh = new Mesh(verts.ToArray(), indeces.ToArray());
			return mesh;
		}

	}
}
