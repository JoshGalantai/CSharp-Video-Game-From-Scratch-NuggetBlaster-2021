using NuggetBlaster.GameCore;
using NuggetBlaster.Properties;
using System;
using System.Drawing;

namespace NuggetBlaster.Entities
{
    class BuffHealEntity : BuffEntity
    {
        public BuffHealEntity(Rectangle gameCanvas, Random random, Image sprite = null) : base(gameCanvas, random, sprite ?? Resources.buffHeart) { }

        public override PlayerEntity AddBuff(PlayerEntity entity)
        {
            if (entity.HitPoints < EntityManager.MaxPlayerHP)
                entity.HitPoints++;
            return entity;
        }
    }
}
