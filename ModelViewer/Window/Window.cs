using System;
using Gtk;
using Toys;

namespace ModelViewer
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

			var render = (MeshDrawer)node.GetComponent(typeof(MeshDrawer));
			if (render.Materials != null)
				SetList(render.Materials);
			if (render.Morphes != null)
				SetMorphList(render.Morphes);

			var anim = (Animator)node.GetComponent(typeof(Animator));
			SetAnimator(anim);

			ShowAll();
		}

		void SetList(Material[] mats)
		{
			int y = 0;

            foreach (var mat in mats)
			{
				Button btn = new Button();
				btn.Label = mat.Name;
                btn.Name = "btn";

                var renderDir = mat.RenderDirrectives;
                btn.Clicked += (sender, e) =>
				{
					renderDir.IsRendered = !renderDir.IsRendered;
                    if (renderDir.IsRendered)
                        btn.SetStateFlags(StateFlags.Normal,true);
                    else
                        btn.SetStateFlags(StateFlags.Checked, true);
                };

                if (renderDir.IsRendered)
                    btn.SetStateFlags(StateFlags.Normal, true);
                else
                    btn.SetStateFlags(StateFlags.Checked, true);

                fixed2.Put(btn, 0, y);
				btn.Show();
				y += 35;
			}
		}

		void SetMorphList(Morph[] morphs)
		{
			int y = 0;
			foreach (var morph in morphs)
			{

				if (!(morph is MorphVertex) && !(morph is MorphMaterial) && !(morph is MorphUV))
					continue;

                //display morph type
                string prefix = "";
                if (morph is MorphVertex)
                    prefix = "(V)";
                else if (morph is MorphMaterial)
                    prefix = "(M)";
                else if (morph is MorphUV)
                    prefix = "(UV)";
                
                Label lbl = new Label();
				lbl.Name = "lbl";
				lbl.Text = prefix + morph.Name;

				fixed3.Put(lbl, 0, y);
				lbl.Show();
				y += 20;

				HScale scale = new HScale(0, 1, 0.1);
				scale.WidthRequest = 180;
				scale.Name = "scale";

				scale.ValueChanged += (sender, e) =>
				{
					core.addTask = () => morph.MorphDegree =(float)scale.Value;
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
				catch (Exception ex)
				{
					Console.WriteLine("cant load animation\n{0}\n{1}",ex.Message,ex.StackTrace);
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
					anim.Stop();
			};

            button4.Clicked += (sender, e) =>
            {
                if (an != null)
                    anim.Reset();
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
