using NuggetBlaster.Properties;
using System.Drawing;

namespace NuggetBlaster.Entities
{
    class BossEntity : EnemyEntity
    {
        public static int MaxHP = 50;

        public BossEntity(Rectangle gameCanvas, Rectangle spriteRectangle, Image sprite = null) : base(gameCanvas, spriteRectangle, sprite ?? Resources.bossPickle)
        {
            PointsOnKill        = 50000;
            ShootCooldownMS     = 2500;
            HitPoints           = MaxHP;
            BaseSpeed           = 0.250;
            IsDamagedOnTouch    = false;
            AllowHorizontalExit = false;
            TripleShot          = true;
            CanShoot            = true;
        }
    }
}