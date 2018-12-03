using System;
//using Assimp;
using System.Collections.Generic;
using OpenTK;

namespace Toys
{
	public class Model
	{
		public MeshDrawer meshes;
		public Matrix4 WorldSpace;
		public AnimController anim;
		public Morph[] morph;


		public Model(MeshDrawer mesh, Bone[] bones)
		{
			meshes = mesh;
			anim = new AnimController(bones,mesh.mesh);
		}

        public Model(MeshDrawer mesh, Bone[] bones, Morph[] morphes)
        {
            morph = morphes;
            meshes = mesh;
            anim = new AnimController(bones, mesh.mesh);
        }
        /*
		public Model(Texture tex, Shader shdr)
		{
			//LoadPlane(shdr, tex);
		}
		*/
        //Model(){}

        //main drawing method
        /*
                public void Draw()
                {
                    foreach (var mesh in meshes)
                    {
                        mesh.Draw();
                    }
                }

                public void DrawSimple()
                {
                    foreach (var mesh in meshes)
                    {
                        mesh.DrawSimple();
                    }
                }

                public void DrawOutline()
                {
                    foreach (var mesh in meshes)
                    {
                        mesh.DrawOutline();
                    }	
                }


                public Model Copy()
                {

                    Model model = new Model(meshes[0],anim.GetBones);
                    model.meshes = meshes;
                    model.WorldSpace = WorldSpace;

                    return model;

                }
        */
        /*
		void LoadPlane(Texture txtr)
		{
			Vertex[] Vertices = {
				new Vertex(new Vector3( 1.0f, 1.0f,  0.0f), new Vector3(0f,0f,0f), new Vector2(1.0f, 1.0f)),
				new Vertex(new Vector3( 1.0f, -1.0f,  0.0f), new Vector3(0f,0f,0f), new Vector2(1.0f, 0.0f)),
				new Vertex(new Vector3( -1.0f, -1.0f, -0.0f), new Vector3(0f,0f,0f), new Vector2(0.0f, 0.0f)),
				new Vertex(new Vector3( -1.0f, 1.0f,  0.0f), new Vector3(0f,0f,0f), new Vector2(0.0f, 1.0f)),
   				};
			int[] indexes = {  0, 1, 3, 1, 2, 3 };
			Mesh mesh = new Mesh(Vertices, indexes);
			meshes.Add(mesh);
			MaterialSimple mat = new MaterialSimple();
			mat.textures = new Texture[] {txtr};
		}
*/

        public IMaterial[] GetMaterials
		{
			get {
				List<IMaterial> mats = new List<IMaterial>();
				mats.AddRange(meshes.mats);

				return mats.ToArray();
			}
		}
    }
}
