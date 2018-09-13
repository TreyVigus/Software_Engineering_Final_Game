// For AC Framework 1.2, ZEROVECTOR and other vectors were removed,
// default parameters were added


using System;



namespace ACFramework
{ 
	
	class cForce 
	{ 
		public static readonly float INTENSITY = 5.0f; //5.0 Default value.  How strong the _intensity is.
		protected float _intensity; 
		
		protected cForce()
        {
            _intensity = INTENSITY;
        } 
		
		public virtual void copy( cForce pforce ) 
		{ 
			_intensity = pforce._intensity; 
		}
 
        public virtual cForce copy( )
        {
            cForce f = new cForce();
            f.copy(this);
            return f;
        }

        public virtual bool IsKindOf( string str )
        {
            return str == "cForce";
        }
		
		public virtual bool isGlobalPhysicsForce(){ return false; } /* This is useful for the cCritterBullet to
	 		know which forces to copy from the shooter.  We return TRUE only for cForceDrag and 	
	 		cForceGravity, the others will give the default return value of FALSE. */ 
		
		public virtual cVector3 force( cCritter pcritter )
        {
            return new cVector3(0.0f, 0.0f, 0.0f);
        } 
	}



    class cForceDrag : cForce
    {
        protected cVector3 _windvector; /* The effective rest direction for this force.  Default is
	 		the zero vector.  */

        public cForceDrag(float friction = 0.5f)
        {
            _windvector = new cVector3();
            _intensity = friction;
        }

        public cForceDrag(float friction, cVector3 windvector)
        {
            _windvector = new cVector3();
            _windvector.copy(windvector);
            _intensity = friction;
        }

        public override void copy(cForce pforce)
        {
            base.copy(pforce);
            if (!pforce.IsKindOf("cForceDrag"))
                return;
            cForceDrag pforcechild = (cForceDrag)pforce; // Cast so as to access fields.
            _windvector = pforcechild._windvector;
        }

        public override cForce copy()
        {
            cForceDrag f = new cForceDrag();
            f.copy(this);
            return f;
        }

        public override bool IsKindOf(string str)
        {
            return str == "cForceDrag" || base.IsKindOf(str);
        }

        public override bool isGlobalPhysicsForce() { return true; }

        public override cVector3 force(cCritter pcritter)
        {
            float area = pcritter.Radius * pcritter.Radius;
            float mass = pcritter.Mass;
            return _windvector.sub(pcritter.Velocity).mult(area * _intensity);
        }
    }		


	class cForceGravity : cForce 
	{ 
		protected cVector3 _pulldirection; 
		
		public cForceGravity( float gravity = 25.0f ) 
		{
            _pulldirection = new cVector3(0.0f, -1.0f, 0.0f);  
			_intensity = gravity; 
		} 

        public cForceGravity( float gravity, cVector3 pulldirection ) 
		{
            _pulldirection = new cVector3();
			_pulldirection.copy( pulldirection ); 
			_intensity = gravity; 
		} 
			/* 25.0 seems to work well as a default value.
			Since I often use this in 2D games, we'll make the default gravity be negative Y reather than
			the negative Z we'd normally want for 3D games. */ 
		
		public override void copy( cForce pforce ) 
		{ 
			base.copy( pforce ); 
			if ( !pforce.IsKindOf( "cForceGravity" )) 
				return ; 
			cForceGravity pforcechild = ( cForceGravity )( pforce ); 
			_pulldirection = pforcechild._pulldirection; 
		}
 
        public override cForce copy( )
        {
            cForceGravity f = new cForceGravity();
            f.copy(this);
            return f;
        }

        public override bool IsKindOf( string str )
        {
            return str == "cForceGravity" || base.IsKindOf( str );
        }
		
		public override bool isGlobalPhysicsForce(){ return true; } 
		
		public override cVector3 force( cCritter pcritter ) 
		{ 
			return _pulldirection.mult( _intensity * pcritter.Mass); 
		} 

		
	} 
	
	
	class cForceVortex : cForceDrag 
	{ /* Note that the _intensity inherited from cForceDrag affects how strongly 
	 		this force acts. */ 
		protected cVector3 _eyeposition; //Center of the vortex.
		protected float _spiralangle; /* Angle that the vortex force makes with the vector from _eyeposition
	 			to a critter position.  Positive means counterclockwise, negative is clockwise,
	 			-0.5 to 0.5 PI is outward, 0.5 to 1.5 PI is inward. +- 0.5 PI is neither. */ 
		
 	    public cForceVortex( float friction = 0.5f )
		    : base(friction) 
        {
            _eyeposition = new cVector3(0.0f, 0.0f, 0.0f);
	        _spiralangle = 0.6f * (float) Math.PI;
        } 
        
