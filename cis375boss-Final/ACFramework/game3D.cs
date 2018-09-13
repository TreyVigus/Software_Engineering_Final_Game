using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;

// mod: setRoom1 doesn't repeat over and over again

namespace ACFramework
{ 
	
	class cCritterDoor : cCritterWall 
	{

	    public cCritterDoor(cVector3 enda, cVector3 endb, float thickness, float height, cGame pownergame ) 
		    : base( enda, endb, thickness, height, pownergame ) 
	    { 
	    }
		
		public override bool collide( cCritter pcritter ) 
		{ 
			bool collided = base.collide( pcritter );
            bool killedBoss = ((cGame3D)Game).IsBossDead(); //Can only enter the door if the boss is killed

            if ( collided && pcritter.IsKindOf( "cCritter3DPlayer" ) && killedBoss) 
			{ 
				(( cGame3D ) Game ).setdoorcollision( ); 
				return true; 
			} 
			return false; 
		}
 
        public override bool IsKindOf( string str )
        {
            return str == "cCritterDoor" || base.IsKindOf( str );
        }

        public override string RuntimeClass
        {
            get
            {
                return "cCritterDoor";
            }
        }
	} 
	
	//==============Critters for the cGame3D: Player, Ball, Treasure ================ 
	
	class cCritter3DPlayer : cCritterArmedPlayer 
	{ 
        public bool left = false;
        public bool right = true;
        cGame game;
        public const int MAX_HEALTH = 100;

		
        public cCritter3DPlayer( cGame pownergame ) 
            : base( pownergame ) 
		{
            game = pownergame;
			BulletClass = new cCritter3DPlayerBullet( );
            Sprite = new cSpriteQuake(ModelsMD2.MrSkeltal);
			Sprite.SpriteAttitude = cMatrix3.scale( 2, 0.8f, 0.4f );
            Sprite.ModelState = State.Idle;
			setRadius( cGame3D.PLAYERRADIUS ); //Default cCritter.PLAYERRADIUS is 0.4.  
			setHealth( 10 ); 
			moveTo( _movebox.LoCorner.add( new cVector3( 0.0f, 0.0f, 2.0f ))); 
			WrapFlag = cCritter.CLAMP; //Use CLAMP so you stop dead at edges.
			Armed = true; //Let's use bullets.
			MaxSpeed =  cGame3D.MAXPLAYERSPEED; 
			AbsorberFlag = true; //Keeps player from being buffeted about.
			ListenerAcceleration = 160.0f; //So Hopper can overcome gravity.  Only affects hop.
		
            // YHopper hop strength 12.0
			Listener = new cListenerScooterYHopperAir( 6.0f, 18.0f ); 
            // the two arguments are walkspeed and hop strength -- JC
            
            addForce( new cForceGravity( 50.0f )); /* Uses  gravity. Default strength is 25.0.
			Gravity	will affect player using cListenerHopper. */ 
            addForce( new CenteringForce());
			AttitudeToMotionLock = false; //It looks nicer is you don't turn the player with motion.
			Attitude = new cMatrix3( new cVector3(0.0f, 0.0f, -1.0f), new cVector3( -1.0f, 0.0f, 0.0f ), 
                new cVector3( 0.0f, 1.0f, 0.0f ), Position);
            Health = MAX_HEALTH;
		}

        public override void update(ACView pactiveview, float dt)
        {
            base.update(pactiveview, dt); //Always call this first
        } 

        public override bool collide( cCritter pcritter ) 
		{ 
			bool playerhigherthancritter = Position.Y - Radius > pcritter.Position.Y; 
		/* If you are "higher" than the pcritter, as in jumping on it, you get a point
	and the critter dies.  If you are lower than it, you lose health and the
	critter also dies. To be higher, let's say your low point has to higher
	than the critter's center. We compute playerhigherthancritter before the collide,
	as collide can change the positions. */
            _baseAccessControl = 1;
			bool collided = base.collide( pcritter );
            _baseAccessControl = 0;
            if (!collided) 
				return false;
		/* If you're here, you collided.  We'll treat all the guys the same -- the collision
	 with a Treasure is different, but we let the Treasure contol that collision. */ 
			if ( playerhigherthancritter ) 
			{
                Framework.snd.play(Sound.Goopy); 
				//addScore( 10 ); 
			} 
			else 
			{
                if (!Shield)
                {
                    damage(1);
                    Framework.snd.play(Sound.HammerOnce); 
                }
			}
            if (pcritter.IsKindOf("KnockbackBullet"))
            {
                KnockbackBullet bullet = (KnockbackBullet)pcritter;
                pcritter.collide(this);
            }
			return true; 
		}

