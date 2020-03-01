using System;
using System.Linq;

namespace Toys
{
	public class MainRenderer
	{
		internal Scene MainScene;
		UniformBufferSkeleton skeleton;
		UniformBufferLight ubl;
		UniformBufferSpace ubs;
		ModelRenderer renderer;

		internal MainRenderer(Scene scene)
		{
			MainScene = scene;
			UniformBufferManager ubm = UniformBufferManager.GetInstance;
			skeleton = (UniformBufferSkeleton)ubm.GetBuffer("skeleton");

			ubs = (UniformBufferSpace)ubm.GetBuffer("space");
			ubl = (UniformBufferLight)ubm.GetBuffer("light");
			ubl.SetNearPlane(0.1f);
			ubl.SetFarPlane(10.0f);
			renderer = new ModelRenderer();
		}

        /// <summary>
        /// obsolette
        /// </summary>
        /// <param name="camera">camera to render from</param>
		public void Render(Camera camera) 
		{
            renderer.Viev = camera.GetLook;
            renderer.Projection = camera.Projection;
            ubl.SetViewPos(camera.GetPos);
			ubs.SetPvSpace(camera.GetLook * camera.Projection);

			foreach (var node in MainScene.GetNodes())
			{
				if (!node.Active)
					continue;

				MeshDrawer md = (MeshDrawer) node.GetComponent(typeof(MeshDrawer));

				if (md == null)
					continue;

				renderer.Render(md);
			}
		}

		public void Render(MeshDrawer[] meshes, Camera camera)
		{
            renderer.Viev = camera.GetLook;
            renderer.Projection = camera.Projection;
            ubl.SetViewPos(camera.GetPos);
            ubs.SetPvSpace(camera.GetLook * camera.Projection);
            foreach (var mesh in meshes)
			{
                if ((mesh.RenderMask & camera.RenderMask) > 0)
				    renderer.Render(mesh);
			}
		}
	}
}
