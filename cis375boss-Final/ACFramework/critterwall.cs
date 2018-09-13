// For AC Framework 1.2, default parameters were added

using System;
using System.Drawing;
using System.Windows.Forms;

namespace ACFramework
{
    class cCritterSlideWall : cCritterWall
    {
        private TYPE wallType;
        private UPDATE_IMPLEMENTATION implementation;
        private cVector3 moveAxisAndDirection;
        private bool bounce;    //used to determine if the wall should change direction
        private int frameLimit = 0; //number of frames that will passs until the wall bounces/teleports
        private int counter = 0;    //number of frames since last bounce/teleport
        private cVector3 startPos = new cVector3(); 
        private cVector3 endPos = new cVector3();




        /// <summary>
        /// The Default Axis We Will Move The Wall
        /// </summary>
        private static cVector3 defaultMoveAxis = new cVector3(-0.01f, 0f, 0f); // Default Is Vertical Down

        /// <summary>
        /// Type Of Movving Wall We Wish To Create
        /// </summary>
        public enum TYPE
        {
            /// <summary>
            /// Wall Will Harm The Player If It Colides
            /// </summary>
            HARMFUL,

            /// <summary>
            /// Wall Will Not Harm The Player, But Will Move Them
            /// </summary>
            PLATFORM
        };

        //The way the wall's update method is handled.
        //COUNT_BASED means the wall will bounce/teleport once a certain number of frames have passed.
        //POSITION_BASED means the wall will bounce/teleport once a certain position is reached.
        public enum UPDATE_IMPLEMENTATION
        {
            COUNT_BASED,
            POSITION_BASED
        }
         
        /// <summary>
        /// Constructor with specified direction
        /// </summary>
        /// <param name="enda">start vector</param>
        /// <param name="endb">end vector</param>
        /// <param name="thickness"></param>
        /// <param name="height"></param>
        /// <param name="pownergame"></param>
        /// <param name="wallType">type of wall</param>
        /// <param name="moveAxisAndDirection">direction wall moves per frame</param>
        /// <param name="bounce"></param>
        //count-based constructor (pass in frame limit before bounce/teleport)
        public cCritterSlideWall(cVector3 enda, cVector3 endb, float thickness, float height, cGame pownergame, TYPE wallType, cVector3 moveAxisAndDirection, bool bounce, int frameLimit)
            : base(enda, endb, thickness, height, pownergame)
        {
            this.bounce = bounce;
            this.wallType = wallType;
            this.moveAxisAndDirection = moveAxisAndDirection;
            this.frameLimit = frameLimit;
            implementation = UPDATE_IMPLEMENTATION.COUNT_BASED;

        }

        //Position based constructor (pass in start and end posion)
        public cCritterSlideWall(cVector3 enda, cVector3 endb, float thickness, float height, cGame pownergame, TYPE wallType, cVector3 moveAxisAndDirection, bool bounce, cVector3 startPos, cVector3 endPos)
            : base(enda, endb, thickness, height, pownergame)
        {
            this.bounce = bounce;
            this.wallType = wallType;
            this.moveAxisAndDirection = moveAxisAndDirection;
            this.startPos.copy(startPos);
            this.endPos.copy(endPos);
            implementation = UPDATE_IMPLEMENTATION.POSITION_BASED;

        }
        public cCritterSlideWall(cVector3 enda, cVector3 endb, float thickness, float height, cGame pownergame, TYPE wallType, int frameLimit)
            : base(enda, endb, thickness, height, pownergame)
        {
            this.bounce = false;
            this.wallType = wallType;
            this.moveAxisAndDirection = defaultMoveAxis;
            this.frameLimit = frameLimit;
            implementation = UPDATE_IMPLEMENTATION.COUNT_BASED;

        }

