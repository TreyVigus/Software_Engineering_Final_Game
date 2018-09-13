// For AC Framework 1.2, default parameters were added -- JC

using System;
using OpenTK.Input;
using System.Drawing;

namespace ACFramework
{
    class cGame
    {
        public static readonly int COUNTSMALL = 4;
        public static readonly int COUNTMEDIUM = 8;
        public static readonly int COUNTLARGE = 25;
        public static readonly int COUNTHUGE = 80;
        public static readonly int COUNTSTART = cGame.COUNTMEDIUM;
        public static readonly int MAXSCORE = 1000;
        public static readonly float WORLDWIDTH = 14.4f;
        public static readonly float WORLDHEIGHT = 9.6f;
        public static readonly float CRITTERMINRADIUS = 0.3f;
        public static readonly float CRITTERMAXRADIUS = 0.8f;
        public static readonly float BULLETRADIUS = 0.05f;
        public static readonly float CRITTERMAXSPEED = 3.0f;
        public static readonly float DEFAULTZDEPTH = 1.0f;
        public static readonly float BACKGROUNDOFFSET = -0.6f;
        public static readonly float FOREGROUNDOFFSET = 0.9f; /* These two are for moving the background
			and foreground out of the plane of the critters for when we do a 
			thee-d view. We use something like -0.1 and 0.9.*/
        //Kinds of sprite that we have on hand.  ST stands for sprite type.
        public static readonly int ST_SPRITETYPENOTUSED = -1; //when you plan to put in your own sprites.
        public static readonly int ST_SIMPLEPOLYGONS = 0; //Simple triangles, squares, pentagons.
        public static readonly int ST_FANCYPOLYGONS = 1; //Diverse regular and star polygons.
        public static readonly int ST_ASTEROIDPOLYGONS = 2; //Polypolygons that have polypolygons at their tips.
        public static readonly int ST_POLYPOLYGONS = 3; //Polypolygons that have polygons at their tips.
        public static readonly int ST_BITMAPS = 4; //cSpriteIcon bitmaps.
        public static readonly int ST_MESHSKIN = 5; //Quake style MD2 Skins on Meshes 
        public static readonly int ST_BUBBLES = 6; //cBubble images.
        public static readonly int ST_SPHERES = 7; //cSpriteSphere 
        public static readonly int ST_ASSORTED = 8; //Mixture from SIMPLEPOLYGONS through BUBBLES.
        public static readonly int ST_TRIPLEPOLYPOLYGONS = 8; //Polypolygons that have polypolygons at their tips.
        public static readonly int MENU_ALL = 0xFFFF;
        public static readonly int MENU_SHAPE = 0x0001;
        public static readonly int MENU_BOUNCEWRAP = 0x0002;
        public static readonly int MENU_2D = 0x0004;
        public static readonly int MENU_3D = 0x0008;
        public static readonly int MENU_AUTOPLAY = 0x0010;
        public static readonly int MENU_COUNT = 0x0020;
        public static readonly int MENU_SHIELD = 0x0040;
        public static readonly int MENU_BACKGROUND = 0x0080;
        public static readonly int MENU_HOPPER = 0x0100;
        public static readonly int MENU_CHANGELISTENER = 0x0200;
        public static readonly int MENU_TRACKPLAYER = 0x0400;
        public static readonly int MENU_RIDEPLAYER = 0x0800;
        public static readonly int SHAPE_RECT = 0;
        public static readonly int SHAPE_XSCROLLER = 1;
        public static readonly int SHAPE_YSCROLLER = 2;
        public static readonly int _baseAccessControl = 0;
        private cCritter _pplayer; /* Pointer to the player's critter.  This  will
	 		never be NULL so that we have continuity from board to board.  We
	 		initialize it in the constructor and delete only in the destructor. 
			Make it private so we remember to use the setPlayer mutator to set it.
			We use the pplayer() accessor to get at it. */
        protected cBiota _pbiota;
        protected cCollider _pcollider;
        protected cCritter _pfocus; // Pointer to the cCritter, if any, being dragged in the CView.
        protected cRealBox3 _border; /* Keep a handy central reference to the size of the world. */
        protected cSpriteTextureBox _pskybox; /* We'll base a cSpriteTextureBox on 
			a cRealBox3 skeleton which will either match _border, or maybe be
			larger. */
        protected cLightingModel _plightingmodel;
        protected int _seedcount; //The current starting number of critters.
        protected bool _gameover; //Is the game finished yet? 
        protected int _maxscore; //Target max score for a game 
        protected int _scorecorrection; //Extra points to give user to make his max score = _maxscore.
        protected int _wrapflag; //cCritter::BOUNCE or cCritter::WRAP 
        protected int _spritetype; //For use in the "design" games that allow random changes to sprites.
        protected int _level; //For later use, like in adjustGameParametes and in seedCritters.  Starts at 1.
        protected bool _newgame; /*  Constructor sets to TRUE. Use _newgame to decide which prompt dialog
			to show if _gameover _newgame for start and !_negame for restart. */
        //Interface variables 
        protected cKeyInfo _pcontroller;
        protected cVector3 _cursorpos; /* We track the "real" location corresponding to the cursor's
	 		pixel position. */
        protected bool _bDragging;
        protected int _menuflags; /* We use the bits of this int as flags to determine if 
			various menu controls will be activated for this game. We define various MENU_***
			constants to & against. */
        //Helper method.
        protected string addOn;

