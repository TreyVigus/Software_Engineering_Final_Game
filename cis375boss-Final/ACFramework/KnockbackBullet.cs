using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ACFramework
{
    class KnockbackBullet: cCritterBulletRubber
    {
        //direction the bullet will knock the critter when it collides.
        private cVector3 forceVector = new cVector3(0.0f, 300.0f, 300.0f);

        public KnockbackBullet()
        {

        }
        //optionally override the direction vector.
        public KnockbackBullet(cVector3 forceVector)
        {
            this.forceVector.copy(forceVector);
        }
        public override cCritterBullet Create()
        // has to be a Create function for every type of bullet -- JC
        {
            return new KnockbackBullet(forceVector);
        }
        public override void initialize(cCritterArmed pshooter)
        {
            base.initialize(pshooter);

            _hitstrength = 0;
            Sprite = new cSpriteSphere();
            Sprite.FillColor = Color.Orange;
            setRadius(0.4f);
            this._fixedlifetime = 10.0f;
            this.Speed = 3.0f;
        }

        public override bool collide(cCritter pcritter)
        {
            bool collided = base.collide(pcritter);
            if (collided)
            {
                pcritter.addForce(new KnockbackForce(forceVector)); //clear forcelist before this? 
            }
            return collided;
        }
        public override bool IsKindOf(string str)
        {
            return str == "KnockbackBullet" || base.IsKindOf(str);
        }
        public override string RuntimeClass
        {
            get
            {
                return "PulseBullet";
            }
        }

    }

    //Force that will shoot the critter into the air when added to a critter.
    class KnockbackForce : cForce
    {
        public const int COUNT_LENGTH = 60;
        
        //vector representing the force that will be applied
        private cVector3 forceVector = new cVector3();
        //time to wait until the force is removed (so the critter will fall back to the ground)
        private int count = 0;

        public KnockbackForce(cVector3 forceVector)
        {
            this.forceVector.copy(forceVector);
        }

        public override cVector3 force(cCritter pcritter)
        {
            count++;

            //if the count length hasn't yet been reached, just return the upward force
            if (count != COUNT_LENGTH)
            {
                cVector3 frc = new cVector3();
                frc.copy(forceVector);
                return frc;
            }
            //once the count length is reached, remove the upward force by setting forceVector to the zero vector.
            //TODO: remove the force from the list altogether.
            else
            {
                forceVector = new cVector3(0, 0, 0);
                cVector3 v = new cVector3();
                v.copy(forceVector);
                return v;
            }

        }
        public override void copy(cForce pforce)
        {
            base.copy(pforce);
            if (!pforce.IsKindOf("KnockbackForce"))
                return;
            KnockbackForce pforcechild = (KnockbackForce)pforce;
            //copy fields here
            forceVector.copy(pforcechild.forceVector);
            count = pforcechild.count;
            
        }
        public override cForce copy()
        {
            KnockbackForce bs = new KnockbackForce(new cVector3());
            bs.copy(this);
            return bs;
        }
        public override bool IsKindOf(string str)
        {
            return str == "KnockbackForce" || base.IsKindOf(str);
        }
        

    }



}