        //how the wall updates varies based on the constructor used.
        //if a frame limit int is passed in, it will be count-based (bounce/teleport once a count is reached)
        //if a start and end position cVector3 are passed in, it will be position-based (bounce/teleport once a position is reached)
        public override void update(ACView pactiveview, float dt)
        {
            base.update(pactiveview, dt);
            
            if(implementation == UPDATE_IMPLEMENTATION.COUNT_BASED)
            {
                countBasedUpdate();
            }
            else if(implementation == UPDATE_IMPLEMENTATION.POSITION_BASED)
            {
                positionBasedUpdate();
            }
        }
        public void countBasedUpdate()
        {
            this.moveTo(this.Position.add(this.moveAxisAndDirection), false);
            counter++;

            if (bounce)
            {
                //bounces once frameLimit reached.
                if (counter == frameLimit)
                {
                    //makes vector go reverse direction
                    moveAxisAndDirection = cVector3.mult(-1, moveAxisAndDirection);
                    counter = 0;
                }
            }
        }
        public void positionBasedUpdate()
        {
            this.moveTo(this.Position.add(this.moveAxisAndDirection), false);
            counter++;

            if (bounce)
            {
                if(Position.Z <= endPos.Z)
                {
                    //swap start and end position
                    cVector3 temp = endPos;
                    endPos = startPos;
                    startPos = temp;

                    //switch direction and reset count
                    moveAxisAndDirection = cVector3.mult(-1, moveAxisAndDirection);
                    counter = 0;
                }
            }
            else
            {
                if (Position.Z <= endPos.Z)
                {
                    moveTo(startPos);
                    counter = 0;
                }
            }
        }

        public override bool collide(cCritter pcritter)
        {
            bool collided = base.collide(pcritter);
            if (wallType == TYPE.HARMFUL && collided && (pcritter.IsKindOf("cCritter3DPlayer")))
            {
                Console.WriteLine("Watch out for that wall!");
                Framework.snd.play(Sound.Crunch);
                if(!pcritter.Shield)
                {
                    pcritter.die(); //player instantly dies unless they have a shield
                }

                return true;
            }

            return false;
        }

        public override bool IsKindOf(string str)
        {
            return str == "cCritterSlideWall" || base.IsKindOf(str);
        }

        public override string RuntimeClass
        {
            get
            {
                return "cCritterSlideWall";
            }
        }

    }
    class cCritterWall : cCritter
    {
        public const float THICKNESS = 0.2f;
        public static readonly float CORNERJIGGLETURN = (float)Math.PI / 4; /* To keep someone from impossibly bouncing up and down on a corner,
			 we jiggle the bounces off corners by a small random amount.*/
        public static readonly float CORNERJIGGLEKICK = 1.15f;
        public static readonly Color WALLFILLCOLOR = Color.LightGray;
        public const float WALLPRISMDZ = 0.75f; //Default z-depth to use for drawing cCritterWall.
        protected Color _defaultfillcolor;
        protected cRealBox3 _pskeleton;

        protected int outcode(cVector3 globalpos)
        {
            return outcodeLocal(globalToLocalPosition(globalpos));
        }


        protected int outcodeLocal(cVector3 localpos)
        { 		/* This tells you which of the 27 possible positions a 
			localpos has relative to the pskeleton box */
            return _pskeleton.outcode(localpos);
        }

        public cCritterWall()
            : base(null)
        {
            _pskeleton = null;
            _defaultfillcolor = WALLFILLCOLOR;
            initialize(new cVector3(-0.5f, 0.0f), new cVector3(0.0f, 0.5f),
                THICKNESS, WALLPRISMDZ, null);
        }

        public cCritterWall(cVector3 enda)
            : base(null)
        {
            _pskeleton = null;
            _defaultfillcolor = WALLFILLCOLOR;
            initialize(enda, new cVector3(0.0f, 0.5f),
                THICKNESS, WALLPRISMDZ, null);
        }

        public cCritterWall(cVector3 enda, cVector3 endb)
            : base(null)
        {
            _pskeleton = null;
            _defaultfillcolor = WALLFILLCOLOR;
            initialize(enda, endb, THICKNESS, WALLPRISMDZ, null);
        }

        public cCritterWall(cVector3 enda, cVector3 endb, float thickness = THICKNESS,
            float height = WALLPRISMDZ, cGame pownergame = null)
            : base(pownergame)
        {
            _pskeleton = null;
            _defaultfillcolor = WALLFILLCOLOR;
            initialize(enda, endb, thickness, height, pownergame);
        }


        public cCritterWall(cVector3 enda, cVector3 endb, float thickness, cGame pownergame)
            : base(pownergame)
        {
            _pskeleton = null;
            _defaultfillcolor = WALLFILLCOLOR;
            initialize(enda, endb, thickness, WALLPRISMDZ, pownergame);
        }


