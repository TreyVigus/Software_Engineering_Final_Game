using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ACFramework
{
    class cCritterBossDrFreak : cCritterBoss
    {
        public cCritterBossDrFreak(cGame3D pownergame)
            : base(pownergame)
        {

        }

        public cCritterBossDrFreak(cGame3D pownergame, cVector3 position, cCritterBullet bullet, float firerate = 4.0f, int health = 20, int model = 6)
            : base(pownergame, position, model, bullet, firerate, health)
        {

        }

        public override void update(ACView pactiveview, float dt)
        {
            base.update(pactiveview, dt);
            //aimAt(Game.Player);
        }

        public override cCritterBullet shoot()
        {
            Framework.snd.play(Sound.Blip);
            return base.shoot();
        }

        public override bool IsKindOf(string str)
        {
            return str == "cCritterBossDrFreak" || base.IsKindOf(str);
        }

        public override string RuntimeClass
        {
            get
            {
                return "cCritterBossDrFreak";
            }
        }
    }
}
