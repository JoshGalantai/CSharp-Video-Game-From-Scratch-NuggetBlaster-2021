// < auto - generated />
namespace NuggetBlaster
{
    partial class GameForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Timer gameTimer;
        private System.Windows.Forms.PictureBox gameCanvas;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
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
            this.gameTimer = new System.Windows.Forms.Timer(this.components);
            this.gameCanvas = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.gameCanvas)).BeginInit();
            this.SuspendLayout();
            // 
            // gameTimer
            // 
            this.gameTimer.Interval = 10;
            this.gameTimer.Tick += new System.EventHandler(this.GameTimer_Tick);
            // 
            // gameCanvas
            // 
            this.gameCanvas.BackColor = System.Drawing.Color.Transparent;
            this.gameCanvas.Location = new System.Drawing.Point(0, 0);
            this.gameCanvas.Margin = new System.Windows.Forms.Padding(0);
            this.gameCanvas.Name = "gameCanvas";
            this.gameCanvas.Size = new System.Drawing.Size(960, 540);
            this.gameCanvas.TabIndex = 7;
            this.gameCanvas.TabStop = false;
            this.gameCanvas.Paint += new System.Windows.Forms.PaintEventHandler(this.GameCanvas_Paint);
            // 
            // GameForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(2)))), ((int)(((byte)(104)))));
            this.ClientSize = new System.Drawing.Size(960, 540);
            this.Controls.Add(this.gameCanvas);
            this.Name = "GameForm";
            this.Text = "Nugget Blaster";
            this.ResizeEnd += new System.EventHandler(this.GameForm_ResizeEnd);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.GameKeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.GameKeyUp);
            this.Resize += new System.EventHandler(this.GameForm_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.gameCanvas)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion
    }
}