        public override cCritterBullet shoot()
        {
            Framework.snd.play(Sound.LaserFire);
            return base.shoot();
        }

        public override bool IsKindOf( string str )
        {
            return str == "cCritter3DPlayer" || base.IsKindOf( str );
        }
		
        public override string RuntimeClass
        {
            get
            {
                return "cCritter3DPlayer";
            }
        }

        public bool Right { 
            get { return right; } 
            set { right = value;} 
        }

        public bool Left { 
            get { return left; } 
            set { left = value; } 
        }
    } 
	
   
	class cCritter3DPlayerBullet : cCritterBullet 
	{

        public cCritter3DPlayerBullet() { }

        public override cCritterBullet Create()
            // has to be a Create function for every type of bullet -- JC
        {
            return new cCritter3DPlayerBullet();
        }
		
		public override void initialize( cCritterArmed pshooter ) 
		{ 
			base.initialize( pshooter );
            Sprite.FillColor = Color.Crimson;
            // can use setSprite here too
            setRadius(0.1f);
		} 

        public override bool IsKindOf( string str )
        {
            return str == "cCritter3DPlayerBullet" || base.IsKindOf( str );
        }
		
        public override string RuntimeClass
        {
            get
            {
                return "cCritter3DPlayerBullet";
            }
        }

        public override bool collide(cCritter pcritter)
        {
            if(pcritter.IsKindOf("KnockbackBullet"))
            {
                return false;
            }

            bool collided = base.collide(pcritter);
            if(Player.Shield)
            {
                pcritter.damage(5);
            }
            return collided;
        }
    } 
	
	class cCritter3Dcharacter : cCritter  
	{ 
		
        public cCritter3Dcharacter( cGame pownergame ) 
            : base( pownergame ) 
		{ 
			addForce( new cForceGravity( 25.0f, new cVector3( 0.0f, -1, 0.00f ))); 
			addForce( new cForceDrag( 20.0f ) );  // default friction strength 0.5 
			Density = 2.0f; 
			MaxSpeed = 30.0f;
            if (pownergame != null) //Just to be safe.
                Sprite = new cSpriteQuake(Framework.models.selectRandomCritter());
            
            // example of setting a specific model
            // setSprite(new cSpriteQuake(ModelsMD2.Knight));
            
            if ( Sprite.IsKindOf( "cSpriteQuake" )) //Don't let the figurines tumble.  
			{ 
				AttitudeToMotionLock = false;   
				Attitude = new cMatrix3( new cVector3( 0.0f, 0.0f, 1.0f ), 
                    new cVector3( 1.0f, 0.0f, 0.0f ), 
                    new cVector3( 0.0f, 1.0f, 0.0f ), Position); 
				/* Orient them so they are facing towards positive Z with heads towards Y. */ 
			} 
			Bounciness = 0.0f; //Not 1.0 means it loses a bit of energy with each bounce.
			setRadius( 1.0f );
            MinTwitchThresholdSpeed = 4.0f; //Means sprite doesn't switch direction unless it's moving fast 
			randomizePosition( new cRealBox3( new cVector3( _movebox.Lox, _movebox.Loy, _movebox.Loz + 4.0f), 
				new cVector3( _movebox.Hix, _movebox.Loy, _movebox.Midz - 1.0f))); 
				/* I put them ahead of the player  */ 
			randomizeVelocity( 0.0f, 30.0f, false ); 

                        
			if ( pownergame != null ) //Then we know we added this to a game so pplayer() is valid 
				addForce( new cForceObjectSeek( Player, 0.5f ));

            int begf = Framework.randomOb.random(0, 171);
            int endf = Framework.randomOb.random(0, 171);

            if (begf > endf)
            {
                int temp = begf;
                begf = endf;
                endf = temp;
            }

			Sprite.setstate( State.Other, begf, endf, StateType.Repeat );


            _wrapflag = cCritter.BOUNCE;

		} 

		
		public override void update( ACView pactiveview, float dt ) 
		{ 
			base.update( pactiveview, dt ); //Always call this first
			if ( (_outcode & cRealBox3.BOX_HIZ) != 0 ) /* use bitwise AND to check if a flag is set. */ 
				delete_me(); //tell the game to remove yourself if you fall up to the hiz.
        } 

