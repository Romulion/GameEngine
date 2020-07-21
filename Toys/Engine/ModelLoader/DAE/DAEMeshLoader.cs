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
        const string sceneNodeName = "library_visual_scenes";
        const string visualscene = "visual_scene";
        Logger logger;
		float max = 0f;
        Dictionary<string, string> controllerMaterialRef = new Dictionary<string, string>();

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
            var scenesRoot = xRoot.FindNodes(sceneNodeName);
            var scenes = scenesRoot[0].FindNodes(visualscene);
            var visualNodes = new List<XmlNode>();
            foreach (var scene in scenes)
            {
                FindVisualNodes(scene, visualNodes);
            }

            MakeControllerMatDict(visualNodes);

            DAEGeometry = new List<DAEGeometryContainer>(xGeometry.ChildNodes.Count);
			foreach (XmlNode mesh in xGeometry.ChildNodes)
			{
                try
                {
                    ReadMesh(mesh);
                }
                catch (Exception e)
                {
                    logger.Warning("Error Parsing Mesh" + e.StackTrace, e.StackTrace);
                }
			}


            foreach (XmlNode mesh in xRoot.FindNodes("library_controllers")[0].ChildNodes)
			{
                //skip morphes
                try
                {
                    ReadWeigth(mesh);
                }
                catch (Exception ) {  }
            }

            foreach (var meshInst in DAEGeometry)
            {
                for (int i = 0; i < meshInst.MaterialIndexTable.Count; i++)
                {
                    //Console.WriteLine("{0} {1}",meshInst.Indeces.Length, meshInst.ControllerId + ";" + meshInst.MaterialIndexTable[i].Item1);
                    meshInst.MaterialIndexTable[i] = new Tuple<string, int>(controllerMaterialRef[meshInst.ControllerId + ";" + meshInst.MaterialIndexTable[i].Item1], meshInst.MaterialIndexTable[i].Item2);
                }
            }
        }

		void ReadMesh(XmlNode geometry)
		{
			string vertID = "";
			string normID = "";
			string textID = "";
			
			XmlNode mesh = geometry.FirstChild;
			DAEGeometryContainer gc = new DAEGeometryContainer();
            List<int> indexes = new List<int>();
            //initializing mesh data
            foreach (XmlNode source in mesh.FindNodes("triangles"))
			{
                var cnt = source.Attributes.GetNamedItem("count").Value;
                string mat = source.Attributes.GetNamedItem("material").Value;
                int offsets = 0;
                int[] indexesNew = null;
                foreach (XmlNode input in source.ChildNodes)
				{
                    if (input.Name == "input")
                    {
                        string src = input.Attributes.GetNamedItem("source").Value;
                        src = src.Replace("#", "");

                        switch (input.Attributes.GetNamedItem("semantic").Value)
                        {
                            case "VERTEX":

                                vertID = src;
                                break;
                            case "NORMAL":
                                var off = int.Parse(input.Attributes.GetNamedItem("offset").Value);
                                if (offsets < off)
                                    offsets++;
                                normID = src;
                                break;
                            case "TEXCOORD":
                                var ofs = int.Parse(input.Attributes.GetNamedItem("offset").Value);
                                if (offsets < ofs)
                                    offsets++;
                                textID = src;
                                break;
                        }
                    }
                    else if (input.Name == "p")
                    {
                        indexesNew = ReduceIndexes(StringParser.readIntArray(input.InnerText),offsets);
                        indexes.AddRange(indexesNew);
                    }
				}

                gc.MaterialIndexTable.Add(new Tuple<string, int>(mat, indexesNew.Length));
            }

            int maxIndex = indexes.Max() + 1;
            gc.Positions = new Vector3[maxIndex];
            gc.Normals = new Vector3[maxIndex];
            gc.UVs = new Vector2[maxIndex];
            gc.Colors = new Vector3[maxIndex];
            gc.BoneWeigths = new Vector4[maxIndex];
            gc.BoneIndeces = new IVector4[maxIndex];

            gc.Indeces = indexes.ToArray();
            gc.ID = geometry.Attributes.GetNamedItem("id").Value;
            gc.Name = geometry.Attributes.GetNamedItem("name").Value;
            //if (gc.Name != "tr0029_00_hairSkin")
            //    return;
            DAEGeometry.Add(gc);
            
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

					float[] flsp = StringParser.readFloatArray(flp[0].InnerText, 0.01f);
					max = (flsp.Max() > max) ? flsp.Max() : max;
					int io = 0;
					for (int n = 0; n < flsp.Length && io < gc.Positions.Length; n += 3)
					{
						gc.Positions[io] = new Vector3(flsp[n], flsp[n + 1], flsp[n + 2]);
						io++;
					}

				}
				else
                    logger.Warning("wrong position semantic type: " + inpts[0].Attributes.GetNamedItem("semantic").Value, "DAEMeshLoader.ReadMesh");

			}

			int i = 0;
			//get normals
			if (normID != "")
			{
				XmlNode norm = mesh.FindId(normID);
				var fl = norm.FindNodes("float_array");

				float[] fls = StringParser.readFloatArray(fl[0].InnerText);

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

                var accessor = uvtex.FindNodes("technique_common")[0].FirstChild;
                int stride = int.Parse(accessor.Attributes.GetNamedItem("stride").Value);

                var fluv = uvtex.FindNodes("float_array");
                float[] fls1 = StringParser.readFloatArray(fluv[0].InnerText);
                i = 0;

                for (int n = 0; n < fls1.Length && i < gc.UVs.Length; n += stride)
                {
                    gc.UVs[i] = new Vector2( fls1[n], 1 - fls1[n + 1]);
                    i++;
                }
            }
        }

		void ReadWeigth(XmlNode controller)
		{
            
            XmlNode skin = controller.FirstChild;

			string geometryID = skin.Attributes.GetNamedItem("source").Value;
			var geometry = DAEGeometry.Find(inst => inst.ID == geometryID.Replace("#","") );
            geometry.ControllerId = controller.Attributes.GetNamedItem("id").Value;
            var weighth = skin.FindNodes("vertex_weights");
			var inputs = weighth[0].FindNodes("input");

            int[] bindindCount = StringParser.readIntArray(weighth[0].FindNodes("vcount")[0].InnerText);

			int[] bindedBones = StringParser.readIntArray(weighth[0].FindNodes("v")[0].InnerText);

			string weigthID = "";
			foreach (XmlNode inpt in inputs)
			{
				if (inpt.Attributes.GetNamedItem("semantic").Value == "WEIGHT")
					weigthID = inpt.Attributes.GetNamedItem("source").Value;
			}

			XmlNode srcs = skin.FindId(weigthID.Replace("#",""));


            if (srcs == null)
                logger.Warning("weigth not found", "ReadWeigth");
			
			float[] weigths = StringParser.readFloatArray(srcs.FindNodes("float_array")[0].InnerText);

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
				offset += count * 2;
			}

		}

        private List<XmlNode> FindVisualNodes(XmlNode root, List<XmlNode> elemets)
        {
            var nodes = root.FindNodes("node");
            foreach (var node in nodes)
            {
                if (node.Attributes.GetNamedItem("type") == null || node.Attributes.GetNamedItem("type").Value == "NODE")
                {
                    elemets.Add(node);
                    FindVisualNodes(node,elemets);
                }
            }
            return elemets;
        }
        private void MakeControllerMatDict(List<XmlNode> nodes)
        {
            foreach (var node in nodes)
            {
                try
                {
                    var instance = node.FindNodes("instance_controller")[0];
                    var controller = instance.Attributes.GetNamedItem("url").Value;
                    var bm = instance.FindNodes("bind_material")[0];
                    var tc = bm.FindNodes("technique_common")[0];
                    var im = tc.FindNodes("instance_material");
                    foreach (var inMat in im)
                    {
                        var materialID = inMat.Attributes.GetNamedItem("target").Value;
                        var symbol = inMat.Attributes.GetNamedItem("symbol").Value;
                        controllerMaterialRef.Add(controller.Remove(0, 1) + ";" + symbol, materialID.Remove(0, 1));
                        //Console.WriteLine("{0} {1}", controller.Remove(0, 1) + ";" + symbol, materialID.Remove(0, 1));
                    }

                }
                catch (Exception e) { //Console.WriteLine(e.StackTrace);
                }
            }
        }

        private int[] ReduceIndexes(int[] raw, int offset)
        {
            int[] array = null;
            if (offset > 0)
            {
                array = new int[raw.Length / (offset + 1)];
                for (int i = 0; i < raw.Length; i += (offset + 1))
                    array[i / (offset + 1)] = raw[i];
            }
            else
            {
                return raw;
            }
            return array;
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
                //Console.WriteLine("{0} {1}", offset,  gc.Indeces.Length);
            }

			Mesh mesh = new Mesh(verts.ToArray(), indeces.ToArray());
            
			return mesh;
		}
        
	}
}