        protected int _index(cCritter pcritter) { return _pbiota._index(pcritter); }

        //Constructors 

        public cGame()
        {
            _seedcount = COUNTSTART;
            _gameover = false;
            _maxscore = MAXSCORE;
            _scorecorrection = 0;
            _wrapflag = cCritter.WRAP;
            _bDragging = false;
            _pfocus = null;
            _pplayer = null;
            _border = new cRealBox3(cGame.WORLDWIDTH, cGame.WORLDHEIGHT); //Default no zsize, computes faster.
            _pskybox = null;
            _cursorpos = new cVector3(0.0f, 0.0f);
            _level = 1;
            _spritetype = cGame.ST_SPRITETYPENOTUSED;
            _menuflags = (cGame.MENU_ALL & ~cGame.MENU_AUTOPLAY & ~cGame.MENU_HOPPER);
            _newgame = true;
            //Fix the static readonlys 
            /* Reset the various static readonlys that may have been set to different values by previously
            running some other game. */
            cCritter.Maxspeed = cGame.CRITTERMAXSPEED;
            cCritter.MinRadius = cGame.CRITTERMINRADIUS;
            cCritter.MaxRadius = cGame.CRITTERMAXRADIUS;
            cCritter.BulletRadius = cGame.BULLETRADIUS;
            //Allocate the pointer variables.
            _pbiota = new cBiota(this);
            _pcollider = new cCollider();
            _pcontroller = Framework.Keydev;
            _plightingmodel = new cLightingModel(true); //Default lighting model enables lights.
            setBorder(cGame.WORLDWIDTH, cGame.WORLDHEIGHT, 0.0f);
            // setBorder initializes _pskybox as well  

            //Set the player, we want to make sure a game always has a player 
            setPlayer(new cCritterPlayer(this)); //Sets _pplayer AND adds it to _pbiota.
        }

        public cGame(cBiota pbiota)
        {
            _pcollider = new cCollider();
            _pcontroller = Framework.Keydev;
            _pbiota = pbiota;
            _pbiota.setGame(this);
            _pfocus = new cCritter();
            _border = new cRealBox3();
            _pskybox = new cSpriteTextureBox();
            _plightingmodel = new cLightingModel();
            _cursorpos = new cVector3();

            //Deleting the _pbiota killed off your _pplayer, so you need to get a new one.
            if (_pbiota.Size > 0) // use the first member of pbiota if there is one.
                _pplayer = _pbiota.GetAt(0);
            else //otherwise make a dummy.  
            {
                _pplayer = new cCritter();
                cCritter pdummyplayer = new cCritter();
                setPlayer(pdummyplayer);
            }
        }

        //Need so that the cBiota default constructor can make an owner game.

        public virtual void removeReferencesTo(cCritter pdeadcritter)
        {
            _pbiota.removeReferencesTo(pdeadcritter);
            //works        if (!pdeadcritter.IsKindOf("cCritterWall"))
            _pcollider.removeReferencesTo(pdeadcritter);
        }

        /* Has the _pbiota, the
            and the _pcollider call removeReferencesTo(pdeadcritter) */

        public virtual int worldShape() { return cGame.SHAPE_RECT; } /* The cCritterViewer::update
			looks at this if _trackplayer is on, so as, if you have a side scroller game, 
			to only track left/right rather than also looking up and down. */
        //Accessors 

        //These three accessors get the values from _pplayer. Virtual in case you want to scale them.

        /// <summary>
        /// Returns a reference to the critter being dragged in the cView, if any (may be null).
        /// </summary>
        /// <returns></returns>
        public cCritter pFocus() { return _pfocus; } 

        /// <summary>
        /// Returns how long a key has been pressed.  May be usedful if events should change depending on how long
        /// a key is pressed.
        /// </summary>
        /// <param name="key">The key being pressed</param>
        /// <returns></returns>
        public float keystateage(int key) { return _pcontroller.keystateage(key); }

