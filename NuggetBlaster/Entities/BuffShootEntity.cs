using NuggetBlaster.GameCore;
using NuggetBlaster.Properties;
using System;
using System.Drawing;

namespace NuggetBlaster.Entities
{
    class BuffShootEntity : BuffEntity
    {
        public BuffShootEntity(Rectangle gameCanvas, Random random, Image sprite = null) : base(gameCanvas, random, sprite ?? Resources.buffShoot){}

        public override PlayerEntity AddBuff(PlayerEntity entity)
        {
            if (entity.ShootBuffLevel < PlayerEntity.MaxShootBuffLevel)
                entity.ShootBuffLevel++;
            return entity;
        }
    }
}
