using System;
using Gtk;

namespace Toys
{
	public partial class Window : Gtk.Window
	{
		public Window(IMaterial[] mats) :
				base(WindowType.Toplevel)
		{
			Build();
            Init();
			SetList(mats);
			DeleteEvent += delegate { Application.Quit(); };

			var disable = new Gdk.Color(10, 200, 10);
			fixed1.ModifyBg(StateType.Normal, disable);
			ShowAll();
		}

		void Init()
		{
			
			//scrolledwindow1. += (o, args) => Console.WriteLine(vscrollbar1.Adjustment.Value);
		}

		void SetList(IMaterial[] mats)
		{
			int y = 0;
			var disable = new Gdk.Color(10, 100, 10);
			var enable = new Gdk.Color(150, 150, 150);

			foreach (var mat in mats)
			{
				Button btn = new Button();
				btn.Label = ((MaterialPMX)mat).Name;
				btn.Name = "btn";
				btn.ModifyBg(StateType.Normal, disable);

				btn.Clicked += (sender, e) =>
				{
					mat.dontDraw = !mat.dontDraw;
					if (mat.dontDraw)
						btn.ModifyBg(StateType.Normal, disable);
					else 
						btn.ModifyBg(StateType.Normal, enable);	
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

	}
}