		// do a delete_me if you hit the left end 
	
		public override void die() 
		{ 
			Player.addScore( Value ); 
			base.die(); 
		} 

       public override bool IsKindOf( string str )
        {
            return str == "cCritter3Dcharacter" || base.IsKindOf( str );
        }
	
        public override string RuntimeClass
        {
            get
            {
                return "cCritter3Dcharacter";
            }
        }
	} 
	
	class cCritterTreasure : cCritter 
	{   // Try jumping through this hoop
		
		public cCritterTreasure( cGame pownergame ) : 
		base( pownergame ) 
		{ 
			/* The sprites look nice from afar, but bitmap speed is really slow
		when you get close to them, so don't use this. */ 
			cPolygon ppoly = new cPolygon( 24 ); 
			ppoly.Filled = false; 
			ppoly.LineWidthWeight = 0.5f;
			Sprite = ppoly; 
			_collidepriority = cCollider.CP_PLAYER + 1; /* Let this guy call collide on the
			player, as his method is overloaded in a special way. */ 
			rotate( new cSpin( (float) Math.PI / 2.0f, new cVector3(0.0f, 0.0f, 1.0f) )); /* Trial and error shows this
			rotation works to make it face the z diretion. */ 
			setRadius( cGame3D.TREASURERADIUS ); 
			FixedFlag = true; 
			moveTo( new cVector3( _movebox.Midx, _movebox.Midy - 2.0f, 
				_movebox.Loz - 1.5f * cGame3D.TREASURERADIUS ));
		}

        public cCritterTreasure(cGame pownergame, cVector3 position) :
            base(pownergame)
        {
            /* The sprites look nice from afar, but bitmap speed is really slow
        when you get close to them, so don't use this. */
            cPolygon ppoly = new cPolygon(24);
            ppoly.Filled = false;
            ppoly.LineWidthWeight = 0.5f;
            Sprite = ppoly;
            _collidepriority = cCollider.CP_PLAYER + 1; /* Let this guy call collide on the
			player, as his method is overloaded in a special way. */
            rotate(new cSpin((float)Math.PI / 2.0f, new cVector3(0.0f, 0.0f, 1.0f))); /* Trial and error shows this
			rotation works to make it face the z diretion. */
            setRadius(cGame3D.TREASURERADIUS);
            FixedFlag = true;
            moveTo(position);
        } 
		
		public override bool collide( cCritter pcritter ) 
		{ 
			if ( contains( pcritter )) //disk of pcritter is wholly inside my disk 
			{
                Framework.snd.play(Sound.Clap); 
				pcritter.addScore( 100 ); 
				pcritter.addHealth( 1 ); 
				pcritter.moveTo( new cVector3( _movebox.Midx, _movebox.Loy + 1.0f,
                    _movebox.Hiz - 3.0f )); 
				return true; 
			} 
			else 
				return false; 
		} 

		//Checks if pcritter inside.
	
		public override int collidesWith( cCritter pothercritter ) 
		{ 
			if ( pothercritter.IsKindOf( "cCritter3DPlayer" )) 
				return cCollider.COLLIDEASCALLER; 
			else 
				return cCollider.DONTCOLLIDE; 
		} 

		/* Only collide
			with cCritter3DPlayer. */ 

       public override bool IsKindOf( string str )
        {
            return str == "cCritterTreasure" || base.IsKindOf( str );
        }
	
        public override string RuntimeClass
        {
            get
            {
                return "cCritterTreasure";
            }
        }
	} 
	
	//======================cGame3D========================== 
	class cGame3D : cGame 
	{ 
		public static readonly float TREASURERADIUS = 1.2f; 
		public static readonly float WALLTHICKNESS = 0.5f; 
		public static readonly float PLAYERRADIUS = 0.3f; 
		public static readonly float MAXPLAYERSPEED = 30.0f; 
		private cCritterTreasure _ptreasure; 
		private bool doorcollision;
        private bool wentThrough = false;
        private int currentRoom = 0;
        // age that new room was started
        private float startNewRoom;

