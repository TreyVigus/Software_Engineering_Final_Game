// For AC Framework 1.2,   ZEROVECTOR and other vectors were removed,
// default parameters were added


using System;


namespace ACFramework
{

    /* We think of the cCritterViewer as looking along its _direction axis. */

    class cCritterViewer : cCritter
    {
        //setup camera facing right
        public static cVector3 customOffset = new cVector3(1.75f, -5.0f, 0.0f);

        public static readonly float STARTFIELDOFVIEWANGLE = (float)Math.PI / 6.0f;
        public static readonly float MINFIELDOFVIEWANGLE = (float)Math.PI / 36.0f;
        public static readonly float MAXFIELDOFVIEWANGLE = (float)Math.PI / 2.0f;
        public static readonly float MOVEBOXTOGAMEBORDERRATIO = 20.0f; /* We keep the critter in a _movebox
			that is this quantity times the pgame()->border() in size. */
        public static readonly float MINZNEAR = 0.1f; /* Smallest value of the _znear clip plane value.  Must be bigger than
			0.0.  Typically use 0.1. */
        public static readonly float DEFAULTZOOMFACTOR = 1.1f; /* Standard factor, about 1.1, to use for
			zooming in or out. */
        public static readonly float ORTHOZOFFSET = 10.0f;
        public static readonly float FOVEAPROPORTION = 0.85f;
        public static readonly float TURNPROPORTION = 0.005f;
        public static readonly float MOVEPROPORTION = 0.1f;
        public static readonly float STARTYOFFSETPROPORTION = 0.25f;
        public static readonly float PROPORTIONOFWORLDTOSHOW = 0.75f;
        public static readonly float MINPROPORTIONOFWORLDTOSHOW = 0.3f;
        public static readonly float MAXPROPORTIONOFWORLDTOSHOW = 3.0f;
        protected ACView _pownerview;
        protected bool _perspective; /* FALSE means use Ortho projection, 
			TRUE means use Perspective. We set this in the constructor according
			to the type of _pownerview->pgraphics(), using Ortho for graphicsMFC
			and Perspective for graphicsOpenGL.  If you like, you can use Ortho
			with graphicsOpenGL, but to make life simpler we don't want to bother
			implementing Perspective for the graphicsMFC. */
        protected bool _trackplayer; /* Whether to always keep the player in view */
        protected cVector3 _lastgoodplayeroffset; /* When tracking the player, remember the last offset from
			the player's postion from which you could comfortably see the player. */
        protected float _proportionofworldtoshow; /* positive number expressing how big the visible world
			shown is relative to the whole game. Default is 1.  0.5 would mean a zoomed-in view,
			while 2.0 would mean a view in which the game is small in the window.  
			Is used in the Ortho mode to set realwindow, can be used in persepctive to
			set _fieldofviewangle and sometimes position. */
        protected float _aspect; // Ratio of desired x width to y height for current projection.
        protected float _fieldofviewangle; /* onsistent with the rest of the framework,
			we use radian measure for the angles, but keep in mind that OpenGL uses
			degree measure. */
        protected float _foveaproportion; /* Number between 0.0 and 1.0 to specify how much of the 
			world you can see.  We pass _foveaproportion as the second argument to isVisible
			which wasy say something is visible if it  appears on the inner foveaproportion
			central box of the viewer's image screen, which is what you see in the view window. */
        protected float _znear, _zfar;
        //Accessors 

        protected cGraphics pgraphics()
        {
            return pownerview().pgraphics();
        }

        //get _pownerview->pgraphics(), and check that this isn't NULL.

        protected ACView pownerview()
        {
            return _pownerview;
        }

        //get _pownerview, and check that it isn't NULL.

        public cCritterViewer() { } /* Don't bother doing anything as this is only
			used in copy and serialize. */

