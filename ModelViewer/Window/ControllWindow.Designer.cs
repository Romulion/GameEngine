
namespace ModelViewer
{
    partial class ControllWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tabControll = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.flowLayoutPanelData = new System.Windows.Forms.FlowLayoutPanel();
            this.flowLayoutPanelComponent = new System.Windows.Forms.FlowLayoutPanel();
            this.flowLayoutPanelScene = new System.Windows.Forms.FlowLayoutPanel();
            this.button1 = new System.Windows.Forms.Button();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.panel1 = new System.Windows.Forms.Panel();
            this.tabControll.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.flowLayoutPanelScene.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControll
            // 
            this.tabControll.Controls.Add(this.tabPage1);
            this.tabControll.Controls.Add(this.tabPage2);
            this.tabControll.Location = new System.Drawing.Point(13, 13);
            this.tabControll.Name = "tabControll";
            this.tabControll.SelectedIndex = 0;
            this.tabControll.Size = new System.Drawing.Size(590, 425);
            this.tabControll.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.flowLayoutPanelData);
            this.tabPage1.Controls.Add(this.flowLayoutPanelComponent);
            this.tabPage1.Controls.Add(this.flowLayoutPanelScene);
            this.tabPage1.Location = new System.Drawing.Point(4, 24);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(582, 397);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Scene";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // flowLayoutPanelData
            // 
            this.flowLayoutPanelData.AutoScroll = true;
            this.flowLayoutPanelData.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.flowLayoutPanelData.Location = new System.Drawing.Point(400, 6);
            this.flowLayoutPanelData.Name = "flowLayoutPanelData";
            this.flowLayoutPanelData.Size = new System.Drawing.Size(180, 384);
            this.flowLayoutPanelData.TabIndex = 4;
            // 
            // flowLayoutPanelComponent
            // 
            this.flowLayoutPanelComponent.AutoScroll = true;
            this.flowLayoutPanelComponent.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.flowLayoutPanelComponent.Location = new System.Drawing.Point(209, 6);
            this.flowLayoutPanelComponent.Name = "flowLayoutPanelComponent";
            this.flowLayoutPanelComponent.Size = new System.Drawing.Size(176, 384);
            this.flowLayoutPanelComponent.TabIndex = 2;
            // 
            // flowLayoutPanelScene
            // 
            this.flowLayoutPanelScene.AutoScroll = true;
            this.flowLayoutPanelScene.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.flowLayoutPanelScene.Controls.Add(this.button1);
            this.flowLayoutPanelScene.Location = new System.Drawing.Point(7, 7);
            this.flowLayoutPanelScene.Name = "flowLayoutPanelScene";
            this.flowLayoutPanelScene.Size = new System.Drawing.Size(176, 383);
            this.flowLayoutPanelScene.TabIndex = 0;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(3, 3);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(152, 32);
            this.button1.TabIndex = 0;
            this.button1.Text = "Refresh";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.panel1);
            this.tabPage2.Location = new System.Drawing.Point(4, 24);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(582, 397);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Morphs";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.AutoScroll = true;
            this.panel1.Location = new System.Drawing.Point(8, 11);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(277, 380);
            this.panel1.TabIndex = 0;
            // 
            // ControllWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(612, 450);
            this.Controls.Add(this.tabControll);
            this.Name = "ControllWindow";
            this.Text = "ControllWindow";
            this.Load += new System.EventHandler(this.ControllWindow_Load);
            this.tabControll.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.flowLayoutPanelScene.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControll;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelComponent;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelScene;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelData;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Panel panel1;
    }
}