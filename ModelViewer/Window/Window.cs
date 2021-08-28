using System;
using Gtk;
using Toys;
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
        Scene scene;
        public Window(Scene scene, CoreEngine core) :
                base(WindowType.Toplevel)
        {
            this.core = core;
            Build();
            DeleteEvent += delegate { Application.Quit(); };
            var disable = new Gdk.Color(10, 200, 10);

            DrawScene(scene);
            //SetAnimator(anim);
            //CreateWindow();
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
                scale.Value = morph.MorphDegree;
                scale.ValueChanged += (sender, e) =>
                {
                    core.AddTask = () => morph.MorphDegree = (float)scale.Value;
                };

                fixed7.Put(scale, 100, y);
                scale.Show();
                y += 40;
            }
        }

        void DrawScene(Scene scene)
        {
            this.scene = scene;
            var fileChooserAdd = new FileChooserButton("Select model", FileChooserAction.Open);
            fileChooserAdd.WidthRequest = 124;
            fileChooserAdd.Name = "filechooserbutton3";
            fileChooserAdd.FileSet += (sender, e) =>
            {
                var wait = core.AddNotyfyTask(() =>
                {
                    SceneNode modelNode = ResourcesManager.LoadAsset<SceneNode>(fileChooserAdd.Filename);
                    scene.AddNode2Root(modelNode);
                    //Set name
                    var path = System.IO.Path.GetDirectoryName(fileChooserAdd.Filename);
                    modelNode.Name = path.Substring(path.LastIndexOf('\\') + 1) + "." + System.IO.Path.GetFileNameWithoutExtension(fileChooserAdd.Filename);
                });
                wait.WaitOne();
                ClearChildrens(fixedScene);
                DrawScene(scene);
            };
            fixedScene.Put(fileChooserAdd, 0, 0);
            fileChooserAdd.Show();
            int y = 35;
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
                fixedScene.Put(btn, 0, y);
                btn.Show();
                y += 35;
            }
            var cont = fixedScene.CreatePangoContext();
        }

        void DrawComponents(SceneNode node)
        {
            int y = 0;
            ClearChildrens(fixedComponents);
            ClearChildrens(fixed4);
            foreach (var component in node.GetComponents())
            {
                if (component is MeshDrawerRigged)
                {
                    Button btn = new Button();
                    btn.Label = "MeshDrawerRigged";
                    btn.Name = "btnComp";
                    btn.Clicked += (sender, e) => { MeshDrawer((MeshDrawerRigged)component); };
                    fixedComponents.Put(btn, 0, y);
                    btn.Show();
                    y += 35;
                }
                else if (component is MeshDrawer)
                {
                    Button btn = new Button();
                    btn.Label = "MeshDrawer";
                    btn.Name = "btnComp";
                    btn.Clicked += (sender, e) => { MeshDrawer((MeshDrawer)component); };
                    fixedComponents.Put(btn, 0, y);
                    btn.Show();
                    y += 35;
                }
                else if (component is Animator)
                {
                    Button btn = new Button();
                    btn.Label = "Animator";
                    btn.Clicked += (sender, e) => { AnimatorWindow((Animator)component); };
                    btn.Name = "btnComp";
                    fixedComponents.Put(btn, 0, y);
                    btn.Show();
                    y += 35;
                }
                else
                {
                    Button btn = new Button();
                    btn.Label = component.Type.Name;
                    btn.Clicked += (sender, e) => {DrawComponent(component); };
                    btn.Name = "btnComp";
                    fixedComponents.Put(btn, 0, y);
                    btn.Show();
                    y += 35;
                    
                }

            }
            //delete button
            
            Button btn1 = new Button();
            btn1.Label = "Delete";
            btn1.Clicked += (sender, e) => {
                scene.RemoveNode(node);
                ResourcesManager.DeleteResource(node);
                ClearChildrens(fixedComponents);
                ClearChildrens(fixed4);
                ClearChildrens(fixedScene);
                DrawScene(scene);
            };
            btn1.Name = "btnComp";
            fixedComponents.Put(btn1, 0, y);
            btn1.Show();
            
        }

        void MeshDrawer(MeshDrawer meshDrawer)
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
                btn.Clicked += (s, e) => { SetMorphList(meshDrawer.Morphes); fixed6.Show(); notebook.Page = morphPanel; };
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

        void DrawComponent(Component comp)
        {
            ClearChildrens(fixed4);
            var fieds = comp.Type.GetProperties();
            int offset = 0;
            foreach (var fied in fieds)
            {
                PropertiesButtons.DrawField(comp, fied, fixed4, ref offset);
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

            fileChooser.FileSet += (sender, e) =>
            {
                try
                {
                    var an = AnimationLoader.Load(fileChooser.Filename);
                    if (an != null)
                        animator.AnimationData = an;
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
                animator.Play();
                play = true;

            };
             
            btn.Clicked += (sender, e) =>
            {
                play = false;
                animator.Stop();
            };

            btnPR.Clicked += (sender, e) =>
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
                {
                    //stream method
                    if (btn.Label == "draw image")
                    {
                        btn.Clicked += (s, ev) =>
                        {
                            stream = !stream;
                            try
                            {
                                if (connection2.Connected)
                                    Execute = (data) => connection2.ExecuteMethod(m, b, data);
                                else
                                    label1.Text.Replace("connected to", "disconnected from");
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                            }

                        };
                    }
                    else if (btn.Label == "reposition")
                    {
                        Entry entryX = new Entry();
                        Entry entryY = new Entry();
                        entryX.InputPurpose = InputPurpose.Digits;
                        entryY.InputPurpose = InputPurpose.Digits;
                        entryY.InsertText("0");
                        entryX.InsertText("0");
                        btn.Clicked += (s, ev) =>
                        {
                            try
                            {
                            if (connection.Connected)
                            {
                                byte[] data = new byte[8];
                                Array.Copy(BitConverter.GetBytes(Int32.Parse(entryX.Text)), data, 4);
                                Array.Copy(BitConverter.GetBytes(Int32.Parse(entryY.Text)),0, data, 4,4);
                                    connection.ExecuteMethod(m, b, data);
                                }
                                else
                                    label1.Text.Replace("connected to", "disconnected from");
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                            }

                        };

                        f.Add(entryX);
                        f.Add(entryY);
                    }
                }
                else
                {
                    btn.Clicked += (s, ev) =>
                    {
                        if (connection.Connected)
                            connection.ExecuteMethod(m, b);
                        else
                            label1.Text.Replace("connected to", "disconnected from");
                    };
                }
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

        /// <summary>
        /// test window for dynamic form script
        /// </summary>
        void CreateWindow()
        {
            var window = new Gtk.Window(WindowType.Toplevel);
            var image = new Image();
            image.File = "test.png";
            window.Decorated = false;
            window.Visual = window.Screen.RgbaVisual;
            CssProvider cssp = new CssProvider();
            cssp.LoadFromData("window { background-color: transparent; }");
            window.StyleContext.AddProvider(cssp, StyleProviderPriority.Application);
            window.Add(image);
            window.ShowAll();
        }
    }
}
