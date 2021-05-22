
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
            this.Icon = Properties.Resources.nugget_1lg_icon;

            this.components = new System.ComponentModel.Container();
            this.GameTimer = new System.Windows.Forms.Timer(this.components);
            this.GameCanvas = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.GameCanvas)).BeginInit();
            this.SuspendLayout();
            // 
            // GameTimer
            // 
            this.GameTimer.Interval = 10;
            this.GameTimer.Tick += new System.EventHandler(this.GameTimer_Tick);
            // 
            // GameCanvas
            // 
            this.GameCanvas.BackColor = System.Drawing.Color.Transparent;
            this.GameCanvas.Location = new System.Drawing.Point(0, 0);
            this.GameCanvas.Margin = new System.Windows.Forms.Padding(0);
            this.GameCanvas.Name = "GameCanvas";
            this.GameCanvas.Size = new System.Drawing.Size(960, 540);
            this.GameCanvas.TabIndex = 7;
            this.GameCanvas.TabStop = false;
            this.GameCanvas.Paint += new System.Windows.Forms.PaintEventHandler(this.GameCanvas_Paint);
            // 
            // GameForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(2)))), ((int)(((byte)(104)))));
            this.ClientSize = new System.Drawing.Size(960, 540);
            this.Controls.Add(this.GameCanvas);
            this.Name = "GameForm";
            this.Text = "Nugget Blaster";
            this.ResizeEnd += new System.EventHandler(this.GameForm_ResizeEnd);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.GameKeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.GameKeyUp);
            this.Resize += new System.EventHandler(this.GameForm_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.GameCanvas)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Timer GameTimer;
        private System.Windows.Forms.PictureBox GameCanvas;
    }
}

