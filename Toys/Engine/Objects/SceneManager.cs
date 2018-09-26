using System;
using OpenTK;
using System.Collections.Generic;

namespace Toys
{
	public class SceneManager
	{
		List<Model> models = new List<Model>();
		public Camera camera = new Camera();
		LightSource shadow;

		UniformBuffer skeleton;
		Shader shdr;
		Matrix4 projection, viev;
		ModelRenderer renderer;

		int i = 0;

		System.Diagnostics.Stopwatch timer;

		static SceneManager sceneMgmr;

		int Width = 640;
		int Height = 480;

		SceneManager()
		{
			Instalize();
			timer = new System.Diagnostics.Stopwatch();
		}

		void Instalize()
		{
			ShaderManager shdmMgmt = ShaderManager.GetInstance;
			try
			{

				shdmMgmt.LoadShader("pmx");
				shdmMgmt.LoadShader("shadow");
				shdmMgmt.LoadShader("outline");
				shdmMgmt.LoadShader("compute","skin.glsl");
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}

			//set camera pos
			viev = Matrix4.CreateTranslation(0.0f, 0.0f, -1f);
			//set projection
			projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI * (30 / 180f), 640 / (float)480, 0.1f, 10.0f);
			// projection = Matrix4.CreateOrthographic(game.Width / (float)game.Height * 2f, 2.0f, 0.1f, 100.0f);
			shdr = shdmMgmt.GetShader("pmx");
			//outline = new Outline();
			//outline.size = 0.03f;
			skeleton = new UniformBuffer(32000,"skeleton");
			shadow = new LightSource(models,skeleton);

			renderer = new ModelRenderer(shadow,camera);
		}

		public void AddModel(Model model)
		{
			models.Add(model);
		}


		public void Resize(int Width, int Height)
		{
			projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI * (30 / 180f), Width / (float)Height, 0.1f, 10.0f);
			shadow.Resize(Width,Height);
			this.Width = Width;
			this.Height = Height;
			renderer.projection = projection;
		}

		//singleton
		public static SceneManager GetInstance
		{
			get
			{
				if (sceneMgmr == null)
					sceneMgmr = new SceneManager();

				return sceneMgmr;
			}
		}


		//scene rendering
		public void Render()
		{

			renderer.viev = camera.GetLook;

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
			//shadow.pos = new Vector3(2 * (float)Math.Cos(radians(i)), 1.5f, 2 * (float)Math.Sin(radians(i)));
			//i++;
			/*
			if (models.Count > 0)
			{
				float angle = (float)Math.Sin(radians(i * 3)) * radians(40) - radians(20);
				models[0].anim.Rotate(4, new Quaternion(0f, angle, 0f) );
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
			get { return shadow; } 
		}

		float radians(float angle)
		{
			return (float)Math.PI * angle / 360.0f;
		}
	}
}
