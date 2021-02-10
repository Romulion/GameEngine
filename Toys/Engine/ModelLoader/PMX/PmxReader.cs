using System;
using System.IO;
using OpenTK.Mathematics;
using System.Diagnostics;
using System.Drawing;
using OpenTK.Graphics.OpenGL;

namespace Toys
{
	public class PmxReader : IModelLoader
	{
		Header header;
		Texture2D[] textures;
		Texture2D empty;
		Material[] mats;
		Bone[] bones;
		public Morph[] morphs;
		float multipler = 0.1f;
		RigidContainer[] rigitBodies;
		JointContainer[] joints;
        int[] boneOrder;

		Mesh mesh;
		Mesh meshRigged;

		string dir;


		public PmxReader(string path)
		{
			int indx = path.LastIndexOf('\\');
            if (indx >= 0)
                dir = path.Substring(0, indx) + '\\';
            else
                dir = "";
            empty = Texture2D.LoadEmpty();
			Stream fs = File.OpenRead(path);
			var reader = new Reader(fs);

			ReadHeader(reader);
            ReadMesh(reader);
            ReadTextures(reader);
            ReadMaterial(reader);
            ReadBones(reader);
            ReadMorhps(reader);
			ReadPanel(reader);
			ReadRigit(reader);
			ReadJoints(reader);
            reader.Close();
		}

		void ReadHeader(Reader reader)
		{
			header = new Header();
			string signature = new String(reader.ReadChars(4));
			float version = reader.ReadSingle();
			int length = reader.ReadByte();
			header.Attributes = reader.ReadBytes(length);
			header.Name = reader.readString();
			header.NameEng = reader.readString();
			header.Comment = reader.readString();
			header.CommentEng = reader.readString();

			reader.EncodingType = header.GetEncoding;
		}


		void ReadMesh(Reader reader)
		{
			int meshSize = reader.ReadInt32();            
			VertexRigged3D [] verticesR = new VertexRigged3D[meshSize];
			//Vertex3D[] vertices = new Vertex3D[meshSize];
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
				byte Weigth = reader.ReadByte();
				switch (Weigth)
				{
					case 0: //BDEF
						bonesIndexes[0] = reader.readVal(header.GetBoneIndexSize);
						bonesWeigth[0] = 1f;
						break;
					case 1: //BDEF2
						
						bonesIndexes[0] = reader.readVal(header.GetBoneIndexSize);
						bonesIndexes[1] = reader.readVal(header.GetBoneIndexSize);
						bonesWeigth[0] = reader.ReadSingle();
						bonesWeigth[1] = 1f - bonesWeigth[0];
						break;
					case 2: //BDEF4
						bonesIndexes[0] = reader.readVal(header.GetBoneIndexSize);
						bonesIndexes[1] = reader.readVal(header.GetBoneIndexSize);
						bonesIndexes[2] = reader.readVal(header.GetBoneIndexSize);
						bonesIndexes[3] = reader.readVal(header.GetBoneIndexSize);
						bonesWeigth[0] = reader.ReadSingle();
						bonesWeigth[1] = reader.ReadSingle();
						bonesWeigth[2] = reader.ReadSingle();
						bonesWeigth[3] = reader.ReadSingle();
						break;
					case 3: //SDEF
						bonesIndexes[0] = reader.readVal(header.GetBoneIndexSize);
						bonesIndexes[1] = reader.readVal(header.GetBoneIndexSize);
						bonesWeigth[0] = reader.ReadSingle();
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
						bonesWeigth[0] = reader.ReadSingle();
						bonesWeigth[1] = reader.ReadSingle();
						bonesWeigth[2] = reader.ReadSingle();
						bonesWeigth[3] = reader.ReadSingle();
						break;
					default:
						throw new Exception("Not suppornet weigth code " + Weigth);
				}

				verticesR[i] = new VertexRigged3D(pos, normal, uv,new IVector4(bonesIndexes),bonesWeigth);
				//vertices[i] = new Vertex3D(pos, normal, uv);
				float outline = reader.ReadSingle();

			}
			int indexSize = reader.ReadInt32();
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
			//mesh = new Mesh(vertices, indexes);
			meshRigged = new Mesh(verticesR, indexes);
		}

