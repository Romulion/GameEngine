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