        public void add(cCritter pcritter, bool immediateadd = false)
        {
            pcritter.setMoveBox(Border);
            pcritter.DragBox = Border;
            pcritter.WrapFlag = _wrapflag; //Make sure to do this after setting _movebox, as it calls a clamp.
            pcritter.add_me(_pbiota, immediateadd);
        }
        /* cBiota.Add is protected to prevent abuse such as adding a member during
    the middle of an update before everyone's ready.  It gets invoked only by a call to the
    cBiota.processServiceRequests method, acting on all the pcritter who filed an add_me request.
    At first I thought I should force the add to happen right away with the following line,
    but it turns out that's not necessary, and would be a bad idea as it defeats the whole
    purpose of our service requests structure.  If for instance an individual critter calls
    the add method during its update, it really is better not to force the processServiceRequests,
    and to first finish the updates of the other critters. If you want to force
    an immediate add, you set immediateadd to TRUE. */
        //	_pbiota->processServiceRequests(); //DON'T DO THIS!!!!!!!!!

        /* The cGame::add makes the following changes to pcritter: sets
            pcritter's _movebox to match game's _border, puts pcritter in _pbiota, sets pcritter's
            _wrapflag to match the game's _wrapflag. Normally this doesn't happen
            until pownerbiota makes a periodic call to processServiceRequests, but you
            can force it to be immediate with immediateadd. */

        /// <summary>
        /// Sets the player to an initial critter (or changes it to a new critter).
        /// </summary>
        /// <param name="pplayernew">The critter to set the player to</param>
        /// <param name="onscreen">Whether or not the player is to appear on the screen.  If false,
        /// the player will not be added to the pbiota list. Also, if false and the player is currently
        /// in the pbiota list, it will be removed.</param>
        public void setPlayer(cCritter pplayernew, bool onscreen = true) //onscreen is TRUE by default 
        {
            int oldplayerindex, newplayerindex;
            //Bail if you're mistakengly trying to set a NULL player 
            //Kill off the existing _pplayer if its not the same as pplayernew.
            if (pplayernew != _pplayer)
            {
                //First remove the old player 
                if (_pfocus == _pplayer)
                    _pfocus = null;
                oldplayerindex = _index(_pplayer);
                _pplayer = null;
                if (oldplayerindex != cBiota.NOINDEX)
                    _pbiota.RemoveAt(oldplayerindex);
            }
            /* You are going to directly manipulate the _pbiota array in the rest of this call,
            so before doing this, make sure there aren't any outstanding service requests
            that might, for instance, add some pointer into the pbiota after you've deleted
            it in here. There's also the likelihood that you already have an outstanding 
            add_me request for your pplayernew, particularly if the call that got you here
            has the form setPlayer(new cCritterArmedPlayer(this)), as the inner constructor
            call will have made an add_me request.
        */
            _pbiota.processServiceRequests();
            //Now initialize the _pplayer field.
            _pplayer = pplayernew;
            //Put the pplayer in the 0 slot, and if it happens somehow to already be present 
            //in some other slot, remove it from there.
            newplayerindex = _index(pplayernew);
            if (onscreen) //You want player onscreen, that is, in cBiota, put it in.
            {
                if (newplayerindex == cBiota.NOINDEX) // newplayer wasn't in cBiota yet 
                {
                    add(pplayernew); //Call this for various cGame.add side-effects 
                    _pbiota.processServiceRequests(); /* Force the add to happen to _pbiota now.
					This forces a call to cBiota.Add(pplayernew), which will also update the
					_pcollider */
                    newplayerindex = _index(pplayernew); //Check the index again.
                }
                if (newplayerindex > 0) //newplayer is in cBiota but in wrong slot.  Move it!
                {
                    _pbiota.RemoveAt(newplayerindex);
                    _pbiota.InsertAt(0, _pplayer);
                }
                // Now my player is in the 0 slot.  
            }
            /* As a final step, in case we wanted the player offscreen and it happened already
        to be in cBiota, we remove it.*/
            if (!onscreen && newplayerindex != cBiota.NOINDEX)
            {
                _pbiota.RemoveAt(0);
                _pcollider.removeReferencesTo(_pplayer); /* The pcollider shouldn't want to bump a
				non onscreen player. */
                _pplayer.Owner = null; //Make sure it doesn't think it's in the cBiota.
            }
        }


        /* Can overlaod to adjust linecolors or exclude some
            critters from having their sprites changed. */

        /// <summary>
        /// Sets the border of the world by using lengths of dimensions instead of coordinates.  The low corner will
        /// have coordinates (0,0,0) and the high corner will be (dx, dy, dz).
        /// </summary>
        /// <param name="dx">The length of the x dimension.</param>
        /// <param name="dy">The length of the y dimension.</param>
        /// <param name="dz">The length of the z dimension.</param>
        public void setBorder(float dx, float dy, float dz = 0.0f)
        {
            _border.set(dx, dy, dz);
            fixSkyBox();
        }

