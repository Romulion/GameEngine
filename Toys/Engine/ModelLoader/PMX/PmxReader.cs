using System;
using System.IO;
using OpenTK;
using System.Windows.Forms;

namespace Toys
{
	public class PmxReader : IModelLoader
	{
		Header header;
		Texture[] textures;
		Texture empty;
		IMaterial[] mats;
		Bone[] bones;
		public Morph[] morphs;
		float multipler = 0.1f;
		RigitContainer[] rigitbodies;

		BinaryReader file;

		Mesh mesh;
		Mesh meshRigged;

		Reader reader;
		string dir;


		public PmxReader(string path)
		{
			int indx = path.LastIndexOf('/');
            if (indx >= 0)
                dir = path.Substring(0, indx) + '/';
            else
                dir = "";

			empty = Texture.LoadEmpty();
			Stream fs = File.OpenRead(path);
			file = new BinaryReader(fs);
			reader = new Reader(file);

			ReadHeader();
            ReadMesh();
			ReadTextures();
			ReadMaterial();
			ReadBones();
            ReadMorhps();
			ReadPanel();
			ReadRigit();
			ReadJoints();
			file.Close();
		}

		void ReadHeader()
		{
			header = new Header();
			string signature = new String(file.ReadChars(4));
			float version = file.ReadSingle();
			int length = file.ReadByte();
			header.attribs = file.ReadBytes(length);
			header.Name = reader.readString();
			header.NameEng = reader.readString();
			header.Comment = reader.readString();
			header.CommentEng = reader.readString();

			reader.encoding = header.GetEncoding;
		}


		void ReadMesh()
		{
			
			int meshSize = file.ReadInt32();
			VertexRigged3D [] verticesR = new VertexRigged3D[meshSize];
			Vertex3D[] vertices = new Vertex3D[meshSize];
			for (int i = 0; i < meshSize; i++)
			{
				Vector3 pos = reader.readVector3() * multipler;
				Vector3 normal = reader.readVector3() * multipler;
				Vector2 uv = reader.readVector2();
				//mirroring x axis
				pos.X = -pos.X;
				normal.X = -normal.X;
				int[] bonesIndexes = {0,0,0,0};
				Vector4 bonesWeigth = new Vector4(0f);

				//skipping appendix
				if (header.GetAppendixUV != 0)
				{
					for (int n = 0; n < header.GetAppendixUV; n++)
						reader.readVector4();
				}

				//bones
				byte Weigth = file.ReadByte();
				switch (Weigth)
				{
					case 0: //BDEF
						bonesIndexes[0] = reader.readVal(header.GetBoneIndexSize);
						bonesWeigth[0] = 1f;
						break;
					case 1: //BDEF2
						
						bonesIndexes[0] = reader.readVal(header.GetBoneIndexSize);
						bonesIndexes[1] = reader.readVal(header.GetBoneIndexSize);
						bonesWeigth[0] = file.ReadSingle();
						bonesWeigth[1] = 1f - bonesWeigth[0];
						break;
					case 2: //BDEF4
						bonesIndexes[0] = reader.readVal(header.GetBoneIndexSize);
						bonesIndexes[1] = reader.readVal(header.GetBoneIndexSize);
						bonesIndexes[2] = reader.readVal(header.GetBoneIndexSize);
						bonesIndexes[3] = reader.readVal(header.GetBoneIndexSize);
						bonesWeigth[0] = file.ReadSingle();
						bonesWeigth[1] = file.ReadSingle();
						bonesWeigth[2] = file.ReadSingle();
						bonesWeigth[3] = file.ReadSingle();
						break;
					case 3: //SDEF
						bonesIndexes[0] = reader.readVal(header.GetBoneIndexSize);
						bonesIndexes[1] = reader.readVal(header.GetBoneIndexSize);
						bonesWeigth[0] = file.ReadSingle();
						bonesWeigth[1] = 1f - bonesWeigth[0];
						reader.readVector3();
						reader.readVector3();
						reader.readVector3();
						break;
					case 4: //QDEF
						bonesIndexes[0] = reader.readVal(header.GetBoneIndexSize);
						bonesIndexes[1] = reader.readVal(header.GetBoneIndexSize);
						bonesIndexes[2] = reader.readVal(header.GetBoneIndexSize);
						bonesIndexes[3] = reader.readVal(header.GetBoneIndexSize);
						bonesWeigth[0] = file.ReadSingle();
						bonesWeigth[1] = file.ReadSingle();
						bonesWeigth[2] = file.ReadSingle();
						bonesWeigth[3] = file.ReadSingle();
						break;
					default:
						throw new Exception("Not suppornet weigth code " + Weigth);
				}

				verticesR[i] = new VertexRigged3D(pos, normal, uv,new IVector4(bonesIndexes),bonesWeigth);
				vertices[i] = new Vertex3D(pos, normal, uv);
				float outline = file.ReadSingle();

			}
			int indexSize = file.ReadInt32();
			int[] indexes = new int[indexSize];
			for (int i = 0; i < indexSize; i++)
			{
				//invering triangles
				int res = i % 3;
				if (res == 0)
					indexes[i + 1] = reader.readVal(header.GetVertexIndexSize);
				else if (res == 1)
					indexes[i - 1] = reader.readVal(header.GetVertexIndexSize);
				else 
					indexes[i] = reader.readVal(header.GetVertexIndexSize);
			
			}

			mesh = new Mesh(vertices, indexes);
			meshRigged = new Mesh(verticesR, indexes);
		}

