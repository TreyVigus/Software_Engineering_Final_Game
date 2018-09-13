using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ACFramework
{
    class cCritterBossMog : cCritterBoss
    {
        public cCritterBossMog(cGame3D pownergame)
            : base(pownergame)
        {

        }

        public cCritterBossMog(cGame3D pownergame, cVector3 position, cCritterBullet bullet, float firerate = 4.0f, int health = 20, int model = 5)
            : base(pownergame, position, model, bullet, firerate, health)
        {

        }

        public override void update(ACView pactiveview, float dt)
        {
            base.update(pactiveview, dt);

            // keep the boss a certain distance from the player
            if (distanceTo(Player) < 6)
            {
                if (Player.Position.Z > this.Position.Z)
                {
                    clearForcelist();
                    addForce(new cForceGravity(25.0f, new cVector3(0.0f, -1, 0.00f)));
                    addForce(new cForceDrag(20.0f));  // default friction strength 0.5 
                    addForce(new CenteringForce());
                    addForce(new KnockbackForce(new cVector3(0, 0, -10.0f)));

                    //if boss facing right, rotate to face player
                    if (facing)
                    {
                        rotate();
                    }
                }
                else
                {
                    clearForcelist();
                    addForce(new cForceGravity(25.0f, new cVector3(0.0f, -1, 0.00f)));
                    addForce(new cForceDrag(20.0f));  // default friction strength 0.5 
                    addForce(new CenteringForce());
                    addForce(new KnockbackForce(new cVector3(0, 0, 10.0f)));

                    //if boss facing left, rotate to face player
                    if (!facing)
                    {
                        rotate();
                    }
                }
            }
            else
            {
                if (Player.Position.Z > this.Position.Z)
                {
                    // rotate boss to face player
                    if (facing)
                    {
                        rotate();
                    }
                }
                else
                {
                    // rotate boss to face player
                    if (!facing)
                    {
                        rotate();
                    }
                }
            }
        }

        private bool rotate()
        {
            this.yaw((float)Math.PI);
            this.rotateAttitude(this.Tangent.rotationAngle(this.AttitudeTangent));
            facing = !facing;
            return facing;
        }

        public override cCritterBullet shoot()
        {
            Framework.snd.play(Sound.BottleRocket);
            return base.shoot();
        }

        public override bool IsKindOf(string str)
        {
            return str == "cCritterBossMog" || base.IsKindOf(str);
        }

        public override string RuntimeClass
        {
            get
            {
                return "cCritterBossMog";
            }
        }
    }
}
