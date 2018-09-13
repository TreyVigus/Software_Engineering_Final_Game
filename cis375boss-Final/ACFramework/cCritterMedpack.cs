using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ACFramework
{
    class cCritterMedpack : cCritterTreasure
    {
        public cCritterMedpack(cGame pownergame) :
            base(pownergame)
        {

        }

        public cCritterMedpack(cGame pownergame, cVector3 position) :
            base(pownergame, position)
        {
            
        }

        public override bool collide(cCritter pcritter)
        {
            cCritter3DPlayer player = (cCritter3DPlayer)pcritter;
            if (contains(player)) //disk of pcritter is wholly inside my disk 
            {
                Framework.snd.play(Sound.Clap);
                player.Health = cCritter3DPlayer.MAX_HEALTH;
                // one time use for the med pack
                delete_me();
                return true;
            }
            else
                return false;
        }
        public override bool IsKindOf(string str)
        {
            return str == "cCritterMedpack" || base.IsKindOf(str);
        }

        public override string RuntimeClass
        {
            get
            {
                return "cCritterMedpack";
            }
        }
    }
}