        /// <summary>
        /// Sets the border of the world using coordinates.  The border is set using two vectors which represent the corners.  The first vector contains the
        /// lowest coordinate values of x, y, and z, and the second vector contains the highest coordinate values of x, y, and z.
        /// </summary>
        /// <param name="locorner">The lowest coordinate values of x, y, and z.</param>
        /// <param name="hicorner">The highest coordinate values of x, y, and z.</param>
        public void setBorder(cVector3 locorner, cVector3 hicorner)
        {
            _border.set(locorner, hicorner);
            fixSkyBox();
        }

        /// <summary>
        /// Sets (or adjusts) only the z dimension of the border of the world.
        /// </summary>
        /// <param name="loz">The lowest coordinate z value.</param>
        /// <param name="hiz">The highest coordinate z value.</param>
        public void setBorderZRange(float loz, float hiz)
        {
            _border.setZRange(loz, hiz);
            fixSkyBox();
        }

        /// <summary>
        /// Sets a default SkyBox, which is a box aligned with the border of the world.  The SkyBox is needed
        /// so that you can set the walls, ceiling or floor to different colors using setSideSolidColor or to
        /// different textures using setSideTexture.  You use these functions on the SkyBox.
        /// </summary>
        public virtual void fixSkyBox()
        {
            cRealBox3 skeleton = _border;
            if (skeleton.ZSize < 0.5f)
                skeleton.setZRange(cGame.BACKGROUNDOFFSET, 1.0f); /* Move LOZ down to 
				avoid z-fighting with sprites in the xy plane.  MOve HIZ up to make
				walls around the world.*/
            setSkyBox(skeleton.LoCorner, skeleton.HiCorner, -1);
            //Make the walls four shades.
            SkyBox.setSideSolidColor(cRealBox3.LOX, Color.FromArgb(180, 30, 200));
            SkyBox.setSideSolidColor(cRealBox3.HIX, Color.FromArgb(30, 10, 180));
            SkyBox.setSideSolidColor(cRealBox3.LOY, Color.FromArgb(255, 200, 200));
            SkyBox.setSideSolidColor(cRealBox3.HIY, Color.FromArgb(10, 255, 200));
            //Make floor be almost black 
            SkyBox.setSideSolidColor(cRealBox3.LOZ, Color.FromArgb(15, 15, 15));
            //Make the top side transparent 
            SkyBox.setSideInvisible(cRealBox3.HIZ); /* Means make it gray and
			don't draw it. */
            SkyBox.Edged = false; //Turns off edges for the filled faces.
            SkyBox.LineColor = Color.DarkGray;
            SkyBox.LineWidthWeight = cColorStyle.LW_IGNORELINEWIDTHWEIGHT;
            SkyBox.LineWidth = 1;
        }

        /// <summary>
        /// Used to set the SkyBox, a box set around the player and critters, which allows colors and textures to
        /// be added to the walls, ceiling and floor (on the SkyBox object, use setSideSolidColor or setSideTexture,
        /// for example).  The skybox is set using two vectors which represent the corners.  The first vector contains the
        /// lowest coordinate values of x, y, and z, and the second vector contains the highest coordinate values of x, y, and z.  
        /// </summary>
        /// <param name="locorner">Contains the lowest coordinate values of x, y, and z.</param>
        /// <param name="hicorner">Contains the highest coordinate values of x, y, and z.</param>
        /// <param name="resourceID">You can use this to set textures to all sides (individual sides can be changed later).
        /// Use BitmapRest to select the texture to use.  This paramater is set to -1 by default, which means just
        /// use plain rectangles initially for the faces.</param>
        public void setSkyBox(cVector3 locorner, cVector3 hicorner, int resourceID = -1)
        {
            _pskybox = new cSpriteTextureBox(locorner, hicorner, resourceID);
            //	_pskybox->setColorStyle(new cColorStyle()); 
        }

        /// <summary>
        /// Used to set the SkyBox, a box set around the player and critters, which allows colors and textures to
        /// be added to the walls, ceiling and floor (on the SkyBox object, use setSideSolidColor or setSideTexture,
        /// for example).  The SkyBox is set by passing in a cRealBox3 object as the first parameter.
        /// </summary>
        /// <param name="pskeleton">The cRealBox3 object to use as the SkyBox.</param>
        /// <param name="resourceID">You can use this to set textures to all sides (individual sides can be changed later).
        /// Use BitmapRest to select the texture to use.  This paramater is set to -1 by default, which means just
        /// use plain rectangles initially for the faces.</param>
        public void setSkyBox(cRealBox3 pskeleton, int resourceID = -1)
        { setSkyBox(pskeleton.LoCorner, pskeleton.HiCorner, resourceID); }

