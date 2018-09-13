using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ACFramework
{
    class DragonBullet : cCritterBulletSilverMissile
    {

        private float radius;

        public DragonBullet(float r) :base()
        {
            _fixedlifetime = 6.0f;
            radius = r;
            _value = 0;
        }

        public override cCritterBullet Create()
        // has to be a Create function for every type of bullet -- JC
        {
            return new DragonBullet(radius);
        }

        public override void initialize(cCritterArmed pshooter)
        {
            base.initialize(pshooter);
            _hitstrength = 1;
            Sprite = new cSpriteSphere();
            //Maybe add some kind of bitmap to bullet
            Sprite.FillColor = Color.Red;
            setRadius(radius);
        }


        public override bool IsKindOf(string str)
        {
            return str == "DragonBullet" || base.IsKindOf(str);
        }
        public override string RuntimeClass
        {
            get
            {
                return "DragonBullet";
            }
        }
    }
}