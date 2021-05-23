using NuggetBlaster.GameCore;
using NuggetBlaster.Properties;
using System;
using System.Drawing;

namespace NuggetBlaster.Entities
{
    class BuffEntity : Entity
    {
        public BuffEntity(Rectangle gameCanvas, Random random, Image sprite = null) : base(gameCanvas, new Rectangle(gameCanvas.Width, random.Next(10, gameCanvas.Height - (int)(gameCanvas.Width * 0.05)), (int)(gameCanvas.Width * 0.05), (int)(gameCanvas.Width * 0.05)), sprite ?? Resources.buffHeart)
        {
            MoveLeft   = true;
            Team       = 1;
            Damageable = false;
        }

        public virtual PlayerEntity AddBuff(PlayerEntity entity)
        {
            if (entity.HitPoints <= Engine.MaxPlayerHP)
                entity.HitPoints++;
            return entity;
        }
    }
}
