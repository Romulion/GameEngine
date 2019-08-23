using System;
using System.IO;
using OpenTK;
using System.Windows.Forms;
using System.Diagnostics;

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
		RigidContainer[] rigitbodies;
		JointContainer[] joints;

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
            Console.WriteLine(meshSize);
            
			VertexRigged3D [] verticesR = new VertexRigged3D[meshSize];
			Vertex3D[] vertices = new Vertex3D[meshSize];
			for (int i = 0; i < meshSize; i++)
			{
				Vector3 pos = reader.readVector3() * multipler;
				Vector3 normal = reader.readVector3() * multipler;
				Vector2 uv = reader.readVector2();
				//mirroring x axis
				//pos.X = -pos.X;
				//normal.X = -normal.X;
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
                indexes[i] = reader.readVal(header.GetVertexIndexSize);
                //invering triangles
                /*
				int res = i % 3;
				if (res == 0)
					indexes[i + 1] = reader.readVal(header.GetVertexIndexSize);
				else if (res == 1)
					indexes[i - 1] = reader.readVal(header.GetVertexIndexSize);
				else 
					indexes[i] = reader.readVal(header.GetVertexIndexSize);
			    */
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
				Texture tex = ResourcesManager.LoadAsset<Texture>(texture);

				if (texture.Contains("toon"))
					tex.ChangeType(TextureType.Toon);
				//textures[i] = new Texture(dir + texture, TextureType.Toon, texture);
				else
				{
					tex.ChangeType(TextureType.Diffuse);
					tex.ChangeWrapper(Texture.Wrapper.Repeat);
				}
					//textures[i] = new Texture(dir + texture, TextureType.Diffuse, texture, false);
				textures[i] = tex;
                //Console.WriteLine("{0}  {1}",i, texture);

            }
		}

		void ReadMaterial()
		{
			string txt = "";
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
                shdrs.DifuseColor = true;


                shdrs.TextureDiffuse = true;
				string name = reader.readString();
				reader.readString(); //eng name

				Vector4 difColor = reader.readVector4();
                //if (difColor.W == 0)//diffuse color
				//	rndr.render = false;

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
                    //Console.WriteLine("{0} {1}",i, envTexIndex);
                    if (textures[envTexIndex].name != "def")
                        envTex = textures[envTexIndex];
                    else
                        shdrs.envType = 0;

                }
                else
                    shdrs.envType = 0;



                byte toonType = file.ReadByte();
				
				Texture toon = empty;
				if (toonType == 0)
				{
					int text = reader.readVal(header.GetTextureIndexSize);
					if (text != 255)
					{
						shdrs.toonShadow = true;
                        textures[text].ChangeWrapper(Texture.Wrapper.ClampToEdge);
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

                mat.UniManager.Set("diffuse_color", difColor);
                //mat.SetValue(difColor,"diffuse_color");
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

                int ParentIndex = 0;
                if (header.GetBoneIndexSize == 2)
                {
                    ParentIndex = unchecked((short)reader.readVal(header.GetBoneIndexSize));
                }
                else
                    ParentIndex = reader.readVal(header.GetBoneIndexSize);

                int Level = file.ReadInt32();
				byte[] flags = file.ReadBytes(2);
				Bone bone = new Bone(Name, NameEng, Position, ParentIndex, flags);
				bone.Level = Level;
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
					bone.localSpace = new Matrix4(local) * Matrix4.CreateTranslation(Position);
				}
				if (bone.LocalCoordinate)
				{
					Vector3 X = reader.readVector3();
					Vector3 Z = reader.readVector3();
					Vector3 Y = Vector3.Cross(Z, X);
					Matrix3 local = new Matrix3(X, Y, Z);
					bone.localSpace = new Matrix4(local) * Matrix4.CreateTranslation(Position);
				}

                //set if not set
                if (bone.localSpace == default(Matrix4))
                {
                    bone.localSpace = Matrix4.CreateTranslation(bone.Position);
                }

				if (bone.ExternalPdeform)
				{
					reader.readVal(header.GetBoneIndexSize);
				}

				if (bone.IK)
				{
                    BoneIK bik = new BoneIK();
					bik.Target = reader.readVal(header.GetBoneIndexSize);
					bik.LoopCount = file.ReadInt32();
					bik.AngleLimit = file.ReadSingle();
					int count = file.ReadInt32();
                    IKLink[] links = new IKLink[count];
					for (int n = 0; n < count; n++)
					{
                        IKLink link = new IKLink();
						link.Bone = reader.readVal(header.GetBoneIndexSize);
						if (file.ReadByte() == 1)
						{
                            link.IsLimit = true;
							link.LimitMin = reader.readVector3();
							link.LimitMax = reader.readVector3();
						}
                        links[n] = link;
					}
                    bik.Links = links;
				    bone.IKData = bik;
				}

                //if (i == 75 || i == 221)
                //    Console.WriteLine(bone.Position);
                bones[i] = bone;
			}

            //convert position from model to parent bone
            for (int i = bones.Length - 1; i > -1; i--)
            {
                Bone bone = bones[i];
                if (bone.ParentIndex > 0 && bone.ParentIndex < bones.Length){
                    bone.Position -= bones[bone.ParentIndex].Position;
                }
            }
			//Bone.MakeChilds(bones);
		}

		void ReadMorhps()
		{
			int morphCount = file.ReadInt32();
			morphs = new Morph[morphCount];

			for (int i = 0; i < morphCount; i++)
			{
				//List<Vector4> vertex_morph = new List<Vector4>();
				string name = reader.readString();
				string nameEng = reader.readString();

				int panel = file.ReadByte();
				int type = file.ReadByte();
				int size = file.ReadInt32();

				if (type == (int)MorphType.Vertex)
				{
					morphs[i] = new MorphVertex(name, nameEng, size);
					((MorphVertex)morphs[i]).meshMorpher = meshRigged.GetMorpher;
				}
				else if (type == (int)MorphType.Material)
					morphs[i] = new MorphMaterial(name, nameEng, size);
				else if (type == (int)MorphType.Uv)
				{
					morphs[i] = new MorphUV(name, nameEng, size);
					((MorphUV)morphs[i]).meshMorpher = meshRigged.GetMorpher;
				}

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
                            ((MorphVertex)morphs[i]).AddVertex(pos, index);
                            //invert x axis
                            //((MorphVertex)morphs[i]).AddVertex(new Vector3(-pos.X, pos.Y, pos.Z), index);
                            //vertex_morph = new Vector4(pos, index);
                            break;
						case 2:  //bone morph
							reader.readVal(header.GetBoneIndexSize);
							reader.readVector3();
							reader.readVector4();
							break;
						case 3:  //uv
							int vIndex = reader.readVal(header.GetVertexIndexSize);
							var value = reader.readVector4();
							((MorphUV)morphs[i]).AddVertex(new Vector2(value.X, value.Y), vIndex);
							break;
						case 8: //material
                            
							int idx = reader.readVal(header.GetMaterialIndexSize);
                            Material mat = null;
                            if (idx == 255)
                                mat = (Material)mats[0];
                            else
                                mat = (Material)mats[idx];
                            var MMorpher = new MaterialMorpher(mat);
                            MMorpher.mode = file.ReadByte();
							MMorpher.diffuse = reader.readVector4();
							reader.readVector3();
							file.ReadSingle();
							reader.readVector3();
							reader.readVector4();
							file.ReadSingle();
							reader.readVector4();
							reader.readVector4();
							reader.readVector4();
                            ((MorphMaterial)morphs[i]).matMorpher.Add(MMorpher);

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

        /*
         * not used
         */
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
			rigitbodies = new RigidContainer[readCount];
			RigidContainer rigit;
			for (int i = 0; i<readCount; i++)
			{
				rigit = new RigidContainer();
				rigit.Name = reader.readString();
                rigit.NameEng = reader.readString();
				rigit.BoneIndex = reader.readVal(header.GetBoneIndexSize);
				rigit.GroupId = file.ReadByte();
                rigit.NonCollisionGroup =  file.ReadInt16();
				rigit.primitive = (PhysPrimitiveType)file.ReadByte();
				rigit.Size = reader.readVector3() * multipler;
				rigit.Position = reader.readVector3() * multipler;
				rigit.Rotation = reader.readVector3();
				rigit.Mass = file.ReadSingle();
				rigit.MassAttenuation = file.ReadSingle() ;
				rigit.RotationDamping = file.ReadSingle() * multipler;
				rigit.Restitution = file.ReadSingle();
				rigit.Friction = file.ReadSingle();
				rigit.Phys = (PhysType)file.ReadByte();

                //inverting x axis
                //rigit.Position = new Vector3(-rigit.Position.X, rigit.Position.Y, rigit.Position.Z);

                rigitbodies[i] = rigit;
			}
		}

		void ReadJoints()
		{
            int jointCount = file.ReadInt32();
			joints = new JointContainer[jointCount];

			for (int i = 0; i<jointCount; i++)
			{
				JointContainer joint = new JointContainer();
				joint.Name = reader.readString();
				joint.NameEng = reader.readString();
				joint.jType =  (JointType)file.ReadByte();
				joint.RigitBody1 =  reader.readVal(header.GetRigidBodyIndexSize);
				joint.RigitBody2 =  reader.readVal(header.GetRigidBodyIndexSize);
				joint.Position = reader.readVector3() * multipler;
				joint.Rotation = reader.readVector3();
				joint.PosMin = reader.readVector3() * multipler;
				joint.PosMax = reader.readVector3() * multipler;
				joint.RotMin = reader.readVector3();
				joint.RotMax = reader.readVector3();
				joint.PosSpring = reader.readVector3() * multipler;
				joint.RotSpring = reader.readVector3();

                //inverting x axis
                //joint.Position = new Vector3(-joint.Position.X, joint.Position.Y, joint.Position.Z);
                if (i >= 90 && i < 100)
                {
                    //joint.Position += new Vector3(0,0,0.1f);
                    //   Console.WriteLine(joint.Position);
                }
                    joints[i] = joint;
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

		public SceneNode GetModel
		{
			get 
			{ 
				MeshDrawer md = new MeshDrawer(mesh, mats);
				md.OutlineDrawing = true;
				var node = new SceneNode();
				node.AddComponent(md);

				return node; 
			}
		}

		public SceneNode GetRiggedModel
		{
			get
			{
				/*
				MeshDrawer md = new MeshDrawer(meshRigged, mats);
				md.OutlineDrawing = true;

				var node = new SceneNode();
				node.model = md;
				node.anim = new BoneController(bones);
				node.morph = morphs;
                node.phys = new PhysicsManager(rigitbodies, joints, node.anim, node.GetTransform);
				return node; 
*/
				MeshDrawerRigged md = new MeshDrawerRigged(meshRigged, mats,new BoneController(bones), morphs);
				md.OutlineDrawing = true;

				var node = new SceneNode();
				node.AddComponent(md);
				node.AddComponent(new Animator(md.skeleton));
				node.phys = (new PhysicsManager(rigitbodies, joints, md.skeleton, node.GetTransform));
				return node;

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
