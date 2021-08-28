using System;
using System.Xml;
using OpenTK.Mathematics;
using System.Collections.Generic;
using System.Linq;

namespace Toys
{
	public class DAEMeshLoader
	{
		public List<DAEGeometryContainer> DAEGeometry;
		XmlNode libraryGeometries = null;
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
					libraryGeometries = xnode;
					break;
				}
			}
            if (libraryGeometries == null)
				throw new Exception();
            var scenesRoot = xRoot.FindNodes(sceneNodeName);
            var visualScenes = scenesRoot[0].FindNodes(visualscene);
            var visualNodes = new List<XmlNode>();
            foreach (var scene in visualScenes)
            {
                
                FindVisualNodes(scene, visualNodes);
            }
            MakeControllerMatDict(visualNodes);
            DAEGeometry = new List<DAEGeometryContainer>(libraryGeometries.ChildNodes.Count);
            foreach (XmlNode mesh in libraryGeometries.ChildNodes)
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
            //Read Bones
            var nod = xRoot.FindNodes("library_controllers");
            if (nod.Length > 0)
            {
                foreach (XmlNode mesh in nod[0].ChildNodes)
                {
                    //skip morphes
                    try
                    {
                        ReadWeigth(mesh);
                    }
                    catch (Exception) { }
                }
            }
            /*
            //Read Materials
            foreach (var meshInst in DAEGeometry)
            {
                for (int i = 0; i < meshInst.MaterialIndexTable.Count; i++)
                {
                    //Console.WriteLine(i);
                    //Console.WriteLine("{0} {1}",meshInst.Indeces.Length, meshInst.ControllerId + ";" + meshInst.MaterialIndexTable[i].Item1);
                    meshInst.MaterialIndexTable[i] = new Tuple<string, int>(controllerMaterialRef[meshInst.MaterialIndexTable[i].Item1], meshInst.MaterialIndexTable[i].Item2);
                }
            }
            */
        }

        void ReadMesh(XmlNode geometry)
        {
            XmlNode mesh = geometry.FirstChild;
            string PositionID = "";
            string NormalID = "";
            string TextureID = "";
            DAEGeometryContainer gc = new DAEGeometryContainer();
            List<int> indexes = new List<int>();

            //initializing mesh data
            int vertexCount = 0;
            foreach (XmlNode source in mesh.FindNodes("triangles"))
            {
                var materialReference = new DAEMaterialBinding();
                var cnt = source.Attributes.GetNamedItem("count").Value;
                materialReference.Count = int.Parse(cnt) * 3;
                vertexCount += materialReference.Count;

                

                //check material present
                XmlNode nod = source.Attributes.GetNamedItem("material");
                if (nod != null)
                    materialReference.Name = nod.Value;
                foreach (XmlNode input in source.ChildNodes)
                {
                    if (input.Name == "input")
                    {
                        string src = input.Attributes.GetNamedItem("source").Value;
                        src = src.Replace("#", "");
                        switch (input.Attributes.GetNamedItem("semantic").Value)
                        {
                            case "VERTEX":
                                materialReference.OffsetPosition = int.Parse(input.Attributes.GetNamedItem("offset").Value);
                                PositionID = src;
                                break;
                            case "NORMAL":
                                materialReference.OffsetNormal = int.Parse(input.Attributes.GetNamedItem("offset").Value);
                                NormalID = src;
                                break;
                            case "TEXCOORD":
                                //skip additional uv maps
                                if (input.Attributes.GetNamedItem("set") == null || input.Attributes.GetNamedItem("set").Value == "0")
                                {
                                    materialReference.OffsetUV = int.Parse(input.Attributes.GetNamedItem("offset").Value);
                                    TextureID = src;
                                }
                                break;
                        }
                    }
                    else if (input.Name == "p")
                        materialReference.Indexes = StringParser.readIntArray(input.InnerText);
                }
                gc.MaterialIndexTable.Add(materialReference);
            }

            gc.Colors = new Vector3[vertexCount];
            gc.BoneWeigths = new Vector4[vertexCount];
            gc.BoneIndeces = new IVector4[vertexCount];
            gc.Indeces = new int[vertexCount];

            //gc.Indeces = indexes.ToArray();
            gc.ID = geometry.Attributes.GetNamedItem("id").Value;
            gc.Name = geometry.Attributes.GetNamedItem("name").Value;
            //if (gc.Name != "tr0029_00_hairSkin")
            //    return;
            DAEGeometry.Add(gc);

            //Create vertexes from idexes


            XmlNode vertS = mesh.FindId(PositionID);
            var inpts = vertS.FindNodes("input");
            if (inpts[0].Attributes != null && inpts[0].Attributes.GetNamedItem("semantic") != null)
            {

                if (inpts[0].Attributes.GetNamedItem("semantic").Value == "POSITION")
                {
                    string src = inpts[0].Attributes.GetNamedItem("source").Value;

                    XmlNode vert = mesh.FindId(src.Replace("#", ""));

                    var flp = vert.FindNodes("float_array");

                    float[] flsp = StringParser.readFloatArray(flp[0].InnerText);
                    gc.Positions = new Vector3[flsp.Length/3];
                    //max = (flsp.Max() > max) ? flsp.Max() : max;
                    //Console.WriteLine(flsp[2182 * 3]);
                    for (int n = 0; n < flsp.Length; n += 3)
                    {
                       // Console.WriteLine(n);
                        gc.Positions[n /3] = new Vector3(flsp[n], flsp[n + 1], flsp[n + 2]);
                    }
                }
                else
                    logger.Warning("wrong position semantic type: " + inpts[0].Attributes.GetNamedItem("semantic").Value, "DAEMeshLoader.ReadMesh");

            }
            int i = 0;
            //get normals
            if (NormalID != "")
            {
                XmlNode norm = mesh.FindId(NormalID);
                var fl = norm.FindNodes("float_array");
                float[] fls = StringParser.readFloatArray(fl[0].InnerText);
                gc.Normals = new Vector3[fls.Length];
                for (int n = 0; n < fls.Length && i < gc.Normals.Length; n += 3)
                {
                    gc.Normals[i] = new Vector3(fls[n], fls[n + 1], fls[n + 2]);
                    gc.Normals[i].Normalize();
                    i++;
                }
            }
            else
            {
                for (int n = 0; i < gc.Normals.Length; n += 3)
                {
                    gc.Normals[i] = Vector3.Zero;
                    i++;
                }
            }
            //get texcoord
            if (TextureID != "")
            {
                XmlNode uvtex = mesh.FindId(TextureID);
                //Console.WriteLine(textID);
                var accessor = uvtex.FindNodes("technique_common")[0].FirstChild;
                int stride = int.Parse(accessor.Attributes.GetNamedItem("stride").Value);

                var fluv = uvtex.FindNodes("float_array");
                float[] fls1 = StringParser.readFloatArray(fluv[0].InnerText);
                gc.UVs = new Vector2[fls1.Length];
                i = 0;
                for (int n = 0; n < fls1.Length && i < gc.UVs.Length; n += stride)
                {
                    gc.UVs[i] = new Vector2(fls1[n], 1 - fls1[n + 1]);
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
                    //if (instance == null)
                        
                    var instance = node.FindNodes("instance_controller");
                    if (instance.Length > 0)
                    {
                        var controller = instance[0].Attributes.GetNamedItem("url").Value;
                        var bm = instance[0].FindNodes("bind_material")[0];
                        var tc = bm.FindNodes("technique_common")[0];
                        var im = tc.FindNodes("instance_material");
                        foreach (var inMat in im)
                        {
                            var materialID = inMat.Attributes.GetNamedItem("target").Value;
                            var symbol = inMat.Attributes.GetNamedItem("symbol").Value;
                            controllerMaterialRef.Add(symbol, materialID.Remove(0, 1));
                            //Console.WriteLine("{0} {1}", controller.Remove(0, 1) + ";" + symbol, materialID.Remove(0, 1));
                        }
                    }
                    else
                    {
                        var geometry =  node.FindNodes("instance_geometry");
                        var bm = geometry[0].FindNodes("bind_material")[0];
                        var tc = bm.FindNodes("technique_common")[0];
                        var im = tc.FindNodes("instance_material");
                        foreach (var inMat in im)
                        {
                            var materialID = inMat.Attributes.GetNamedItem("target").Value;
                            var symbol = inMat.Attributes.GetNamedItem("symbol").Value;
                            controllerMaterialRef.Add(symbol, materialID.Remove(0, 1));
                            //Console.WriteLine("{0} {1}", symbol, materialID.Remove(0, 1));
                        }
                    }
                }
                catch (Exception e) 
                {
                    //Console.WriteLine(e.StackTrace);
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
		public Mesh[] LoadMesh()
		{
            Mesh[] mesh = new Mesh[DAEGeometry.Count];
            

            for (int i = 0; i < DAEGeometry.Count; i++)
			{
                var verts = new VertexRigged3D[DAEGeometry[i].Indeces.Length];
                
                int offset = 0;
                foreach (var mat in DAEGeometry[i].MaterialIndexTable)
                {
                    mat.Offset = offset;
                    for (int n = 0; n < mat.Count; n++)
                    {
                        var indx = mat.Indexes;
                        var mul = indx.Length / mat.Count;
                        verts[offset] = new VertexRigged3D(DAEGeometry[i].Positions[indx[n * mul + mat.OffsetPosition]], 
                            DAEGeometry[i].Normals[indx[n * mul + mat.OffsetNormal]], DAEGeometry[i].UVs[indx[n * mul + mat.OffsetUV]], 
                            new IVector4(new int[4] { 0, 0, 0, 0 }), Vector4.Zero);
                        DAEGeometry[i].Indeces[offset] = offset++;
                    }

                }
                mesh[i] = new Mesh(verts, DAEGeometry[i].Indeces);
                
            }            
			return mesh;
		}
        
	}
}
