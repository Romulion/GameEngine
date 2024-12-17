using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using Toys;

namespace ModelViewerWF
{
    public partial class Form1 : Form
    {
        OpenFileDialog openFileDialog;
        internal Scene scene;

        internal Camera cam;
        BackgroundSkybox bgs;
        Animator animator;
        Animation an;
        bool play;
        bool pause;

        public Form1()
        {
            InitializeComponent();
        }
         
        private void Form1_Load(object sender, EventArgs e)
        {
            button2.Enabled = false;
            button3.Enabled = false;
            button4.Enabled = false;
            openFileDialog = new OpenFileDialog();
            openFileDialog1.ShowHelp = true;
            openFileDialog1.Filter = "PMM files (*.lmd) | *.lmd| All files | *.*";
            bgs = new BackgroundSkybox();
            //attach first animator
            if (scene != null)
            {
                foreach (var node in scene.GetNodes())
                {
                    var animComp = node.GetComponent<Animator>();
                    if (animComp)
                    {
                        animator = (Animator)animComp;
                        break;
                    }
                }
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            if (!animator)
            {
                textBox1.Text = "animator not found";
            }
            else if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string file = openFileDialog1.FileName;
                    an = ResourcesManager.LoadAsset<Animation>(file);
                    button2.Enabled = true;
                    button3.Enabled = true;
                    button4.Enabled = true;
                    button1.Text = file.Substring(file.LastIndexOf('\\')+1);
                }
                catch (Exception exc)
                {
                    button1.Text = "Choose animation file";
                    textBox1.Text = exc.Message + "\n" + exc.StackTrace;
                    button2.Enabled = false;
                    button3.Enabled = false;
                    button4.Enabled = false;
                }
            }
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            play = true;
            animator.Play();
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            play = false;
            if (an != null)
                animator.Stop();
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            if (an != null)
            {
                if (play && !pause)
                {
                    animator.Pause();
                    button4.Text = "Resume";
                    pause = !pause;
                }
                else if (play)
                {
                    animator.Resume();
                    button4.Text = "Pause";
                    pause = !pause;
                }
            }
        }
    }
}