        /// <summary>
        /// A factory method to return one of the various kinds of sprites randomly forned. 
        /// </summary>
        /// <param name="spritetypeindex">An index for the type of sprite.  To select the type of
        /// sprite, use cGame and select a type that begins with ST_. </param>
        /// <returns></returns>
        public cSprite randomSprite(int spritetypeindex)
        {
            cPolygon newpoly;
            cSpriteBubble newbubble;
            cPolyPolygon newpolypoly;
            cSpriteSphere psphere;

            if (spritetypeindex == cGame.ST_ASSORTED)
                spritetypeindex = (int)Framework.randomOb.random((uint)cGame.ST_ASSORTED);
            //Select a random index less than cGame.ST_ASSORTED 
            /* This next block should be a switch, but the compiler won't let me use the cGame constants
            in a switch. */
            if (spritetypeindex == cGame.ST_SIMPLEPOLYGONS)
            {
                newpoly = new cPolygon(Framework.randomOb.random(3, 5));
                newpoly.randomize( //cSprite.MF_RADIUS |  
                    cPolygon.MF_COLOR);
                return newpoly;
            }
            else if (spritetypeindex == cGame.ST_FANCYPOLYGONS)
            {
                newpoly = new cPolygon();
                newpoly.randomize( //cSprite.MF_RADIUS |  
                    cPolygon.MF_COLOR | cPolygon.MF_LINEWIDTH | cPolygon.MF_DOTS | cPolygon.MF_VERTCOUNT);
                return newpoly;
            }
            else if (spritetypeindex == cGame.ST_ASTEROIDPOLYGONS)
            {
                newpoly = new cPolygon();
                newpoly.setRandomAsteroidPolygon(5, 20, Framework.randomOb.randomReal(0.0f, 0.4f));
                newpoly.randomize( //cSprite.MF_RADIUS  
                    cPolygon.MF_COLOR);
                return newpoly;
            }
            else if (spritetypeindex == cGame.ST_BUBBLES)
            {
                newbubble = new cSpriteBubble();
                newbubble.randomize( //cSprite.MF_RADIUS |  
                    cPolygon.MF_COLOR | cPolygon.MF_LINEWIDTH);
                return newbubble;
            }
            else if (spritetypeindex == cGame.ST_SPHERES)
            {
                psphere = new cSpriteSphere();
                psphere.randomize(cPolygon.MF_COLOR);
                Color fill = psphere.FillColor;
                return psphere;
            }
            else if (spritetypeindex == cGame.ST_POLYPOLYGONS)
            {
                newpolypoly = new cPolyPolygon();
                newpolypoly.randomize( //cSprite.MF_RADIUS |  
                    cPolygon.MF_COLOR | cPolygon.MF_LINEWIDTH | cPolygon.MF_DOTS |
                    cPolygon.MF_VERTCOUNT);
                return newpolypoly;
            }
            else if (spritetypeindex == cGame.ST_TRIPLEPOLYPOLYGONS)
            {
                newpolypoly = new cPolyPolygon();
                newpolypoly.randomize(cPolygon.MF_VERTCOUNT);
                newpolypoly.TipShape = new cPolyPolygon();
                newpolypoly.randomize( //cSprite.MF_RADIUS |  
                    cPolygon.MF_COLOR | cPolygon.MF_LINEWIDTH | cPolygon.MF_DOTS | cPolygon.MF_VERTCOUNT);
                return newpolypoly;
            }

            return new cSprite(); //Default in the cGame.ST_SPRITETYPENOTUSED case 
        }

        public virtual void onMouseMove(ACView pview, Point point)
        {
            if (GameOver)
                return; //Don't use mouse or keyborad messages until game starts.
            if (PlayerListenerClass == "cListenerCursor")
                return;

            /* We pass this on, but ordinarily the critters don't do anything with it. */
            //The _cursorpos gets set in CView.OnMouseMove if you're dragging.
            // Drag Hand Case, with (_hcursor == ((CpopApp*).AfxGetApp())->_hCursorDragger) 
            if ((pFocus() != null) && _bDragging)
            {
                cVector3 cursorforcritter = pview.pixelToCritterPlaneVector(point.X, point.Y, pFocus());
                /* It's going to easier, at least to start with, to drag only within the focus critter
            plane. */
                pFocus().dragTo(cursorforcritter, _pcontroller.dt()); /* Feed the current _dt to dragTo
				so as to set the critter's velocity to match the speed of the drag; this way you
				can "throw" a critter by dragging it. */
                _pbiota.processServiceRequests(); /* In case critter reacts.
			If you wait for the timer to trigger CpopDoc.stepDoc
			to call the processServiceRequest, you might possibly manage to
			click or drag the same critter again.  The reason is that you may
			have several OnMouseMove messages in the message queue, and when they
			are processed you will get several calls to onMouseMove. */
            }
        }


        //Special Methods 