        public cCritterViewer(ACView pview)
        {
            _pownerview = pview;
            _proportionofworldtoshow = PROPORTIONOFWORLDTOSHOW;
            _aspect = (4.0f / 3.0f); //Start with standard screen aspect.
            _fieldofviewangle = STARTFIELDOFVIEWANGLE;
            _trackplayer = false; /* Depending on the game, you might want to start with either FALSE 
		    or TRUE for _trackplayer. In general the place where your game can make adjustments 
		    to the critterviewer is in the cGame.initializeView method. */
            _perspective = false; //Will be reset by setViewer, 
            _foveaproportion = FOVEAPROPORTION;
            _movebox = new cRealBox3(Game.Border.Center,
                MOVEBOXTOGAMEBORDERRATIO * Game.Border.MaxSize); /* Put the
			viewer in a cube (or square in the 2D case) whose edge is a multiple 
			of the world's largest dimension. */
            _dragbox = new cRealBox3();
            _dragbox.copy(_movebox);
            _wrapflag = cCritter.CLAMP; /* I had WRAP, which was a big mistake,
			as sometimes I'd back way up above the world and then suddenly be
			under it. */
            //We will set _znear and _zfar in the cCritterViewer.setZClipPlanes call.
            setViewpoint(); /* Default viewpoint is up the z axis looking down at the origin.
			This call also initializes some of the remaining vairables. */
            /* Don't bother to set a listener, as the various cGame.initializeView will do
        that indirectly via a setGraphicsClass call, and then possibly with a 
        direct call to change the listener again. */
            Density = 0.0f; //so you don't move things you bump into.
            AbsorberFlag = true; //So you don't bounce when you bump things.
            Sprite.Radius = 0.1f; /* Although this guy is invisible, it seems to be
			good to have a radius of 1.0 for use in the COLLIDEVIEWER code. */
            _lastgoodplayeroffset = new cVector3();
        }

        //This is the only constructor we use.

        public override void copy(cCritter pcritter)
        {
            base.copy(pcritter);
        }

        public override cCritter copy()
        {
            cCritterViewer c = new cCritterViewer();
            c.copy(this);
            return c;
        }

        public override bool IsKindOf(string str)
        {
            return str == "cCritterViewer" || base.IsKindOf(str);
        }

        //Accessors 

        public float toFarZ() { return Math.Abs(_zfar - _position.Z); } /* Use this in pixelToPlayerYonWallVector 
			calls to set the cursor pos, made inside CpopView OnMouseMouve and SetCursor,
			when viewer uses a cListenerViewerRide. */
        //Mutators 

        public void setDefaultView()
        {
            Game.Viewpoint = this;
        }

        /* Center above origin, or offset a bit in _perspective case,
            looking down. */
        public void setViewpoint()
        {
            setViewpoint(new cVector3(0.0f, 0.0f, 1.0f), new cVector3(0.0f, 0.0f, 0.0f), true);
        }