        public cGame3D() 
		{
			doorcollision = false; 
			_menuflags &= ~ cGame.MENU_BOUNCEWRAP; 
			_menuflags |= cGame.MENU_HOPPER; //Turn on hopper listener option.
			_spritetype = cGame.ST_MESHSKIN; 
			setBorder( 64.0f, 16.0f, 64.0f ); // size of the world
		
			cRealBox3 skeleton = new cRealBox3();
            skeleton.copy(_border);
			setSkyBox( skeleton );
		/* In this world the coordinates are screwed up to match the screwed up
		listener that I use.  I should fix the listener and the coords.
		Meanwhile...
		I am flying into the screen from HIZ towards LOZ, and
		LOX below and HIX above and
		LOY on the right and HIY on the left. */ 
			SkyBox.setSideSolidColor( cRealBox3.HIZ, Color.Aqua ); //Make the near HIZ transparent 
			SkyBox.setSideSolidColor( cRealBox3.LOZ, Color.Aqua ); //Far wall 
			SkyBox.setSideSolidColor( cRealBox3.LOX, Color.DarkOrchid ); //left wall 
            SkyBox.setSideTexture( cRealBox3.HIX, BitmapRes.Wall2, 2 ); //right wall 
			SkyBox.setSideTexture( cRealBox3.LOY, BitmapRes.Graphics3 ); //floor 
			SkyBox.setSideTexture( cRealBox3.HIY, BitmapRes.Sky ); //ceiling 
		
			WrapFlag = cCritter.BOUNCE; 
			_seedcount = 7; 
			setPlayer( new cCritter3DPlayer( this )); 
			_ptreasure = new cCritterMedpack(this); 
		
			/* In this world the x and y go left and up respectively, while z comes out of the screen.
		A wall views its "thickness" as in the y direction, which is up here, and its
		"height" as in the z direction, which is into the screen. */ 
			//First draw a wall with dy height resting on the bottom of the world.
			float zpos = 0.0f; /* Point on the z axis where we set down the wall.  0 would be center,
			halfway down the hall, but we can offset it if we like. */ 
			float height = 0.1f * _border.YSize; 
			float ycenter = -_border.YRadius + height / 2.0f; 
			float wallthickness = cGame3D.WALLTHICKNESS;
            cCritterWall pwall = new cCritterWall( 
				new cVector3( _border.Midx + 5.0f, ycenter, zpos ), 
				new cVector3( _border.Hix, ycenter, zpos ), 
				height, //thickness param for wall's dy which goes perpendicular to the 
					//baseline established by the frist two args, up the screen 
				wallthickness, //height argument for this wall's dz  goes into the screen 
				this );
			cSpriteTextureBox pspritebox = 
				new cSpriteTextureBox( pwall.Skeleton, BitmapRes.Wall3, 16 ); //Sets all sides 
				/* We'll tile our sprites three times along the long sides, and on the
			short ends, we'll only tile them once, so we reset these two. */
          pwall.Sprite = pspritebox; 
		
		
			//Then draw a ramp to the top of the wall.  Scoot it over against the right wall.
			float planckwidth = 0.75f * height; 
			pwall = new cCritterWall( 
				new cVector3( _border.Hix -planckwidth / 2.0f, _border.Loy, _border.Hiz - 2.0f), 
				new cVector3( _border.Hix - planckwidth / 2.0f, _border.Loy + height, zpos ), 
				planckwidth, //thickness param for wall's dy which is perpenedicualr to the baseline, 
						//which goes into the screen, so thickness goes to the right 
				wallthickness, //_border.zradius(),  //height argument for wall's dz which goes into the screen 
				this );
            cSpriteTextureBox stb = new cSpriteTextureBox(pwall.Skeleton, 
                BitmapRes.Wood2, 2 );
            pwall.Sprite = stb;
		
			cCritterDoor pdwall = new cCritterDoor( 
				new cVector3( _border.Midx, _border.Loy, _border.Loz ), 
				new cVector3( _border.Midx, _border.Midy - 3, _border.Loz ), 
				2.0f, 2, this ); 
			cSpriteTextureBox pspritedoor = 
				new cSpriteTextureBox( pdwall.Skeleton, BitmapRes.Door ); 
			pdwall.Sprite = pspritedoor; 
		}