        public void processServiceRequests() { _pbiota.processServiceRequests(); }

        public virtual void start()
        {
            _newgame = false; //Next time use restart.
            _gameover = false; //Start running 
        }

        //Just sets _gameoverflag = FALSE.

        public virtual void restart()
        {
            /* This  sets	_pgame-_gameoverflag to FALSE to start
            the game in case its not currently running.  In addition, it calls _pgame->reset()
            to reset the player, call pgame->seedCritters, etc. */
            reset(); //Also sets _pplayer->age() to 0, which is what we get in the cGame.age() call.
            _gameover = false; //Start running.
        }

        //Calls reset() and start() 

        public virtual void reset()
        {
            _level = 1; /* Go back to the first level.  If you've changed the furniture of the
			world at higher levels, you'll need to put that stuff back as well. */
            _pplayer.reset();
            seedCritters();
            _pbiota.processServiceRequests();
            //Do this here to double check that the adds from a possibly overloaded seedCritters take effect.
        }


        public virtual void step(float dt, ACView pactiveview)
        {
            float runspeed = Framework.Runspeed;
            float realdt = dt / runspeed;
            _pcontroller.update(realdt); /* Do controller update first, when you're fresh from the OnIdle
			call that did the checking of messages as this, too, checks user input. The mouse
			click handling code of cController expects you to do the update right after the 
			standard mouse processing of OnIdle, so don't move this call. This call also stores
			the current dt value inside the controller. And don't use the dt = 0.0.*/
            if (_gameover) //Meaning you haven't pressed ENTER for the first time.
                dt = 0.0f; /*  Prevents the lurch at startup when I turn _gameover off, also
				prevents anything from happening in the move or update methods when _gameover. */
            adjustGameParameters();
            _pbiota.feellistener(dt); /* Critters listen to the _pcontroller data,
				possibly using _cursorpos. 	The cCritterArmedPlayer, in particular, will
				look at the _cursorpos if left button is down.  Use the dt to adjust velocity if you have a 
				cListenerCursor, */

            _pbiota.move(dt); /* Critters save current position as _oldposition, use
				their _velocity and _acceleration to compute a new position, possibly wrap or bounce
				this position off the _border and then set the new _position. */
            _pbiota.update(pactiveview, dt); /* Feel any forces acting on the critter, possibly call sniff
				on pview to	check some pixel colors in the world to maybe back off from something 
				or 	kill something. We don't presently use the dt argument, but could use it for
				shrinking critter radius. */
            if (dt > 0.0f) //Prevent constant readjustment when paused 
                // pause is not implemented yet -- JC
                collideStep(); //Critter may abruptly change _position and _velocity in here.
            _pbiota.processServiceRequests();
            _pbiota.animate(dt);
        }


        /* The step function is called in ACDoc.stepDoc(dt), followed by a call to
        ACView.OnDraw */

        public void buildCollider() { _pcollider.build(_pbiota); }

        public virtual void collideStep() /* By default calls _pcollider->iterateCollide(), but you can give it
			a void {} body if you don't want collisions. */
        {
            _pcollider.iterateCollide();
        }

        public virtual void drawCritters(cGraphics pgraphics, int drawflags)
        {
            _pbiota.draw(pgraphics, drawflags);
        }

        /* Call the 
            _pbiota draw to walk the array. */
        //Special Methods you are likely to overload 

        public virtual void drawWorld(cGraphics pgraphics, int drawflags)
        {
            drawBackground(pgraphics, drawflags); //Uses skybox 
        }


        public virtual void drawBackground(cGraphics pgraphics, int drawflags)
        {
            /* I'm going to interpret the DF_FULL_BACKGROUND flag
        as meaning draw the solid skybox, and I'm going to interpret
        the DF_SIMPLIFIED_BACKGROUND as meaning draw a wireframe skybox. 
            I don't make a separate case for !pgraphics()->is3D, as in that
        case my cSpriteTexturebox.draw just calls
        call _childspriteptr[LOZ]->draw(pgraphics, drawflags);*/
            if ((drawflags & ACView.DF_FULL_BACKGROUND) != 0)
            {
                drawflags &= ~ACView.DF_WIREFRAME; //turn off wireframe 
                _pskybox.draw(pgraphics, drawflags);
            }
            else if ((drawflags & ACView.DF_SIMPLIFIED_BACKGROUND) != 0)
            {
                drawflags |= ACView.DF_WIREFRAME; //turn on wireframe 
                if (_pskybox != null)
                    _pskybox.draw(pgraphics, drawflags);
            }
            //else don't draw SIMPLIFIED or FULL background, that is, draw nothing.
        }