        public void setViewpoint(cVector3 toViewer, cVector3 lookatPoint,
            bool trytoseewholeworld = true)
        {
            //First do some default setup stuff 
            cVector3 toviewer = new cVector3();
            toviewer.copy(toViewer);
            cVector3 lookatpoint = new cVector3();
            lookatpoint.copy(lookatPoint);
            _fieldofviewangle = cCritterViewer.STARTFIELDOFVIEWANGLE;
            Speed = 0.0f;
            _attitude = new cMatrix3(new cVector3(0.0f, 0.0f, -1.0f),
                new cVector3(-1.0f, 0.0f, 0.0f), new cVector3(0.0f, 1.0f, 0.0f),
                new cVector3(0.0f, 0.0f, 0.0f));
            /* To get a reasonable default orientation, we arrange the viewer axes so that:  
        viewer x axis = world -z axis, viewer y axis = world -x axis, viewer z axis = world y axis.
        We pick this orientation so that if the viewer moves "forward" (along its tangent vector)
        it moves towards the world.  (We correct the mismatch between the coordinate systems in the 
        cCritterViewer.loadViewMatrix method, which has a long comment about this.)
         Note that we will adjust _position (fourth column) later in this  call
         with a moveTo, also we may rotate the _attitude a bit. */
            if (!_perspective) //Ortho view, simply move up.
            {
                _proportionofworldtoshow = 1.0f; //Show all of a flat world.
                moveTo(lookatpoint.add((new cVector3(0.0f, 0.0f, 1.0f)).mult(cCritterViewer.ORTHOZOFFSET))); // Get above the world 
                _maxspeed = _maxspeedstandard = 0.5f * cCritterViewer.ORTHOZOFFSET; //Mimic perspective case.
            }
            else //_perspective 
            {
                if (toviewer.IsPracticallyZero) //Not usable, so pick a real direction.
                    toviewer.copy(new cVector3(0.0f, 0.0f, 1.0f)); //Default is straight up.
                if (trytoseewholeworld) /* Treat toviewer as a direction, and back off in that direction
				enough to see the whole world */
                {
                    toviewer.normalize(); //Make it a unit vector.
                    _proportionofworldtoshow = cCritterViewer.PROPORTIONOFWORLDTOSHOW;
                    //Trying to show all of a world when flying around it, often leaves too big a space around it.
                    float furthestcornerdistance = Game.Border.maxDistanceToCorner(lookatpoint);
                    float tanangle = (float)Math.Tan(_fieldofviewangle / 2.0f); /* We work with half the fov in this calculation, 
					the tanangle will be the ratio of visible distance to distance above the world,
					that is, tanangle = dr/dz, where
					Our dr is _proportionofworldtoshow * furthestcornerdistance, and
					our dz is the unknown seeallz height we need to back off to. 
					Swap tangangle and dz to get the next formula. */
                    float seeallz = _proportionofworldtoshow * furthestcornerdistance / tanangle;
                    moveTo(lookatpoint.add(toviewer.mult(seeallz)));
                }
                else /*Not trytoseewholeworld.  In this case we don't normalize toviewer, instead	
				we treat it as a displacment from the lookatpoint. */
                    moveTo(lookatpoint.add(toviewer));
                lookAt(lookatpoint);
                _maxspeed = _maxspeedstandard = 0.5f * (Position.sub(lookatpoint)).Magnitude;
                /* Define the speed like this so it typically takes two seconds (1/0.5)
            to fly in to lookatpoint. */
                _lastgoodplayeroffset = Position.sub(Game.Player.Position);
            }
        }


        public void setAspect(float aspect) { _aspect = aspect; } /* Idea is to make _aspect always match the 
			xpixelcount to ypixelcount ratio of the pixel size of the view window. This prevents
			distortion. This gets called in CpopView::OnSize. */

        public bool zoom(float zoomfactor)
        { /* Sets the _proportionofworldtoshow parameter, and, if _perspective,
			adjusts the _fieldofviewangle and possibly the position if the
			proportionofworldtoshow would make _fieldofviewangle exceed WIDESTANGLE. */
            _proportionofworldtoshow /= zoomfactor; /* We think of a positive zoom
			factor as making the image bigger.  To do this, we show a smaller
			part of the world. */
            if (_proportionofworldtoshow < MINPROPORTIONOFWORLDTOSHOW)
                _proportionofworldtoshow = MINPROPORTIONOFWORLDTOSHOW;
            else if (_proportionofworldtoshow > MAXPROPORTIONOFWORLDTOSHOW)
                _proportionofworldtoshow = MAXPROPORTIONOFWORLDTOSHOW;
            //REcent defaults for these zoom limiters were  0.2 and 5.0 
            //Need to do some more work here.
            if (_perspective)
            {
                _fieldofviewangle /= zoomfactor; /* A 1.1 zoomfactor, say, is intended to make the
				image bigger. To make the image look bigger in a perspective view, you make
				the viewangle narrower. */
                if (_fieldofviewangle < MINFIELDOFVIEWANGLE)
                    _fieldofviewangle = MINFIELDOFVIEWANGLE;
                else if (_fieldofviewangle > MAXFIELDOFVIEWANGLE)
                    _fieldofviewangle = MAXFIELDOFVIEWANGLE;
            }
            return false;
        }

        /* Sets the 
            _proportionofworldtoshow parameter, and, if _perspective,
            adjusts the _fieldofviewangle and possibly the position if the _proportionofworldtoshow
            would make _fieldofviewangle exceed WIDESTANGLE. Return tells if you had
            to move to achieve your effect. */


        //Special methods 

