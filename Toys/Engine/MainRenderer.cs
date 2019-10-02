using System;
using System.Linq;

namespace Toys
{
	public class MainRenderer
	{
		public Scene mainScene;

		UniformBufferSkeleton skeleton;
		UniformBufferLight ubl;
		UniformBufferSpace ubs;
		ModelRenderer renderer;

		internal MainRenderer(Scene scene)
		{
			mainScene = scene;
			UniformBufferManager ubm = UniformBufferManager.GetInstance;
			skeleton = (UniformBufferSkeleton)ubm.GetBuffer("skeleton");

			ubs = (UniformBufferSpace)ubm.GetBuffer("space");
			ubl = (UniformBufferLight)ubm.GetBuffer("light");
			ubl.SetNearPlane(0.1f);
			ubl.SetFarPlane(10.0f);
			renderer = new ModelRenderer();
		}

		public void Render(Camera camera) 
		{
            renderer.viev = camera.GetLook;
            renderer.projection = camera.projection;
            ubl.SetViewPos(camera.GetPos);
			ubs.SetPvSpace(camera.GetLook * camera.projection);

			foreach (var node in mainScene.GetNodes())
			{
				if (!node.Active)
					continue;

				MeshDrawer md = (MeshDrawer) node.GetComponent(typeof(MeshDrawer));

				if (md == null)
					continue;

				renderer.Render(md);
				/*
				if (node.anim != null)
					skeleton.SetBones(node.anim.GetSkeleton);
				if (node.model != null)
					renderer.Render(node);
				*/
			}
		}

		public void Render(MeshDrawer[] meshes, Camera camera)
		{
            renderer.viev = camera.GetLook;
            renderer.projection = camera.projection;
            ubl.SetViewPos(camera.GetPos);
			ubs.SetPvSpace(camera.GetLook * camera.projection);

            foreach (var mesh in meshes)
			{
				renderer.Render(mesh);
			}
		}
	}
}
