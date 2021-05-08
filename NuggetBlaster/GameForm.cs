using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NuggetBlaster
{
    public partial class GameForm : Form
    {
        int left = 0;
        int right = 0;
        int up = 0;
        int down = 0;

        public GameForm()
        {
            InitializeComponent();
            gameTimer.Start();
        }

        private void gameKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up)
            {
                this.up = 10;
            } 
            if (e.KeyCode == Keys.Down)
            {
                this.down = 10;
            }
            if (e.KeyCode == Keys.Left)
            {
                this.left = 10;
            }
            if (e.KeyCode == Keys.Right)
            {
                this.right = 10;
            }
        }

        private void gameKeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up)
            {
                this.up = 0;
            }
            if (e.KeyCode == Keys.Down)
            {
                this.down = 0;
            }
            if (e.KeyCode == Keys.Left)
            {
                this.left = 0;
            }
            if (e.KeyCode == Keys.Right)
            {
                this.right = 0;
            }
        }

        private void gameTimer_Tick(object sender, EventArgs e)
        {
            nuggetHero.Top -= up - down;
            nuggetHero.Left -= left - right;
        }
    }
}
