using System;
using System.Windows.Forms;

namespace ACFramework
{
    class ACDoc
    {
        static protected bool RESTART = false;
        protected cGame _pgame;
        protected float _dt;
        
        public ACDoc()
        {
            _pgame = null;
            setGameClass("cGame3D");  // I'm allowing for the programmer to 
                // make some other game class, possibly, and put it here -- 
                // if the programmer does this, they should derive it from cGame -- JC
        }

        public void stepDoc(float dt, ACView pactiveview) //Called by Framework.  It calls 
                                                        // _pgame.step and ACView.OnDraw
        {
            _dt = dt;
            _pgame.step(dt, pactiveview); /* Move the critters and maybe add or
			delete some. Critters possibly use the pview to sniff out the pixel color at some locations. */
            pactiveview.OnDraw();  // sets up drawings for everything in the game 
            //Possibly wait for user to start or restart game.
            bool didareset = false;
            if (_pgame.NewGame)
            {
                string msg = "*Objective: Defeat all bosses to win." +
                    "\n *Controls:" +
                    "\n - W = jump" +
                    "\n - A = move left" +
                    "\n - S = crouch" +
                    "\n - D = move right" +
                    "\n - L = toggle invincibility" +
                    "\n - [SpaceBar] , click = shoot" +
                    "\n*Hint: Remember to pick up the medpacks!" +
                    "\nPress Enter\nTo Start Game";
                MessageBox.Show(msg);
                //MessageBox.Show("PRESS ENTER\nTo Start Game.");
                RESTART = true;
                _pgame.start();
                didareset = true;
            }
            if (didareset && _pgame.Player.Listener.IsKindOf("cListenerCursor"))
                pactiveview.setCursorPosToCritter(_pgame.Player); // Match cursor to player pos. 
        }



        public float getdt()
        {
            return _dt;
        }

        public cGame pgame()
	    {
		    return _pgame;
	    }

	    public cBiota pbiota()
	    {
		    return pgame().Biota;
	    }

	    public cRealBox3 border()
	    {
		    cRealBox3 bord = new cRealBox3();
		    bord.copy( pgame().Border);
		    return bord;
	    }

	    public void setGameClass(string str)
	    {
		    Framework.setRunspeed(1.0f); /* Restore this in case
			    you changed in a game constructor, like cGameSpacewarSun. */
		    _pgame = null;
		    if ( str == "cGame3D" )
			    _pgame = new cGame3D();
		    _pgame.seedCritters();
		    _pgame.processServiceRequests(); /* To process all the add requests made by
			    critter constructors in the construtor and the seedCritters call. */
	    }

        static public bool Restart
        {
            get { return RESTART; }
            set { RESTART = value; }
        }


    }
}