        public void initialize(cVector3 enda, cVector3 endb, float thickness, float height, cGame pownergame)
        {
            _defaultprismdz = height; //Used if cSprite is a cPolygon 
            FixedFlag = true; /* By default a wall is fixed;
			remember this if you want to use one for a paddle. */
            _collidepriority = cCollider.CP_WALL; /* Don't use
			the setCollidePriority mutator, as that
			forces a call to pgame()->buildCollider(); */
            _wrapflag = cCritter.WRAP; /* In case a wall extends
			across the _border, don't bounce it. Note that
			we have overloaded setWrap so you can't turn
			off the WRAP */
            setEndsThicknessHeight(enda, endb,
                thickness, _defaultprismdz);
        }


        public override void copy(cCritter pcritter)
        {
            /*We need to overload the cCritter copy because if I have 
        cCritterWallRobot which is a cCritterWall, and it gets split in two 
        by a replicate call, then I want the new copy to have all the same 
        shooting behavior as the old one.  In general, I should
        overload copy for each of my critter child classes, but for now this 
        one is the most important.  The overload does the regular copy, then 
        looks if the thing being copied is a cCritterWall, and if it is then 
        it copies the additional fields. */
            base.copy(pcritter);
            if (!pcritter.IsKindOf("cCritterWall"))
                return; //You're done if pcritter isn't a cCritterWall*.
            cCritterWall pcritterwall = (cCritterWall)(pcritter);
            /* I know it is a cCritterWall at this point, but I need
        to do a cast, so the compiler will let me
        access cCritterWall methods and fields. */
            cRealBox3 r = new cRealBox3();
            r.copy(pcritterwall.Skeleton);
            Skeleton = r;
            _defaultfillcolor = pcritterwall._defaultfillcolor;
        }

        public override cCritter copy()
        {
            cCritterWall c = new cCritterWall();
            c.copy(this);
            return c;
        }

        /// <summary>
        /// Tests to see if this critter is any kind of class derived from the base class being used.  
        /// If you would like to see if this critter is any kind of bullet, for example,
        /// you would pass in the string for the class name "cCritterBullet".  If will return true if is a
        /// cCritterBullet object or any object of a class derived from cCritterBullet.
        /// </summary>
        /// <param name="str">The class name to test.</param>
        /// <returns>Returns true if this critter is a kind of the class, and false otherwise.</returns>
        public override bool IsKindOf(string str)
        {
            return str == "cCritterWall" || base.IsKindOf(str);
        }

        //Mutators 

        public void setEndsThicknessHeight(cVector3 enda, cVector3 endb,
            float thickness = THICKNESS, float height = WALLPRISMDZ)
        {
            _position = enda.add(endb).mult(0.5f);
            _wrapposition1.copy(_position);
            _wrapposition2.copy(_position);
            _wrapposition3.copy(_position);
            /* This line is important, as otherwise the 
        cCritter.draw will thing this thing was wrapped,
        and it'll get drawn in two places. */
            _tangent = endb.sub(enda);
            float length = _tangent.Magnitude;
            _tangent.normalize();
            _oldtangent.copy(_tangent);
            _normal = _tangent.defaultNormal(); /* We orient so that
			the normal is oriented to the tangent as the "y-axis"
			is to the the "x-axis".*/
            _binormal = _tangent.mult(_normal);
            _attitude = new cMatrix3(_tangent, _normal, _binormal, _position);
            Skeleton = new cRealBox3(length, thickness, height);
            Speed = 0.0f; /* Also sets _velocity to ZEROVECTOR,
			but doesn't wipe out _direction. */
            /*In looking at these settings, think of the wall as aligned horizontally with endb - enda pointing to the right and the normal pointing into the screen*/
            cPolygon ppolygon = new cPolygon(4);
            ppolygon.Edged = true;
            ppolygon.FillColor = Color.Gray;
            ppolygon.LineWidthWeight = cColorStyle.LW_IGNORELINEWIDTHWEIGHT;
            ppolygon.LineWidth = 1;
            //Means draw a one-pixel edge line.
            ppolygon.setVertex(0, new cVector3(0.5f * length, 0.5f * thickness));
            ppolygon.setVertex(1, new cVector3(-0.5f * length, 0.5f * thickness));
            ppolygon.setVertex(2, new cVector3(-0.5f * length, -0.5f * thickness));
            ppolygon.setVertex(3, new cVector3(0.5f * length, -0.5f * thickness));
            ppolygon.fixCenterAndRadius(); /* Use this call after a bunch
			of setVertex if points are just where you want. */
            ppolygon.SpriteAttitude = cMatrix3.translation(new cVector3(0.0f, 0.0f, -height / 2.0f));
            /* This corrects for the fact that we always draw the ppolygon with its
        bottom face in the xy plane and its top in the plane z = height.  We
        shift it down so it's drawn to match the skeleton positon. */
            Sprite = ppolygon; /* Also sets cSprite._prismdz to
			cCritter._defaultprismdz, which we set to 
			CritterWall.WALLPRISMDZ in our cCritterWall 
			constructor. */
        }


