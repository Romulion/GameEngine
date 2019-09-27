using System;
using OpenTK;

namespace Toys
{
	public class MeshDrawerRigged : MeshDrawer
	{
		public BoneController skeleton { get; private set; }
		UniformBufferSkeleton ubs;
        ModelSkinning ms;

		public MeshDrawerRigged(Mesh mesh, Material[] mats, BoneController skelet, Morph[] mor = null) : base(mesh, mats, mor)
		{
			skeleton = skelet;
			UniformBufferManager ubm = UniformBufferManager.GetInstance;
			ubs = (UniformBufferSkeleton)ubm.GetBuffer("skeleton");
            ms = new ModelSkinning(mesh);
		}

        public MeshDrawerRigged(Mesh mesh, BoneController skelet, Morph[] mor = null) : base(mesh, null, mor)
        {
            skeleton = skelet;
            UniformBufferManager ubm = UniformBufferManager.GetInstance;
            ubs = (UniformBufferSkeleton)ubm.GetBuffer("skeleton");
        }

        public override void Draw()
		{
			//ubs.SetBones(skeleton.GetSkeleton);
            //ms.Skin();
            base.Draw();
		}

        public override void Prepare()
        {
            skeleton.UpdateSkeleton();
            ubs.SetBones(skeleton.GetSkeleton);
            ms.Skin();
        }

		public override void DrawSimple()
		{
			//ubs.SetBones(skeleton.GetSkeleton);
            mesh.BindVAO();
			foreach (var mat in mats)
			{
				if (!mat.rndrDirrectives.render)
					continue;
				mesh.Draw(mat.offset, mat.count);
			}

			mesh.ReleaseVAO();
		}
	}
}