        /// <summary>
        /// Provides score, health, and if invincibility is on
        /// </summary>
        /// <returns></returns>
        public override string statusMessage()
        {
            string cStrStatusMessage;
            string cStrHealth;
            string cStrScore;
            string cStrShieldStatus;

            cStrScore = "Score: " + Score.ToString();
            cStrHealth = "Health: " + Health.ToString();
            cStrShieldStatus = "Invincibility: " + (Player.Shield ? "ON" : "OFF");    //shows shield is on or off
            cStrStatusMessage = "AC Framework       " + cStrScore + "   " + cStrHealth + "    " + cStrShieldStatus + "    " + addOn;
            return cStrStatusMessage;
        }

        public void setRoom1( )
        {
            Biota.purgeCritters("cCritterWall");
            Biota.purgeCritters("cCritter3Dcharacter");
            Biota.purgeCritters("cCritterBoss");
            setBorder(50.0f, 20.0f, 50.0f); 
	        cRealBox3 skeleton = new cRealBox3();
            skeleton.copy( _border );
	        setSkyBox(skeleton);
	        SkyBox.setAllSidesTexture( BitmapRes.Sky, 2 );
	        SkyBox.setSideTexture( cRealBox3.LOY, BitmapRes.Concrete );

	        _seedcount = 0;
	        Player.setMoveBox( new cRealBox3( 50.0f, 20.0f, 50.0f ) );
            wentThrough = true;
            startNewRoom = Age;

            _ptreasure = new cCritterMedpack(this, new cVector3(_border.Midx, _border.Loy + 2.0f,
                _border.Midz + 2.0f * cGame3D.TREASURERADIUS));

            float rampRaise = 0.5f;
            float height = 1.0f;
            float width = 10.0f;

            // add ramp
            cCritterWall pwall = new cCritterWall(
                new cVector3(_border.Midx, _border.Loy - rampRaise, _border.Hiz - 3.0f),
                new cVector3(_border.Midx, _border.Loy + rampRaise, _border.Hiz - 5.0f),
                width, //thickness param for wall's dy which goes perpendicular to the 
                //baseline established by the frist two args, up the screen 
                height, //height argument for this wall's dz  goes into the screen 
                this);
            cSpriteTextureBox pspritebox =
                new cSpriteTextureBox(pwall.Skeleton, BitmapRes.Metal, 16); //Sets all sides 
            pwall.Sprite = pspritebox; 

            // add boss platform
            pwall = new cCritterWall(
                new cVector3(_border.Midx, _border.Loy + rampRaise, _border.Hiz - 5.0f),
                new cVector3(_border.Midx, _border.Loy + rampRaise, _border.Loz),
                width, //thickness param for wall's dy which goes perpendicular to the 
                //baseline established by the frist two args, up the screen 
                height, //height argument for this wall's dz  goes into the screen 
                this);
            pspritebox =
                new cSpriteTextureBox(pwall.Skeleton, BitmapRes.Metal, 16); //Sets all sides 
            pwall.Sprite = pspritebox; 

            // add door
            cCritterDoor pdwall = new cCritterDoor(
                new cVector3(_border.Midx, _border.Loy + rampRaise, _border.Loz),
                new cVector3(_border.Midx, _border.Midy - 3 + rampRaise, _border.Loz),
                2.0f, 2, this);
            cSpriteTextureBox pspritedoor =
                new cSpriteTextureBox(pdwall.Skeleton, BitmapRes.Door);
            pdwall.Sprite = pspritedoor;

            seedCritters();

            new cCritterBossMog(this, new cVector3(_border.Midx, _border.Loy + rampRaise, _border.Loz), new PulseBullet());
        }