 	    public cForceVortex(float friction, float spiralangle )
		    : base(friction) 
        {
            _eyeposition = new cVector3(0.0f, 0.0f, 0.0f);
	        _spiralangle = spiralangle;
        } 
        
        public cForceVortex(float friction, float spiralangle, cVector3 eyeposition)
		: base(friction) 
        {
            _eyeposition = new cVector3();
	        _spiralangle = spiralangle;
	        _eyeposition.copy(eyeposition);
        } 
        
        public override void copy( cForce pforce ) 
		{ 
			base.copy( pforce ); 
			if ( !pforce.IsKindOf( "cForceVortex" )) 
				return ; 
			cForceVortex pforcechild = ( cForceVortex )( pforce ); // Cast so next lines work.
			_eyeposition.copy( pforcechild._eyeposition ); 
			_spiralangle = pforcechild._spiralangle; 
		} 

        public override cForce copy( )
        {
            cForceVortex f = new cForceVortex();
            f.copy(this);
            return f;
        }
		
        public override bool IsKindOf( string str )
        {
            return str == "cForceVortex" || base.IsKindOf( str );
        }

        public override cVector3 force( cCritter pcritter ) 
		{ 
			_windvector = ( pcritter.Position.sub( _eyeposition )); 
			_windvector.turn( _spiralangle ); 
			return base.force( pcritter ); 
		} 

		
	} 
	
	class cForceObject : cForce 
	{ 
		protected cCritter _pnode; /* Reference to an existing critter you are attracted to or
	 		repelled by. Note that the destructor does NOT delete this.  If _pnode gets
	 		deleted elsewhere, the owner cBiota will find this reference and set it to NULL. */ 
		
		protected cForceObject( cCritter pnode = null )
        {
            _pnode = pnode;
        }

        public virtual cCritter Node
        {
            get
                { return _pnode; }
        }
		
		public override void copy( cForce pforce ) 
		{ 
			base.copy( pforce ); 
			if ( !pforce.IsKindOf( "cForceObject" )) 
				return ; 
			cForceObject pForceObject = ( cForceObject )( pforce );
			_pnode = pForceObject._pnode; //Want a shallow copy
		} 

        public override cForce copy( )
        {
            cForceObject f = new cForceObject();
            f.copy(this);
            return f;
        }

        public override bool IsKindOf( string str )
        {
            return str == "cForceObject" || base.IsKindOf( str );
        }
	
	} 
	
	class cForceObjectSeek : cForceObject 
	{ 
		
		public cForceObjectSeek(){} 
		
     	public cForceObjectSeek(cCritter pnode, float maxacceleration) :
 		    base(pnode) {_intensity = maxacceleration;}

		public override cVector3 force( cCritter pcritter ) 
		{
            if (_pnode == null)
                return new cVector3(0.0f, 0.0f, 0.0f);
			cVector3 pursueforcevector = 
				( pcritter.directionTo( _pnode ).mult( pcritter.MaxSpeed)).sub( pcritter.Velocity ); 
			pursueforcevector.Magnitude = _intensity; 
			pursueforcevector.multassign( pcritter.Mass );
            cVector3 p = new cVector3();
            p.copy( pursueforcevector );
			return p; 
		} 

        public override cForce copy( )
        {
            cForceObjectSeek f = new cForceObjectSeek();
            f.copy(this);
            return f;
        }

        public override bool IsKindOf( string str )
        {
            return str == "cForceObjectSeek" || base.IsKindOf( str );
        }
		
	} 
	
	class cForceClass : cForce 
	{ 
		protected string _pnodeclass; /* Default is RUNTIME_CLASS(cBullet).  Note that the destructor
	 		does NOT delete this; MFC handles clean-up of CRuntimeClass* pointers itself. */ 
		protected bool _includechildclasses; //Whether to also be affected by the child classes of _pnodeclass.
		
		protected cForceClass(string pnodeclass = null, bool includechildclasses = false ) 
        {
 		    _pnodeclass = pnodeclass; 
            _includechildclasses = includechildclasses;
        }
 			/* protected constructors to make this like an abstract class you don't use
 				instances of. */

        public override void copy( cForce pforce ) 
		{ 
			base.copy( pforce ); 
			if ( !pforce.IsKindOf( "cForceClass" )) 
				return ; 
			cForceClass pforcechild = ( cForceClass )( pforce ); 
			_pnodeclass = pforcechild._pnodeclass; 
			_includechildclasses = pforcechild._includechildclasses; 
		} 

        public override cForce copy( )
        {
            cForceClass f = new cForceClass();
            f.copy(this);
            return f;
        }

        public override bool IsKindOf( string str )
        {
            return str == "cForceClass" || base.IsKindOf( str );
        }
		
	} 
	
