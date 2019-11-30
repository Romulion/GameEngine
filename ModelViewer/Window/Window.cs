using System;
using Gtk;
using Toys;
using System.Reflection;
using UtilityClient;

namespace ModelViewer
{
    public delegate void ExecuteMethod(byte[] data);
    public partial class Window : Gtk.Window
    {
        CoreEngine core;
        delegate void DisplayComponent();
        ClientMaster connection;
        ClientMaster connection2;
        public ExecuteMethod Execute;
        public volatile bool stream;
        public Window(Scene scene, CoreEngine core) :
                base(WindowType.Toplevel)
        {
            this.core = core;
            Build();
            DeleteEvent += delegate { Application.Quit(); };
            var disable = new Gdk.Color(10, 200, 10);

            DrawScene(scene);
            //SetAnimator(anim);

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

                fixed7.Put(lbl, 0, y);
                lbl.Show();
               // y += 30;

                HScale scale = new HScale(0, 1, 0.1);
                scale.WidthRequest = 180;
                scale.Name = "scale";

                scale.ValueChanged += (sender, e) =>
                {
                    core.addTask = () => morph.MorphDegree = (float)scale.Value;
                };


                fixed7.Put(scale, 100, y);
                scale.Show();
                y += 40;
            }
        }
/*
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
                    Console.WriteLine("cant load animation\n{0}\n{1}", ex.Message, ex.StackTrace);
                }
            };

            //Play
            bool play = false;
            bool pause = false;

            button2.Clicked += (sender, e) =>
            {
                if (an != null)
                    anim.Play(an);
                play = true;

            };

            button3.Clicked += (sender, e) =>
            {
                play = false;
                if (an != null)
                    anim.Stop();
            };

            button4.Clicked += (sender, e) =>
            {
                if (an != null)
                {
                    if (play && !pause)
                    {
                        anim.Pause();
                        button4.Label = "Resume";
                        pause = !pause;
                    }
                    else if (play)
                    {
                        anim.Resume();
                        button4.Label = "Pause";
                        pause = !pause;
                    }
                }
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
      //  }

        void DrawScene(Scene scene)
        {
            int y = 0;
            foreach (var node in scene.GetNodes())
            {
                Button btn = new Button();
                btn.Label = node.Name;
                btn.TooltipText = node.Name;
                btn.Name = "btn";
                btn.HeightRequest = 20;
                btn.Clicked += (sender, e) =>
                {
                    DrawComponents(node);
                };

                fixed2.Put(btn, 0, y);
                btn.Show();
                y += 35;
            }
        }

        void DrawComponents(SceneNode node)
        {
            int y = 0;
            ClearChildrens(fixed3);
            ClearChildrens(fixed4);
            foreach (var component in node.GetComponents())
            {
                if (component is MeshDrawerRigged)
                {
                    Button btn = new Button();
                    btn.Label = "MeshDrawerRigged";
                    btn.Name = "btnComp";
                    btn.Clicked += (sender, e) => { MeshDrawerRig((MeshDrawerRigged)component);};
                    fixed3.Put(btn, 0, y);
                    btn.Show();
                    y += 35;
                }
                else if (component is MeshDrawer)
                {
                    Button btn = new Button();
                    btn.Label = "MeshDrawer";
                    btn.Name = "btnComp";
                    fixed3.Put(btn, 0, y);
                    btn.Show();
                    y += 35;
                }
                else if (component is Animator)
                {
                    Button btn = new Button();
                    btn.Label = "Animator";
                    btn.Clicked += (sender, e) => { AnimatorWindow((Animator)component); };
                    btn.Name = "btnComp";
                    fixed3.Put(btn, 0, y);
                    btn.Show();
                    y += 35;
                }
            }
        }

        void MeshDrawerRig(MeshDrawerRigged meshDrawer)
        {
            ClearChildrens(fixed4);
            int y = 0;
            if (meshDrawer.Morphes != null)
            {
                Button btn = new Button();
                btn.Label = "Morhps";
                btn.Name = "btn";
                fixed4.Put(btn, 0, y);
                btn.Show();
                y += 35;
                btn.Clicked += (s, e) => { SetMorphList(meshDrawer.Morphes); fixed6.Show();  notebook.Page = morphPanel; };
            }
            foreach (var mat in meshDrawer.Materials)
            {
                var renderDir = mat.RenderDirrectives;

                Button btn = new Button();
                btn.Label = mat.Name;
                btn.Name = "btn";
                btn.TooltipText = mat.Name;
                btn.Clicked += (sender, e) =>
                {

                    renderDir.IsRendered = !renderDir.IsRendered;
                    if (renderDir.IsRendered)
                        btn.SetStateFlags(StateFlags.Normal, true);
                    else
                        btn.SetStateFlags(StateFlags.Checked, true);

                };


                if (renderDir.IsRendered)
                    btn.SetStateFlags(StateFlags.Normal, true);
                else
                    btn.SetStateFlags(StateFlags.Checked, true);


                fixed4.Put(btn, 0, y);
                btn.Show();
                y += 35;
            }
            
        }

        void AnimatorWindow(Animator animator)
        {
            var timer = new Time();
            
            ClearChildrens(fixed4);
            fileChooser = new FileChooserButton("Select a File", FileChooserAction.Open);
            fileChooser.WidthRequest = 124;
            fileChooser.Name = "filechooserbutton2";
            fixed4.Put(fileChooser, 0, 19);
            fileChooser.Show();
            // Container child fixed1.Gtk.Fixed+FixedChild
            var btnStart = new Button();
            btnStart.WidthRequest = 109;
            btnStart.Name = "button2";
            btnStart.Label = "Play";
            fixed4.Put(btnStart, 0, 63);
            btnStart.Show();
            // Container child fixed1.Gtk.Fixed+FixedChild
            var btn = new Button();
            btn.WidthRequest = 110;
            btn.CanFocus = true;
            btn.Name = "button3";
            btn.Label = "Stop";
            fixed4.Put(btn, 0, 108);
            btn.Show();
            // Container child fixed1.Gtk.Fixed+FixedChild
            var btnPR = new Button();
            btnPR.WidthRequest = 110;
            btnPR.CanFocus = true;
            btnPR.Name = "button4";
            btnPR.Label = "Pause";
            fixed4.Put(btnPR, 0 , 153);
            btnPR.Show();
            Animation an = null;

            fileChooser.FileSet += (sender, e) =>
            {
                try
                {
                    an = AnimationLoader.Load(fileChooser.Filename);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("cant load animation\n{0}\n{1}", ex.Message, ex.StackTrace);
                }
            };

            //Play
            bool play = false;
            bool pause = false;

            btnStart.Clicked += (sender, e) =>
            {
                if (an != null)
                    animator.Play(an);
                play = true;

            };
             
            btn.Clicked += (sender, e) =>
            {
                play = false;
                if (an != null)
                    animator.Stop();
            };

            btnPR.Clicked += (sender, e) =>
            {
                if (an != null)
                {
                    if (play && !pause)
                    {
                        animator.Pause();
                        btnPR.Label = "Resume";
                        pause = !pause;
                    }
                    else if (play)
                    {
                        animator.Resume();
                        btnPR.Label = "Pause";
                        pause = !pause;
                    }
                }
            };
        }

        private void BtnStart_Clicked(object sender, EventArgs e)
        {
            try
            {
                ClearChildrens(hbox1);
                int x = 570;
                int y = 60;
                var servers = ClientMaster.ScanLocalNetwork(9000);
                if (servers == null)
                    return;

                foreach (var server in servers)
                {
                    Button btn = new Button();
                    btn.Name = "btnHost";
                    btn.Label = server.GetHostname;
                    btn.Clicked += (s, ev) => {
                        if (connection != null)
                            connection.Disconnect();
                        connection = new ClientMaster(server.GetIP, (ushort)server.GetIP.Port);
                        connection2 = new ClientMaster(server.GetIP, (ushort)server.GetIP.Port);
                        ClearChildrens(hbox2);
                        ClearChildrens(hbox3);
                        label1.Text = String.Format("connected to {0} {1}", server.GetHostname, server.GetIP.Address);
                        DrawMethods(hbox2, connection.GetMethods(Methods.Simple), Methods.Simple);
                        DrawMethods(hbox3, connection.GetMethods(Methods.Data), Methods.Data);
                    };

                    hbox1.Add(btn);
                    btn.Show();
                    y += 35;
                }
                
            }
            catch (Exception em)
            {
                Console.WriteLine(em.Message);
                Console.WriteLine(em.StackTrace);
            }
        }

        
        public void DrawMethods(Box f, string[] names, Methods m)
        {
            int y = 20;
            for (int i = 0; i < names.Length; i++)
            {
                if (names[i] == "")
                    continue;
                Button btn = new Button();
                btn.Name = "btn";
                btn.Label = names[i];
                byte b = (byte)i;
                if (m == Methods.Data)
                    btn.Clicked += (s, ev) =>
                    {
                        stream = !stream;
                        try
                        {
                            if (connection.Connected)
                                Execute = (data) => connection2.ExecuteMethod(m, b, data);
                            else
                                label1.Text.Replace("connected to", "disconnected from");
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                        
                    };
                else
                    btn.Clicked += (s, ev) =>
                    {
                        if (connection.Connected)
                            connection.ExecuteMethod(m, b);
                        else
                            label1.Text.Replace("connected to", "disconnected from");
                    };
                f.Add(btn);
                y += 25;
            }

            ShowAll();
        }
        
        void ClearChildrens(Fixed fixd)
        {
            foreach (var wid in fixd.Children)
            {
                wid.Dispose();
            }
        }

        void ClearChildrens(Box fixd)
        {
            foreach (var wid in fixd.Children)
            {
                wid.Dispose();
            }
        }
    }
}