		void ReadTextures(Reader reader)
		{
			int texCount = reader.ReadInt32();
			textures = new Texture2D[texCount];
			for (int i = 0; i < texCount; i++)
			{
				string texture = reader.readString();
                Texture2D tex = ResourcesManager.LoadAsset<Texture2D>(dir+texture);

				if (texture.Contains("toon"))
					tex.ChangeType(TextureType.Toon);
				else
				{
					tex.ChangeType(TextureType.Diffuse);
					tex.WrapMode =TextureWrapMode.Repeat;
				}

				textures[i] = tex;
            }
		}

		void ReadMaterial(Reader reader)
		{
			int materiaCount = reader.ReadInt32();
			int offset = 0;
			mats = new Material[materiaCount];
			for (int i = 0; i < materiaCount; i++)
			{
				ShaderSettings shdrs = new ShaderSettings();
				RenderDirectives rndr = new RenderDirectives();

				shdrs.HasSkeleton = true;
				shdrs.DiscardInvisible = true;
				shdrs.AffectedByLight = true;
                shdrs.DifuseColor = true;
                shdrs.Ambient = true;
                shdrs.SpecularColor = true;

                shdrs.TextureDiffuse = true;
				string name = reader.readString();
				reader.readString(); //eng name

				Vector4 difColor = reader.readVector4();
                //if (difColor.W == 0)//diffuse color
				//	rndr.render = false;

				Vector3 specularColour = reader.readVector3(); //specular color
				float specularPower = reader.ReadSingle(); //specular
                //fix zero power bug
                if (specularPower == 0)
                    specularPower = 0.000001f;
                Vector3 ambientColour = reader.readVector3(); //ambient color
				//setting values from flags
				var flags = new MaterialFlags(reader.ReadByte());
				shdrs.RecieveShadow = flags.ReceiveShadow;
				shdrs.AffectedByLight = flags.ReceiveShadow;
				rndr.CastShadow = flags.CastShadow;
				rndr.HasEdges = flags.HasEdge;
				rndr.NoCull = flags.NoCull;
                
				var outln = new Outline();
				outln.EdgeColour = reader.readVector4();
				outln.EdgeScaler = reader.ReadSingle() * 0.3f;

				int difTexIndex = reader.readVal(header.GetTextureIndexSize);

				//sphericar texture for false light sources effect
				int envTexIndex = reader.readVal(header.GetTextureIndexSize);
				int envBlend = reader.ReadByte();
				shdrs.EnvType = (EnvironmentMode)envBlend;
				Texture2D envTex = empty;
                if (envTexIndex != 255 && envBlend > 0)
                {
                    if (textures[envTexIndex].Name != "def")
                        envTex = textures[envTexIndex];
                    else
                        shdrs.EnvType = 0;
                }
                else
                    shdrs.EnvType = 0;



                byte toonType = reader.ReadByte();
				
				Texture2D toon = empty;
				if (toonType == 0)
				{
					int text = reader.readVal(header.GetTextureIndexSize);
					if (text != 255)
					{
						shdrs.ToonShadow = true;
                        textures[text].WrapMode = TextureWrapMode.ClampToEdge;
                        toon = textures[text];
						//toon.GetTextureType = TextureType.toon;
					}
					else
					{
						//disable shadowing if no toon texture
						shdrs.AffectedByLight = false;
						shdrs.RecieveShadow = false;
					}
				}
				else
				{
					byte toontex = reader.ReadByte();
                    toontex++;
                    string texturePath = String.Format("Toys.Resourses.textures.PMX.toon{0}.bmp", toontex.ToString().PadLeft(2,'0'));
                    Texture2D toonTex = ResourcesManager.GetResourse<Texture2D>(texturePath);
                    if (toonTex == null)
                    {
                        var assembly = System.Reflection.IntrospectionExtensions.GetTypeInfo(typeof(Texture2D)).Assembly;
                        using (Bitmap pic = new Bitmap(assembly.GetManifestResourceStream(texturePath)))
                        {
                            toonTex = new Texture2D(pic, TextureType.Toon, texturePath);
                        }
                    }
                    
                    toon = toonTex;
                    shdrs.ToonShadow = true;
				}
                reader.readString();
				int count = reader.ReadInt32();
				Texture2D tex = empty;
				if (difTexIndex != 255)
				{
                    tex = textures[difTexIndex];
				}
				var mat = new MaterialPMX(shdrs, rndr);
				mat.Name = name;
				mat.Outline = outln;
                mat.SetTexture(tex,TextureType.Diffuse);
				mat.SetTexture(toon, TextureType.Toon);
				mat.SetTexture(envTex, TextureType.Sphere);
                mat.SpecularColour = specularColour;
                mat.Specular = specularPower;
                mat.DiffuseColor = difColor;
                mat.AmbientColour = ambientColour;
                mat.UniManager.Set("specular_color", specularColour);
                mat.UniManager.Set("ambient_color", ambientColour);
                mat.UniManager.Set("specular_power", specularPower);
                mat.UniManager.Set("diffuse_color", difColor);

                if (mat.DiffuseColor.W < 0.001f)
                    mat.RenderDirrectives.IsRendered = false;

                //skip empty materials
                if (count == 0)
                    mat.RenderDirrectives.IsRendered = false;
                mat.Offset = offset;
				mat.Count = count;
				mats[i] = mat;
				offset += count;
			}

		}