        /// <summary>
        /// Can use to set a new height for the wall.
        /// </summary>
        /// <param name="newheight">The new height to make the wall (default is 0, so it disappears).</param>
        public void setHeight(float newheight = 0.0f)
        {
            Skeleton = new cRealBox3(Length, Thickness, newheight);
            Sprite.PrismDz = newheight;
            Sprite.SpriteAttitude = cMatrix3.translation(new cVector3(0.0f, 0.0f, -Height / 2.0f));
            /* This corrects for the fact that we always draw the ppolygon with its
        bottom face in the xy plane and its top in the plane z = height.  We
        shift it down so it's drawn to match the skeleton positon. */
        }

        /// <summary>
        /// This is exactly the same as the base class cCritter.mutate except that the sprite is not mutated.
        /// </summary>
        /// <param name="mutationflags">Indicates the types of mutations to be performed.  To mutate position, 
        /// do a bitwise |= with MF_POSITION.  To mutate velocity, do a bitwise |= with MF_VELOCITY.
        /// To perform a weak mutation, do a bitwise |= with MF_NUDGE. </param>
        /// <param name="mutationstrength">The strength of the mutation.</param>
        public override void mutate(int mutationflags, float mutationstrength) 
        {
            if ((mutationflags & MF_NUDGE) != 0)  //Special kind of weak mutation
            {
                float turnangle = Framework.randomOb.randomSign()
                    * Framework.randomOb.randomReal((float)-Math.PI / 2, (float)Math.PI / 2);
                _velocity.turn(turnangle);
                _velocity.multassign(
                    Framework.randomOb.randomReal(0.5f, 1.5f));
                randomizePosition(RealBox);
            }
            if ((mutationflags & MF_POSITION) != 0)
                randomizePosition(_movebox);
            if ((mutationflags & MF_VELOCITY) != 0)
                randomizeVelocity(cCritter.MINSPEED, _maxspeedstandard);
        }

        //Accessors 

        //Serialize methods 
        //Overloads 


        /// <summary>
        /// Use to drag the wall to a new position (if it is draggable()).  Clamps against the _dragbox.
        /// </summary>
        /// <param name="newposition">The new position to drag it to.</param>
        /// <param name="dt">The change in time between frames.</param>
        /// <returns>Returns the outcode.</returns>
        public override int dragTo(cVector3 newposition, float dt)
        {
            if (!draggable())
                return cRealBox3.BOX_INSIDE; //Don't change the velocity.
            /* I'm going to allow for the possibility that I have a 3D creature in
        a 2D game world, as when I put a cSpriteQuake into a board game like 
        DamBuilder.  When I drag the walls, I still want them to be positioned
        so their butts are sitting on the xy plane. I'll run the in3DWorld test
        on the pgame->border().zsize as opposed to on the _movebox.zsize(). */
            _position.copy(newposition);
            _wrapposition1.copy(_position);
            _wrapposition2.copy(_position);
            _wrapposition3.copy(_position);
            return clamp(_dragbox);
        }

        /* Overload this so as not to change
            velocity as I normally want my walls to be stable and not drift after being dragged. */