        public virtual void seedCritters()
        {
            cCritter newcritter;
            _pbiota.purgeNonPlayerNonWallCritters(); /* Clean out all critters but player and
	 		walls, in case you have walls. */
            for (int i = 0; i < _seedcount; i++)
            {
                newcritter = new cCritter(this); /* The this argument adds the critter to the game,
				and sets its _movebox and _dragbox to the game _border, sets its _wrapflag
				to match the game's _wrapflag. */
                newcritter.randomize(cCritter.MF_POSITION | cCritter.MF_VELOCITY);
                newcritter.Sprite = randomSprite(_spritetype);
                newcritter.addForce(new cForceEvadeBullet()); /* Default force for evading the 
	 			cBullet objects that the player fires. This force is a child of 
	 			cForceClassEvade with default constructor equivalent 
	 			to cForceClassEvade(cForceEvadeBullet.DARTACCELERATION, cForceEvadeBullet.DARTSPEEDUP,
	 			RUNTIME_CLASS(cCritterBullet), FALSE). The FALSE in the fourth arg, means don't
	 			bother evading children of cCritterBullet. */
            }
        }

        /// <summary>
        /// Does nothing, but can be overridden to set the _gameover flag, reseed the world, or 
        /// change the background bitmaps.
        /// </summary>
        public virtual void adjustGameParameters() { } /* This may set the _gameover flag, may
	 		also reseed the world, or use change the background bitmap. */

        /// <summary>
        /// Just provides the score and the health, but can be overridden to provide more info.
        /// </summary>
        /// <returns></returns>
        public virtual string statusMessage()
        {
            string cStrStatusMessage;
            string cStrHealth;
            string cStrScore;

            cStrScore = "Score: " + Score.ToString();
            cStrHealth = "Health: " + Health.ToString();
            cStrStatusMessage = "AC Framework       " + cStrScore + "   " + cStrHealth + "    " + addOn;
            return cStrStatusMessage;
        }

        public virtual int WorldShape
        {
            get
                { return cGame.SHAPE_RECT; }
        }

        /// <summary>
        /// Used to get the Biota object in this game.
        /// </summary>
        public virtual cBiota Biota
        {
            get
                { return _pbiota; }
        }

        public virtual cCollider Collider
        {
            get
                { return _pcollider; }
        }

        public virtual cLightingModel LightingModel
        {
            get
                { return _plightingmodel; }
        }

        /// <summary>
        /// Used to get or set the cursor position.
        /// </summary>
        public virtual cVector3 CursorPos
        {
            get
            {
                cVector3 c = new cVector3();
                c.copy(_cursorpos);
                return c;
            }
            set
                { _cursorpos.copy(value); }
        }

        /// <summary>
        /// Used to get or set the wrap flag for the game.  Use cCritter.BOUNCE for example, to have
        /// critters bounce off of the edges of the world when they hit them.
        /// </summary>
        public virtual int WrapFlag
        {
            get
                { return _wrapflag; }
            set
            {
                _wrapflag = value;
                _pbiota.setWrapflag(_wrapflag);
            }
        }

        /// <summary>
        /// Used to get or set the seed count (the number of seedable critters).  The seedable critters
        /// can be defined in the seedCritters function by overridding it.
        /// </summary>
        public virtual int SeedCount
        {
            get
                { return _seedcount; }
            set
            {
                _seedcount = value;
                seedCritters();
                _pbiota.processServiceRequests();
                //Do this here to double check that the adds from a possibly overloaded seedCritters take effect.
            }
        }

        /// <summary>
        /// Gets the currend score of the player.
        /// </summary>
        public virtual int Score
        {
            get
            {
                return _pplayer.Score;
            }
        }

        /// <summary>
        /// Gets the current health of the player.
        /// </summary>
        public virtual int Health
        {
            get
            {
                return _pplayer.Health;
            }
        }

        /// <summary>
        /// Gets the current "age" of the player, determined by the change in time, or dt, since the 
        /// player was initialized.
        /// </summary>
        public virtual float Age
        {
            get
            {
                return _pplayer.Age;
            }
        }

        /// <summary>
        /// Gets or sets the _gameover flag.
        /// </summary>
        public virtual bool GameOver
        {
            get
                { return _gameover; }
            set
                { _gameover = value; }
        }

        public virtual bool NewGame
        {
            get
                { return _newgame; }
        }

        /// <summary>
        /// Gets or sets the _pfocus (the critter of interest).
        /// </summary>
        public virtual cCritter Focus
        {
            get
                { return _pfocus; }
            set
                { _pfocus = value; }
        }

        /// <summary>
        /// Gets the player object.
        /// </summary>
        public virtual cCritter Player
        {
            get
                { return _pplayer; }
        }

        /// <summary>
        /// Gets the border as a cRealBox3 object.
        /// </summary>
        public virtual cRealBox3 Border
        {
            get
            {
                cRealBox3 b = new cRealBox3();
                b.copy(_border);
                return b;
            }
        }

