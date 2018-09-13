using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace ACFramework
{
    class PulseBullet: cCritterBulletRubber
    {
        //time passed since the bullet was fired.
        private float totalTime = 0;
        private int pulseRate = 0;
        private float minRadius = 0;
        private float maxRadius = 0;
        

        public PulseBullet(int pulseRate = 4, float minRadius = 0.2f, float maxRadius = 0.5f)
        {
            this.pulseRate = pulseRate;
            this.minRadius = minRadius;
            this.maxRadius = maxRadius;

            this._fixedlifetime = 10.0f;
        }

        public override cCritterBullet Create()
        // has to be a Create function for every type of bullet -- JC
        {
            return new PulseBullet();
        }

        public override void initialize(cCritterArmed pshooter)
        {
            base.initialize(pshooter);
            Sprite = new cSpriteSphere();
            Sprite.FillColor = Color.Orange;
            setRadius(minRadius);
        }

        public override void update(ACView pactiveview, float dt)
        {
            base.update(pactiveview, dt);
            totalTime = totalTime + dt;
            float newRadius = (float)(Math.Abs((maxRadius-minRadius)*Math.Sin(pulseRate*totalTime)) + minRadius );
            setRadius(newRadius);
        }

        public override bool collide(cCritter pcritter)
        {
            return base.collide(pcritter);
        }

        public override bool IsKindOf(string str)
        {
            return str == "PulseBullet" || base.IsKindOf(str);
        }

        public override string RuntimeClass
        {
            get
            {
                return "PulseBullet";
            }
        }


    }
}
