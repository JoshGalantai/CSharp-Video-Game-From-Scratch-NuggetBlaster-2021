
namespace NuggetBlaster
{
    partial class GameForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.nuggetHero = new System.Windows.Forms.PictureBox();
            this.pickleMonster = new System.Windows.Forms.PictureBox();
            this.gameTimer = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.nuggetHero)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pickleMonster)).BeginInit();
            this.SuspendLayout();
            // 
            // nuggetHero
            // 
            this.nuggetHero.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
            this.nuggetHero.Location = new System.Drawing.Point(12, 224);
            this.nuggetHero.Name = "nuggetHero";
            this.nuggetHero.Size = new System.Drawing.Size(100, 50);
            this.nuggetHero.TabIndex = 0;
            this.nuggetHero.TabStop = false;
            // 
            // pickleMonster
            // 
            this.pickleMonster.BackColor = System.Drawing.Color.Lime;
            this.pickleMonster.Location = new System.Drawing.Point(966, 54);
            this.pickleMonster.Name = "pickleMonster";
            this.pickleMonster.Size = new System.Drawing.Size(57, 50);
            this.pickleMonster.TabIndex = 1;
            this.pickleMonster.TabStop = false;
            // 
            // gameTimer
            // 
            this.gameTimer.Interval = 33;
            this.gameTimer.Tick += new System.EventHandler(this.gameTimer_Tick);
            // 
            // GameForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1074, 532);
            this.Controls.Add(this.pickleMonster);
            this.Controls.Add(this.nuggetHero);
            this.Name = "GameForm";
            this.Text = "Form1";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.gameKeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.gameKeyUp);
            ((System.ComponentModel.ISupportInitialize)(this.nuggetHero)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pickleMonster)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox nuggetHero;
        private System.Windows.Forms.PictureBox pickleMonster;
        private System.Windows.Forms.Timer gameTimer;
    }
}

