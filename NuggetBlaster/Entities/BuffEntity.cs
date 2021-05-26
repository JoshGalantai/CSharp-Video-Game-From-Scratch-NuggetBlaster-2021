using NuggetBlaster.GameCore;
using NuggetBlaster.Properties;
using System;
using System.Drawing;

namespace NuggetBlaster.Entities
{
    abstract class BuffEntity : Entity
    {
        public BuffEntity(Rectangle gameCanvas, Random random, Image sprite = null) : base(gameCanvas, new Rectangle(gameCanvas.Width, random.Next((int)(gameCanvas.Height/2 - gameCanvas.Height*0.1), (int)(gameCanvas.Height/2 + gameCanvas.Height*0.1)), (int)(gameCanvas.Width * 0.05), (int)(gameCanvas.Width * 0.05)), sprite ?? Resources.buffHeart)
        {
            MoveLeft   = true;
            Team       = 1;
            Damageable = false;
            Damage     = 0;
        }

        public abstract PlayerEntity AddBuff(PlayerEntity entity);
    }
}
