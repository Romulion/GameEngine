using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Toys;

namespace ModelViewer
{
    public partial class ControllWindow : Form
    {
        public Scene mainScene;
        List<Button> sceneNodes;
        OpenFileDialog ofg;
        public ControllWindow()
        {
            sceneNodes = new List<Button>();
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DrawScene();
        }

        internal void DrawScene()
        {
            int yOffset = 36;
            //clean list before filling
            foreach (var node in sceneNodes)
                flowLayoutPanelScene.Controls.Remove(node);
            sceneNodes.Clear();

            if (mainScene == null)
                return;

            foreach (var node in mainScene.GetNodes())
            {
                
                var button = new Button();
                button.Text = node.Name;
                button.Size = new Size(152, 32);
                button.Location = new Point(0, yOffset);
                flowLayoutPanelScene.Controls.Add(button);
                
                yOffset += 36;
                sceneNodes.Add(button);
                button.Click += (s,a) => DrawComponents(node);
            }
            
        }

        private void DrawComponents(SceneNode node)
        {
            int childStep = 5;

            //clean list before filling
            flowLayoutPanelComponent.Controls.Clear();
            int yOffset = 36;
            foreach (var component in node.GetAllComponents())
            {
                Button btn = new Button();
                btn.Size = new Size(152, 32);
                btn.Location = new Point(0, yOffset);
                btn.Name = "btnComp";
                if (component is MeshDrawerRigged)
                {
                    btn.Text = "MeshDrawerRigged";
                    btn.Click += (sender, e) => { MeshDrawer((MeshDrawerRigged)component); };
                }
                else if (component is MeshDrawer)
                {
                    btn.Text = "MeshDrawer";
                    btn.Click += (sender, e) => { MeshDrawer((MeshDrawer)component); };
                }
                else if (component is Animator)
                {
                    btn.Text = "Animator";
                    btn.Click += (sender, e) => { AnimatorWindow((Animator)component); };
                }
                else
                {
                    btn.Text = component.GetType().Name;
                    btn.Click += (sender, e) => { DrawComponent(component); };
                }
                

                btn.Location = new Point(0, yOffset);
                flowLayoutPanelComponent.Controls.Add(btn);
                yOffset += 36;
            }

            var buttonDel = new Button();
            buttonDel.Text = "Delete";
            buttonDel.Size = new Size(152, 32);
            buttonDel.Location = new Point(0, yOffset);
            buttonDel.Click += (s, a) =>
            {
                mainScene.RemoveNode(node);
                ResourcesManager.DeleteResource(node);
                flowLayoutPanelComponent.Controls.Clear();
                DrawScene();
            };
            flowLayoutPanelComponent.Controls.Add(buttonDel);
        }


        void MeshDrawer(MeshDrawer meshDrawer)
        {
            flowLayoutPanelData.Controls.Clear();
            int y = 0;
            if (meshDrawer.Morphes != null)
            {
                Button btn = CreateBtn();
                btn.Location = new Point(0, y);
                btn.Text = "Morhps";
                flowLayoutPanelData.Controls.Add(btn);
                y += 36;
                btn.Click += (s, e) => { 
                    DrawMorphList(meshDrawer.Morphes.ToArray()); 
                    tabControll.SelectedTab = tabPage2;
                };
            }

            foreach (var mat in meshDrawer.Materials)
            {
                var renderDir = mat.RenderDirrectives;

                Button btn = CreateBtn();
                btn.Text = mat.Name;
                //btn.Tooltip = mat.Name;
                btn.Click += (sender, e) =>
                {

                    renderDir.IsRendered = !renderDir.IsRendered;
                    if (renderDir.IsRendered)
                        btn.BackColor = Color.White;
                    else
                        btn.BackColor = Color.Red;
                };


                if (renderDir.IsRendered)
                    btn.BackColor = Color.White;
                else
                    btn.BackColor = Color.Red;

                btn.Location = new Point(0, y);
                flowLayoutPanelData.Controls.Add(btn);
                y += 35;
            }

        }