        public void setRoom2()
        {
            Biota.purgeCritters("cCritterWall");
            Biota.purgeCritters("cCritter3Dcharacter");
            Biota.purgeCritters("cCritterBoss");
            setBorder(50.0f, 40.0f, 50.0f);
            cRealBox3 skeleton = new cRealBox3();
            skeleton.copy(_border);
            setSkyBox(skeleton);
            SkyBox.setAllSidesTexture(BitmapRes.Graphics1, 2);
            SkyBox.setSideTexture(cRealBox3.LOY, BitmapRes.Concrete);
            SkyBox.setSideSolidColor(cRealBox3.HIY, Color.Blue);
            _seedcount = 0;
            Player.setMoveBox(new cRealBox3(50.0f, 40.0f, 50.0f));
            wentThrough = true;
            startNewRoom = Age;

            //add door
            cCritterDoor pdwall = new cCritterDoor(new cVector3(_border.Midx, _border.Loy + 4, _border.Loz),new cVector3(_border.Midx, _border.Loy + 10, _border.Loz),2.0f, 3.0f, this);
            cSpriteTextureBox pspritedoor = new cSpriteTextureBox(pdwall.Skeleton, BitmapRes.Door);
            pdwall.Sprite = pspritedoor;

            float wallThickness = 5.0f, wallHeight = 10.0f;
            //starting block
            cVector3 enda = new cVector3(Border.Midx, Border.Loy + 1, Border.Hiz);
            cVector3 endb = new cVector3(Border.Midx, Border.Loy + 1, Border.Hiz-5);
            cCritterWall startingBlock = new cCritterWall(enda, endb, wallThickness, wallHeight, this);
            cSpriteTextureBox pspritebox =
                new cSpriteTextureBox(startingBlock.Skeleton, BitmapRes.Wood2, 16); //Sets all sides 
            startingBlock.Sprite = pspritebox;
            //ending block
            cVector3 enda2 = new cVector3(Border.Midx, Border.Loy + 1, Border.Loz+7);
            cVector3 endb2 = new cVector3(Border.Midx, Border.Loy + 1, Border.Loz);
            cCritterWall endBlock = new cCritterWall(enda2, endb2, wallThickness, 8.0f, this);
            cSpriteTextureBox pspritebox2 =
                new cSpriteTextureBox(endBlock.Skeleton, BitmapRes.Wood2, 16); //Sets all sides 
            endBlock.Sprite = pspritebox2;
            //floor 
            cVector3 enda3 = new cVector3(Border.Midx, Border.Loy, Border.Loz);
            cVector3 endb3 = new cVector3(Border.Midx, Border.Loy, Border.Hiz);
            DamagingWall floor = new DamagingWall(enda3, endb3, 0.5f ,0.5f, this);
            //med pack
            _ptreasure = new cCritterMedpack(this, new cVector3(Border.Midx, Border.Loy + 7, Border.Hiz - 4));
    
            //sliding walls (spawned based on pos of starting block)
            cVector3 leftEnd = new cVector3();
            leftEnd.addassign(startingBlock.Position.add(new cVector3(0, 4.0f, -0.5f)));
            cVector3 rightEnd = new cVector3();
            rightEnd.copy(leftEnd);
            rightEnd.addassign(new cVector3(0, 0, -5.0f));
            float thickness = 0.1f, height = 0.3f;
            cVector3 moveAxisandDirection = new cVector3(0, 0, -0.005f);
            cVector3 startPos = new cVector3();
            startPos.copy(rightEnd);
            cVector3 endPos = new cVector3();
            endPos.copy(startPos);
            endPos.Z = Border.Loz + 9.5f;

            //spawn sliding walls in a separate thread
            new Thread(() => 
                spawnWall(leftEnd, rightEnd, thickness, height, this, cCritterSlideWall.TYPE.PLATFORM, moveAxisandDirection, false, startPos, endPos)).Start();

            seedCritters();
            Player.moveTo(startingBlock.Position.add(new cVector3(0, 10.0f, 0)));
            new cCritterBossDrFreak(this, endBlock.Position.add(new cVector3(0,10.0f,0)), new KnockbackBullet(),5);
         
        }

        
        //function to be passed into the thread that walls are spawned in
        //this allows us to wait a few seconds before spawning walls in order to maintain a gap between them.
        public void spawnWall(cVector3 enda, cVector3 endb, float thickness, float height, cGame pownergame, cCritterSlideWall.TYPE wallType, cVector3 moveAxisAndDirection, bool bounce, cVector3 startPos, cVector3 endPos)
        {
            int numWalls = 3;
            float spawnPos = 10; //position the ith wall must reach before the (i + 1)th wall spawns
            
            //spawn the first wall
            cCritterSlideWall wall = new cCritterSlideWall(enda, endb, thickness, height, pownergame, wallType, moveAxisAndDirection, bounce, startPos, endPos);
            cSpriteTextureBox spriteBox = new cSpriteTextureBox(wall.Skeleton, BitmapRes.Wood2, 16);
            wall.Sprite = spriteBox;

            //spawn the remaining walls
            for (int i = 0; i < numWalls - 1; i++)
            {
                //wait until the previous wall reaches the spawn position
                while (wall.Position.Z > spawnPos)
                {
                    ;
                }
                //spawn the wall and add textures.
                wall = new cCritterSlideWall(enda, endb, thickness, height, pownergame, wallType, moveAxisAndDirection, bounce, startPos, endPos);
                spriteBox = new cSpriteTextureBox(wall.Skeleton, BitmapRes.Wood2, 16); 
                wall.Sprite = spriteBox;
            }
        }

