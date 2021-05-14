
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
            this.gameTimer = new System.Windows.Forms.Timer(this.components);
            this.background = new System.Windows.Forms.PictureBox();
            this.NuggetBlasterText = new System.Windows.Forms.Label();
            this.InsertCoinText = new System.Windows.Forms.Label();
            this.PressEnterText = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.background)).BeginInit();
            this.SuspendLayout();
            // 
            // gameTimer
            // 
            this.gameTimer.Interval = 10;
            this.gameTimer.Tick += new System.EventHandler(this.GameTimer_Tick);
            // 
            // background
            // 
            this.background.Image = global::NuggetBlaster.Properties.Resources.Background;
            this.background.Location = new System.Drawing.Point(0, -3);
            this.background.Name = "background";
            this.background.Size = new System.Drawing.Size(1920, 540);
            this.background.TabIndex = 3;
            this.background.TabStop = false;
            // 
            // NuggetBlasterText
            // 
            this.NuggetBlasterText.AutoSize = true;
            this.NuggetBlasterText.Font = new System.Drawing.Font("Showcard Gothic", 72F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.NuggetBlasterText.ForeColor = System.Drawing.Color.White;
            this.NuggetBlasterText.Location = new System.Drawing.Point(60, 166);
            this.NuggetBlasterText.Name = "NuggetBlasterText";
            this.NuggetBlasterText.Size = new System.Drawing.Size(840, 119);
            this.NuggetBlasterText.TabIndex = 4;
            this.NuggetBlasterText.Text = "NUGGET BLASTER";
            // 
            // InsertCoinText
            // 
            this.InsertCoinText.AutoSize = true;
            this.InsertCoinText.Font = new System.Drawing.Font("Arial Narrow", 26.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.InsertCoinText.ForeColor = System.Drawing.Color.White;
            this.InsertCoinText.Location = new System.Drawing.Point(380, 285);
            this.InsertCoinText.Name = "InsertCoinText";
            this.InsertCoinText.Size = new System.Drawing.Size(200, 42);
            this.InsertCoinText.TabIndex = 5;
            this.InsertCoinText.Text = "INSERT COIN";
            // 
            // PressEnterText
            // 
            this.PressEnterText.AutoSize = true;
            this.PressEnterText.Font = new System.Drawing.Font("Arial Narrow", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.PressEnterText.ForeColor = System.Drawing.Color.White;
            this.PressEnterText.Location = new System.Drawing.Point(418, 327);
            this.PressEnterText.Name = "PressEnterText";
            this.PressEnterText.Size = new System.Drawing.Size(124, 25);
            this.PressEnterText.TabIndex = 6;
            this.PressEnterText.Text = "- Press Enter -";
            // 
            // GameForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(2)))), ((int)(((byte)(104)))));
            this.ClientSize = new System.Drawing.Size(944, 501);
            this.Controls.Add(this.PressEnterText);
            this.Controls.Add(this.InsertCoinText);
            this.Controls.Add(this.NuggetBlasterText);
            this.Controls.Add(this.background);
            this.Name = "GameForm";
            this.Text = "Nugget Blaster";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.GameKeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.GameKeyUp);
            ((System.ComponentModel.ISupportInitialize)(this.background)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Timer gameTimer;
        private System.Windows.Forms.PictureBox background;
        private System.Windows.Forms.Label NuggetBlasterText;
        private System.Windows.Forms.Label InsertCoinText;
        private System.Windows.Forms.Label PressEnterText;
    }
}