        public void loadViewMatrix()
        {
            /* What we do here is a bit tricky.  It involves matching two "trihedrons" (where a trihedron is a set
        of three mutually perpendicular vectors used as the basis of a coordinate system). 
        (a) On the one hand, the standard OpenGL-style graphics pipeline
        sets up its projection matrix with the expectation that the viewer is situated so that the points
        of interest are on the negative z-axis of the viewer's coordinates.  The viewers x and y axes are
        thought of as situated with x pointing right and y pointing up. 
        (b) We plan to sometimes let our
        viewer either "fly around" or "ride the back of the player."  In this case, we expect to have the
        viewer's attitudeTangent pointing towards the thing it is looking at, with its attitudeNormal in the plane 
        it's turning in, and its attitudeBinormal pointing up.  Note that the attitudeNormal will seem to 
        point to the left, so that the attiudeTangent * attitudeNormal = attitudeBinormal points up.
        In the constructor, we set up a standard view by these three replacements:  x = -z, y = -x, z = y. 
        (c) In order to match the trihedron of (b) to the expectation of (a), we want to essentially relabel
        the axes of (b), so that the attitudeTangent is now the -z axis we look towards, the attitudeBinormal
        is the y axis of "up", and the attitudeNormal is the positinve x-axis. So that's how we get the
        formula in the line below.  Essentially we undo what the constructor does, by setting
        z = -x, x = -y, y = z.
        (note to b) By the way, if setAttitudeToMotoinLock(TRUE) has been called,
        as we will do when riding a critter, then the attitude vectors will match, respetively, 
        the critterviwer's _tangent, _normal and _binormal. */
            cMatrix3 viewmatrix = new cMatrix3(AttitudeNormal.neg(), AttitudeBinormal,
                AttitudeTangent.neg(), Position);
            /* The next thing to realize is that transforming a world position into the coordinates of a viewer
        means pre-multiplying the world by the inverse of the viewer's position in world coordinates.  Here's
        why.  If the viewer attitude is matrix V in world coordinates, and an object attitude is matrix W in 
        world coordinates, then our task is to find the matrix W' of the object in viewer coordiantes.  Well,
        if the matrix T transforms attitude V into the standard origin trihedron of the world --- which is the
        identity matrix --- then T should transform W into W'.  But if T * V = I, then T is V.inverse().  So
        W' = V.inverse() * W. */
            pgraphics().loadMatrix(viewmatrix.Inverse);
        }

        /* Puts the inverse of a modified aspect matrix of this
            critter into the modelveiw matrix of the pgraphics of the _pownerview. 
            The "modification" has to do with the fact that we think of a critterviewer as looking
            along its plus x-axis, but for the view matrix we think of looking along 
            its negatiave z axis. */

        public virtual void loadProjectionMatrix()
        { /* Set the perspective matrix of the pgraphics
			of the _pownerview so that something like _proportionofworldtoshow much of
			world shows (0.1 would be a tenth of the world, 1.0 would be all of it,
			2.0 would mean show space around the world so it takes up half the
			view).  Also use _znear and _zfar so that all of the visible world can
			fit	in between these bounds.  We regularly compute _znear and _zfar in 
			the setZClipPlanes method, called by our cCritterViewer.update call. */
            pgraphics().MatrixModeProperty = cGraphics.PROJECTION;
            pgraphics().loadIdentity();
            if (!_perspective)
            //We don't have perspective implemented for cGraphicsMFC yet.
            { //use Ortho view. ortho call takes (left, right, bottom, top, nearzclip, farzclip).
                cRealBox2 orthoviewrect = OrthoViewRect;
                /* The call to ortho() becomes a _realpixelconverter.setRealWindow call 
                in cGraphicsMFC, replacing our old call to
                _pgraphics->setRealBox(border()); */
                pgraphics().ortho(orthoviewrect.Lox, orthoviewrect.Hix,
                    orthoviewrect.Loy, orthoviewrect.Hiy, _znear, _zfar);
            }
            else // _perspective is TRUE. perspective call takes (fieldofview, xtoyratio, nearzclip, farzclip).
            {
                pgraphics().perspective(FieldOfViewDegrees, _aspect, _znear, _zfar);
            }
            pgraphics().MatrixModeProperty = cGraphics.MODELVIEW;
        }

        /* Set the perspective matrix of the pgraphics
            of the _pownerview so that something like _proportionofworldtoshow much of the world
            shows (0.1 would be a tenth of the world, 1.0 would be all of it,
            2.0 would mean show space aroudn the world so it takes up half the
            view.  Not const as stereo viewer flips position temporarily. */