        public void setRoom3()
        {
            Biota.purgeCritters("cCritterWall");
            Biota.purgeCritters("cCritter3Dcharacter");
            Biota.purgeCritters("cCritterBoss");
            setBorder(50.0f, 20.0f, 50.0f);
            cRealBox3 skeleton = new cRealBox3();
            skeleton.copy(_border);
            setSkyBox(skeleton);
            SkyBox.setAllSidesTexture(BitmapRes.Graphics3, 2);
            SkyBox.setSideTexture(cRealBox3.LOY, BitmapRes.Concrete);
            SkyBox.setSideSolidColor(cRealBox3.HIY, Color.Blue);
            _seedcount = 0;
            Player.setMoveBox(new cRealBox3(50.0f, 20.0f, 50.0f));
            
            wentThrough = true;
            startNewRoom = Age;

            cCritterDoor pdwall = new cCritterDoor(
                new cVector3(_border.Midx, _border.Loy, _border.Loz),
                new cVector3(_border.Midx, _border.Midy - 3, _border.Loz),
                2.0f, 2, this);
            cSpriteTextureBox pspritedoor =
                new cSpriteTextureBox(pdwall.Skeleton, BitmapRes.Door);
            pdwall.Sprite = pspritedoor;

            float height = 0.1f * _border.YSize;
            float ycenter = -_border.YRadius + height / 2.0f;
            float wallthickness = cGame3D.WALLTHICKNESS + 1.0f;
            cCritterSlideWall pwall = new cCritterSlideWall(new cVector3(_border.Midx, ycenter - 10, _border.Midz+10), new cVector3(_border.Hix + 50, ycenter - 10, _border.Midz+10),
            height, wallthickness, this, cCritterSlideWall.TYPE.HARMFUL, new cVector3(0.0f, -0.05f, 0.0f), false, 200);
            cSpriteTextureBox pspritebox = new cSpriteTextureBox(pwall.Skeleton, BitmapRes.Wood2, 16);
            pwall.Sprite = pspritebox;

            seedCritters();

            _ptreasure = new cCritterMedpack(this, new cVector3(_border.Midx, _border.Midy - 2.0f,
                _border.Loz - 1.5f * cGame3D.TREASURERADIUS));

            cCritterBossDragonKnight boss = new cCritterBossDragonKnight(this, new cVector3(_border.Midx, _border.Loy, _border.Midz - 5), new DragonBullet(1.0f));
        }

        public void setTestRoom()
        {
            Biota.purgeCritters("cCritterWall");
            Biota.purgeCritters("cCritter3Dcharacter");
            setBorder(50.0f, 20.0f, 100.0f);
            cRealBox3 skeleton = new cRealBox3();
            skeleton.copy(_border);
            setSkyBox(skeleton);
            SkyBox.setSideTexture(cRealBox3.BOX_HIX, BitmapRes.Wood2);
            SkyBox.setSideTexture(cRealBox3.BOX_LOX, BitmapRes.Graphics2);
            SkyBox.setSideTexture(cRealBox3.BOX_HIY, BitmapRes.Sky);
            SkyBox.setSideTexture(cRealBox3.BOX_LOY, BitmapRes.Metal);
            SkyBox.setSideTexture(cRealBox3.BOX_HIZ, BitmapRes.Wall1);
            SkyBox.setSideTexture(cRealBox3.BOX_LOZ, BitmapRes.Graphics3);
            _seedcount = 0;
            Player.setMoveBox(new cRealBox3(50.0f, 20.0f, 100.0f));

        }

        /// <summary>
        /// Counts number of bosses in room
        /// </summary>
        /// <returns>returns true if no boss is in room</returns>
        public bool IsBossDead()
        {
            if (_pbiota.count("cCritterBoss") == 0)
            {
                return true;
            } else
            {
                return false;
            }
        }
		