        /// <summary>
        /// There's a collision if pcritter has crossed the wall or if the radius of pcritter is more than
        /// the pcritter's distance to the wall.  Has code to prevent the pcritter from going through the wall.
        /// Returns true if a collision was detected and false otherwise.
        /// </summary>
        /// <param name="pcritter">The critter to test for a collision with the wall.</param>
        /// <returns></returns>
        public override bool collide(cCritter pcritter)
        {
            cVector3 oldlocalpos, newlocalpos;
            float newdistance;
            int oldoutcode, newoutcode;
            bool crossedwall;

            oldlocalpos = globalToLocalPosition(pcritter.OldPosition);
            oldoutcode = _pskeleton.outcode(oldlocalpos);
            newlocalpos = globalToLocalPosition(pcritter.Position);
            newdistance = _pskeleton.distanceToOutcode(newlocalpos,
                out newoutcode); //Sets the newoutcode as well.
            crossedwall = crossed(oldoutcode, newoutcode);

            if (newdistance >= pcritter.Radius && !crossedwall) //No collision 
                return false; /*See if there's a collision at all. We
		say there's a collision if crossedwall or if the
		cCritterWall.distance is less than radius.  Remember that
		cCritterWall.distance measures the distance to the OUTSIDE 
		PERIMETER of the box, not the distance to the box's center. */

            /* I collided, so I need to move back further into the last good
            zone I was in outside the wall.  I want to set newlocalpos so 
            the rim of its critter is touching the wall. The idea is to back
            up in the direction of oldlocalpos.  To allow the possibility
            of skidding along the wall, we plan to back up from the
            the face (or edge or corner) facing oldlocalpos.  This works
            only if oldlocalpos was a good one, not inside the box.  In 
            principle this should always be true, but some rare weird circumstance
            (like a triple collsion) might mess this up, so we check for the
            bad case before starting. */

            if (oldoutcode == cRealBox3.BOX_INSIDE) //Note that this almost never happens.
            {
                cVector3 insidepos = new cVector3();
                insidepos.copy(oldlocalpos);
                oldlocalpos.subassign(pcritter.Tangent.mult(_pskeleton.MaxSize));
                //Do a brutally large backup to get out of the box for sure.
                oldoutcode = _pskeleton.outcode(oldlocalpos);
                //Recalculate outcode at this new position.
                oldlocalpos = _pskeleton.closestSurfacePoint(oldlocalpos, oldoutcode,
                    insidepos, cRealBox3.BOX_INSIDE, false);
                //Go to the closest surface point from there.
                oldoutcode = _pskeleton.outcode(oldlocalpos);
                //Recalculate outcode one more time to be safe.
                crossedwall = crossed(oldoutcode, newoutcode);
                //Recalculate crossedwall 
            }
            /* I find that with this code, the mouse can drag things through walls,
        so I do a kludge to block it by setting crossedwall to TRUE, this
        affects the action of cRealBox.closestSurfacePoint, as modified
        in build 34_4. */
            if (pcritter.Listener.IsKindOf("cListenerCursor"))
                crossedwall = true; //Don't trust the mouse listener.
            newlocalpos = _pskeleton.closestSurfacePoint(oldlocalpos, oldoutcode,
                newlocalpos, newoutcode, crossedwall);
            /* This call to closestSurfacePoint will move the newlocal pos
        from the far new side (or inside, or overlapping) of the box back to 
        the surface, usually on the old near side, edge, or corner given by
        oldoutcode. This prevents going through the	wall.
            If oldoutcode is a corner position and you are in fact heading
        towards a face near the corner, we used to bounce off the corner
        even though visually you can see you should bounce off the
        face.  This had the effect of making a scooter player get hung up on
        a corner sometimes. As of build 34_3, I'm moving the 
        newlocalpos to the newoutocode side in the case where oldlocalpos
        is an edge or a corner, and where crossedwall isn't TRUE.  I
        have to force in a TRUE for the cCursorLIstener case.  The USEJIGGLE
        code below also helps keep non-player critters from getting stuck
        on corners. */
            //Now back away from the box.
            newoutcode = _pskeleton.outcode(newlocalpos);
            cVector3 avoidbox = _pskeleton.escapeVector(newlocalpos, newoutcode);
            newlocalpos.addassign(avoidbox.mult(pcritter.Radius));
            newoutcode = _pskeleton.outcode(newlocalpos);
            pcritter.moveTo(localToGlobalPosition(newlocalpos), true);
            //TRUE means continuous motion, means adjust tangent etc.
            //Done with position, now change the velocity 
            cVector3 localvelocity = globalToLocalDirection(pcritter.Velocity);
            cVector3 oldlocalvelocity = new cVector3();
            oldlocalvelocity.copy(localvelocity);
            _pskeleton.reflect(localvelocity, newoutcode);
            /* I rewrote the reflect code on Feb 22, 2004 for VErsion 34_3, changing
        it so that when you reflect off an edge or corner, you only bounce
        the smallest of your three velocity components. Balls stll seem to
        get hung up on the corner once is awhile. */
            /* Now decide, depending on the pcritter's absorberflag and bounciness,
        how much you want to use the new localvelocity vs. the 
        oldlocalvelocity. We decompose the new localvelocity into the
        tangentvelocity parallel to the wall and the normalvelocity
        away from the wall. Some pencil and paper drawings convince
        me that the tangent is half the sum of the oldlocalvelocity
        and the reflected new localvelocity. */
            cVector3 tangentvelocity = localvelocity.add(oldlocalvelocity).mult(0.5f);
            cVector3 normalvelocity = localvelocity.sub(tangentvelocity);
            float bouncefactor = 1.0f;
            if (pcritter.AbsorberFlag)
                bouncefactor = 0.0f;
            else
                bouncefactor = pcritter.Bounciness;
            localvelocity = tangentvelocity.add(normalvelocity.mult(bouncefactor));
            /* Maybe the rotation should depend on the kind of edge or corner.
            Right now let's just use critter's binormal. Don't to it 
            to the player or viewer as it's confusing.  */
            if (!(cRealBox3.isFaceOutcode(newoutcode)) && //edge or corner 
                !(pcritter.IsKindOf("cCritterViewer")) && //not viewer 
                !(pcritter.IsKindOf("cCritterArmedPlayer")))
            //Not player.  Note that cPlayer inherits from cCritterArmedPlayer, 
            //so don't use cCritterPlayer as the base class here.
            {
                localvelocity.rotate(new cSpin(
                    Framework.randomOb.randomReal(
                        -cCritterWall.CORNERJIGGLETURN,
                        cCritterWall.CORNERJIGGLETURN), //A random turn 
                        pcritter.Binormal)); //Around the critter's binormal 
                localvelocity.multassign(cCritterWall.CORNERJIGGLEKICK); //Goose it a little 
            }
            pcritter.Velocity = localToGlobalDirection(localvelocity);
            return true;
        }