        /*This tries to set _znear and _zfar
            so that each of the eight corners of the worldbox is between
            the planes cutting the attitudeTangent() direction at distances of _znear and _zfar from the
            position().  We don't take into account the corners that are behind the viewer.  If
            any point is behind or almost behind the viewer pos relative to the
            viewedirection we set _znear to cCritterViewer::MINZNEAR, which is typically 0.1. */

        public bool isVisible(cVector3 testpos)
        {
            /* _foveaproportion lies between 0.0 and 1.0. We say something is visible if it 
            appears on the inner foveaproportion central box of the viewer's image screen,
            which is what you see in the view window. */
            if (!_perspective)
            {
                cRealBox2 fovea;
                fovea = OrthoViewRect;
                fovea = fovea.innerBox((1.0f - _foveaproportion) * fovea.MinSize);
                return (fovea.inside(new cVector2(testpos).sub(new cVector2(_position))));
            }
            else
            {
                cVector3 totestpos = (testpos.sub(Position)).normalize();
                return Math.Abs(totestpos.angleBetween(AttitudeTangent)) <
                    _foveaproportion * 0.5f * _fieldofviewangle;
            }
        }


        /* Uses _foveaproportion which lies between 0.0 and 1.0. We say something is visible if it 
            appears on the inner _foveaproportion central box of the viewer's image screen,
            which is what you see in the view window. */
        //overloads 

        //Use _pownerview to get the CpopDoc and get the cGame from that.

