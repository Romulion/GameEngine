using Gtk;
using System.Reflection;
using System;
// This file has been generated by the GUI designer. Do not modify.
namespace ModelViewer
{
	public partial class Window
	{
		private Fixed fixed1;
		private ScrolledWindow scrolledwindow1;
		private Fixed fixed2;
		private ScrolledWindow scrolledwindow2;
		private Fixed fixed3;
        private Fixed fixed4;
        private ScrolledWindow scrolledwindow3;
        private FileChooserButton fileChooser;

        protected virtual void Build()
        {
            Stetic.Gui.Initialize(this);
            // Widget Toys.Window
            Name = "ModelViewer";
            Title = "Window";
            WindowPosition = (WindowPosition)4;
            // Container child Toys.Window.Gtk.Container+ContainerChild
            fixed1 = new Fixed();
            fixed1.Name = "fixed1";
            fixed1.HasWindow = false;
            // Container child fixed1.Gtk.Fixed+FixedChild
            scrolledwindow1 = new ScrolledWindow();
            scrolledwindow1.WidthRequest = 154;
            scrolledwindow1.HeightRequest = 300;
            scrolledwindow1.CanFocus = true;
            scrolledwindow1.Name = "scrolledwindow1";
            scrolledwindow1.VscrollbarPolicy = 0;
            scrolledwindow1.HscrollbarPolicy = (PolicyType)2;
            scrolledwindow1.ShadowType = (ShadowType)1;
            // Container child scrolledwindow1.Gtk.Container+ContainerChild
            Viewport w1 = new Viewport();
            w1.ShadowType = 0;
            // Container child GtkViewport.Gtk.Container+ContainerChild
            fixed2 = new Fixed();
            fixed2.Name = "fixed2";
            fixed2.HasWindow = false;
            w1.Add(fixed2);
            scrolledwindow1.Add(w1);
            fixed1.Add(scrolledwindow1);
            // Container child fixed1.Gtk.Fixed+FixedChild
            scrolledwindow2 = new ScrolledWindow();
            scrolledwindow2.WidthRequest = 200;
            scrolledwindow2.HeightRequest = 300;
            scrolledwindow2.CanFocus = true;
            scrolledwindow2.Name = "scrolledwindow2";
            scrolledwindow2.HscrollbarPolicy = (PolicyType)2;
            scrolledwindow2.ShadowType = (ShadowType)1;
            // Container child scrolledwindow2.Gtk.Container+ContainerChild
            Viewport w5 = new Viewport();
            w5.ShadowType = 0;
            // Container child GtkViewport1.Gtk.Container+ContainerChild
            fixed3 = new Fixed();
            fixed3.Name = "fixed3";
            fixed3.HasWindow = false;
            w5.Add(fixed3);
            scrolledwindow2.Add(w5);
            fixed1.Add(scrolledwindow2);

            Fixed.FixedChild w8 = (Fixed.FixedChild)fixed1[scrolledwindow2];
            w8.X = 165;
            /*
                        // Container child fixed1.Gtk.Fixed+FixedChild
                        filechooserbutton2 = new FileChooserButton("Select a File", 0);
                        filechooserbutton2.WidthRequest = 124;
                        filechooserbutton2.Name = "filechooserbutton2";
                        fixed1.Add(filechooserbutton2);
                        Fixed.FixedChild w9 = (Fixed.FixedChild)fixed1[filechooserbutton2];
                        w9.X = 379;
                        w9.Y = 19;
                        // Container child fixed1.Gtk.Fixed+FixedChild
                        button2 = new Button();
                        button2.WidthRequest = 109;
                        button2.CanFocus = true;
                        button2.Name = "button2";
                        button2.UseUnderline = true;
                        button2.Label = "Play";
                        fixed1.Add(button2);
                        Fixed.FixedChild w10 = (Fixed.FixedChild)fixed1[button2];
                        w10.X = 379;
                        w10.Y = 63;
                        // Container child fixed1.Gtk.Fixed+FixedChild
                        button3 = new Button();
                        button3.WidthRequest = 110;
                        button3.CanFocus = true;
                        button3.Name = "button3";
                        button3.UseUnderline = true;
                        button3.Label = "Stop";
                        fixed1.Add(button3);
                        Fixed.FixedChild w11 = (Fixed.FixedChild)fixed1[button3];
                        w11.X = 379;
                        w11.Y = 108;
                        // Container child fixed1.Gtk.Fixed+FixedChild
                        button4 = new Button();
                        button4.WidthRequest = 110;
                        button4.CanFocus = true;
                        button4.Name = "button4";
                        button4.UseUnderline = true;
                        button4.Label = "Pause";
                        fixed1.Add(button4);
                        Fixed.FixedChild w12 = (Fixed.FixedChild)fixed1[button4];
                        w12.X = 379;
                        w12.Y = 153;
            */
            scrolledwindow3 = new ScrolledWindow();
            scrolledwindow3.WidthRequest = 200;
            scrolledwindow3.HeightRequest = 300;
            scrolledwindow3.CanFocus = true;
            scrolledwindow3.Name = "scrolledwindow3";
            scrolledwindow3.VscrollbarPolicy = 0;
            scrolledwindow3.HscrollbarPolicy = (PolicyType)2;
            scrolledwindow3.ShadowType = (ShadowType)1;
            // Container child scrolledwindow1.Gtk.Container+ContainerChild
            Viewport w4 = new Viewport();
            w4.ShadowType = 0;
            // Container child GtkViewport.Gtk.Container+ContainerChild
            fixed4 = new Fixed();
            fixed4.Name = "fixed4";
            fixed4.HasWindow = false;
            w4.Add(fixed4);
            scrolledwindow3.Add(w4);
            fixed1.Add(scrolledwindow3);
            Fixed.FixedChild w6 = (Fixed.FixedChild)fixed1[scrolledwindow3];
            w6.X = 370;

            Add(fixed1);
            if (Child != null)
            {
                Child.ShowAll();
            }
            DefaultWidth = 700;
            DefaultHeight = 342;
            Show();

            /*
            var provider = new CssProvider();
            string css = ReadFromAssetStream("ModelViewer.gtk_gui.gtkmain.css");
            provider.LoadFromData(css);
            StyleContext.AddProviderForScreen(Gdk.Screen.Default, provider, StyleProviderPriority.User);
            */
        }

        static string ReadFromAssetStream(string path)
        {
            var assembly = IntrospectionExtensions.GetTypeInfo(typeof(Window)).Assembly;
            var stream = assembly.GetManifestResourceStream(path);

            string str = "";
            using (var reader = new  System.IO.StreamReader(stream))
            {
                str = reader.ReadToEnd();
            }
            return str;
        }
    }
}