	class cForceClassEvade : cForceClass 
	{ 
		public static readonly float COSINESMALLANGLE = (float) Math.Cos( Math.PI /40); /* Default cos(PI/40.0).  Move off to the side if the 
			velocity of	the thing chasing you is with a cone of twice this angle size around your the 
			direction towards you.  cos changes very slowly near 0.  	cos(PI/40) is 0.997 and
			cos(PI/7) about 0.9. So PI/40 would mean 2*180/40 degrees or 9 degrees.  Normally
		 	is something like 0.99 or so, but can be any number between -1.0 and 1.0*/ 
		public static readonly float COSINEIGNOREANGLE = -0.8f; /* Default -0.8. Ignore a nearest enemy whose direction
	 		of motion makes an angle less than this with the direction to you.  If I put -1.0
			here that means not to ignore any enemy at all and to be skittish.   If I put 0.0
			here that means ignore any enemy that is not at all moving towards me and be fairly
			bold.  Normally I use something like -0.2, but I can use any number between -1.0 and 1.0.*/ 
		public static readonly uint TURNPERSONALITYBIT = 0x00000020; //A bit of the personality field to use in evading bullets.
				// Remember that you inherit a _pnodeclass and _includechildclasses from cForceClass.
		protected float _dartspeedup; /* A multiplier used for a temporary speed up when fleeing _pnodeclass.
				Gets multiplied times the force-calling pcritter's _maxspeed. */ 
		
		public cForceClassEvade()
        {
            _dartspeedup = 0.0f;
        } 
		
        public cForceClassEvade(float dartacceleration, float dartspeedup, 
            string pnodeclass = null, bool includechildclasses = false ) 
            : base(pnodeclass, includechildclasses)/* Note in this line that since these
 			fields are members of cForceClass, we can't set them in an intializer list with
 			a call like pnodeclass(_pnodeclass).  Members of a parent class can only be set
 			in an initializer list by calling a parent-class constructor. */
        {
            _dartspeedup = dartspeedup; 
            _intensity = dartacceleration;
        }
		
        public override void copy( cForce pforce ) 
		{ 
			base.copy( pforce ); 
			if ( !pforce.IsKindOf( "cForceClassEvade" )) 
				return ; 
			cForceClassEvade pforcechild = ( cForceClassEvade )( pforce ); 
			_dartspeedup = pforcechild._dartspeedup; 
		} 

        public override cForce copy( )
        {
            cForceClassEvade f = new cForceClassEvade();
            f.copy(this);
            return f;
        }
		
        public override bool IsKindOf( string str )
        {
            return str == "cForceClassEvade" || base.IsKindOf( str );
        }

        public override cVector3 force( cCritter pcritter ) 
		{ 
			cVector3 evadedirection, evadeforcevector; 	
			cCritter pclosestcritter = pcritter.OwnerBiota.pickClosestCritter( pcritter, 
				_pnodeclass, _includechildclasses ); 
		//Case (1) No enemies to evade ----------------------------------------------
			if ( pclosestcritter == null ) //NULL if there aren't any of these guys around 
			{ 
				pcritter.restoreMaxspeed(); 	//Go back to my normal non-darting speed and bail.
				return new cVector3(0.0f, 0.0f, 0.0f);
			} 
			evadedirection = pclosestcritter.directionTo( pcritter ); /* Start with a unit vector that 
			points	from my enemy towards me. */ 
			float cosangle = evadedirection.mod( pclosestcritter.Tangent ); /* Dot product of two
				unit vectors is the cosine of the angle between them.  I can use the cosine to figure
				out if the enemy is headed away from me (cosangle < 0) , generally towards me
				(cosangle > 0),	or directly towards me (cosangle near 1.0). */ 
		//Case (2) Closest enemy is moving away from me. ------------------------------
			if ( cosangle < cForceClassEvade.COSINEIGNOREANGLE ) // If nearest enemy is moving away, relax.
			{ 
				pcritter.restoreMaxspeed(); 	//Go back to my normal non-darting speed and bail.
				return new cVector3(0.0f, 0.0f, 0.0f);
			} 
		//Case (3a) Closest enemy is directly towards  me. ------------------------------
			if ( cosangle > cForceClassEvade.COSINESMALLANGLE ) /* If the nearest enemy is heading close
				to straight towards me, I shouldn't run straight away like a rabbit down a railorad
				track.  So in this case I'll head off at a 90 degree angle.  I'll use a	bit of the 
				pointer as a "personality trait" to consistently act like a "righty" or a "lefty" in
				terms of which side of the railroad track I jump off of. */ 
				evadedirection.turn( 
					( (cForceClassEvade.TURNPERSONALITYBIT & pcritter.Personality) != 0 )? 
                        (float) Math.PI / 2 
                        : (float) -Math.PI / 2 ); 
		//Case (3a and 3b) Closest enemy is moving towards me, directly or otherwise.
			pcritter.TempMaxSpeed = _dartspeedup * pcritter.MaxSpeedStandard; 
				//Go faster while I'm darting away.
			evadeforcevector = evadedirection.mult( pcritter.MaxSpeed).sub( pcritter.Velocity); 
			evadeforcevector.Magnitude = _intensity; 
			evadeforcevector.multassign( pcritter.Mass ); 
			return evadeforcevector.mult( _intensity ); 
		} 

		
	} 
	
