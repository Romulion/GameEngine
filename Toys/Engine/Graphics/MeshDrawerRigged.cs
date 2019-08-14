using System;
using OpenTK;

namespace Toys
{
	public class MeshDrawerRigged : MeshDrawer
	{
		public BoneController skeleton { get; private set; }
		UniformBufferSkeleton ubs;

        //test
        int test = 0;
        //end test

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
			mesh.ReleaseVAO();
		}

        public override void Prepare()
        {
            //test  
            //skeleton.GetBone(0).SetTransform(new Quaternion(0f,test * (float)Math.PI / 180 ,0f),Vector3.Zero);
            //test += 3;
            //end test
            skeleton.UpdateSkeleton();
        }

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
