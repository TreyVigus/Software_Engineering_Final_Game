using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ACFramework
{
    class DamagingWall: cCritterWall
    {
        public const int DAMAGE = 1;
        //time you have to be touching the wall to take damage.
        public const int DELAY = 30;
        //keeps track of the frames that have passed while touching the wall.
        public int count = 0;
        
        public DamagingWall(cVector3 enda, cVector3 endb, float thickness, float height, cGame pownergame)
            :base(enda,endb,thickness,height,pownergame)
        {

        }
        public override bool collide(cCritter pcritter)
        {
            bool collided = base.collide(pcritter);
            if (collided && pcritter.IsKindOf("cCritter3DPlayer"))
            {
                count++;
                cCritter3DPlayer player = (cCritter3DPlayer)pcritter;
                if (count >= DELAY)
                {
                    player.damage(DAMAGE);
                    count = 0;
                }
            }

            return collided;
        }

        public override bool IsKindOf(string str)
        {
            return str == "DamagingWall" || base.IsKindOf(str);
        }
        //override the draw method to do nothing so that the floor is invisible.
        public override void draw(cGraphics pgraphics, int drawflags = 0)
        {
            
        }


    }
}
