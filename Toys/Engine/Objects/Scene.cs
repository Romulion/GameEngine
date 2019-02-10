using System;
using OpenTK;
using System.Collections.Generic;

namespace Toys
{
	public class Scene
	{

		List<Model> models = new List<Model>();
        List<SceneNode> nodes = new List<SceneNode>();
        public Camera camera = new Camera();
		LightSource light;

		UniformBufferSkeleton skeleton;
		UniformBufferLight ubl;
		Shader shdr;
		Matrix4 projection, viev;
		ModelRenderer renderer;

		int Width = 640;
		int Height = 480;
        int i = 0;

		public Scene()
		{
			Instalize();
        }

		void Instalize()
		{
			ShaderManager shdmMgmt = ShaderManager.GetInstance;
			try
			{

				shdmMgmt.LoadShader("pmx");
				shdmMgmt.LoadShader("shadow");
				shdmMgmt.LoadShader("outline");
				//shdmMgmt.LoadShader("compute","skin.glsl");
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}

			//set camera pos
			viev = Matrix4.CreateTranslation(0.0f, 0.0f, -1f);
			//set projection
			projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI * (30 / 180f), 640 / (float)480, 0.1f, 10.0f);
			shdr = shdmMgmt.GetShader("pmx");

            //outline = new Outline();
            //outline.size = 0.03f;
            UniformBufferManager ubm = UniformBufferManager.GetInstance;
			skeleton = (UniformBufferSkeleton)ubm.GetBuffer("skeleton");
            light = new LightSource(models);
            
            ubl = (UniformBufferLight)ubm.GetBuffer("light");
			ubl.SetNearPlane(0.1f);
			ubl.SetFarPlane(10.0f);
			renderer = new ModelRenderer(light, camera);
            
        }

		public void AddModel(Model model)
		{
            var node = new SceneNode();
            node.model = model;
            nodes.Add(node);
            models.Add(model);
		}

		public void Resize(int Width, int Height)
		{
			projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI * (30 / 180f), Width / (float)Height, 0.1f, 10.0f);
            //need for fix
            light.Resize(Width,Height);
            light.BindShadowMap();
            this.Width = Width;
			this.Height = Height;
			renderer.projection = projection;
        }


		//scene rendering
		public void Render()
		{
            
            renderer.viev = camera.GetLook;
			ubl.SetLightPos(light.GetPos);
			ubl.SetViewPos(camera.GetPos);

			//timer.Start();
			foreach (var model in models)
			{
				renderer.Render(model);
				skeleton.SetBones(model.anim.GetSkeleton);
			}
			/*
			timer.Stop();
			Console.WriteLine(timer.Elapsed);
			timer.Reset();
*/
		}

		public void Update()
		{

            //light.pos = new Vector3(2 * (float)Math.Cos(radians(i)), 1.5f, 2 * (float)Math.Sin(radians(i)));
			//i++;
			//models[0].morph[20].morphDegree = Math.Abs((float)Math.Sin(radians(i * 2)));
            /*
			if (models.Count > 0)
			{
				float angle = (float)Math.Sin(radians(i * 3)) * radians(40) - radians(20);
				models[0].anim.Rotate("EndHips", new Quaternion(0f, angle, 0f) );
				//models[0].anim.Rotate("上半身", new Quaternion(0f, angle, 0f) );
				i++;
			}
            */
		}


		public Camera GetCamera
		{
			get { return camera; }
		}

		public LightSource GetLight
		{
			get { return light; } 
		}

		float radians(float angle)
		{
			return (float)Math.PI * angle / 360.0f;
		}

        //void AddObject( )
	}
}
