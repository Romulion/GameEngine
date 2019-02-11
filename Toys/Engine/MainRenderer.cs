using System;

namespace Toys
{
	public class MainRenderer
	{
		public Camera mainCamera;
		public Scene mainScene;

		UniformBufferSkeleton skeleton;
		UniformBufferLight ubl;
		UniformBufferSpace ubs;
		ModelRenderer renderer;

		internal MainRenderer(Camera camera, Scene scene)
		{
			mainCamera = camera;
			mainScene = scene;
			UniformBufferManager ubm = UniformBufferManager.GetInstance;
			skeleton = (UniformBufferSkeleton)ubm.GetBuffer("skeleton");

			ubs = (UniformBufferSpace)ubm.GetBuffer("space");
			ubl = (UniformBufferLight)ubm.GetBuffer("light");
			ubl.SetNearPlane(0.1f);
			ubl.SetFarPlane(10.0f);
			renderer = new ModelRenderer();
			renderer.projection = camera.projection;
		}

		public void Render() 
		{
            renderer.viev = mainCamera.GetLook;
			ubl.SetViewPos(mainCamera.GetPos);

			foreach (var node in mainScene.GetNodes())
			{
				if (!node.Active)
					continue;
				
				if (node.anim != null)
					skeleton.SetBones(node.anim.GetSkeleton);
				if (node.model != null)
					renderer.Render(node);
			}
		}

		public void Resize()
		{
			renderer.projection = mainCamera.projection;
		}
	}
}