        public override int collidesWith(cCritter pcritterother)
        {
            /* Make sure I don't ever waste time colliding walls with
    walls. I only call this the one time that I enroll the cCritterWall
     into the cGame's _pcollider. */
            if (pcritterother.IsKindOf("cCritterWall"))
                return cCollider.DONTCOLLIDE;
            return base.collidesWith(pcritterother);
        }

        /* Overload to rule out possibliity of 
            all/wall collision,	even if they aren't fixed. */

        /// <summary>
        /// Finds and returns the distance from the wall to a point.
        /// </summary>
        /// <param name="vpoint">The point from which the distance to the wall will be measured.</param>
        /// <returns></returns>
        public new float distanceTo(cVector3 vpoint)
        {
            return _pskeleton.distanceTo(globalToLocalPosition(vpoint));
        }


        public override int clamp(cRealBox3 border)
        { /* We don't change _pskeleton as it has the geometric info.  We 
		just change _position. */
            if (_baseAccessControl == 1)
                return base.clamp(border);
            cRealBox3 effectivebox = border;
            cVector3 oldcorner;
            cVector3 newcorner = new cVector3();
            int outcode = 0;
            int totaloutcode = 0;
            for (int i = 0; i < 8; i++) //Step through the wall's corners 
            {
                oldcorner = _pskeleton.corner(i).add(_position);
                newcorner.copy(oldcorner);
                outcode = effectivebox.clamp(newcorner);
                if (outcode != cRealBox3.BOX_INSIDE) //corner was moved 
                {
                    _position.addassign(newcorner.sub(oldcorner));
                    /* As long at the wall is small enough to 
                fit inside the border, the successive 
                corrections won't cancel each other out. */
                    totaloutcode |= outcode;
                }
            }
            _wrapposition1.copy(_position);
            _wrapposition2.copy(_position);
            _wrapposition3.copy(_position);
            /* So it won't think it wrapped. */
            return outcode;
        }

        /// <summary>
        /// Returns the name of the class being used for this object as a string (for polymorphism)
        /// </summary>
        public override string RuntimeClass
        {
            get
            {
                return "cCritterWall";
            }
        }

