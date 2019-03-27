using System;
using Gtk;

namespace Toys
{
	public partial class Window : Gtk.Window
	{
		CoreEngine core;

		public Window(SceneNode node, CoreEngine core) :
				base(WindowType.Toplevel)
		{
			this.core = core;
			Build();
			DeleteEvent += delegate { Application.Quit(); };
			var disable = new Gdk.Color(10, 200, 10);
			fixed1.ModifyBg(StateType.Normal, disable);


			var render = (MeshDrawer) node.GetComponent(typeof(MeshDrawer));
			if (render.mats != null)
				SetList(render.mats);
			if (render.morph != null)
				SetMorphList(render.morph);

			var anim = (Animator)node.GetComponent(typeof(Animator));
			SetAnimator(anim);

			ShowAll();
		}

		void SetList(IMaterial[] mats)
		{
			int y = 0;
			var disable = new Gdk.Color(10, 100, 10);
			var enable = new Gdk.Color(150, 150, 150);

			foreach (var mat in mats)
			{
				Button btn = new Button();
				btn.Label = mat.Name;
				btn.Name = "btn";
				btn.ModifyBg(StateType.Normal, disable);

				btn.Clicked += (sender, e) =>
				{
					var renderDir = mat.rndrDirrectives;
					renderDir.render = !renderDir.render;
					if (renderDir.render)
						btn.ModifyBg(StateType.Normal, disable);
					else 
						btn.ModifyBg(StateType.Active, enable);	
					
				};
				/*
				if (mat.dontDraw)
					btn.ModifyBg(StateType.Normal, disable);
				else 
					btn.ModifyBg(StateType.Normal, enable);
					*/

				fixed2.Put(btn, 0, y);
				btn.Show();
				y += 25;
			}
		}

		void SetMorphList(Morph[] morphs)
		{
			int y = 0;
			foreach (var morph in morphs)
			{
				//skip non vertex morphs
				//if (!(morph is MorphVertex) && !(morph is MorphMaterial))
				//	continue;
				if (!(morph is MorphVertex) && !(morph is MorphMaterial) && !(morph is MorphUV))
					continue;



				Label lbl = new Label();
				lbl.Name = "lbl";
				lbl.Text = morph.Name;

				fixed3.Put(lbl, 0, y);
				lbl.Show();
				y += 20;

				HScale scale = new HScale(0, 1, 0.5);
				scale.WidthRequest = 180;
				scale.Name = "scale";

				scale.ValueChanged += (sender, e) =>
				{
					core.addTask = () => morph.morphDegree =(float)scale.Value;
				};


				fixed3.Put(scale, 0, y);
				scale.Show();
				y += 40;

				/*
				btn.ModifyBg(StateType.Normal, disable);

				btn.Clicked += (sender, e) =>
				{
					var renderDir = mat.rndrDirrectives;
				renderDir.render = !renderDir.render;
					if (renderDir.render)
						btn.ModifyBg(StateType.Normal, disable);
					else 
						btn.ModifyBg(StateType.Active, enable);	
					
				};
				*/
				/*
				if (mat.dontDraw)
					btn.ModifyBg(StateType.Normal, disable);
				else 
					btn.ModifyBg(StateType.Normal, enable);
					*/
				/*
				fixed2.Put(btn, 0, y);
				btn.Show();
				y += 25;
				*/
			}
		}

		void SetAnimator(Animator anim)
		{
			//FileChooserButton btn1 = new FileChooserButton("Load Animation", FileChooserAction.Open);
			//btn1.Name = "btnLoadAnim";
			Animation an = null;

			filechooserbutton2.FileSet += (sender, e) => 
			{
				try
				{
					an = AnimationLoader.Load(filechooserbutton2.Filename);
				}
				catch (Exception)
				{
					Console.WriteLine("cant load animation");
				}
			};


			//Play

			button2.Clicked += (sender, e) =>
			{
				if (an != null)
					anim.Play(an);
			};

			button3.Clicked += (sender, e) =>
			{
				if (an != null)
					anim.Stop();;
			};
			/*
			if (mat.dontDraw)
				btn.ModifyBg(StateType.Normal, disable);
			else 
				btn.ModifyBg(StateType.Normal, enable);


			fixed2.Put(btn, 0, y);
			btn.Show();
			y += 25;
*/
		}
	}
}
