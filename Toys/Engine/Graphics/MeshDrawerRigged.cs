
namespace Toys
{
	public class MeshDrawerRigged : MeshDrawer
	{
		public BoneController skeleton { get; private set; }
		UniformBufferSkeleton uniformBufferSkeleton;
        ModelSkinning modelSkinner;

		public MeshDrawerRigged(Mesh mesh, Material[] mats, BoneController skelet, Morph[] mor = null) : base(mesh, mats, mor)
		{
			skeleton = skelet;
			UniformBufferManager ubm = UniformBufferManager.GetInstance;
			uniformBufferSkeleton = (UniformBufferSkeleton)ubm.GetBuffer("skeleton");
            modelSkinner = new ModelSkinning(mesh);
		}

        public MeshDrawerRigged(Mesh mesh, BoneController skelet, Morph[] mor = null) : base(mesh, null, mor)
        {
            skeleton = skelet;
            UniformBufferManager ubm = UniformBufferManager.GetInstance;
            uniformBufferSkeleton = (UniformBufferSkeleton)ubm.GetBuffer("skeleton");
        }

        public override void Draw()
		{
#if VertexSkin
            ubs.SetBones(skeleton.GetSkeleton);
#endif
            base.Draw();
		}

        public override void Prepare()
        {
            skeleton.UpdateSkeleton();
#if !VertexSkin
            uniformBufferSkeleton.SetBones(skeleton.GetSkeleton);
            modelSkinner.Skin();
#endif
        }

        public override void DrawSimple()
		{
#if VertexSkin
            ubs.SetBones(skeleton.GetSkeleton);
#endif
            Mesh.BindVAO();
			foreach (var mat in Materials)
			{
				if (!mat.RenderDirrectives.IsRendered)
					continue;
				Mesh.Draw(mat.Offset, mat.Count);
			}

			Mesh.ReleaseVAO();
		}
	}
}