	class cForceEvadeBullet : cForceClassEvade 
	{ 
		public new const float INTENSITY = 8.0f; //Default 8.0. Rate you accelerate away from an enemy bullet.
		public const float DARTSPEEDUP = 2.0f; //Default 2.0. When fleeing a bullet your maxspeed is multiplied by this.
		
        public cForceEvadeBullet( float dartacceleration = cForceEvadeBullet.INTENSITY, 
            float dartspeedup = cForceEvadeBullet.DARTSPEEDUP, bool includechildclasses = false ) 
            : base( dartacceleration, dartspeedup, "cCritterBullet", includechildclasses ) 
				//Just call the baseclass constructor with args.
		{} 

		
			/* This  force  evades cBullet class objects.  The default FALSE in the fourth 
	 		cForceClassEvade constructor arg means only evade objects whose class is exactly
	 		cBullet such as the player fires, (and not a child class of cBullet, such as the
	 		cCritterBulletSilver that the UFOs fire). If you want something to evade all bullets,
	 		you need feed in a TRUE as the third constructor argument. */ 

        public override cForce copy( )
        {
            cForceEvadeBullet f = new cForceEvadeBullet();
            f.copy(this);
            return f;
        }

        public override bool IsKindOf( string str )
        {
            return str == "cForceEvadeBullet" || base.IsKindOf( str );
        }
	
	} 
	
	class cForceObjectSpringRod : cForceObject 
	{ 
		protected float _rodlength; 
		
        public cForceObjectSpringRod(cCritter pnode = null, float rodlength = 1.0f, 
            float springforce = 1.0f )
		: base(pnode)
        {
            _rodlength = rodlength; 
            _intensity = springforce;
        }

        public override void copy( cForce pforce ) 
		{ 
			base.copy( pforce ); 
			if ( !pforce.IsKindOf( "cForceGravity" )) 
				return ; 
			cForceObjectSpringRod pforcechild = ( cForceObjectSpringRod )( pforce ); 
			_rodlength = pforcechild._rodlength; 
		} 

        public override cForce copy( )
        {
            cForceObjectSpringRod f = new cForceObjectSpringRod();
            f.copy(this);
            return f;
        }
		
        public override bool IsKindOf( string str )
        {
            return str == "cForceObjectSpringRod" || base.IsKindOf( str );
        }

        public override cVector3 force( cCritter pcritter ) 
		{ 
			if ( _pnode == null )
				return new cVector3(0.0f, 0.0f, 0.0f);
            if ( pcritter.distanceTo( _pnode ) < _rodlength ) 
			{ 
				pcritter.moveTo( _pnode.Position.add( 
					_pnode.directionTo( pcritter ).mult( _rodlength )), true ); /* The TRUE arg means
					to allow the motion cause a change in pcritter's _tangent, _normal, etc. */ 
				return new cVector3(0.0f, 0.0f, 0.0f);
			} 
			return _pnode.Position.sub( pcritter.Position).mult( _intensity ); 
		} 

		
	} 
	
	
	//============ 
	class cForceObjectGravity : cForceObject 
	{ 
		
		public cForceObjectGravity(){} 
		
	 	public cForceObjectGravity(cCritter pnode, float gforce) 
            : base(pnode) {_intensity = gforce;}

        public override cVector3 force( cCritter pcritter ) 
		{
            if (_pnode == null)
                return new cVector3( 0.0f, 0.0f, 0.0f );
            cDistanceAndDirection dirdic = 
				pcritter.distanceAndDirectionTo( _pnode ); 
			if ( dirdic._distance < 0.00001f ) 
				return new cVector3(0.0f, 0.0f, 0.0f); 
			float pull = ( _intensity * _pnode.Mass * pcritter.Mass ) / 
				(( dirdic._distance )*( dirdic._distance )); 
			return dirdic._direction.mult( pull ); 
		} 

        public override cForce copy( )
        {
            cForceObjectGravity f = new cForceObjectGravity();
            f.copy(this);
            return f;
        }
		
        public override bool IsKindOf( string str )
        {
            return str == "cForceObjectGravity" || base.IsKindOf( str );
        }

        public override bool isGlobalPhysicsForce() { return true; } 
			/* "Global physics forces" get copied from the
			shooter to its bullets.  So if we return TRUE,
			the gravity pull to the sun is copied to the
			bullets shot by the player and the smart UFOs,
			if we return FALSE the bullets don't have the
			sun gravity. */ 
	} 
}