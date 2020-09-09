using System;
using System.Linq;

namespace Toys
{
	public class MainRenderer
	{
		UniformBufferSkeleton skeleton;
		UniformBufferLight ubl;
		UniformBufferSpace ubs;
		ModelRenderer renderer;

		internal MainRenderer()
		{
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

			foreach (var node in CoreEngine.MainScene.GetNodes())
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
            //frustrum culling and filltering
            MeshDrawer[] meshesSorted = new MeshDrawer[meshes.Length];
            int count = 0;
            for (int i = 0; i < meshes.Length; i++)
            {
                if (meshes[i].Node.Active && (meshes[i].RenderMask & camera.RenderMask) > 0)
                {
                    if (meshes[i].GetBoundingBox.CheckFrustrumPresence( meshes[i].Node.GetTransform.GlobalTransform * camera.GetLook * camera.Projection))
                        meshesSorted[count++] = meshes[i];
                }
            }

            renderer.Viev = camera.GetLook;
            renderer.Projection = camera.Projection;
            ubl.SetViewPos(camera.GetPos);
            ubs.SetPvSpace(camera.GetLook * camera.Projection);
            for (int n = 0; n < count; n++)
			{
		        renderer.Render(meshesSorted[n]);
			}
		}
	}
}