        public override void update(ACView pactiveview, float dt)
        {
            base.update(pactiveview, dt);
            ZClipPlanes = Game.Border.outerBox(cSprite.MAXPRISMDZ);
            if (_trackplayer &&
                !((Listener != null) && Listener.RuntimeClass == "cListenerViewerRide"))
            /* The meaning of the visibleplayer() condition is that it doesn't make sense
        to track the player if it's not an onscreen player. The reason for the
        listener condition is that you don't want to stare at the player when
        riding it. */
            /*  I should  explain that the goal here is to not bother turning when the player 
        is  moving around in the middle of the veiw area, and only to turn when he's near
        the edge, but to have the turning when he's near the edge be smoooth.
            The use of the 0.85 foveaproportion parameter means that you react before the player
        gets right up to the edge.  The reactproportion factor in lookAtProportional and
        moveToProportional is delicate and should probably be adjusted according to the
        current player speed relative to the visible window.  The issue is that (a) if I make
        reactproportion too small, like 0.01, then the viewer doesn't turn (or move) fast
        enough to catch up with the player and keep it in view, but (b) if I make reactpropotion
        too big, like 0.5, then the turning or moving is such an abrupt jump that the visual
        effect is jerky.  The goal is to do turns that are just big enough to not look jerky,
        but to have the turns be big enough so you aren't turning more often than you really
        have to.  Another downside of a toosmall reactproportion, by the way, is that it can be
        computationally expensive to react. 
            The way we finally solved this is to do a while loop to turn just
        far enough, moving just a little at a time so as to not overshoot. */
            {
                if (isVisible(Game.Player.Position)) // Uses _foveaproportion 
                    _lastgoodplayeroffset = Position.sub(Game.Player.Position);
                /*I'm not sure about constantly changing _lastgoodplayeroffset.  On the
            one hand, the offset I set in setViewpoint was a standard good one, so why
            not keep it.  On the other, if I want to move my viewpoint around then I
            do want to be able to get a new value here. It seems ok for now.*/
                else //not visible, so do somehting about it.  
                {
                    int loopcount = 0; /* Never have a while loop without a loopcount
					to make sure you don't spin inside the while forever under some
					unexpected situation like at startup. */
                    cVector3 lookat = Game.Player.Position;
                    cVector3 viewerpos = lookat.add(_lastgoodplayeroffset);
                    if (Game.worldShape() == cGame.SHAPE_XSCROLLER)
                    {
                        lookat = new cVector3(Game.Player.Position.X,
                            Game.Border.Midy, Game.Player.Position.Z);
                        viewerpos = new cVector3(lookat.X, Position.Y, Position.Z);
                    }
                    if (Game.worldShape() == cGame.SHAPE_YSCROLLER)
                    {
                        lookat = new cVector3(Game.Border.Midx,
                            Game.Player.Position.Y, Game.Player.Position.Z);
                        viewerpos = new cVector3(Position.X, lookat.Y, Position.Z);
                    }
                    if (_perspective)
                        while (!isVisible(lookat) && loopcount < 100) // Uses _foveaproportion 
                        {
                            moveToProportional(viewerpos, cCritterViewer.TURNPROPORTION);
                            loopcount++;
                        }
                    else //ortho case 
                        while (!isVisible(lookat) && loopcount < 100) // Uses _foveaproportion 
                        {
                            moveToProportional(lookat.add(Game.Player.Binormal.mult(10.0f)),
                                cCritterViewer.TURNPROPORTION);
                            loopcount++;
                        }
                }
            }
            //Possibly ride the player.  
            if (Listener.IsKindOf("cListenerViewerRide"))
            {
                cCritter pplayer = Game.Player;
                if (customOffset == null)
                {
                    cVector3 offset = ((cListenerViewerRide)Listener).Offset;
                    //cVector3 offset = ((cListenerViewerRide) Listener).Offset;
                    moveTo(pplayer.Position.add(
                        pplayer.AttitudeTangent.mult(offset.X).add(
                        pplayer.AttitudeNormal.mult(offset.Y).add(
                        pplayer.AttitudeBinormal.mult(offset.Z)))));
                    cRealBox3 skeleton = pplayer.MoveBox;
                    if (skeleton.ZSize < 0.5f)
                        skeleton.setZRange(0.0f, offset.Z);
                    if (skeleton.YSize < 0.5f)
                        skeleton.setYRange(0.0f, offset.Z);
                    skeleton.clamp(_position);
                    for (int i = 0; i < Game.Biota.count(); i++)
                    {
                        cCritter pother = Game.Biota.GetAt(i);
                        if (pother.IsKindOf("cCritterWall"))
                            pother.collide(this);
                    }
                    /* colliding with the wall may have twisted the viwer's orientation,
                so align it once again. */
                    Attitude = pplayer.Attitude; /* Before we call lookAt, 
				make sure your attitude matches the player.  For one thing,
				you may have gotten twisted around in the COLLIDEVIEWER code. */
                    lookAt(pplayer.Position.add(
                        pplayer.AttitudeTangent.mult(cListenerViewerRide.PLAYERLOOKAHEAD * pplayer.Radius))
                        );
                    /* This has the effect that as offset gets large you change your
                looking direction see right in front of the player. The multiplier
                cCritterViewer.PLAYERLOOKAHEAD is tweaked to work well
                with the default cCritterViewer.OFFSET. */
                } else
                {
                    //cVector3 offset = ((cListenerViewerRide)Listener).Offset;
                    //cVector3 offset = ((cListenerViewerRide) Listener).Offset;
                    moveTo(pplayer.Position.add(
                        pplayer.AttitudeTangent.mult(customOffset.X).add(
                        pplayer.AttitudeNormal.mult(customOffset.Y).add(
                        pplayer.AttitudeBinormal.mult(customOffset.Z)))));
                    cRealBox3 skeleton = pplayer.MoveBox;
                    if (skeleton.ZSize < 0.5f)
                        skeleton.setZRange(0.0f, customOffset.Z);
                    if (skeleton.YSize < 0.5f)
                        skeleton.setYRange(0.0f, customOffset.Z);
                    skeleton.clamp(_position);
                    for (int i = 0; i < Game.Biota.count(); i++)
                    {
                        cCritter pother = Game.Biota.GetAt(i);
                        if (pother.IsKindOf("cCritterWall"))
                            pother.collide(this);
                    }
                    /* colliding with the wall may have twisted the viwer's orientation,
                so align it once again. */
                    Attitude = pplayer.Attitude; /* Before we call lookAt, 
				make sure your attitude matches the player.  For one thing,
				you may have gotten twisted around in the COLLIDEVIEWER code. */
                    lookAt(pplayer.Position.add(
                        pplayer.AttitudeTangent.mult(cListenerViewerRide.PLAYERLOOKAHEAD * pplayer.Radius))
                        );
                    /* This has the effect that as offset gets large you change your
                looking direction see right in front of the player. The multiplier
                cCritterViewer.PLAYERLOOKAHEAD is tweaked to work well
                with the default cCritterViewer.OFFSET. */
                }
            }
        }