        /// <summary>
        /// Gets or sets the thickness of the wall.
        /// </summary>
        public virtual float Thickness
        {
            get
                { return _pskeleton.YSize; }
            set
            {
                Skeleton = new cRealBox3(Length, value, Height);
                cPolygon ppolygon = (cPolygon) Sprite;
                ppolygon.setVertex(0, new cVector3(0.5f * Length, 0.5f * Thickness));
                ppolygon.setVertex(1, new cVector3(-0.5f * Length, 0.5f * Thickness));
                ppolygon.setVertex(2, new cVector3(-0.5f * Length, -0.5f * Thickness));
                ppolygon.setVertex(3, new cVector3(0.5f * Length, -0.5f * Thickness));
                ppolygon.fixCenterAndRadius(); /* Use this call after a bunch
			    of setVertex if points are just where you want. */
            }
        }

        /// <summary>
        /// Gets or sets _pskeleton, the frame of the wall.
        /// </summary>
        public virtual cRealBox3 Skeleton
        {
            get
            { return _pskeleton; }
            set
            {
                _pskeleton = value;
            }
        }

        /// <summary>
        /// Sets the FillColor of the wall.
        /// </summary>
        public virtual Color FillColor
        {
            set
            {
                _defaultfillcolor = value;
                if (_psprite != null)
                    _psprite.FillColor = value;
            }
        }

        /// <summary>
        /// Gets the radius (effective size) of the wall in the x direction (length).
        /// </summary>
        public virtual float XRadius
        {
            get
                { return _pskeleton.XRadius; }
        }

        /// <summary>
        /// Gets the radius (effective size) of the wall in the y direction.
        /// </summary>
        public virtual float YRadius
        {
            get
                { return _pskeleton.YRadius; }
        }

        /// <summary>
        /// Gets the radius (effective size) of the wall in the z direction (height).
        /// </summary>
        public virtual float ZRadius
        {
            get
                { return _pskeleton.ZRadius; }
        }

        /// <summary>
        /// Gets the length of the wall.
        /// </summary>
        public virtual float Length
        {
            get
                { return _pskeleton.XSize; }
        }

        /// <summary>
        /// Gets the height of the wall.
        /// </summary>
        public virtual float Height
        {
            get
                { return _pskeleton.ZSize; }
        }


        public override int WrapFlag
        {
            set
            {
                if (_baseAccessControl == 1)
                    base.WrapFlag = value;
            }
        }
        //Don't allow _wrapflag to change from cCritter::WRAP.
        //Special method 

        public bool crossed(int startoutcode, int endoutcode)
        {
            /* If crossed is TRUE then moving from start to end may
        mean you moved across the wall, even though neither start 
        nor end has to be close to the wall.  The only way to get a false
        positive is if you move very rapidly from, like LOY to HIX, skipping
        over the corner zone.  If you have a largish radius and smallish
        speed this shouldn't happen.  Our checks work by noticing when you
        leave a side zone.  To check against moving into the block exactly from
        a corner we include the BOX_INSIDE checks as well. */

            return
                (startoutcode == cRealBox3.BOX_LOX && ((endoutcode & cRealBox3.BOX_LOX) == 0)) ||
                (startoutcode == cRealBox3.BOX_HIX && ((endoutcode & cRealBox3.BOX_HIX) == 0)) ||
                (startoutcode == cRealBox3.BOX_LOY && ((endoutcode & cRealBox3.BOX_LOY) == 0)) ||
                (startoutcode == cRealBox3.BOX_HIY && ((endoutcode & cRealBox3.BOX_HIY) == 0)) ||
                (startoutcode == cRealBox3.BOX_LOZ && ((endoutcode & cRealBox3.BOX_LOZ) == 0)) ||
                (startoutcode == cRealBox3.BOX_HIZ && ((endoutcode & cRealBox3.BOX_HIZ) == 0)) ||
                startoutcode == cRealBox3.BOX_INSIDE || endoutcode == cRealBox3.BOX_INSIDE; //For corners 
        }

        /// <summary>
        /// Returns true if a line from start to end passes through the wall.  Othersize, returns false.
        /// </summary>
        /// <param name="start">The starting point for the line.</param>
        /// <param name="end">The ending point for the line.</param>
        /// <returns></returns>
        public virtual bool blocks(cVector3 start, cVector3 end)
        {
            return crossed(outcode(start), outcode(end));
        }

    }
}
 
//                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              