        /// <summary>
        /// Gets the _pskybox, so the functions on it can be used (such as setting the color of a wall).
        /// </summary>
        public virtual cSpriteTextureBox SkyBox
        {
            get
                { return _pskybox; }
        }

        public virtual cKeyInfo Controller
        {
            get
                { return _pcontroller; }
        }

        /// <summary>
        /// Gets the player's listener (returns the name of the class as a string)
        /// </summary>
        public virtual string PlayerListenerClass
        {
            get
            {
                return (_pplayer.Listener.RuntimeClass);
            }
        }

        public virtual int SpriteType
        {
            get
                { return _spritetype; }
            set
            {
                _spritetype = value;
                seedCritters();
            }
        }

        /// <summary>
        ///  Gets _menuflags.  Useful if you are creating menus in your game.
        /// </summary>
        public virtual int MenuFlags
        {
            get
                { return _menuflags; }
        }

        /// <summary>
        /// Just returns true.  Z is up unless you do something different, and then you need to override this.
        /// </summary>
        public virtual bool UpIsZ
        {
            get
                { return true; }
        }

        public virtual bool NewGeometryFlag
        {
            set
            {
                Biota.NewGeometryFlag = value;
                SkyBox.NewGeometryFlag = value;
            }
        }

        /// <summary>
        /// Sets the floor to the texture of your choice (use BitmapRes.)
        /// </summary>
        public virtual int BackgroundBitmap
        {
            set
            {
                SkyBox.Skeleton.setZRange(-3.0f, SkyBox.Skeleton.Hiz);
                SkyBox.setSideTexture(cRealBox3.LOZ, value, 1);
            }
        }

        /// <summary>
        /// Just tells the score, but you can override it to give more info.
        /// </summary>
        public virtual string GameOverMessage
        {
            get
            {
                return "Your Score Was " + Score.ToString();
            }
        }

        /// <summary>
        /// View sets the cursor type, the background, the graphics type, and 
        /// whether the pviewpointcritter ignores, watches the player or rides it. 
        /// When you switch game types or start a new game, the ACView.OnUpdate gets called 
        /// with the VIEWHINT_STARTGAME) parameter and this generates calls to initializeView and
        /// to initializeViewpoint. 
        /// </summary>
        public virtual ACView View
        {
            set
            {
                value.setUseBackground(ACView.FULL_BACKGROUND); /* The background type can be
			ACView.NO_BACKGROUND, ACView.SIMPLIFIED_BACKGROUND, or 
			ACView.FULL_BACKGROUND, which often means: nothing, lines, or
			planes&bitmaps, depending on how the skybox is defined. */
                value.pviewpointcritter().TrackPlayer = false; //Do not track player.
                cRealBox3 viewbox = new cRealBox3(Border.Center,
                    cCritterViewer.MOVEBOXTOGAMEBORDERRATIO * Border.MaxSize);
                /* Put the viewer in a cube whose edge is a
            multiple of the world's largest dimension. */
                value.pviewpointcritter().setMoveBox(viewbox);
                value.pviewpointcritter().DragBox = viewbox;
                //Some possible overrides: 
                //value->setUseSolidBackground(FALSE); //For no background at all, faster in 3D.
                //value->setCursor(((CpopApp*).AfxGetApp())->_hCursorPlay); 
                //To use the crosshair cursor for shooting with mouse clicks.
                //value->pviewpointcritter()->setTrackplayer(TRUE); 
                /* To scroll after the player critter if it moves off screen.  This can be
            confusing, but is useful if you plan to use a zoomed in view. */
                //value->setGraphicsClass(RUNTIME_CLASS(cGraphicsOpenGL)); //For 3D graphics 
                //value->pviewpointcritter()->setListener(new cListenerViewerRide()); 
                //To ride the player; this only works in 3D.
            }
        }

        /// <summary>
        /// Viewpoint sets the pviewpointcritter's zoom, and its postion and direction.  
        /// Viewpoint gets called (a) when you start a new game, (b) when you switch graphics modes, 
        /// (c) when you alter the player ignoring/riding/watching options. 
        /// </summary>
        public virtual cCritterViewer Viewpoint
        {
            set
            {
                /* The two args to setViewpoint are (directiontoviewer, lookatpoint).
                Note that directiontoviewer points FROM the origin TOWARDS the viewer. */
                value.setViewpoint(new cVector3(0.0f, -1.0f, 2.0f), _border.Center);
                //Direction to viewer is down a bit, and back off less than that.
            }
        }

        /// <summary>
        /// For adding on to the end of the status message (set only).  Assign a string.
        /// </summary>
        public virtual string AddOn
        {
            set
            {
                addOn = value;
            }
        }
        
    }


}

	
//                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                               