        public override string RuntimeClass
        {
            get
            {
                return "cCritterViewer";
            }
        }

        public virtual float Aspect
        {
            get
            { return _aspect; }
            set
            { _aspect = value; }
        }

        public virtual float FieldOfViewDegrees
        {
            get
            {
                return (180.0f * +_fieldofviewangle) / (float)Math.PI;
            }
            set
            {
                _fieldofviewangle = value * (float)Math.PI / 180.0f;
            }
        }

        public virtual float FieldOfViewRadians
        {
            get
                { return _fieldofviewangle; }
            set
                { _fieldofviewangle = value; }
        }

        public virtual cRealBox2 OrthoViewRect
        {
            get
            {
                float dx = _proportionofworldtoshow * Game.Border.XSize;
                float dy = _proportionofworldtoshow * Game.Border.YSize;
                float tempaspect = dx / dy;
                if (tempaspect > _aspect) //dy is too small, make it bigger.
                    dy = dx / _aspect;
                if (tempaspect < _aspect) //dx is too small, make it bigger.
                    dx = _aspect * dy;
                return new cRealBox2(dx, dy);
                /* This constructor makes a box centred at the origin.
                We don't center at positon, because once we do
                the loadViewMatrix translation, the origin is effectively at the critter
                location. Earlier I mistakenly had return cRealBox2(_position, dx, dy), 
                and my ortho images were always off by a factor of two. */
            }
        }

        public virtual bool TrackPlayer
        {
            get
                { return _trackplayer; }
            set
                { _trackplayer = value; }
        }

        public virtual float ToFarZ
        {
            get
            { return Math.Abs(_zfar - _position.Z); }
        }

        public virtual cVector3 Viewpoint
        {
            set
            {
                setViewpoint(value, new cVector3(0.0f, 0.0f, 0.0f), true);
            }
        }

        public virtual bool Perspective
        {
            set
            {
                _perspective = value;
            }
        }

        public virtual cRealBox3 ZClipPlanes
        {
            set
            {   /*This tries to set _znear and _zfar so that each of the eight corners of the value is between
		        the planes cutting the attitudeTangent() direction at distances of _znear and _zfar from the
		        position().  We don't take into account the corners that are behind the viewer.  If
		        any point is behind or almost behind the viewer pos relative to the
		        viewedirection we set _znear to cCritterViewer.MINZNEAR, which is typically 0.1. */
                cVector3 viewdirection = AttitudeTangent;
                float neardistance = 1000000000.0f;
                float fardistance = 0.0f;
                float testdistance;
                for (int i = 0; i < 8; i++) // We are assuming we have cast the value to cRealBox3 if necessary.
                {
                    testdistance = viewdirection.mod(value.corner(i).sub(Position));
                    /* Recall that cRealBox3.corner
                    steps through the eight corners of the box.  This gives the distance
                    along the viewdirection.  If testdistance is negative, then	the point is behind the
                    critterviewer relative to the direction its looking in.  
                    Note that testdistance may be negative. */
                    if (testdistance < neardistance)
                        neardistance = testdistance;
                    if (testdistance > fardistance)
                        fardistance = testdistance;
                }
                _znear = 0.1f; //__max(neardistance, cCritterViewer.MINZNEAR);  
                //Has to be positive and bigger than 0.0.
                float modznear = _znear + 2 * MINZNEAR;
                _zfar = (fardistance > modznear) ? fardistance : modznear;
                //Has to be bigger than _znear.
            }
        }

        public override cGame Game
        {
            get
            {
                return pownerview().pgame();
            }
        }

        public virtual float FoveaProportion
        {
            set
            {
                if (value < 0.0f)
                    value = 0.0f;
                else if (value < 1.0f)
                    value = 1.0f;
                _foveaproportion = value;
            }
        }

        public virtual ACView OwnerView
        {
            set
                { _pownerview = value; } //Used in  CpopView::Serialize.
        }
    }
}
//                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   