		void ReadTextures()
		{
			//Console.Write(Encoding.Default);
			int texCount = file.ReadInt32();
			textures = new Texture[texCount];
			for (int i = 0; i < texCount; i++)
			{
				string texture = reader.readString();
				if (texture.Contains("toon"))
					textures[i] = new Texture(dir + texture, TextureType.Toon, texture);
				else
					textures[i] = new Texture(dir + texture, TextureType.Diffuse, texture);
			}
		}

		void ReadMaterial()
		{
			int materiaCount = file.ReadInt32();
			int offset = 0;
			mats = new IMaterial[materiaCount];
			for (int i = 0; i < materiaCount; i++)
			{
				ShaderSettings shdrs = new ShaderSettings();
				RenderDirectives rndr = new RenderDirectives();

				shdrs.hasSkeleton = true;
				shdrs.discardInvisible = true;
				shdrs.affectedByLight = true;


				shdrs.TextureDiffuse = true;
				string name = reader.readString();
				reader.readString(); //eng name
				if (reader.readVector4().W == 0)//diffuse color
					rndr.render = false;
				reader.readVector3(); //specular color
				file.ReadSingle(); //specular
				reader.readVector3(); //ambient color
				//setting values from flags
				var flags = new MaterialFlags(file.ReadByte());
				shdrs.recieveShadow = flags.receiveShadow;
				shdrs.affectedByLight = flags.receiveShadow;
				rndr.castShadow = flags.drawShadow;
				rndr.hasEdges = flags.hasEdge;
				rndr.nocull = flags.noCull;

				var outln = new Outline();
				outln.EdgeColour = reader.readVector4();
				outln.EdgeScaler = file.ReadSingle() * 0.3f;

				int difTexIndex = reader.readVal(header.GetTextureIndexSize);

				//sphericar texture for false light sources effect
				int envTexIndex = reader.readVal(header.GetTextureIndexSize);
				int envBlend = file.ReadByte();
				shdrs.envType = (EnvironmentMode)envBlend;
				Texture envTex = empty;
				if (envTexIndex != 255 && envBlend > 0)
				{
					envTex = textures[envTexIndex];
				}


				byte toonType = file.ReadByte();
				
				Texture toon = empty;
				if (toonType == 0)
				{
					int text = reader.readVal(header.GetTextureIndexSize);
					if (text != 255)
					{
						shdrs.toonShadow = true;
						toon = textures[text];
						//toon.GetTextureType = TextureType.toon;

					}
					else
					{
						//disable shadowing if no toon texture
						shdrs.affectedByLight = false;
						shdrs.recieveShadow = false;
					}
						//MessageBox.Show(name + " " + text);
				}
				else
				{
					byte toontex = file.ReadByte();
					shdrs.toonShadow = true;
				}
                reader.readString();
				int count = file.ReadInt32();
				Texture tex = empty;
				if (difTexIndex != 255)
				{
					tex = textures[difTexIndex];
				}
				var mat = new Material(shdrs, rndr);
				mat.Name = name;
				mat.outln = outln;
				mat.SetTexture(tex,TextureType.Diffuse);
				mat.SetTexture(toon, TextureType.Toon);
				mat.SetTexture(envTex, TextureType.Sphere);
				/* old material class
				MaterialPMX mat = new MaterialPMX();
				mat.Name = reader.readString();
				mat.NameEng = reader.readString();
				mat.DiffuseColor = reader.readVector4();
				if (mat.DiffuseColor.W == 0)
					mat.dontDraw = true;
				mat.SpecularColour = reader.readVector3();
				mat.Specular = file.ReadSingle();
				mat.AmbientColour = reader.readVector3();
				mat.SetFlags = file.ReadByte();
				mat.EdgeColour = reader.readVector4();
				mat.EdgeScaler = file.ReadSingle();
				//texture
				int texture =  reader.readVal(header.GetTextureIndexSize);
				reader.readVal(header.GetTextureIndexSize);
				int blend = file.ReadByte();
				byte toonType = file.ReadByte();
				Texture toon = empty;

				if (toonType == 0)
				{
					int text = reader.readVal(header.GetTextureIndexSize);
					if (text != 255)
						toon = textures[text];
				}
				else
					 file.ReadByte();

                reader.readString();
				int count = file.ReadInt32();
				Texture tex = empty;
				if (texture != 255)
				{
					tex = textures[texture];
				}
				mat.textures = new Texture[] {tex, toon};
				mat.offset = offset;
				mat.count = count;

				*/

				mat.offset = offset;
				mat.count = count;
				mats[i] = mat;
				offset += count;
			}


		}