		void ReadBones(Reader reader)
		{
			int bonesCount = reader.ReadInt32();
			bones = new Bone[bonesCount];
            int maxLevel = 0;
			for (int i = 0; i < bonesCount; i++)
			{                
				string name = reader.readString();
                string nameEng = reader.readString();

				Vector3 Position = reader.readVector3() * multipler;

                int parentIndex = 0;
                if (header.GetBoneIndexSize == 2)
                {
                    parentIndex = unchecked((short)reader.readVal(header.GetBoneIndexSize));
                }
                else
                    parentIndex = reader.readVal(header.GetBoneIndexSize);

                int Level = reader.ReadInt32();
				byte[] flags = reader.ReadBytes(2);

				Bone bone = new Bone(name, nameEng, Position, parentIndex, flags);
				bone.Level = Level;
				if (bone.tail)
					reader.readVal(header.GetBoneIndexSize);
				else
					reader.readVector3();

				if (bone.InheritRotation || bone.InheritTranslation)
				{
					bone.ParentInheritIndex = reader.readVal(header.GetBoneIndexSize);
					bone.ParentInfluence = reader.ReadSingle();
				}

                bone.Parent2Local = Matrix4.Identity;
				//probably cant be both true
				if (bone.FixedAxis)
				{
					Vector3 X = reader.readVector3();
					Vector3 Z = Vector3.Cross(X, new Vector3(0f, 1f, 0f));
					Vector3 Y = Vector3.Cross(Z, X);
					Matrix3 local = new Matrix3(X,Y,Z);
                    local.Normalize();
                    bone.Parent2Local = new Matrix4(local);
				}
				if (bone.LocalCoordinate)
				{
					Vector3 X = reader.readVector3();
					Vector3 Z = reader.readVector3();
					Vector3 Y = Vector3.Cross(Z, X);
					Matrix3 local = new Matrix3(X, Y, Z);
                    local.Normalize();
                    bone.Parent2Local = new Matrix4(local);
				}

				if (bone.ExternalPdeform)
				{
                    reader.ReadInt32();
				}

				if (bone.IK)
				{
                    BoneIK ikBone = new BoneIK();
					ikBone.Target = reader.readVal(header.GetBoneIndexSize);
					ikBone.LoopCount = reader.ReadInt32();
					ikBone.AngleLimit = reader.ReadSingle();
					int count = reader.ReadInt32();
                    IKLink[] links = new IKLink[count];
					for (int n = 0; n < count; n++)
					{
                        IKLink link = new IKLink();
						link.Bone = reader.readVal(header.GetBoneIndexSize);
						if (reader.ReadByte() == 1)
						{
                            link.IsLimit = true;
							link.LimitMin = reader.readVector3();
							link.LimitMax = reader.readVector3();
						}
                        links[n] = link;
					}
                    ikBone.Links = links;
				    bone.IKData = ikBone;
				}

                if (maxLevel < bone.Level)
                    maxLevel = bone.Level;

                bones[i] = bone;
			}

            //convert position from model to parent bone
            for (int i = bones.Length - 1; i > -1; i--)
            {
                Bone bone = bones[i];
                if (bone.ParentIndex >= 0 && bone.ParentIndex < bones.Length){
                    bone.Parent2Local =  Matrix4.CreateTranslation(bone.Position - bones[bone.ParentIndex].Position);
                }
            }


            boneOrder = new int[bones.Length];
            int m = 0;
            for (int n = 0; n <= maxLevel; n++)
            {
                for (int i = 0; i < bones.Length; i++)
                {
                    if (bones[i].Level == n)
                    {
                        boneOrder[m] = i;
                        m++;
                    }
                }
            }
        }

