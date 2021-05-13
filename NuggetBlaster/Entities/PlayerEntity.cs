using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuggetBlaster.Entities
{
    public class PlayerEntity : Entity
    {
        public override int baseSpeed { get; set; } = 400;

        public override int  Team     { get; set; } = 1;
        public override bool CanShoot { get; set; } = true;
        public override bool Spacebar { get; set; } = false;
        public override long ShootCooldownTimer { get; set; } = 0;
        public override int ShootCooldownMS { get; set; } = 200;

        public PlayerEntity() {
            MaxSpeed = baseSpeed;
        }

        public override ProjectileEntity Shoot()
        {
            ShootCooldown();
            return new ProjectileEntity
            {
                MoveRight = true,
                MaxSpeed = ProjectileEntity.baseSpeed * 2,
                Team = Team
            };
        }



    }
}