		void ReadBones()
		{
			int bonesCount = file.ReadInt32();
			bones = new Bone[bonesCount];
			for (int i = 0; i < bonesCount; i++)
			{
				string Name = reader.readString();
				string NameEng = reader.readString();
				Vector3 Position = reader.readVector3() * multipler;
				int ParentIndex = reader.readVal(header.GetBoneIndexSize);
				int Layer = file.ReadInt32();
				byte[] flags = file.ReadBytes(2);
				Bone bone = new Bone(Name, NameEng, Position, ParentIndex, flags);
				bone.Layer = Layer;
				if (bone.tail)
					reader.readVal(header.GetBoneIndexSize);
				else
					reader.readVector3();

				if (bone.InheritRotation || bone.InheritTranslation)
				{
					bone.ParentInheritIndex = reader.readVal(header.GetBoneIndexSize);
					bone.ParentInfluence = file.ReadSingle();
				}

				//probably cant be both true
				if (bone.FixedAxis)
				{
					Vector3 X = reader.readVector3();
					Vector3 Z = Vector3.Cross(X, new Vector3(0f, 1f, 0f));
					Vector3 Y = Vector3.Cross(Z, X);
					Matrix3 local = new Matrix3(X,Y,Z);
					bone.localSpace = new Matrix4(local);
				}
				if (bone.LocalCoordinate)
				{
					Vector3 X = reader.readVector3();
					Vector3 Z = reader.readVector3();
					Vector3 Y = Vector3.Cross(Z, X);
					Matrix3 local = new Matrix3(X, Y, Z);
					bone.localSpace = new Matrix4(local);
				}

				if (bone.ExternalPdeform)
				{
					reader.readVal(header.GetBoneIndexSize);
				}

				if (bone.IK)
				{
					reader.readVal(header.GetBoneIndexSize);
					file.ReadInt32();
					file.ReadSingle();
					int count = file.ReadInt32();
					for (int n = 0; n < count; n++)
					{
						reader.readVal(header.GetBoneIndexSize);
						if (file.ReadByte() == 1)
						{
							reader.readVector3();
							reader.readVector3();
						}

					}
						
				}

				bones[i] = bone;
			}
			Bone.MakeChilds(bones);
		}