        void DrawMorphList(Morph[] morphs)
        {
            int y = 0;
            foreach (var morph in morphs)
            {

                if (!(morph is MorphVertex) && !(morph is MorphMaterial) && !(morph is MorphUV) && !(morph is MorphSkeleton) && !(morph is MorphGroup))
                    continue;

                //display morph type
                string prefix = "";
                if (morph is MorphVertex)
                    prefix = "(V)";
                else if (morph is MorphMaterial)
                    prefix = "(M)";
                else if (morph is MorphUV)
                    prefix = "(UV)";
                else if (morph is MorphSkeleton)
                    prefix = "(B)";
                else if (morph is MorphGroup)
                    prefix = "(G)";
                Label lbl = new Label();
                lbl.Name = "lbl";
                lbl.Text = prefix + morph.Name;
                lbl.Location = new Point(0, y);
                panel1.Controls.Add(lbl);
                // y += 30;

                var scale = new TrackBar();
                scale.Name = "scale";
                scale.Value = (int)(morph.MorphDegree * 10);
                scale.ValueChanged += (sender, e) =>
                {
                    var value = (float)scale.Value / 10;
                    CoreEngine.ActiveCore.AddTask = () => morph.MorphDegree = value;
                };
                scale.Size = new Size(150,30);
                scale.Location = new Point(100, y);
                panel1.Controls.Add(scale);
                y += 40;
            }
        }

        void DrawComponent(Toys.Component comp)
        {
            flowLayoutPanelData.Controls.Clear();
            var fieds = comp.GetType().GetProperties();
            int offset = 0;
            foreach (var fied in fieds)
            {
                PropertiesButtons.DrawField(comp, fied, flowLayoutPanelData, ref offset);
            }
        }

        void AnimatorWindow(Animator animator)
        {
            flowLayoutPanelData.Controls.Clear();

            var fileChooser = CreateBtn();
            fileChooser.Text = "Select a File";
            fileChooser.Name = "filechooserbutton2";
            fileChooser.Location = new Point(0, 19);
            flowLayoutPanelData.Controls.Add(fileChooser);
            // Container child fixed1.Gtk.Fixed+FixedChild
            var btnStart = CreateBtn();
            btnStart.Name = "button2";
            btnStart.Text = "Play";
            btnStart.Location = new Point(0, 63);
            flowLayoutPanelData.Controls.Add(btnStart);
            // Container child fixed1.Gtk.Fixed+FixedChild
            var btn = CreateBtn();
            btn.Name = "button3";
            btn.Text = "Stop";
            btn.Location = new Point(0, 108);
            flowLayoutPanelData.Controls.Add(btn);
            // Container child fixed1.Gtk.Fixed+FixedChild
            var btnPR = CreateBtn();
            btnPR.Name = "button4";
            btnPR.Text = "Pause";
            btnPR.Show();
            btnPR.Location = new Point(0, 153);
            flowLayoutPanelData.Controls.Add(btnPR);

            fileChooser.Click += (sender, e) => OpenAnimation(animator);


            //Play
            bool play = false;
            bool pause = false;
            btnStart.Click += (sender, e) =>
            {
                animator.Play();
                play = true;

            };

            btn.Click += (sender, e) =>
            {
                play = false;
                animator.Stop();
            };

            btnPR.Click += (sender, e) =>
            {

                if (play && !pause)
                {
                    animator.Pause();
                    btnPR.Text = "Resume";
                    pause = !pause;
                }
                else if (play)
                {
                    animator.Resume();
                    btnPR.Text = "Pause";
                    pause = !pause;
                }

            };
        }

        void OpenAnimation(Animator animator)
        {
            if (ofg.ShowDialog(this) == DialogResult.OK)
            {

                try
                {
                    var an = AnimationLoader.Load(System.IO.File.OpenRead(ofg.FileName), ofg.FileName);
                    if (an != null)
                        animator.AnimationData = an;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("cant load animation\n{0}\n{1}", ex.Message, ex.StackTrace);
                }
            }
        }
        Button CreateBtn()
        {
            Button btn = new Button();
            btn.Name = "btn";
            btn.Size = new Size(152, 32);
            btn.ForeColor = Color.Gray;
            return btn;
        }

        private void ControllWindow_Load(object sender, EventArgs e)
        {
            ofg = new OpenFileDialog();
            ofg.Multiselect = false;
        }
    }
}
