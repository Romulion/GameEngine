using System;
namespace Toys
{
	public class MeshDrawerRigged : MeshDrawer
	{
		public BoneController skeleton { get; private set; }
		UniformBufferSkeleton ubs;

		public MeshDrawerRigged(Mesh mesh, IMaterial[] mats, BoneController skelet, Morph[] mor = null) : base(mesh, mats, mor)
		{
			skeleton = skelet;
			UniformBufferManager ubm = UniformBufferManager.GetInstance;
			ubs = (UniformBufferSkeleton)ubm.GetBuffer("skeleton");
		}

		public override void Draw()
		{
			ubs.SetBones(skeleton.GetSkeleton);

			mesh.BindVAO();
			foreach (var mat in mats)
			{
				var rdirs = mat.rndrDirrectives;
				if (!rdirs.render)
					continue;

				mat.ApplyMaterial();
				mesh.Draw(mat.offset, mat.count);
			}
			mesh.ReleaseVAO();		}

		public override void DrawSimple()
		{
			ubs.SetBones(skeleton.GetSkeleton);
			mesh.BindVAO();
			foreach (var mat in mats)
			{

				if (!mat.rndrDirrectives.render)
					continue;
				//mat.GetShader.ApplyShader();
				mesh.Draw(mat.offset, mat.count);
			}

			mesh.ReleaseVAO();
		}
	}
}
