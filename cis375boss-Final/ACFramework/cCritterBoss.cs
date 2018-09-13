using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ACFramework
{
    class cCritterBoss : cCritterArmedRobot
    {
        // True is facing right, false is left
        protected bool facing;

        public cCritterBoss(cGame pownergame)
            : base(pownergame)
        {
            addForce(new cForceGravity(25.0f, new cVector3(0.0f, -1, 0.00f)));
            addForce(new cForceDrag(20.0f));  // default friction strength 0.5 
            addForce(new CenteringForce());

            Density = 2.0f;
            MaxSpeed = 30.0f;
            facing = false;
            if (pownergame != null) //Just to be safe.
                Sprite = new cSpriteQuake(Framework.models.selectRandomCritter());

            // example of setting a specific model
            // setSprite(new cSpriteQuake(ModelsMD2.Knight));

            if (Sprite.IsKindOf("cSpriteQuake")) //Don't let the figurines tumble.  
            {
                AttitudeToMotionLock = false;
                Attitude = new cMatrix3(new cVector3(0.0f, 0.0f, 1.0f),
                    new cVector3(1.0f, 0.0f, 0.0f),
                    new cVector3(0.0f, 1.0f, 0.0f), Position);
                /* Orient them so they are facing towards positive Z with heads towards Y. */
            }
            Bounciness = 0.0f; //Not 1.0 means it loses a bit of energy with each bounce.
            setRadius(1.0f);
            MinTwitchThresholdSpeed = 4.0f; //Means sprite doesn't switch direction unless it's moving fast 
            randomizePosition(new cRealBox3(new cVector3(_movebox.Lox, _movebox.Loy, _movebox.Loz + 4.0f),
                new cVector3(_movebox.Hix, _movebox.Loy, _movebox.Midz - 1.0f)));
            /* I put them ahead of the player  */
            randomizeVelocity(0.0f, 30.0f, false);


            //if (pownergame != null) //Then we know we added this to a game so pplayer() is valid 
                //addForce(new cForceObjectSeek(Player, 0.5f));

            int begf = Framework.randomOb.random(0, 171);
            int endf = Framework.randomOb.random(0, 171);

            if (begf > endf)
            {
                int temp = begf;
                begf = endf;
                endf = temp;
            }

            Sprite.setstate(State.Other, begf, endf, StateType.Repeat);


            _wrapflag = cCritter.BOUNCE;
        }

        public cCritterBoss(cGame pownergame, cVector3 position, int model, cCritterBullet bullet, float firerate, int health)
            : base(pownergame)
        {
            addForce(new cForceGravity(25.0f, new cVector3(0.0f, -1, 0.00f)));
            addForce(new cForceDrag(20.0f));  // default friction strength 0.5
            addForce(new CenteringForce());

            Density = 2.0f;
            MaxSpeed = 30.0f;
            facing = false;
            if (pownergame != null) //Just to be safe.
                Sprite = new cSpriteQuake(model);

            // example of setting a specific model
            // setSprite(new cSpriteQuake(ModelsMD2.Knight));

            if (Sprite.IsKindOf("cSpriteQuake")) //Don't let the figurines tumble.  
            {
                AttitudeToMotionLock = false;
                Attitude = new cMatrix3(new cVector3(0.0f, 0.0f, 1.0f),
                    new cVector3(1.0f, 0.0f, 0.0f),
                    new cVector3(0.0f, 1.0f, 0.0f), Position);
                /* Orient them so they are facing towards positive Z with heads towards Y. */
            }
            Bounciness = 0.0f; //Not 1.0 means it loses a bit of energy with each bounce.
            setRadius(1.0f);
            MinTwitchThresholdSpeed = 4.0f; //Means sprite doesn't switch direction unless it's moving fast 
            moveTo(position);
            /* I put them ahead of the player  */
            randomizeVelocity(0.0f, 30.0f, false);

            int begf = Framework.randomOb.random(0, 171);
            int endf = Framework.randomOb.random(0, 171);

            if (begf > endf)
            {
                int temp = begf;
                begf = endf;
                endf = temp;
            }

            setTarget(Player);

            Sprite.setstate(State.Other, begf, endf, StateType.Repeat);

            BulletClass = bullet;

            WaitShoot = firerate;
            Health = health;

            _wrapflag = cCritter.BOUNCE;
        }

        public override void update(ACView pactiveview, float dt)
        {
            base.update(pactiveview, dt); //Always call this first
            //Console.WriteLine(" Player x: " + Player.Position.X + " Boss x: " + Position.X);
        }

        public override bool IsKindOf(string str)
        {
            return str == "cCritterBoss" || base.IsKindOf(str);
        }

        public override string RuntimeClass
        {
            get
            {
                return "cCritterBoss";
            }
        }
    }

    class CenteringForce : cForce
    {

        public override cVector3 force(cCritter pcritter)
        {
            
            //cCritterBoss boss = (cCritterBoss)pcritter;
            if(pcritter.Position.X < pcritter.Player.Position.X)
            {
                //Console.WriteLine("boss is less");
                return new cVector3(5.0f, 0, 0);
            }
            else if(pcritter.Position.X > pcritter.Player.Position.X)
            {
                //Console.WriteLine("boss is more");
                return new cVector3(-5.0f, 0, 0);
            }

            return new cVector3(0, 0, 0);
            
        }

        public override void copy(cForce pforce)
        {
            base.copy(pforce);
            if (!pforce.IsKindOf("CenteringForce"))
                return;
            CenteringForce pforcechild = (CenteringForce)pforce;
            //copy fields here
            
        }
        public override cForce copy()
        {
            CenteringForce bs = new CenteringForce();
            bs.copy(this);
            return bs;
        }
        public override bool IsKindOf(string str)
        {
            return str == "CenteringForce" || base.IsKindOf(str);
        }


    }
}
