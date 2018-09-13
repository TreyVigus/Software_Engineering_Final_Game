using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ACFramework
{
    class cCritterBossDragonKnight : cCritterBoss
    {
        public cCritterBossDragonKnight(cGame3D pownergame)
            : base(pownergame)
        {

        }

        public cCritterBossDragonKnight(cGame3D pownergame, cVector3 position, cCritterBullet bullet, float firerate = 4.0f, int health = 20, int model = 7)
            : base(pownergame, position, model, bullet, firerate, health)
        {
            setRadius(4.0f);
            //custom animation frames
            Sprite.setstate(State.Other, 53, 63, StateType.Repeat);
        }

        public override void update(ACView pactiveview, float dt)
        {
            base.update(pactiveview, dt);
            addForce(new CenteringForce());
        }

        public override cCritterBullet shoot()
        {
            Framework.snd.play(Sound.Blip);
            return base.shoot();
        }

        public override bool IsKindOf(string str)
        {
            return str == "cCritterBossDragonKnight" || base.IsKindOf(str);
        }

        public override string RuntimeClass
        {
            get
            {
                return "cCritterBossDragonKnight";
            }
        }
    }
}
