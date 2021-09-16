using System;
using System.Linq;
using OpenTK.Graphics.OpenGL;

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

		public virtual void Render(MeshDrawer[] meshes, Camera camera)
		{
			camera.CalcLook();
			GL.Viewport(0, 0, camera.Width, camera.Height);

			//render scene to primary buffer
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, camera.RenderBuffer);
			//SetCullMode(FaceCullMode.Disable);
			//GL.ClearColor(MainCamera.ClearColor);
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			//render background first due to model transperancy
			if (camera.Background != null)
				camera.Background.DrawBackground(camera);

			RenderScene(meshes, camera);
		}

		internal void RenderScene(MeshDrawer[] meshes, Camera camera)
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
	
		internal virtual void Destroy()
        {

        }
	}
}