		public override void seedCritters() 
		{
			Biota.purgeCritters( "cCritterBullet" ); 
			Biota.purgeCritters( "cCritter3Dcharacter" );
            for (int i = 0; i < _seedcount; i++) 
				//new cCritter3Dcharacter( this );
            Player.moveTo(new cVector3(0.0f, Border.Loy, Border.Hiz - 3.0f)); 
				/* We start at hiz and move towards	loz */ 
		} 

		
		public void setdoorcollision( ) { doorcollision = true; } 
		
		public override ACView View 
		{
            set
            {
                base.View = value; //You MUST call the base class method here.
                value.setUseBackground(ACView.FULL_BACKGROUND); /* The background type can be
			    ACView.NO_BACKGROUND, ACView.SIMPLIFIED_BACKGROUND, or 
			    ACView.FULL_BACKGROUND, which often means: nothing, lines, or
			    planes&bitmaps, depending on how the skybox is defined. */
                value.pviewpointcritter().Listener = new cListenerViewerRide();
            }
		} 

		
		public override cCritterViewer Viewpoint 
		{ 
            set
            {
                //value is set to cListenerViewerRide by default in ACView.cs line 39 
			    if ( value.Listener.RuntimeClass == "cListenerViewerRide" ) 
			    { 
				    value.setViewpoint( new cVector3( 0.0f, 0.3f, -1.0f ), _border.Center); 
					//Always make some setViewpoint call simply to put in a default zoom.
				    value.zoom( 0.35f ); //Wideangle 
				    cListenerViewerRide prider = ( cListenerViewerRide )( value.Listener);
                    prider.Offset = (new cVector3(1.5f, -5.0f, 0.0f)); 
                    // prider.Offset = (new cVector3( -1.5f, 0.0f, 1.0f)); 
                    /* This offset is in the coordinate
				    system of the player, where the negative X axis is the negative of the
				    player's tangent direction, which means stand right behind the player. */
                } 
			    else //Not riding the player.
			    { 
				    value.zoom( 1.0f ); 
				    /* The two args to setViewpoint are (directiontoviewer, lookatpoint).
				    Note that directiontoviewer points FROM the origin TOWARDS the viewer. */ 
				    value.setViewpoint( new cVector3( 0.0f, 0.3f, 1.0f ), _border.Center); 
			    }
            }
		}

        public bool Doorcollision { get { return doorcollision; } set { doorcollision = value; } }

        /* Move over to be above the
			lower left corner where the player is.  In 3D, use a low viewpoint low looking up. */

        public override void adjustGameParameters() 
		{
		// (1) End the game if the player is dead 
			if ( (Health == 0) && !_gameover ) //Player's been killed and game's not over.
			{ 
				_gameover = true; 
				Player.addScore( _scorecorrection ); // So user can reach _maxscore  
                Framework.snd.play(Sound.Hallelujah);
                return ; 
			} 
		// (2) Also don't let the the model count diminish.
					//(need to recheck propcount in case we just called seedCritters).
			int modelcount = Biota.count( "cCritter3Dcharacter" ); 
			int modelstoadd = _seedcount - modelcount; 
			for ( int i = 0; i < modelstoadd; i++) 
				//new cCritter3Dcharacter( this );
            // (3) Maybe check some other conditions.
            if (doorcollision == true && currentRoom == 0)
            {
                Player.moveTo(new cVector3(0.0f, Border.Loy, Border.Hiz - 3.0f));
                setRoom1();
                doorcollision = false;
                currentRoom++;
            }

            if (doorcollision == true && currentRoom == 1)
            {
                Player.moveTo(new cVector3(0.0f, Border.Loy, Border.Hiz - 3.0f));
                setRoom2();
                doorcollision = false;
                currentRoom++;
            }

            if (doorcollision == true && currentRoom == 2)
            {
                Player.moveTo(new cVector3(0.0f, Border.Loy, Border.Hiz - 3.0f));
                setRoom3();
                doorcollision = false;
                currentRoom++;
            }

            if (doorcollision == true && currentRoom == 3)
            {
                setTestRoom();
                doorcollision = false;
                currentRoom++;
                Player.moveTo(new cVector3(0.0f, Border.Loy, Border.Hiz - 3.0f));
                // game is over
                _gameover = true; 
                Framework.snd.play(Sound.Hallelujah);
                return;
            }
		} 
		
	} 
	
}