		void ReadMorhps()
		{
			int morphCount = file.ReadInt32();
			morphs = new Morph[morphCount];
			//Vector4 vertex_morphs = new Vector4();
			for (int i = 0; i < morphCount; i++)
			{
				//List<Vector4> vertex_morph = new List<Vector4>();
				string name = reader.readString();
				string nameEng = reader.readString();

				int panel = file.ReadByte();
				int type = file.ReadByte();
				int size = file.ReadInt32();

				if (type == 1)
					morphs[i] = new MorphVertex(name, nameEng, size);

				for (int n = 0; n < size; n++)
				{
					switch (type)
					{
						case 0: //group
							reader.readVal(header.GetMorphIndexSize);
							file.ReadSingle();
							break;
						case 1: //vertex
							int index = reader.readVal(header.GetVertexIndexSize);
							Vector3 pos = reader.readVector3() * multipler;
							((MorphVertex)morphs[i]).AddVertex(new Vector3(-pos.X, pos.Y, pos.Z), index);
                            ((MorphVertex)morphs[i]).meshMorpher = meshRigged.GetMorpher;
                            //vertex_morph = new Vector4(pos, index);
                            break;
						case 2:  //bone morph
							reader.readVal(header.GetBoneIndexSize);
							reader.readVector3();
							reader.readVector4();
							break;
						case 3:  //uv
							reader.readVal(header.GetVertexIndexSize);
							reader.readVector4();
							break;
						case 8: //material
							reader.readVal(header.GetMaterialIndexSize);
							file.ReadByte();
							reader.readVector4();
							reader.readVector3();
							file.ReadSingle();
							reader.readVector3();
							reader.readVector4();
							file.ReadSingle();
							reader.readVector4();
							reader.readVector4();
							reader.readVector4();
							break;
						case 9: //flip
							reader.readVal(header.GetMaterialIndexSize);
							file.ReadSingle();
							break;
						case 10: //impulse
							reader.readVal(header.GetRigidBodyIndexSize);
							file.ReadByte();
							reader.readVector3();
							reader.readVector3();
							break;
						default:
							Console.WriteLine(type);
							break;
					}
				}

			}



		}

		void ReadPanel()
		{
			int panelCount = file.ReadInt32();
			for (int i = 0; i < panelCount; i++)
			{
				reader.readString();
				reader.readString();
				file.ReadByte();
				int count = file.ReadInt32();
				for (int n = 0; n < count; n++)
				{ 
					int type = file.ReadByte();
					if (type == 0)
						reader.readVal(header.GetBoneIndexSize);
					else if (type == 1)
						reader.readVal(header.GetMorphIndexSize);
							
				}
			}
		}

		/// <summary>
		/// reading rigidbodies data
		/// </summary>
		void ReadRigit()
		{
			int readCount = file.ReadInt32();
			rigitbodies = new RigitContainer[readCount];
			RigitContainer rigit;
			for (int i = 0; i<readCount; i++)
			{
				rigit = new RigitContainer();
				reader.readString();
				reader.readString();
				rigit.BoneIndex = reader.readVal(header.GetBoneIndexSize);
				rigit.GroupId = file.ReadByte();
				rigit.NonCollisionGroup =  file.ReadInt16();
				rigit.primitive = (PhysPrimitiveType)file.ReadByte();
				rigit.Size = reader.readVector3();
				rigit.Position = reader.readVector3() * multipler;
				rigit.Rotation = reader.readVector3();
				rigit.Mass = file.ReadSingle();
				rigit.MassAttenuation = file.ReadSingle();
				rigit.RotationDamping = file.ReadSingle();
				rigit.Repulsion = file.ReadSingle();
				rigit.Friction = file.ReadSingle();
				rigit.Phys = (PhysType)file.ReadByte();
				rigitbodies[i] = rigit;
			}
		}

		void ReadJoints()
		{
			int jointCount = file.ReadInt32();

			for (int i = 0; i<jointCount; i++)
			{
				reader.readString();
				reader.readString();
				file.ReadByte();
				reader.readVal(header.GetRigidBodyIndexSize);
				reader.readVal(header.GetRigidBodyIndexSize);

				reader.readVector3();
				reader.readVector3();
				reader.readVector3();
				reader.readVector3();
				reader.readVector3();
				reader.readVector3();
				reader.readVector3();
				reader.readVector3();

			}

		}

		//properties
		//
		public Mesh GetMesh{
			get {return mesh; }
		}

		public Mesh GetMeshRigged
		{
			get { return meshRigged; }
		}

		public Texture[] GetTextures
		{
			get { return textures; }
		}

		public IMaterial[] GetMaterials
		{
			get { return mats; }
		}

		public Model GetModel
		{
			get 
			{ 
				MeshDrawer md = new MeshDrawer(mesh, mats);
				md.OutlineDrawing = true;
				return new Model(md, bones, morphs); 
			}
		}

		public Model GetRiggedModel
		{
			get 
			{
				MeshDrawer md = new MeshDrawer(meshRigged, mats);
				md.OutlineDrawing = true;
				return new Model(md, bones, morphs); 
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