		void ReadMorhps(Reader reader)
		{
			int morphCount = reader.ReadInt32();
			morphs = new Morph[morphCount];

			for (int i = 0; i < morphCount; i++)
			{
				string name = reader.readString();
				string nameEng = reader.readString();
				int panel = reader.ReadByte();
				int type = reader.ReadByte();
				int size = reader.ReadInt32();

				if (type == (int)MorphType.Vertex)
				{
					morphs[i] = new MorphVertex(name, nameEng, size, meshRigged.GetMorpher);
				}
				else if (type == (int)MorphType.Material)
					morphs[i] = new MorphMaterial(name, nameEng, size);
				else if (type == (int)MorphType.Uv)
				{
					morphs[i] = new MorphUV(name, nameEng, size, meshRigged.GetMorpher);
				}

                for (int n = 0; n < size; n++)
				{
					switch (type)
					{
						case 0: //group
							reader.readVal(header.GetMorphIndexSize);
                            reader.ReadSingle();
							break;
						case 1: //vertex
							int index = reader.readVal(header.GetVertexIndexSize);
							Vector3 pos = reader.readVector3() * multipler;
                            ((MorphVertex)morphs[i]).AddVertex(pos, index);
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
                            if (idx >= 0 && idx < mats.Length)
                                mat = mats[idx];

                            var MMorpher = new MaterialMorpher(mat);
                            MMorpher.mode = reader.ReadByte();
							MMorpher.DiffuseColor = reader.readVector4();
                            MMorpher.SpecularColor = reader.readVector3();
                            reader.ReadSingle();
                            MMorpher.AmbientColor = reader.readVector3();
							reader.readVector4();
                            reader.ReadSingle();
							reader.readVector4();
							reader.readVector4();
							reader.readVector4();
                            ((MorphMaterial)morphs[i]).MaterialMorphers.Add(MMorpher);

                            break;
						case 9: //flip
							reader.readVal(header.GetMaterialIndexSize);
                            reader.ReadSingle();
							break;
						case 10: //impulse
							reader.readVal(header.GetRigidBodyIndexSize);
                            reader.ReadByte();
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
		void ReadPanel(Reader reader)
		{
			int panelCount = reader.ReadInt32();
			for (int i = 0; i < panelCount; i++)
			{
				reader.readString();
				reader.readString();
                reader.ReadByte();
				int count = reader.ReadInt32();
				for (int n = 0; n < count; n++)
				{ 
					int type = reader.ReadByte();
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
		void ReadRigit(Reader reader)
		{
            int readCount = reader.ReadInt32();
			rigitBodies = new RigidContainer[readCount];
			RigidContainer rigit;
			for (int i = 0; i<readCount; i++)
			{
				rigit = new RigidContainer();
				rigit.Name = reader.readString();
                rigit.NameEng = reader.readString();
				rigit.BoneIndex = reader.readVal(header.GetBoneIndexSize);
                //shift for 16 bytes to prevent interfer with other physical instances
				rigit.GroupId = (byte)(16 + reader.ReadByte());
                rigit.NonCollisionGroup =  (int)reader.ReadUInt16() << 16;
				rigit.PrimitiveType = (PhysPrimitiveType)reader.ReadByte();
				rigit.Size = reader.readVector3() * multipler;
				rigit.Position = reader.readVector3() * multipler;
				rigit.Rotation = reader.readVector3();
				rigit.Mass = reader.ReadSingle();
				rigit.MassAttenuation = reader.ReadSingle();
				rigit.RotationDamping = reader.ReadSingle();
				rigit.Restitution = reader.ReadSingle();
				rigit.Friction = reader.ReadSingle();
				rigit.Phys = (PhysType)reader.ReadByte();

                rigitBodies[i] = rigit;
			}
		}

		void ReadJoints(Reader reader)
		{
            int jointCount = reader.ReadInt32();
			joints = new JointContainer[jointCount];

			for (int i = 0; i<jointCount; i++)
			{
				JointContainer joint = new JointContainer();
				joint.Name = reader.readString();
				joint.NameEng = reader.readString();
				joint.Type =  (JointType)reader.ReadByte();
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

		public Texture2D[] GetTextures
		{
			get { return textures; }
		}

		public Material[] GetMaterials
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
				MeshDrawerRigged md = new MeshDrawerRigged(meshRigged, mats,new BoneController(bones, boneOrder), morphs);
				md.OutlineDrawing = true;

				var node = new SceneNode();
				node.AddComponent(md);
				node.AddComponent(new Animator(md.skeleton));
                node.AddComponent(new PhysicsManager(rigitBodies, joints, md.skeleton));
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
