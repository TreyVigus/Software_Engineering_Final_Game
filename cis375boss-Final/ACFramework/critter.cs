// For AC Framework 1.2, I've added some default parameters -- JC

using System;
using System.Drawing;

// mod:  ZEROVECTOR and others removed

namespace ACFramework
{


    class cCritter
    {
        // Statics ========================================= 
        //Constant Statics ================================= 
        //The MF_ static readonlys are mutation flags used in the mutate methods.
        //They are defined in static readonly.cpp.
        public static readonly int MF_NUDGE = 0x00000001;
        public static readonly int MF_POSITION = 0x00000002;
        public static readonly int MF_VELOCITY = 0x00000004;
        public static readonly int MF_ALL = cCritter.MF_POSITION | cCritter.MF_VELOCITY; //MF_POSITION | MF_VELOCITY 
        //Wrapflag values specify possible behaviors when critter hits edge of world.
        public static readonly int BOUNCE = 0;
        public static readonly int WRAP = 1;
        public static readonly int CLAMP = 2;
        //special high density used for player or other immovable critter.
        public static readonly float INFINITEDENSITY = 1000.0f;
        //Variable Statics ================================= 
        //These might (rarely) be reset by a cGame constructor.
        //Motion Statics ================================= 
        public const float MINSPEED = 0.5f; //Used in randomizing critter _speed.
        protected static float MAXSPEED = 3.0f; // Used in randomizing, and to clamp _speed in move(dt).
        public static readonly float MINTWITCHTHRESHOLDSPEED = 0.0f; //Default for _mintwitchthresholdspeed 
        public const float NEAREDGEPERCENT = 0.15f; // Default arg for moveToMoveboxEdge.
        public static readonly int STARTWRAPFLAG = WRAP;
        public static readonly float DENSITY = 1.0f; //Default density.
        //State Statics ================================= 
        public static readonly float MUTATIONSTRENGTH = 0.6f; //Default argument to mutate method.
        protected static float MINRADIUS = 0.3f; //Used in randomizing 
        protected static float MAXRADIUS = 0.8f;
        protected static float BULLETRADIUS = 0.05f; //Gets set to cGame::BULLETRADIUS in cGame constructor.
        public static readonly float PLAYERRADIUS = 0.4f;
        public static readonly float LISTENERACCELERATION = 10.0f; //Default for _listeneracceleration 
        public static readonly int STARTHEALTH = 1; //Default is 1.
        public static readonly float SAFEWAIT = 0.3f; /* Time in seconds of invulnerability, use at start up and after
			damage, gives critters breathing room so they don't get damaged twice in a row,
			like by the same bullet volley. */
        public static readonly float FIXEDLIFETIME = 3.0f; // Default lifetime for critters with _usefixedlifetime TRUE.
        protected static int _baseAccessControl = 0; // used to access a grandparent base method from
        // a grandchild override, or to access base methods from the constructor chain                
        //================================================ 
        //State Fields. ================================= 
        //================================================ 
        protected float _age; //Measure in seconds of time simulated, start at 0.0 when constructed.
        protected bool _usefixedlifetime; //If TRUE, then die when _age > _fixedlifetime.
        protected float _fixedlifetime; //Max lifetime in seconds, applies only if _usefixedlifetime.
        protected int _health; //Lose by being hit and taking damage().  Usually die when _health is 0.
        protected bool _shieldflag; //Immunity to being damage() calls.
        protected uint _personality; /* Random bits to sometimes use for making critters have different
			behaviors, as when using evasion forces. */
        protected float _mutationstrength; /* Number between 0.0 and 1.0 controlling how different
			a spawned copy will be. */
        protected cCritter _ptarget; /* In case you are following or dragging or watching or aimed at
			someone else, use this field to track them. _ptarget is one of the only fields
			that is NOT serialized. We use the _targetindex with the _pownerbiota to copy
			or serialize _ptarget. */
        //================================================ 
        //Game Fields ================================= 
        //================================================ 
        protected cBiota _pownerbiota; /* Used in makeServiceRequest and in other places.  It allows
		the	critter to be aware of all the other critters. Gets set by
		the cCritter(cGame *pownergame) constructor. _pownerbiota is NOT serialized. */
        protected int _score; //Usually gain by eating or shooting others.
        protected int _value; //Value to another critter shooting or eating this one.
        protected int _newlevelscorestep; //Step size between score levels that are rewarded.
        protected int _newlevelreward; //Health reward for new score level.
        //================================================ 
        //Motion Fields. ================================= 
        //================================================ 
        //Position Fields ================================= 
        protected cVector3 _position;
        protected cRealBox3 _movebox; //Keep critter inside _movebox.
        protected cRealBox3 _dragbox; /* Usually same as _movebox, but in cGamePickNPop, it's bigger, so
			can drag a critter outside of its _movebox. */
        protected int _wrapflag; //BOUNCE, WRAP, or CLAMP when you bump a wall.
        protected int _outcode; // Flag info about which wall, if any, the last move bumped.
        //Velocity Fields ================================= 
        protected bool _fixedflag; //Refuse to move.
        protected cVector3 _velocity;
        protected float _speed;
        protected cVector3 _tangent; /* We always keep _velocity = _speed * _tangent.  It's
			useful to have _tangent around even when _speed goes to 0 and _velocity
			is zero, this way we know what direction to start back up in. */
        protected cVector3 _normal; /* We maintain a _normal and _binormal vector to fully
			express the	motion of the critter through 3D space. */
        protected cVector3 _binormal; //Always cVector::ZAXIS in 2D worlds.
        protected float _curvature; /* This measures the rate at which the tangent is turning,
			specifically, we adjust it so that the change of the tangent vector has
			size _curvature * dt.  See Frenet formula dT/dt = _curvature * N. */
        protected float _maxspeed; //Clamp _speed below this in move().
        protected float _maxspeedstandard; /* In case  _maxspeed might be temporarily increased, for
			instance if the critter is allowed to move extra fast while
			fleeing or chasing another. */
        //Acceleration Mass, and Force Fields ================================= 
        protected cVector3 _acceleration; /* _acceleration gets reset during every cycle, using the
			_forcelist and possibly the _plistener to change it. */
        protected float _mass; //Use fixMass() helper to maintain _mass = _density * radius()^3.
        protected float _density; /* Default is 1.  We often assign the cCritterPlayer a very 
			large _density so that it can whack others around. */
        LinkedList<cForce> _forcelist;
        //Listener Fields. ================================= 
        protected cListener _plistener; //Never NULL.  We serialize the plistener.
        protected float _listeneracceleration; /* This is the acceleration used by listeners such as
			cListenerCar and cListenerSpaceship that "drive" the critter around. Like the
			critter's engine strength. */
        //Collision Fields ================================= 
        protected float _collidepriority;
        /* These are default cCritter _collidepriority values, in increasing size for
            increasingly high priority, where in a pair of critters, the higher priority
            critter is the caller of the collide method, and the lower priority critter
            is the argument to the collide call. */
        protected float _lastcollidepartnerpriority; /* At each fresh call of cGame::collide, I reset all
			the critters' _lastcollidepartnerpriority to cCollider::CP_MINIMUM, and then set to match the 
			_collidepriority of the latest collision allowed.  If I find multiple collisions
			cause problems, I may want to disallow later, lower priority collisions. */
        protected bool _absorberflag; /* Don't change your own velocity after a collision.  This
			siphons energy out of the system, cooling down the motions by absorbing it. */
        protected float _bounciness; /* ranges from 0.0 to 1.0.  Determines how elastically
			you bounce off of walls	or off of other critters.  1.0 is perfect bounce,
			0.9 is pretty reasonable, 0.0  don't bounce at all. */
        protected float _mintwitchthresholdspeed; /* If we have _attitudetomotionlock, 
			and we have some critters barely bouncing on a "floor" it looks bad if they
			keep twitching their orientation up and down.  Don't change the _attitude to
			match the motion if the speed is less than _mintwitchtriggerspeed. */
        //================================================ 
        //Attitude and Display Fields ================================= 
        //================================================ 
        protected cSprite _psprite; //Never NULL.  We serialize the _psprite.
        protected bool _attitudetomotionlock; /* Shall I lock together the display sprite and
			the motion? By default the player has _attitudetomotionlock
			FALSE and all other critters have it TRUE. */
        protected cMatrix3 _attitude; /* The attitude expresses the way that the critter
			is situated for rendering.  When _attitudetomotionlock is
			TRUE, _attitude has the columns	_tangent, _normal, _binormal, _position.
			 If _attitudetomotionlock is FALSE, _attitude can be instead controlled by
			_spin or by the _plistener actions. The _attitude transformation rotates
			to match the standard ijk trihedron to the critter's _tangent-_normal-
			_binormal trihedron, then translates the origin to _position. */
        protected cMatrix3 _inverseattitude; /* If we eventually want to do box-style collisions instead of 
			sphere-style collisions, it will be useful to have the _inverseattitude 
			available to convert global coordinates into the local coordinates of the
			critter. Certainly we'll need this for cCritteWall, and maybe later for
			character critters. We will update this in the cCritter::update method. 
			The _inverseattitude transformation does a translation that moves _position to
			origin and then	rotates to match the standard ijk trihedron to the critter's
			_tangent-_normal-_binormal trihedron. */
        protected cSpin _spin; /* A cSpin holds the spinangle in radians per second and
			the spinaxis which is the axis to spin around (z by default).  Presently
			used only when _attitudetomotionlock is OFF. */
        protected float _defaultprismdz; /* We copy this into the psprite's _prismdz field in setSprite.  
			If we are in 3D and if the sprite is, for instance, a polygon that makes use of 
			the _prismdz field, then _prismdz will determine the z-thickness of the sprite. */
        //================================================ 
        //Bookkeeping Fields ================================= 
        //================================================ 
        //Serialized Bookkeeping Fields ================================= 
        protected float _lasthit_age; //Age at last hit (or age at birth), use to time invulnerability.
        protected bool _oldrecentlydamaged; // Used in update() in connection with sprite display lists.
        protected cVector3 _oldposition; //This is used by the cCritterWall::collide method.
        protected cVector3 _oldtangent; //This is used by the cCritter::fixNormalAndBinormal method.
        protected cVector3 _wrapposition1, _wrapposition2, _wrapposition3; //Use for showing wrap in 2D 
        protected float _lastShield;
        //Nonserialized Bookkeeping Fields ================================= 
//        protected int _metrickey; /* Index into the _pownerbiota cBiota's _metric, can be used to
//			look up metric values. _metrickey is NOT serialized.*/
        //================================================ 
        //Constructor and destructor and helpers ================================= 
        //================================================ 

        public cCritter(cGame pownergame = null)
        {
            _pownerbiota = null;
            _age = 0.0f;
            _lasthit_age = -SAFEWAIT; /* We do this so that critters don't start out
			thinking they were just hit.  cCritter.SAFEWAIT is currently 0.3 seconds. */
            _oldrecentlydamaged = false; //Can use to notice when you need to change sprite.
            _health = STARTHEALTH; //Default 1.
            _usefixedlifetime = false;
            _fixedlifetime = FIXEDLIFETIME;
            _shieldflag = false;
            _outcode = 0;
            _score = 0;
            _newlevelscorestep = 0;
            _newlevelreward = 0;
            _value = 1000;
            _personality = Framework.randomOb.random(); //Use our static readonly randomizing method.
            _movebox = new cRealBox3(4.0f, 3.0f, 0.0f); //Dummy defaults to be reset with setMoveBox 
            _dragbox = new cRealBox3(4.0f, 3.0f, 0.0f); //Dummy defaults to be reset with setDrag 
            _wrapflag = STARTWRAPFLAG; //cCritter.BOUNCE 
            _defaultprismdz = cSprite.CRITTERPRISMDZ;
            _density = DENSITY; //This standard value is currently 1.0.
            _mass = 1.0f; //Dummy default is reset by fixMass.
            _collidepriority = cCollider.CP_CRITTER;
            _lastcollidepartnerpriority = cCollider.CP_MAXIMUM;
            _absorberflag = false;
            _fixedflag = false;
            _position = new cVector3(); // default constructor for cVector3 is zero vector 
            _oldposition = new cVector3();
            _wrapposition1 = new cVector3();
            _wrapposition2 = new cVector3();
            _wrapposition3 = new cVector3();
            _velocity = new cVector3();
            _curvature = 0.0f;
            _speed = 0.0f; //Must match _velocity.magnitude().
            _tangent = new cVector3(1.0f, 0.0f); //We always want some unit vector _tangent.
            _oldtangent = new cVector3(1.0f, 0.0f);
            _normal = new cVector3(0.0f, 1.0f);
            _binormal = new cVector3(0.0f, 0.0f, 1.0f);
            _attitudetomotionlock = true;
            _acceleration = new cVector3(); // zero vector 
            _listeneracceleration = LISTENERACCELERATION;
            _spin = new cSpin(); //_spin is initialized to 0 spinangle around ZAXIS by default constructor 
            _maxspeed = MAXSPEED; //Default 3.0 
            _maxspeedstandard = MAXSPEED;
            _mintwitchthresholdspeed = MINTWITCHTHRESHOLDSPEED;
            _bounciness = 1.0f;
            _mutationstrength = MUTATIONSTRENGTH; //Default 0.6 (out of 1.0 max) 
            _ptarget = null;
//            _metrickey = -1; //Put in a bad index by default 
            _psprite = new cSprite(); /* Let's always have a valid sprite.  The default cSprite looks
			    like a circle, by the way. */
            _plistener = new cListener(); /* For uniformity, always have a valid listener as well.  The
			default listener does nothing.  Don't call setListener(new cListener()) here as this 
			call may have side-effects I don't want yet. */
            _attitude = new cMatrix3();
            _attitude.LastColumn = _position;
            /* The default _attitude constructor has set the matrix to the identity matrix, and it's
            more accurate to the make the fourth column match the position. */
            _inverseattitude = new cMatrix3();
            _forcelist = new LinkedList<cForce>(
                delegate(out cForce f1, cForce f2)
                {
                    f1 = f2.copy(); // for polymorphism
                }
                );
            fixMass();
            _baseAccessControl = 1;
            if (pownergame != null)
                pownergame.add(this, false); /* This call will set _movebox and _dragbox to
				match pownergame->_border, and will set _wrapflag to match pownergame->wrapflag).
				The second argument controls whehter to insert the critter into the game cBiota array
				right away. It's better not to insert right away adn to let the critter finish
				being constructed so that the cCollider.smartAdd will use the properly
				overlaoded form of collidesWith. */
            _baseAccessControl = 0;
        }

        /* Initializes fields, adds to pownergame
            if not null. With the NULL default for the pownergame argument, this  constructor
            doubles	as a no-argument constructor.  */

        /// <summary>
        /// Makes a deep copy of the parameter and places it into the host object.
        /// </summary>
        /// <param name="pcritter">The critter from which the deep copy is made.</param>
        public virtual void copy(cCritter pcritter)
        {
            _psprite = (pcritter.Sprite).copy();
            _age = pcritter._age;
            _lasthit_age = pcritter._lasthit_age;
            _oldrecentlydamaged = pcritter._oldrecentlydamaged;
            _health = pcritter._health;
            _usefixedlifetime = pcritter._usefixedlifetime;
            _fixedlifetime = pcritter._fixedlifetime;
            _shieldflag = pcritter._shieldflag;
            _score = pcritter._score;
            _newlevelscorestep = pcritter._newlevelscorestep;
            _newlevelreward = pcritter._newlevelreward;
            _personality = pcritter._personality;
            _value = pcritter._value;
            _defaultprismdz = pcritter._defaultprismdz;
            _density = pcritter._density;
            _mass = pcritter._mass;
            _collidepriority = pcritter._collidepriority;
            _lastcollidepartnerpriority = pcritter._lastcollidepartnerpriority;
            _absorberflag = pcritter._absorberflag;
            _mutationstrength = pcritter._mutationstrength;
            _wrapflag = pcritter._wrapflag;
            _outcode = pcritter._outcode;
            _movebox.copy(pcritter._movebox);
            _dragbox.copy(pcritter._dragbox);
            _fixedflag = pcritter._fixedflag;
            _position.copy(pcritter._position);
            _oldposition.copy(pcritter._oldposition);
            _oldtangent.copy(pcritter._oldtangent);
            _wrapposition1.copy(pcritter._wrapposition1);
            _wrapposition2.copy(pcritter._wrapposition2);
            _wrapposition3.copy(pcritter._wrapposition3);
            _velocity.copy(pcritter._velocity);
            _curvature = pcritter._curvature;
            _speed = pcritter._speed;
            _tangent.copy(pcritter._tangent);
            _normal.copy(pcritter._normal);
            _binormal.copy(pcritter._binormal);
            _attitude.copy(pcritter._attitude);
            _maxspeed = pcritter._maxspeed;
            _maxspeedstandard = pcritter._maxspeedstandard;
            _mintwitchthresholdspeed = pcritter._mintwitchthresholdspeed;
            _bounciness = pcritter._bounciness;
            _acceleration.copy(pcritter._acceleration);
            _listeneracceleration = pcritter._listeneracceleration;
            _spin.copy(pcritter._spin);
            _attitudetomotionlock = pcritter._attitudetomotionlock;
            _pownerbiota = pcritter._pownerbiota; //Will cause a problem if pasting to a different document.
            copyForcelist(pcritter);
            _ptarget = pcritter._ptarget; // do a shallow copy here to avoid infinite loop! --JC
            //We don't copy the _plistener 
        }

        /// <summary>
        /// Returns a deep copy of the critter.
        /// </summary>
        /// <returns>A deep copy of the critter.</returns>
        public virtual cCritter copy()
        {
            cCritter c = new cCritter();
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
        public virtual bool IsKindOf(string str)
        {
            return str == "cCritter";
        }

        /// <summary>
        /// Deletes references to this critter and calls cBiota.removeReferencesTo(this). It is virtual so that child critter destructors can do extra cleanup before the baseclass destructor. */
        /// </summary>
        public virtual void destruct()
        {
            if (_pownerbiota != null && Game != null)
                Game.removeReferencesTo(this); /* Calls (a)  
				_pbiota->removeReferencesTo(this) to check all critters in the cBiota and
				destroy any cCritter _ptarget or cForceObject _pnode variables that refer to this,
				and	(b) calls pgame()->pcollider()->_pbiota->removeReferencesTo(this) to remove the
				cCollisionPair involving this. */
        }

        /// <summary>
        /// Don't let pdeadcritter be the Target or the Node of any cForceObject in the _forcelist.
        /// </summary>
        /// <param name="pdeadcritter">The critter from which Target and/or Node references need removed.</param>
        public void removeReferencesTo(cCritter pdeadcritter)
        {
            if (Target == pdeadcritter)
                setTarget(null);

            if (_forcelist.Size == 0)
                return;

            cForce f;

            _forcelist.First(out f);

            do
            {
                if (f.IsKindOf("cForceObject"))
                {
                    cForceObject pforcenodal = (cForceObject)f;
                    if (pforcenodal.Node == pdeadcritter)
                        _forcelist.RemoveNext();
                }
            } while (_forcelist.GetNext(out f));
        }

        //================================================ 
        //Mutators ================================= 
        //================================================ 
        //State Field Mutators  ================================= 

        //The velocity, direction, and speed mutators always keep _velocity = _speed * _tangent.

        /// <summary>
        /// Sets a critter to be a target of this critter.  You can then use the Target property to easily access this critter 
        /// (you may also consider using a cForceObject).  
        /// </summary>
        /// <param name="pcritter">The critter to use as the Target.</param>
        public virtual void setTarget(cCritter pcritter) { _ptarget = pcritter; } 
        
        /// <summary>
        /// Resets a critter to an initial state (age = 0, velocity = (0,0,0), position = (0,0,0), health starts over, score = 0, etc.)
        /// </summary>
        public virtual void reset()
        {
            _lasthit_age = -SAFEWAIT;
            _age = 0.0f;
            _health = cCritter.STARTHEALTH;
            _score = 0;
            _position.setZero();
            _velocity.setZero();
            _acceleration.setZero();
            _spin.setZero();
            _speed = 0;
        }

        //Game Field Mutators ================================= 

        //Used in Add and CBiota::Serialize 

        /// <summary>
        /// This adds the score and checks if you reached a new multiple of the _newlevelscorestep value, and if so it adds to your health.  
        /// We do this without bothering to remember the last time you got a health reward, instead we just figure out what int "scorelevel"
		///	you're at by doing int division to get a level value.  
        /// </summary>
        /// <param name="scorechange">The score to add in.</param>
        public virtual void addScore(int scorechange)
        { 
            int oldscore = _score;
            _score += scorechange;
            if (_newlevelscorestep == 0)
                return;
            if (oldscore / _newlevelscorestep < _score / _newlevelscorestep) //int division. Check old level < new level.  
                _health += _newlevelreward;
        }

        /// <summary>
        /// Sets the critter's health.  
        /// </summary>
        /// <param name="health">The health value to set the critter's health to.</param>
        public void setHealth(int health) { _health = health; if (_health < 0) _health = 0; } 

        /// <summary>
        /// Add health points at certain score levels. Or by eating health packs.
        /// </summary>
        /// <param name="healthchange">The additional health to add to current health.</param>
        public void addHealth(int healthchange) { setHealth(_health + healthchange); }

        //================================================ 
        //Motion Field Mutators ================================= 
        //================================================ 
        //Position Field Mutators ================================= 

        /// <summary>
        /// Sets the space around the critter that it will be confined to.
        /// </summary>
        /// <param name="box">The space to confine the critter to.</param>
        /// <returns>Returns the new outcode.</returns>
        public int setMoveBox(cRealBox3 box)
        {
            _movebox = box;
            return clamp();
        }


        //We have a kludge overload for cCritterWall 

        /// <summary>
        /// Moves a critter to a new location.  
        /// /// </summary>
        /// <param name="newposition">The position to move the critter to.</param>
        /// <param name="treatascontinuousmotion">/* Set to false by default.  We only
		///	turn treatascontinuousmotion to true when we are using moveTo in bouncing
		///	off a wall or colliding with another critter, as in these cases we want
		///	the moveTo to be like a physical motion, and thus affect the _tangent,
		///	_normal, and _binormal.  In the default case as in randomizePosition or
		///	in some of the cListener code, we just want to move the cCritter but not
		///	affect its motion trihedron. </param>
        /// <returns>Returns the new outcode.</returns>
        public virtual int moveTo(cVector3 newposition, bool treatascontinuousmotion = false)
        {
            _position = newposition; //Allow this even if _fixedflag is TRUE.
            _outcode = cRealBox3.BOX_INSIDE;
            if (_wrapflag != cCritter.WRAP)
                _outcode = clamp();
            else
                _outcode = _movebox.wrap(_position, _wrapposition1, _wrapposition2,
                    _wrapposition3, Radius);
            if (!treatascontinuousmotion) 
                _oldtangent.copy(_tangent); //So fixNormalAndBinormal doesn't react.
            _attitude.LastColumn = _position; /* So this gets rendered in the right
			position immediately. */
            return _outcode;
        }

        /// <summary>
        /// Moves the critter along the z axis without affecting x or y.
        /// </summary>
        /// <param name="z">The new z value for the critter's location.</param>
        /// <returns>Returns the new outcode.</returns>
        public virtual int moveToZ(float z) { return moveTo(new cVector3(_position.X, _position.Y, z)); }

        /// <summary>
        /// Moves a critter part of the way to a new position.
        /// </summary>
        /// <param name="newposition">The position that the critter moves towards.</param>
        /// <param name="amount">Between 0.0 and 1.0 -- how much of the way you want to move towards newposition.</param>
        /// <param name="treatascontinuousmotion">/* Set to false by default.  We only
        ///	turn treatascontinuousmotion to true when we are using moveTo in bouncing
        ///	off a wall or colliding with another critter, as in these cases we want
        ///	the moveTo to be like a physical motion, and thus affect the _tangent,
        ///	_normal, and _binormal.  In the default case as in randomizePosition or
        ///	in some of the cListener code, we just want to move the cCritter but not
        ///	affect its motion trihedron. </param>
        /// <returns>Returns the new outcode</returns>
        public virtual int moveToProportional(cVector3 newposition, float amount,
            bool treatascontinuousmotion = false)
        {
            if (amount < 0.0f)
                amount = 0.0f;
            else if (amount > 1.0f)
                amount = 1.0f;
            return moveTo(cVector3.mult((1.0f - amount), _position).add(
                cVector3.mult(amount, newposition)), treatascontinuousmotion);
        }

        /// <summary>
        /// Used on a critter to drag it to a new location (if the critter is draggable()).
        /// Will clamp against the _dragbox.  The dt is used to set critter velocity to 
        /// match the drag velocity.  A deep copy of newpos is made, because dragTo 
        /// may possibly change it.
        /// </summary>
        /// <param name="newpos">The position to drag the critter to.</param>
        /// <param name="dt">Change in time between frames; pass in the available dt calculated in ACFramework.cs</param>
        /// <returns>Returns the outcode.</returns>
        public virtual int dragTo(cVector3 newpos, float dt)
        {
            cVector3 newposition = new cVector3();
            newposition.copy(newpos);
            if (!draggable())
                return cRealBox3.BOX_INSIDE;
            /* We aren't going to use this velocity in move, because a dragged critter is a focus critter,
            and will opt out of the move call.  Even so, we store this velocity so that when we 
            release a dragged critter and it is no longer the focus, it will then move with the velocity
            we dragged it at. */
            _oldtangent.copy(_tangent); //So fixNormalAndBinormal doesn't react.
            /* I'm going to allow for the possibility that I have a 3D creature in
        a 2D game world, as when I put a cSpriteQuake into a board game like 
        DamBuilder.  When I drag the walls, I still want them to be positioned
        so their butts are sitting where I put them initially, like in the xy
        plane or a little bit below.  I don't want to change the z, in other words.
        So I'll run the in3DWorld test on the pgame->border().zsize as opposed to
        on the _movebox.zsize(). */
            if (dt > 0.00001f) //Don't divide by 0.
                Velocity = (newposition.sub(_position)).mult(1.0f / dt);
            _position = newposition;
            int outcode = clamp(_dragbox);
            _attitude.LastColumn = _position; /* So this gets rendered in the right
			position immediately. */
            return outcode;
        }

        /// <summary>
        /// Useful in some games, to start a critter near the _movebox edge.  The position is randomized.  Can possibly fail
        /// if the randomization does not produce an acceptable location after 100 attempts.
        /// </summary>
        /// <param name="nearedgepercent">The percentage (as a decimal) of _movebox.MinSize used for the distance from the movebox edge.
        /// NEAREDGEPERCENT by default.</param>
        public void moveToMoveboxEdge(float nearedgepercent = NEAREDGEPERCENT)
        {
            int safetycount = 100; /* Use a counter like this when you have a possibly endless
			loop to ensure that you get out of the loop even if the task fails. */
            cRealBox3 centralbox = _movebox.innerBox(
                nearedgepercent * _movebox.MinSize);
            while (centralbox.inside(Position) && safetycount > 0)
            {
                safetycount--;
                randomizePosition();
            }
        }


        //Velocity Field Mutators ================================= 



        /// <summary>
        /// Adds on to the current velocity
        /// </summary>
        /// <param name="velocitychange">The velocity to add to the current velocity</param>
        public void addVelocity(cVector3 velocitychange) { Velocity = _velocity.add(velocitychange); }

        /// <summary>
        /// Changes the direction that the critter is facing or moving in while keeping the same velocity.
        /// </summary>
        /// <param name="spin">The amount to rotate the tangent (current direction) by. cSpin is a way to express general 3D angles.</param>
        public void rotate(cSpin spin)
        {
            _tangent.rotate(spin);
            _velocity = _tangent.mult(_speed);
            _normal.rotate(spin);
            _binormal.rotate(spin);
            _oldtangent.copy(_tangent); //This signals fixNormalAndBinormal not to react to this.
        }

        /// <summary>
        /// Rotates around the binormal, changing the current direction that the critter is facing or moving in.
        /// </summary>
        /// <param name="turnangle">The angle (in radians) to use for the rotation around the binormal.</param>
        public void yaw(float turnangle) //Means rotate aroudn binormal axis.
        {
            cSpin diryaw = new cSpin(turnangle, _binormal);
            _tangent.rotate(diryaw);
            _velocity = _tangent.mult(_speed);
            _normal.rotate(diryaw);
            _oldtangent.copy(_tangent); //This signals fixNormalAndBinormal not to react to this.
            updateAttitude(0.0f, true); //Force the attitude to reflect this change if _attitudetomotionlock.
        }

        /// <summary>
        /// Rotates around the tangent, or current direction the critter is moving in.
        /// </summary>
        /// <param name="turnangle">The angle (in radians) to use for the rotation around the tangent.</param>
        public void roll(float turnangle)
        {
            cSpin rollturn = new cSpin(turnangle, _tangent);
            _binormal.rotate(rollturn);
            _normal.rotate(rollturn);
            _oldtangent.copy(_tangent); //This signals fixNormalAndBinormal not to react to this.
            updateAttitude(0.0f, true); //Force the attitude to reflect this change if _attitudetomotionlock.
            //	TRACE("rolled turnangle %f binormal %f %f %f\n", turnangle, _binormal.x(), _binormal.y(), _binormal.z()); 
        }

        /// <summary>
        /// Rotates around the normal vector, changing the tangent (current direction) the critter is facing or moving in.
        /// </summary>
        /// <param name="turnangle">The angle (in radians) to use for the rotation around the normal vector.</param>
        public void pitch(float turnangle)
        {
            cSpin pitchturn = new cSpin(turnangle, _normal);
            _tangent.rotate(pitchturn);
            _velocity = _tangent.mult(_speed);
            _binormal.rotate(pitchturn);
            _oldtangent.copy(_tangent); //This signals fixNormalAndBinormal not to react to this.
            updateAttitude(0.0f, true); //Force the attitude to reflect this change if _attitudetomotionlock.
        }

        /// <summary>
        /// Make sure _tangent, _normal, and _binormal are orthogonal units.
        /// </summary>
        public void orthonormalize()
        {
            _tangent.normalize();
            _normal.subassign(_tangent.mult(_normal.mod(_tangent))); //Make it perpendicular to _tangent.
            _normal.normalize(); //Make it a unit vector.
            _binormal = _tangent.mult(_normal);
            _oldtangent.copy(_tangent);
        }

        /// <summary>
        /// Restores the maximum speed of the critter (to _maxspeedstandard)
        /// </summary>
        public void restoreMaxspeed() { _maxspeed = _maxspeedstandard; }
        //Acceleration, Mass and Force Field Mutators ================================= 

        /// <summary>
        /// Adds acceleration to the current acceleration.
        /// </summary>
        /// <param name="acceleration">The amount of acceleration to add to the current acceleration.</param>
        public void addAcceleration(cVector3 acceleration) { _acceleration.addassign(acceleration); }

        /// <summary>
        /// Keep _mass = _density * _radius()^3
        /// </summary>
        public void fixMass()
        {
            if (Radius < 0.00001f)
                _mass = 1.0f; //Don't allow a zero mass() as we divide by it in force().
            else _mass = _density * (float)Math.Pow(Radius, 3); /* We will think of our critters as
			three-dimensional objects, as this seems to give more natural looking motion.
			You could use 2 here instead to make them more like disks. */
        }

        /// <summary>
        /// Adds a new force to the force list.
        /// </summary>
        /// <param name="pforce">The force to add to the force list.</param>
        public void addForce(cForce pforce)
        {
            _forcelist.Add(pforce);
        }

        /// <summary>
        /// Removes all forces from the force list.
        /// </summary>
        public void clearForcelist()
        {
            _forcelist.RemoveAll();
        }

        /// <summary>
        /// Changes the force list to another critter's force list.  Empties the existing force list and 
        /// copies all of the forces in the pcritter force list.
        /// </summary>
        /// <param name="pcritter">The critter whose force list is to be used for this critter.</param>
        public void copyForcelist(cCritter pcritter)
        {
            _forcelist.RemoveAll();
            foreach (cForce f in pcritter.ForceList)
                addForce(f);
        }

        /// <summary>
        /// A more modest kind of force copying. Here we don't wipeout the existing forces in the caller, and we only 
        /// copy the "physics" forces like cForceGravity and cForceDrag from pcritter.  Use the bool cForce.isGlobalPhysicsForce() 
        /// to tell us which ones.  We need this method so that bullets can copy the physics of their shooters but not their 
        /// behavioral forces.
        /// </summary>
        /// <param name="pcritter">The critter from which to copy the "physics" forces.</param>
        public virtual void copyPhysicsForces(cCritter pcritter)
        {
            foreach (cForce f in pcritter.ForceList)
            {
                if (f.isGlobalPhysicsForce())
                    addForce(f);
            }
        }


        //================================================ 
        //Attitude and Display Field Mutators ================================= 
        //================================================ 

        
        /// <summary>
        /// Sets the spin to a cSpin object made with a spinvector.  A cSpin holds the spinangle in radians per second 
        /// and the spinaxis which is the axis to spin around (z by default).  Presently 
        /// used only when _attitudetomotionlock is OFF.  
        /// </summary>
        /// <param name="spinvector">The vector used to make a cSpin object.  The spinangle (in radians per second)
        /// is determined by the Magnitude of the spinvector.</param>
        public void setSpin(cVector3 spinvector) { _spin = new cSpin(spinvector); }

        /// <summary>
        /// Sets the spin around the z axis.  Presently 
        /// used only when _attitudetomotionlock is OFF.  
        /// </summary>
        /// <param name="spinangle">The spin angle to use (in radians per second) around the z axis.</param>
        public void setSpin(float spinangle)
        { _spin = new cSpin(spinangle, new cVector3(0.0f, 0.0f, 1.0f)); }

        /// <summary>
        /// Sets the spin around a spin axis at a spin angle (in radians per second).  Presently used only 
        /// when _attitudetomotionlock is OFF.
        /// </summary>
        /// <param name="spinangle">The angle (in radians per second) to spin the critter by.</param>
        /// <param name="spinaxis">The axis that the critter will spin on.</param>
        public void setSpin(float spinangle, cVector3 spinaxis)
        { _spin = new cSpin(spinangle, spinaxis); }

        /// <summary>
        /// This changes the orientation aspect of _attitude, but NOT the _position aspect, that is,
        /// it leaves the last column alone.
        /// </summary>
        /// <param name="angle">This angle will be used to make a cSpin object, to be used 
        /// in rotating the attitude.</param>
        public void rotateAttitude(float angle)
        {
            rotateAttitude(new cSpin(angle));
        }

        /// <summary>
        /// This changes the orientation aspect of _attitude, but NOT the _position aspect, that is,
        /// it leaves the last column alone.
        /// </summary>
        /// <param name="spin">This spin will be used in the cMatrix3.rotation function, and _attitude
        /// will be multiplied (and changed) by the result.</param>
        public void rotateAttitude(cSpin spin)
        {
            /* When we rotate the attitude, we right multiply the rotation times the _attitude, so the fact
            that the _position is in the _attitude doesn't matter. */

            _attitude.multassign(cMatrix3.rotation(spin));
        }

        /// <summary>
        /// Assume the identity orientation.  Will not affect position.
        /// </summary>
        public void resetAttitude()
        {
            Attitude = cMatrix3.identityMatrix(); /* Note that setAttitude leaves _position in the last column, 
			so this doesn't	change position. */
        }

        /// <summary>
        /// Copies _tangent, _normal, _binormal, and _position into _attitude
        /// </summary>
        public void copyMotionMatrixToAttitudeMatrix()
        {
            _attitude = new cMatrix3(_tangent, _normal, _binormal, _position);
            _attitude.orthonormalize(); //To be safe.
        }

        /// <summary>
        /// Copies _attitude into _tangent, _normal, _binormal, and _position
        /// </summary>
        public void copyAttitudeMatrixToMotionMatrix()
        {
            _attitude.orthonormalize(); //To be safe.
            _tangent = AttitudeTangent;
            _velocity = cVector3.mult(_speed, _tangent);
            _normal = AttitudeNormal;
            _binormal = AttitudeBinormal;
        }

        /// <summary>
        /// Aim attitudeTangent at a target position and try and perverse attitudeNormal.
        /// </summary>
        /// <param name="targetpos">The point to look at (the target position).</param>
        /// <returns>Returns false if the targetpos is right on top of you, 
        /// preventing you from looking at it.  Otherwise, returns true.</returns>
        public bool lookAt(cVector3 targetpos)
        {
            cVector3 newtangent = targetpos.sub(Position);
            if (newtangent.IsPracticallyZero) //You can't look at this as it's on top of you.
                return false;
            newtangent.normalize();
            cVector3 temporaryup = newtangent.mult(AttitudeNormal);
            if (temporaryup.IsPracticallyZero)
                AttitudeTangent = newtangent;
            else
            {
                cVector3 newnormal = temporaryup.mult(newtangent);
                newnormal.normalize();
                Attitude = new cMatrix3(newtangent, newnormal, newtangent.mult(newnormal), _position);
            }
            return true;
        }

        /// <summary>
        /// Aims attitudeTangent part of the way towards a target position.
        /// </summary>
        /// <param name="targetpos">The point to turn towards.</param>
        /// <param name="amount">Between 0.0 and 1.0, specifiying how far towards targetpos
        /// you turn to look.</param>
        /// <returns>Returns false if the targetpos is right on top of you.
        /// Otherwise, returns true.</returns>
        public bool lookAtProportional(cVector3 targetpos, float amount)
        {
            if (amount < 0.0f)
                amount = 0.0f;
            else if (amount > 1.0f)
                amount = 1.0f;
            cVector3 targettangent = (targetpos.sub(_position)).normalize();
            cVector3 weightedlookpos = _position.add(
                AttitudeTangent.mult(1.0f - amount).add(targettangent.mult(amount)));
            return lookAt(weightedlookpos);
        }


        /// <summary>
        /// Sets the radius (size) of the critter.
        /// </summary>
        /// <param name="radius">The size to set the critter to.</param>
        /// <returns>Returns the outcode.</returns>
        public int setRadius(float radius)
        {
            _psprite.Radius = radius;
            fixMass();
            return clamp();
        }


        //================================================ 
        //Randomizing mutators ================================= 
        //================================================ 

        /// <summary>
        /// Moves the critter to a random position inside a box.
        /// </summary>
        /// <param name="startbox">The box to use for the random position.</param>
        public void randomizePosition(cRealBox3 startbox)
        {
            cVector3 newposition = startbox.randomVector();
            moveTo(newposition); //this clamps to fit in the startbox if no wrap.
            //	if (_psprite) 
            //		_psprite->moveTo(_position); 
            /* Need this becasue we abruptly move this way and sprite isn't caught up.
        If you don't have this line, then a reseed shows the sprites at middle of screen 
        for one update. */
        }

        /// <summary>
        /// Moves critter to a random position.
        /// </summary>
        public void randomizePosition() { randomizePosition(_movebox); }

        /// <summary>
        /// Produces a random size (radius) for the critter.
        /// </summary>
        /// <param name="minradius">The lowest random size the critter can take on.</param>
        /// <param name="maxradius">The highest random size the critter can take on.</param>
        public void randomizeRadius(float minradius, float maxradius)
        { setRadius(Framework.randomOb.randomReal(minradius, maxradius)); }

        /// <summary>
        /// Produces a random velocity for the critter.
        /// </summary>
        /// <param name="minspeed">The minimum speed for the random velocity.</param>
        /// <param name="maxspeed">The maximum speed for the random velocity.</param>
        /// <param name="force2D">If set to true, will only randomize the velocity in the XY plane (Z set to zero).  Is false by default.</param>
        public void randomizeVelocity(float minspeed, float maxspeed, bool force2D = false)
        {
            cVector3 randomunitvector = cVector3.randomUnitVector();
            if (force2D)
            {
                randomunitvector.Z = 0.0f;
                randomunitvector.normalize();
            }
            Velocity = cVector3.mult(Framework.randomOb.randomReal(minspeed, maxspeed), randomunitvector);
        }

        /// <summary>
        /// Produces a random velocity up to the _maxspeed of the critter.
        /// </summary>
        /// <param name="speed">The minimum speed for the random velocity (set to MINSPEED by default).</param>
        public void randomizeVelocity(float speed = MINSPEED) { randomizeVelocity(speed, _maxspeed); }

        /// <summary>
        /// Produces a random spin on the critter (the speed and axis of rotation are both randomized).
        /// </summary>
        /// <param name="minspeed">The minimum speed for the random spin.</param>
        /// <param name="maxspeed">The maximum speed for the random spin.</param>
        public void randomizeSpin(float minspeed, float maxspeed)
        {
            _spin = new cSpin(Framework.randomOb.randomReal(minspeed, maxspeed),
                cVector3.randomUnitVector()); //cSpin constuctor takes spinangle, spinaxis args.
        }

        /// <summary>
        /// Mutates flagged position, velocity, and sprite properties.
        /// </summary>
        /// <param name="mutationflags">Indicates the types of mutations to be performed.  To mutate position, 
        /// do a bitwise |= with MF_POSITION.  To mutate velocity, do a bitwise |= with MF_VELOCITY.
        /// To perform a weak mutation, do a bitwise |= with MF_NUDGE. </param>
        /// <param name="mutationstrength">The strength of the mutation for the sprite.</param>
        public virtual void mutate(int mutationflags, float mutationstrength)
        { //Note that cCritterWall has its own version of this method.
            if ((mutationflags & MF_NUDGE) != 0) //Special kind of weak mutation 
            {
                float turnangle = Framework.randomOb.randomSign() *
                    Framework.randomOb.randomReal((float)-Math.PI / 2, (float)Math.PI / 2);
                _velocity.turn(turnangle);
                _velocity.multassign(Framework.randomOb.randomReal(0.5f, 1.5f));
                randomizePosition(RealBox); /* cCritter.realBox() is the
				smallest box holding the current sprite. */
            }
            if ((mutationflags & MF_POSITION) != 0)
                randomizePosition(_movebox);
            if ((mutationflags & MF_VELOCITY) != 0)
                randomizeVelocity(cCritter.MINSPEED, _maxspeedstandard);
            _psprite.mutate(mutationflags, mutationstrength);
            fixMass(); // In case the radius got changed.
        }

        /// <summary>
        /// Mutates flagged position, velocity, and sprite properties.  The strength of the mutation is
        /// determined by _mutationstrength.
        /// </summary>
        /// <param name="mutationflags">Indicates the types of mutations to be performed.  To mutate position, 
        /// do a bit-wise |= with MF_POSITION.  To mutate velocity, do a bitwise |= with MF_VELOCITY.
        /// To perform a weak mutation, do a bitwise |= with MF_NUDGE.</param>
        public void mutate(int mutationflags) { mutate(mutationflags, _mutationstrength); }
        //Uses the member _mutationstrength, which defaults to 0.6.

        /// <summary>
        /// Mutates flagged position, velocity, and sprite properties.  The strength of the mutation is
        /// 1.0.
        /// </summary>
        /// <param name="mutationflags">Indicates the types of mutations to be performed.  To mutate position, 
        /// do a bit-wise |= with MF_POSITION.  To mutate velocity, do a bitwise |= with MF_VELOCITY.
        /// To perform a weak mutation, do a bitwise |= with MF_NUDGE.</param>
        public void randomize(int mutationflags) { mutate(mutationflags, 1.0f); } //1.0 is maximum.

        /// <summary>
        /// Determines if the critter was recently damaged by taking the current age and subtracting
        /// the age of the last hit.  If this is less than SAFEWAIT, the critter was recently damaged.
        /// </summary>
        /// <returns>Returns true if recently damaged.  Otherwise, returns false.</returns>
        public bool recentlyDamaged() { return (_age - _lasthit_age) < SAFEWAIT; }
        //Game Field Accessors ================================= 

        /// <summary>
        /// Used to see if a critter is willing to be dragged.
        /// </summary>
        /// <returns>Returns true.  (If you ever want a non-draggable critter child class, overload
        /// draggable to return false.)</returns>
        public virtual bool draggable() { return true; } 

        /// <summary>
        /// Makes a service request to _pownerbiota
        /// </summary>
        /// <param name="request">Valid requests are "add_me", "delete_me", "spawn", "zap", and "recplicate"</param>
        public void makeServiceRequest(string request)
        { /* This is one place we use _pownerbiota, other than the destructor  */
            _pownerbiota.addServiceRequest(new cServiceRequest(this, request));
        }

        /// <summary>
        /// Make a request to the pownerbiota to add yourself to its list, normally this doesn't happen
        /// until pownerbiota makes a periodic call to processServiceRequests, but you
        /// can force it to be immediate with immediateadd. 
        /// </summary>
        /// <param name="pownerbiota">The biota object for which to make the request.</param>
        /// <param name="immediateadd">False by default.  If set to true, it forces the pownerbiota
        /// object to make the request right away. </param>
        public void add_me(cBiota pownerbiota, bool immediateadd = false)
        {
            Owner = pownerbiota;
            makeServiceRequest("add_me");
            if (immediateadd)
                pownerbiota.processServiceRequests();
        }

        /// <summary>
        /// Make a request to _pownerbiota to delete yourself from its list. 
        /// </summary>
        public void delete_me() { _health = 0; makeServiceRequest("delete_me"); }

        /// <summary>
        /// Currently nonfunctional.
        /// </summary>
        public void spawn() { makeServiceRequest("spawn"); }

        /// <summary>
        /// Makes a request to _pownerbiota to randomly change the size, velocity, orientation and/or other features of the critter.
        /// </summary>
        public void zap() { makeServiceRequest("zap"); }

        /// <summary>
        /// Currently nonfunctional.
        /// </summary>
        public void replicate() { makeServiceRequest("replicate"); } //copy yourself to all the others.
        //Helper Methods for Move Methods ================================= 

        /// <summary>
        /// Clamps against the _movebox.
        /// </summary>
        /// <returns>Returns the outcode.</returns>
        public virtual int clamp()
        {
            return clamp(_movebox);
        }

        /// <summary>
        /// Clamps against border.
        /// </summary>
        /// <param name="border">The border to clamp against.</param>
        /// <returns>Returns the outcode.</returns>
        public virtual int clamp(cRealBox3 border)
        { //Clamp against border 
            cRealBox3 effectivebox = border.innerBox(Radius);
            int outcode = effectivebox.clamp(_position);
            _wrapposition1.copy(_position);
            _wrapposition2.copy(_position);
            _wrapposition3.copy(_position);
            return outcode;
        }

        public virtual void addvelocityandcheckedges(float dt)
        { // This helper method is only called by cCritter.move.
            cRealBox3 effectiveborder = _movebox.innerBox(Radius);
            if (_wrapflag == cCritter.BOUNCE)
            {
                _outcode = effectiveborder.addBounce(_position, ref _velocity, _bounciness, dt); /* The _position and
				_velocity arguments are passed as non-constant references, and may both be changed.
				Due to a possibly damped bounce (if _bounciness is less than 1.0), the newvelocity's
				speed can also change. */
                synchSpeedAndDirectionToVelocity();
            }
            else if (_wrapflag == cCritter.WRAP)
            {
                _position.addassign(_velocity.mult(dt));
                _outcode = _movebox.wrap(_position, _wrapposition1, _wrapposition2,
                    _wrapposition3, Radius);
                if (_outcode != 0)
                    //	_oldposition = _position; 
                    /* I used to do this because otherwise a wrap can make it look like a 
                critter moved through a wall because the outcodes of the two positions
                change. But when I upgdraded my cCritterWall.collide in May, 2003, 
                BUGFIX this broke the wall collide code!!!! */
                    _oldposition = _position.sub(_velocity.mult(dt)); /* This way the old position is
					in a correct orietnation relative to the wrapped position, even though
					the old position will probably be outside the _movebox */
            }
            else //_wrapflag == cCritter.CLAMP 
            {
                _position.addassign(_velocity.mult(dt));
                _outcode = effectiveborder.clamp(_position, _velocity);
                synchSpeedAndDirectionToVelocity();
            }
        }

        /* do _position += dt*_velocity,
            and clamp, wrap, or bounce the  new position off the _movebox.  Set _outcode to
            tell which edges. Called by move(). Need the dt to figure out a velocity bounce. */

        /// <summary>
        /// Enforces _speed * _tangent = _velocity and avoids having _speed less than SMALL_REAL.
        /// </summary>
        public void synchSpeedAndDirectionToVelocity()
        { //This sets _speed and _tangent to match _velocity 
            _speed = _velocity.Magnitude;
            if (_speed > 0.00001f) /*  Don't allow small positive speeds as they cause wobble,
				as when a mass is sitting on a border slightly bouncing. */
                _tangent = _velocity.mult(1.0f / _speed);
            else
                _speed = 0.0f;
        }

        public void fixNormalAndBinormal()
        { // This helper method is called by cCritter.move, by cCritter.setVelocity and cCritter.setTangent.
            // Assume _tangent and _oldtangent are unit vectors.  Guess newnormal as the difference.
            cVector3 tangentchange = _tangent.sub(_oldtangent);
            //If _tangent has changed very little or none, just orthonormalize.
            if (tangentchange.IsPracticallyZero) //Nothing major has changed.
            {
                _curvature = 0.0f;
                orthonormalize();
                return;
            }
            /* Otherwise we've made a substantial change in _tangent.  We could try and preserve the direction
        of the normal as an "up", but instead we'll try and keep the normal pointing in the direction of the
        latest turn, that is, guess that newnormral is (_tangent-olddirection).normalize(). */
            tangentchange.subassign(_tangent.mult(tangentchange.mod(_tangent))); //Make it perpendicular to _tangent.
            _curvature = tangentchange.normalizeAndReturnMagnitude();
            /*Make it a unit vector.  Also save the size of it as your curvature. */
            float howparallel = tangentchange.mod(_normal);
            //Case where tangentchange is close to _normal 
            if (howparallel > cVector3.PRACTICALLY_PARALLEL_COSINE)
                /* If howparallel is near 1, then the vectors are
                nearly the same, so we comfortably will set _normal
                to tangentchange. */
                _normal = tangentchange;
            //Case where tangentchange is close to -_normal 
            else if (howparallel < -cVector3.PRACTICALLY_PARALLEL_COSINE)
                /* If howparallel is near -1, means _normal and
                tangentchange are pointing almost in opposite directions,
                which can happen if the critter has changed the direction
                its path is turning in.  We don't want to do an abrupt 180 degree
                flip in the normal as this will make a discontinuous flip in the
                binormal, so in this case we abandon our desire to have normal 
                pointing in the direction of the path's bending, and are satisfied
                to have it point in the opposite direciton of the bend, and nearly
                in the same direction as before. */
                _normal = tangentchange.neg();
            //Hard case, where tangentchange is quite diffrent from _normal.
            else
            /* The case where _normal and tangentchange have a medium dot product.
        This means not parallel, not antiparallel,  might
        happen if you have a zero tangentchange, or it's flipped by
        a bounce or a discontinous direction change like a
        mouse drag or a velocity impulse, and then we use a different
        method to find a new value for _normal. */
            {
                _curvature = 0.0f;
                /* In this case its better not to try and guess at the cuvature */
                //Make sure everything's kosher coming into the rotation.
                _tangent.normalize();
                _oldtangent.normalize();
                _normal.normalize();
                _normal.subassign(_oldtangent.mult(_normal.mod(_oldtangent))); //Make it perpendicular to _oldtangent.
                _normal.normalize(); //Make it a unit vector.
                cMatrix3 turnmatrix = cMatrix3.rotationFromUnitToUnit(
                    _oldtangent, _tangent);
                /* This is a computation-intensive method in 3D,
             so we don't call it unless we have to. */
                _normal = turnmatrix.mult(_normal);
                //Make sure everything's kosher coming out of the rotation.
                _normal.subassign(_tangent.mult(_normal.mod(_tangent))); //Make it perpendicular to _tangent.
                _normal.normalize(); //To be safe.
            }
            cVector3 oldbinormal = _binormal; //Save this for use in the NOFLIPBINORMAL code.
            _binormal = _tangent.mult(_normal); //Fix the binormal.
            _binormal.normalize();

            // Don't allow this method to unexpectedly flip the binormal.  Always call this to be safe.
            if ((_binormal.add(oldbinormal)).IsPracticallyZero) //You flipped over. Undo it.
            { //By the way, you don't want to use this test in the 2D case, as both vects will be 0.
                _binormal.multassign(-1.0f);
                _normal = _binormal.mult(_tangent);
            }
            _oldtangent.copy(_tangent); 	/* We're done with _tangent and _oldtangent for now.  So we 
			save the current _tangent. The fixNormalAndBinormal method won't 
			get called again till the end of the next move method call, so whatever happens to _tangent
			will be accumulated and taken into account, whether the changes come from forces, from 
			listeners, from collisions or from the next move. */
        }

        /* This is easy in 2D, subtler in 3D.  Call this from
            inside move on every update.  It also orthonormolizes
            _tangent, _normal, and _binormal. */


        //================================================ 
        //Distance, touch, sniff, collide methods ================================= 
        //================================================ 

        /// <summary>
        /// Finds the direction to a critter from this critter.  
        /// </summary>
        /// <param name="pcritter">The critter to determine a direction towards.</param>
        /// <returns>Returns the direction as a cVector3.</returns>
        public virtual cVector3 directionTo(cCritter pcritter)
        {
            return _pownerbiota.direction(this, pcritter); //Uses the cMetricCritter in cBiota	 
        }

        /// <summary>
        /// Finds the distance to a critter from this critter.
        /// </summary>
        /// <param name="pcritter">The critter to determine the distance to.</param>
        /// <returns>Returns the distance.</returns>
        public float distanceTo(cCritter pcritter)
        {
            return _pownerbiota.distance(this, pcritter); 
        }

        /// <summary>
        /// Finds the distance to a line (at a right angle to the line).
        /// </summary>
        /// <param name="testline">The line object to determine the distance towards.</param>
        /// <returns>Returns the distance.</returns>
        public float distanceTo(cLine testline) { return testline.distanceTo(_position); } //Direct 

        /// <summary>
        /// Finds the distance and direction to a critter from this critter.
        /// </summary>
        /// <param name="pcritter">The critter to determine the distance and direction towards.</param>
        /// <returns>Returns the distance and direction as a cDistanceAndDirection object.</returns>
        public cDistanceAndDirection distanceAndDirectionTo(cCritter pcritter)
        {
            return _pownerbiota.distanceAndDirection(this, pcritter);
        }

        /// <summary>
        /// Determines the distance to a point from this critter.
        /// </summary>
        /// <param name="vpoint">The point to determine the distance towards.</param>
        /// <returns>Returns the distance.</returns>
        public float distanceTo(cVector3 vpoint)
        {
            return _position.distanceTo(vpoint); /*Use the cVector function for now,
			but later we should adjust this to be different if _wrapflag is on. */
        }

        /// <summary>
        /// Determines whether or not this critter is touching a point (by using the Radius of the critter)
        /// </summary>
        /// <param name="vpoint">The point of touch.</param>
        /// <returns>Returns true if touching the point; returns false otherwise.</returns>
        public virtual bool touch(cVector3 vpoint)
        {
            return distanceTo(vpoint) < Radius;
        }

        /// <summary>
        /// Determines whether or not this critter is touching a line (by using the Radius of the critter).
        /// Note that in 3D, clicking the screen really picks a line of sight rather than a particular point
        /// in space, so it may be utilized in mouse clicks.
        /// </summary>
        /// <param name="sightline">The line of touch.</param>
        /// <returns>Returns true if the line is touched; returns false otherwise.</returns>
        public virtual bool touch(cLine sightline)
        {
            return sightline.distanceTo(Position) < Radius;
        }

        /// <summary>
        /// Determines whether this critter is touching another critter by using both critters' radii.
        /// (Uses cBiota's cMetricCritter)
        /// </summary>
        /// <param name="pcritter">The critter of touch.</param>
        /// <returns>Returns true if pcritter is different from this and the distance between the
        /// centers is less than the sum of the radii; otherwise returns false.</returns>
        public virtual bool touch(cCritter pcritter)
        {
            return (pcritter != this) && (distanceTo(pcritter) < Radius + pcritter.Radius);
        }

        /// <summary>
        /// Checks to see if a critter is inside this critter.
        /// </summary>
        /// <param name="pcritter">The critter which may be inside.</param>
        /// <returns>Returns true if the disk of pcritter is inside the disk of this critter (using the Radius of the disks); 
        /// otherwise returns false.</returns>
        public virtual bool contains(cCritter pcritter)
        {
            return distanceTo(pcritter) + pcritter.Radius < Radius;
        }


        public virtual int collidesWith(cCritter pcritterother)
        {
            /* I only call this within cCollider.smartAdd, which is called in such a way to
        ensure the ASSERT condition.  But I check it for testing. */
            float othercollidepriority = pcritterother.CollidePriority;
            if (_fixedflag && pcritterother.FixedFlag)
                return cCollider.DONTCOLLIDE;
            if (_collidepriority == othercollidepriority)
                return cCollider.COLLIDEEITHERWAY;
            if (_collidepriority > othercollidepriority)
                return cCollider.COLLIDEASCALLER;
            //else (_collidepriority < othercollidepriority) 
            return cCollider.COLLIDEASARG;
        }

        /* Returns cCollider::DONTCOLLIDE,
            ::COLLIDEASCALLER,	or ::COLLIDEASARG to specify which of the pair, if either, gets
            to call for a collision. Default just checks _fixedflag and _collidepriority. */

        
        /// <summary>
        /// Does a physically natural collision and possibly overloads to make the critters react in some other way such as damage.
        /// </summary>
        /// <param name="pother">The critter used to check for a collision with this critter.</param>
        /// <returns>Returns true if a collision took place; otherwise returns false.</returns>
        public virtual bool collide(cCritter pother)
        {
            cVector3 toOther = new cVector3();
            float distanceToOther;
            float givecomponent;
            cVector3 give;
            float receivecomponent;
            cVector3 receive;
            float massratio;
            cVector3 contactpoint;

            if (pother == this)
                return false;
            cDistanceAndDirection distanceanddirection = distanceAndDirectionTo(pother);
            distanceToOther = distanceanddirection._distance;
            if (distanceToOther > Radius + pother.Radius)
                return false;
            toOther.copy(distanceanddirection._direction);
            givecomponent = (_velocity.mod(toOther));
            give = toOther.mult(givecomponent);
            receivecomponent = (pother._velocity).mod(toOther);
            receive = toOther.mult(receivecomponent); //else leave it at 0.
            /* We think of the calling critter as having a mass of 1, so we
        give the other critter a proportional to the cube or the square of the radius 
        ratios.  Either one works pretty well, though the cube seems to look more realistic,
        that is, looks more like what we're used to seeing. */
            if (Radius == 0.0f) //Check because we'll divide by this.
                return false;
            massratio = pother.Mass / Mass; /* The mass() function is by default
			pow(radius(), 3.0), but we overload it for player to be very large. */
            float massdivisor = (1.0f / (1.0f + massratio)); //Use this twice, so calculate once.
            /* Now we subtract off the components of velocity that lie along the line
        of collision and add in the new components. */
            if (!_absorberflag && !_fixedflag)
                Velocity = _velocity.sub(give).add(
                    give.mult(_bounciness * massdivisor * (1.0f - massratio)).add(receive.mult(2.0f * massratio)));
            if (!pother.AbsorberFlag && !pother.FixedFlag)
                pother.Velocity = (pother._velocity).sub(receive).add(
                    (give.mult(2.0f).add(receive.mult(massratio - 1.0f))).mult(pother.Bounciness * massdivisor));
            /* In general we want to move the critters apart so that they are just touching
        at a point we call contactpoint, on the line between the two centers.
        Rather than making contactpoint the midpoint, we weight it so that it divides
        this line in the same ratio as radius and pother->radius().  That
        is, we need to pick a sublength "contactdistance" of the "distance"
        length between the two centers so that 
        radius/radius+otheradius = contactdistance/distance.
        Multiply both sides of this equation to solve for contactdistance,
        which is the multiplier of toOther in the line just below. */
            if (!_fixedflag && !pother.FixedFlag)
            {
                contactpoint = _position.add(
                    toOther.mult((Radius * distanceToOther) / (Radius + pother.Radius)));
                moveTo(contactpoint.sub(toOther.mult(Radius)), true); /* The TRUE arg here means that
				we should treat motion as a physical thing that can change the
				_tangent, _normal, and _binormal. */
                pother.moveTo(contactpoint.add(toOther.mult(pother.Radius)), true);
            }
            /* We special case the situations where one of the critters must remain fixed, such
        as a bumper in a pinball game. */
            else if (_fixedflag && !pother.FixedFlag)
                pother.moveTo(_position.add(toOther.mult(Radius + pother.Radius)), true);
            else if (!_fixedflag && pother.FixedFlag)
                moveTo((pother.Position).sub(toOther.mult(Radius + pother.Radius)), true);
            //else the (_fixedflag && pother->fixedflag()) case, where we move neither.
            return true;
        }

        //================================================ 
        //Coordinate methods ================================= 
        //================================================ 
        /* The coordinate methods transform directions (thought of as vectors not attached to
        a location) and positions between the global world coordinates and the local
        trihedron coordinates of the critter, which has its origin at _position and
        its ijk axes as the trihedron _tangent-_normal-_binormal.  The methods are
        used in box-style collision calculations, as with cCritterWall. */

        public cVector3 globalToLocalDirection(cVector3 globalvec)
        {
            return _inverseattitude.mult(globalvec.add(_position));
            /* The _inverseattitude transformation translates _position to origin and then
        rotates to match critter's attitude. So to just change a direction, we add on
        the _position before subracting it.*/
        }

        public cVector3 localToGlobalDirection(cVector3 localvec)
        {
            return _attitude.mult(localvec).sub(_position);
            /* The _attitude transformation rotates to match critter, then translates
        to _position.  So to just change a direction, we subtract off the _position
        after we're done.*/
        }


        public cVector3 globalToLocalPosition(cVector3 globalpos)
        {
            return _inverseattitude.mult(globalpos);
        }


        public cVector3 localToGlobalPosition(cVector3 localpos)
        {
            return _attitude.mult(localpos);
        }


        //================================================ 
        //Game methods ================================= 
        //================================================ 

        /// <summary>
        /// Can be overloaded to add dying behavior.  But should eventually produce a delete_me() call.
        /// </summary>
        public virtual void die() { delete_me(); } 
        
        /// <summary>
        /// dieOfOldAge is called in the update method if(_usefixedlifetime && _age > _fixedlifetime).  
        /// We distinguish between die() and dieOfOldAge() so die() can make a different sound for instance. */
        /// </summary>
        public virtual void dieOfOldAge() { delete_me(); } 	
        
        /// <summary>
        /// Deducts a hit strength from _health, and calls die if this is below zero.  The hit is not allowed
        /// if the _shieldflag is true or if this critter was just recently damaged.
        /// </summary>
        /// <param name="hitstrength">To deduct from _health.</param>
        /// <returns>Returns _value as a reward to the damager.  Returns 0 if not killed yet.</returns>
        public virtual int damage(int hitstrength)
        {
            // If we have our shield on, or were just hit, then don't allow a hit 
            if (_shieldflag || recentlyDamaged())
                return 0;
            _lasthit_age = _age; //Save age for use by the recentlyDamaged() accessor.
            _health -= hitstrength;
            if (_health <= 0)
            {
                _health = 0;
                die(); //Make a delete_me service request, possibly make noise or more.
                return _value; //The reward for killing this critter.
            }
            return 0; //Not killed yet.
        }

        //================================================ 
        //Force and Listen methods ================================= 
        //================================================ 

        /// <summary>
        /// Calls _plistener_listen, but can be overloaded to do more.
        /// </summary>
        /// <param name="dt"></param>
        public virtual void feellistener(float dt)
        {
            /* The cGame.step calls feellistener(), move(), update(), feellistener(), move(), update(), feellistener(),
                move(), and so on.  In other words, after start up the process is to 
                (1) call update() and, within update, call feelforce().
                (2) call feellistener() and possibly add in some more acceleration
                (3) use the _acceleration in move().  */

            _plistener.listen(dt, this); /* We pass the pointer "this" to the listener so that it can 
			change the fields of this calling cCritter as required.  The caller critter's pgame()
			hold the cController object thatstores all of the keys and mouse actions you need
			to process. */
        }

        /// <summary>
        /// Gets the sum of the forces, then divides by Mass to calculate _acceleration.  Can be overloaded
        /// because you might possibly want to select which forces you feel, depending on the situation, like
        /// whether you are pursuing or fleeing.
        /// </summary>
        public virtual void feelforce()
        {
            cVector3 forcesum = new cVector3(); //Default constructor (0,0,0) 
            foreach (cForce f in _forcelist)
                forcesum.addassign(_forcelist.ElementAt().force(this));
            _acceleration = forcesum.div(Mass); //From Newton's Law: Force = Mass * Acceleration.
        }

        //================================================ 
        //Drawing methods ================================= 
        //================================================ 

        public void updateAttitude(float dt, bool forceattitudeadjustment = false)
        {
            _attitude.LastColumn = _position; //always update position.
            //And then deal with the rotational aspect of _attitude in one of three ways.
            if (!_attitudetomotionlock)
                rotateAttitude(cSpin.mult(dt, _spin));
            else //_attitudetomotionlock is TRUE 
                if (_speed >= _mintwitchthresholdspeed || forceattitudeadjustment)
                    //don't update attitude for tiny jostling unless caller insists.
                    copyMotionMatrixToAttitudeMatrix();
        }

        /* This keeps 
            graphical attitude matrix of the critter in synch with its motionmatrix.  To prevent
            a too-busy look, we normally don't do the update if the _speed is less than
            _mintwitchthresholdspeed. But if we are controlling the critter with arrow key
            calls to, e.g. the yaw, pitch and roll methods, we do want to force the update of
            the appearance, and then you set the forceattitudeupdate argument to TRUE. */

        public virtual void draw(cGraphics pgraphics, int drawflags = 0) //Look at simple listing just above.
        {
            /* We don't have to call _psprite->moveTo(_position) because we call this inside
            the render method that gets called in the draw method. */
            pgraphics.pushMatrix(); //Normal call just does these three lines followed by the popMatrix below.
            pgraphics.multMatrix(_attitude);
            _psprite.draw(pgraphics, drawflags);
            pgraphics.popMatrix();
        }


        /* Calls _psprite->draw.  Has to be virtual because some child critters draw stuff
             (like guns) on top of sprite. */

        public virtual void drawHighlight(cGraphics pgraphics, float highlightsizeratio)
        {
            float focusradius = highlightsizeratio * Radius;
            cSpriteCircle focuscircle = new cSpriteCircle();
            focuscircle.Filled = false;
            focuscircle.LineColor = Color.LightGray;
            focuscircle.Radius = focusradius;
            focuscircle.PrismDz = 0.0f;

            pgraphics.pushMatrix();
            pgraphics.multMatrix(Attitude);
            pgraphics.translate(Sprite.Center.add((new cVector3(0.0f, 0.0f, 1.0f)).mult(0.01f)));
            //Scoot in case the sprite is off-center, also  scoot out of the plane.
            focuscircle.draw(pgraphics);
            //	pgraphics->drawcircle(cVector.ZEROVECTOR, psprite()->radius(), &highlightcolorstyle); 
            pgraphics.popMatrix();
        }

        /* Draw a
            highlighted XOR circle around the critter with a size = highlightratio * radius(). 
            Or draw a sphere in 3D if you like. */

        //================================================ 
        //Simulation methods ================================= 
        //================================================ 

        public virtual void animate(float dt)
        {
            updateAttitude(dt);
            _inverseattitude = _attitude.Inverse;
            _psprite.animate(dt, this);
        }

        //Calls _psprite->animate(dt). Can overload to setAimVector.

        /// <summary>
        /// Override to update the critter the way that you want to.
        /// </summary>
        /// <param name="pactiveview">This argument could be useful if you are using cListenerCursor</param>
        /// <param name="dt">The change in time between frame updates.  Might be useful if an update should not occur until a specified
        /// amount of time has elapsed.</param>
        public virtual void update(ACView pactiveview, float dt)
        {
            feelforce();
            if (recentlyDamaged() != _oldrecentlydamaged)
            {
                Sprite.NewGeometryFlag = true; /* I do this as some critters are drawn differently
				when recently damaged. */
                _oldrecentlydamaged = recentlyDamaged();
            }
            if (_usefixedlifetime && _age > _fixedlifetime)
                dieOfOldAge(); /* I don't call die() because I like to use die for when a critter dies
				of unnatural causes, like getting shot.  It's more likely that I overload die() to
				do something dramatic than that I overlaod dieOfOldAge(). */
        }

        public int move(float dt)
        {
            _age += dt;
            _oldposition.copy(_position); /* We save the _oldposition here so we can compare it with
			_position in cCritterWall.collide(cCritter *) to see if this move call jumps us
			over a wall. */
            if (_fixedflag)
                return cRealBox3.BOX_INSIDE;
            _velocity.addassign(_acceleration.mult(dt));
            //Now clamp the _speed to be either 0.0 or between SMALL_REAL and _maxspeed.
            synchSpeedAndDirectionToVelocity(); //This sets _speed and _tangent to match _velocity.
            if (_speed > _maxspeed)
                _speed = _maxspeed;
            _velocity = _tangent.mult(_speed); //Reset the _velocity in case you changed _speed.
            //Now bounce or clamp, and when that's done do a final fix of _tangent, _tangent, and _binormal.
            addvelocityandcheckedges(dt); /* Do a bounce or wrap or clamp, depending on _wrapflag.
			The addvelocityandcheckedges call sets the _outcode field as well. */
            fixNormalAndBinormal(); /* Now that we're through changing the _velocity and the accompanying
			_tangent, make the _normal and _binormal (in 3D case) match.*/
            if (dt > 0.00001f)
                _curvature /= dt;
            /* _curvature measures the rate at which the tangent is turning,
            specifically, if we write dTangent to stnad for the size change of the tangent
            vecotr, we want the size of dTangent/dt = _curvature.  
            Now, the code in fixNormalAndBinomral has set _curvature = dTangent,
            so now we have to divide by dt. */
            else
                _curvature = 0.0f; //If you're not moving, you have no curvature.
            //A totally different issue is to spin the aspect, but we do that in cCritter.animate.
            return _outcode;
        }

        /* You really should NOT change the delicately constructed move
            method, which is why its not virtual. */

        /// <summary>
        /// Gets or sets _value, which is a reward for damaging a critter (to be used whatever way you see fit).
        /// _value is returned from the damage function.
        /// </summary>
        public virtual int Value
        {
            get
            { return _value; }
            set
            { _value = value; }
        }

        /// <summary>
        /// Used to set or get a _shieldflag (true or false).  When _shieldflag is set to true, it prevents
        /// damage being done to the critter.
        /// </summary>
        public virtual bool Shield
        {
            get
            { return _shieldflag; }
            set
            { _shieldflag = value; }
        }

        /// <summary>
        /// Used to set or get the _usefixedlifetime flag (true or false).  A critter will "die of old age"
        /// if UseFixedLiftime is set to true, and critter's Age > FixedLifetime (both properties).
        /// </summary>
        public virtual bool UseFixedLifetime
        {
            get
            { return _usefixedlifetime; }
            set
            { _usefixedlifetime = value; }
        }

        /// <summary>
        /// Used to get or set the _fixedlifetime of the critter.  When critter's Age > FixedLifetime,
        /// it will "die of old age" (note: the UseFixedLifetime property must also be set to true)
        /// </summary>
        public virtual float FixedLifetime
        {
            get
            { return _fixedlifetime; }
            set
            { _fixedlifetime = value; }
        }

        /// <summary>
        /// Gets or sets the _mutationstrength of the critter.  This will be the strength of the mutation 
        /// when calling the mutate function.
        /// </summary>
        public virtual float MutationStrength
        {
            get
            { return _mutationstrength; }
            set
            { _mutationstrength = value; }
        }

        /// <summary>
        /// Gets or sets a critter that this critter "keeps an eye on".  When used by the armed robot class, it will
        /// aim at the Target when shooting.
        /// </summary>
        public virtual cCritter Target
        {
            get
            { return _ptarget; }
            set
            { _ptarget = value; }
        }

//        public virtual int MetricKey
//        {
//            get
//            { return _metrickey; }
//            set
//            { _metrickey = value; }
//        }

        /// <summary>
        /// Used to get or set the _age of the critter.  Can be useful to check how much time elapsed before
        /// an event occurs.  If Age is not set, it is automatically updated using dt after each frame has
        /// elapsed. 
        /// </summary>
        public virtual float Age
        {
            get
            { return _age; }
            set
            { _age = value; _lasthit_age = _age - cCritter.SAFEWAIT; }
        }

        public virtual cBiota Owner
        {
            set
            { _pownerbiota = value; }
        }

        /// <summary>
        /// Gets or sets the _health of the critter.
        /// </summary>
        public virtual int Health
        {
            get
            { return _health; }
            set
            { _health = value; if (_health < 0) _health = 0; }
        }

        /// <summary>
        /// Sets _newlevelreward, which is added to the health of the critter upon attaining a new level.
        /// </summary>
        public virtual int NewLevelReward
        {
            set
            { _newlevelreward = value; }
        }

        /// <summary>
        /// Sets _newlevelscorestep, the number of points needed to get to a new level.
        /// When a new level is attained, _newlevelreward is added to the health.
        /// (_newlevelreward is set with the NewLevelReward property).
        /// </summary>
        public virtual int NewLevelScoreStep
        {
            set
            { _newlevelscorestep = value; }
        }

        /// <summary>
        /// Gets and sets the _dragbox, the draggable space that the critter can be moved to by mouse.
        /// This can be different than the _movebox.
        /// </summary>
        public virtual cRealBox3 DragBox
        {
            get
            {
                cRealBox3 d = new cRealBox3();
                d.copy(_dragbox);
                return d;
            }
            set
            { _dragbox = value; }
        }

        /// <summary>
        /// Gets and sets the _wrapflag, which deals with the behavior that a critter has when it
        /// hits its _movebox.  The _wrapflag can be set to cCritter.WRAP, cCritter.BOUNCE, or cCritter.CLAMP 
        /// </summary>
        public virtual int WrapFlag
        {
            get
            { return _wrapflag; }
            set
            {
                _wrapflag = value;
                if (_wrapflag == cCritter.BOUNCE || _wrapflag == cCritter.CLAMP)
                    clamp();
                else
                {
                    _wrapposition1.copy(_position);
                    _wrapposition2.copy(_position);
                    _wrapposition3.copy(_position);
                }
            }
        }

        /// <summary>
        /// Gets or sets the _speed of the critter.  If you set to a negative value, it will convert it to positive.
        /// If you wish to go backwards, reverse _tangent.
        /// </summary>
        public virtual float Speed
        {
            get
            { return _speed; }
            set
            {
                _speed = Math.Abs(value); //We don't allow negative value.  To go backwards, reverse _tangent.
                /* We are committed to always having _velocity = _speed * _tangent, so if we change one
            we need to change the other two. We do a more thorough checking, e.g. against _maxspeed,
            in the cCritter.move method. */
                _velocity = _tangent.mult(_speed);
            }
        }

        /// <summary>
        /// Gets or sets _maxspeed, the maximum speed of the critter.
        /// </summary>
        public virtual float MaxSpeed
        {
            get
            { return _maxspeed; }
            set
            { _maxspeed = _maxspeedstandard = value; }
        }

        /// <summary>
        /// Sets _maxspeed to temporarily go faster than the current maximum speed. This can be used, for
        /// example, when fleeing or chasing.  Call restoreMaxspeed() to bring the maximum speed back to normal.
        /// </summary>
        public virtual float TempMaxSpeed
        {
            set
            { _maxspeed = value; }
        }

        /// <summary>
        /// Gets or sets the _acceleration of the critter.
        /// </summary>
        public virtual cVector3 Acceleration
        {
            get
            {
                cVector3 a = new cVector3();
                a.copy(_acceleration);
                return a;
            }
            set
            { _acceleration = value; }
        }

        /// <summary>
        /// Gets or sets the _density of the critter.
        /// </summary>
        public virtual float Density
        {
            get
            { return _density; }
            set
            { _density = value; fixMass(); }
        }

        /// <summary>
        /// Gets or sets the listener (_plistener) of the critter.
        /// </summary>
        public virtual cListener Listener
        {
            get
            { return _plistener; }
            set
            {
                _plistener = value;
                _plistener.install(this); /* Note that this may change _attitudetomotionlock and will by default set
			_mintwitchthresholdspeed to 0.0 */
            }
        }

        /// <summary>
        /// Gets or sets the _listeneracceleration of the critter, the critter's "engine strength".
        /// </summary>
        public virtual float ListenerAcceleration
        {
            get
            { return _listeneracceleration; }
            set
            { _listeneracceleration = value; }
        }

        /// <summary>
        /// Gets or sets the _mintwitchthresholdspeed, the minimum speed for the attitude update.  
        /// If the speed is too low and the attitude is updated, it can look bad, like a bouncing ball
        /// almost done bouncing but "twitching" on the floor.
        /// </summary>
        public virtual float MinTwitchThresholdSpeed
        {
            get
            { return _mintwitchthresholdspeed; }
            set

            { _mintwitchthresholdspeed = value; }
        }

        /// <summary>
        /// Gets or sets the _bounciness of the critter.  Set between 0.0 and 1.0; if not, 
        /// it will be clamped to one of the extremes of these values.
        /// </summary>
        public virtual float Bounciness
        {
            get
            { return _bounciness; }
            set
            {
                if (value < 0.0f)
                    value = 0.0f;
                else if (value > 1.0f)
                    value = 1.0f;
                _bounciness = value;
            }
        }

        /// <summary>
        /// Gets or sets _absorberflag.  Set to true to avoid changing velocity upon a collision.
        /// Otherwise, set to false.
        /// </summary>
        public virtual bool AbsorberFlag
        {
            get
            { return _absorberflag; }
            set
            {
                _absorberflag = value;
                _bounciness = (_absorberflag) ? 0.0f : 1.0f;
            }
        }

        /// <summary>
        /// Gets or sets _collidepriority for the critter.  In a collision, the critter with the higher
        /// _collidepriority value calls the collide function, and the other critter gets passed in.
        /// If the _collideprriority is equal between the critters, the critter's collide function
        /// that is called will be unknown -- therefore, collide needs to be implemented for both.
        /// </summary>
        public virtual float CollidePriority
        {
            get
            { return _collidepriority; }
            set
            { //Rebuild the pgame()->_pcollider just in case.
                _collidepriority = value;
                Game.buildCollider();
            }
        }

        public virtual float LastCollidePartnerPriority
        {
            get
            { return _lastcollidepartnerpriority; }
            set
            { _lastcollidepartnerpriority = value; }
        }

        public virtual float LastShield
        {
            get { return _lastShield; }
            set { _lastShield = value; }
        }

        /// <summary>
        /// Gets or sets the sprite (_psprite) of the critter.
        /// </summary>
        public virtual cSprite Sprite
        {
            get
            { return _psprite; }
            set
            {
                _psprite = value;
                _psprite.PrismDz = _defaultprismdz; //Do I need this? 
                fixMass(); //Use the sprite's radius() to recompute your mass.
            }
        }

        /// <summary>
        /// Gets or sets _attitudetomotionlock, which locks the display sprite with the motion.
        /// By dafault, this is false for the player and true for all other critters.
        /// </summary>
        public virtual bool AttitudeToMotionLock
        {
            get
            { return _attitudetomotionlock; }
            set
            { _attitudetomotionlock = value; }
        }

        /// <summary>
        /// Gets or sets the _attitude of the critter.
        /// </summary>
        public virtual cMatrix3 Attitude
        {
            get
            {
                cMatrix3 a = new cMatrix3();
                a.copy(_attitude);
                return a;
            }
            set
            //Change tan, norm, binorm, but NOT position.
            {
                _attitude = value;
                _attitude.LastColumn = _position; /* Don't use the last column of the value argument. */
                if (_attitudetomotionlock)
                {
                    _tangent = AttitudeTangent;
                    _velocity = _tangent.mult(_speed);
                    _normal = AttitudeNormal;
                    _binormal = AttitudeBinormal;
                    _oldtangent.copy(_tangent); //So fixNormalAndBinormal doesn't undo this.
                }
            }
        }

        /// <summary>
        /// Gets the tangent from the attitude matrix, or sets tangent in the attitude matrix.
        /// </summary>
        public virtual cVector3 AttitudeTangent
        {
            get
            { return _attitude.column(0); }
            set
            {
                cVector3 oldtangent = AttitudeTangent;
                cVector3 newtangent = value;
                newtangent.normalize();
                if ((newtangent.sub(oldtangent)).IsPracticallyZero)
                    return;
                if (_attitudetomotionlock) // LOCK_ON 
                {
                    Tangent = value;
                    //updateAttitude(0.0, TRUE);  
                    /* I could call this to set _attitude to match _tangent, _normal and _binormal.
                But seems safe to just wait for the updateAttitude to get called by cCritter.animate. */
                }
                else //case of not _attitudetomotionlock 
                {
                    cMatrix3 turnmatrix = cMatrix3.rotationFromUnitToUnit(oldtangent, newtangent);
                    /* When you multiply _atttude by a matrix to rotate it, you have to temporarily
                    set the last column to 0.*/
                    _attitude.LastColumn = new cVector3(0.0f, 0.0f, 0.0f);
                    _attitude = turnmatrix.mult(_attitude);
                    _attitude.LastColumn = _position;
                    //_attitude.orthonormalize(); //Could do this to be safe, but is probably superflous.
                }
            }
        }

        /// <summary>
        /// Gets or sets the radius of the critter. (works using _psprite.Radius)
        /// </summary>
        public virtual float Radius
        {
            get
            {
                return _psprite.Radius;
            }
            set
            {
                _psprite.Radius = value;
                fixMass();
                // return clamp();
            }
        }

        /// <summary>
        /// Sets _defaultprismdz, the thickness of what would otherwise be a 2D polygon
        /// </summary>
        public virtual float PrismDz
        {
            set
            {
                _defaultprismdz = value;
                if (_psprite != null)
                    _psprite.PrismDz = _defaultprismdz;
            }
        }

        /// <summary>
        /// Gets the _personality of the critter.  This is a collection of bits to use with
        /// bit-wise operators to give critters different characteristics.
        /// </summary>
        public virtual uint Personality
        {
            get
            { return _personality; }
        }

        /// <summary>
        /// Gets the biota object (_pownerbiota) that owns this critter.
        /// </summary>
        public virtual cBiota OwnerBiota
        {
            get
            {
                return _pownerbiota;
            }
        }

        /// <summary>
        /// Gets the game object (_pownerbiota.pgame()) that this critter is in.
        /// </summary>
        public virtual cGame Game
        {
            get
            {
                if (_pownerbiota == null)
                    return null;
                return _pownerbiota.pgame();
            }
        }

        /// <summary>
        /// Gets the player (Game.Player)
        /// </summary>
        public virtual cCritter Player
        {
            get
            {
                cCritter pplayergame = Game.Player;
                return pplayergame;
            }
        }

        /// <summary>
        /// Gets the critter's _score
        /// </summary>
        public virtual int Score
        {
            get
            { return _score; }
        }

        /// <summary>
        /// Gets the critter's _position
        /// </summary>
        public virtual cVector3 Position
        {
            get
            {
                cVector3 pos = new cVector3();
                pos.copy(_position);
                return pos;
            }
        }

        /// <summary>
        /// Gets the critter's _oldposition (previous position)
        /// </summary>
        public virtual cVector3 OldPosition
        {
            get
            {
                cVector3 pos = new cVector3();
                pos.copy(_oldposition);
                return pos;
            }
        }

        public virtual cPlane Plane
        {
            get
            { return new cPlane(_position, _binormal); }
        }

        /// <summary>
        /// Gets the _movebox for the critter.  To set the _movebox, use the setMoveBox function,
        /// which will return the resulting _outcode.
        /// </summary>
        public virtual cRealBox3 MoveBox
        {
            get
            {
                cRealBox3 m = new cRealBox3();
                m.copy(_movebox);
                return m;
            }
        }

        /// <summary>
        /// Gets the smallest cube holding the critter.
        /// </summary>
        public virtual cRealBox3 RealBox
        {
            get
            {
                return new cRealBox3(_position, 2 * Radius);
            }
        }


        /// <summary>
        /// Gets or sets _fixedflag.  When set to true, it keeps the critter from moving (more or less).
        /// </summary>
        public virtual bool FixedFlag
        {
            get
            { return _fixedflag; }
            set
            { _fixedflag = value; }

        }

        /// <summary>
        /// Gets or sets the velocity of the critter.
        /// </summary>
        public virtual cVector3 Velocity
        {
            get
            {
                cVector3 v = new cVector3();
                v.copy(_velocity);
                return v;
            }
            set
            {
                _velocity = value;
                /* We are committed to always having _velocity = _speed * _tangent, so if we change one
            we need to change the other two. */
                synchSpeedAndDirectionToVelocity(); //This sets _speed and _tangent to match _velocity.
                fixNormalAndBinormal();
            }

        }

        /// <summary>
        /// Gets or sets the _tangent of the critter (the direction it is currently moving in.
        /// </summary>
        public virtual cVector3 Tangent
        {
            get
            {
                cVector3 t = new cVector3();
                t.copy(_tangent);
                return t;
            }
            set
            {
                _tangent = value;
            }
        }

        /// <summary>
        /// Gets the _normal vector of the critter, the direction the critter was recently turning in.  
        /// </summary>
        public virtual cVector3 Normal
        {
            get
            {
                cVector3 n = new cVector3();
                n.copy(_normal);
                return n;
            }
        }

        /// <summary>
        /// Gets the _binormal vector of the critter, perpendicular to the plane formed by the _tangent and _normal vectors.
        /// </summary>
        public virtual cVector3 Binormal
        {
            get
            {
                cVector3 b = new cVector3();
                b.copy(_binormal);
                return b;
            }
        }

        /// <summary>
        /// Gets the _curvature, which is basically the rate at which the _tangent vector is changing direction.
        /// </summary>
        public virtual float Curvature
        {
            get
            { return _curvature; }
        }

        /// <summary>
        /// Gets _maxspeedstandard, the normal maximum speed (the speed can be abnormally
        /// increased above it by using the TempMaxSpeed property, like when fleeing or pursuing).
        /// </summary>
        public virtual float MaxSpeedStandard
        {
            get
            { return _maxspeedstandard; }
        }

        /// <summary>
        /// Gets the list of forces acting upon the critter (_forcelist)
        /// </summary>
        public virtual LinkedList<cForce> ForceList
        {
            get
            { return _forcelist; }
        }

        /// <summary>
        /// Gets the _mass of the critter
        /// </summary>
        public virtual float Mass
        {
            get
            { return _mass; }
        }

        /// <summary>
        /// Gets the _inverseattitude of the critter.  You probably don't want to use this.  If you do,
        /// it is helpful to know that The _inverseattitude transformation does a translation that 
        /// moves _position to the origin and then rotates to match the standard ijk trihedron 
        /// to the critter's _tangent-_normal-_binormal trihedron.
        /// </summary>
        public virtual cMatrix3 InverseAttitude
        {
            get
            {
                cMatrix3 i = new cMatrix3();
                i.copy(_inverseattitude);
                return i;
            }
        }

        /// <summary>
        /// Gets the normal vector from the _attitude matrix.  The normal vector is the direction
        /// that the critter was most recently moving in.
        /// </summary>
        public virtual cVector3 AttitudeNormal
        {
            get
            { return _attitude.column(1); }
        }

        /// <summary>
        /// Gets the binormal vector from the _attitude matrix.  The binormal vector is the vector
        /// perpendicular to the plane formed by the tangent and normal vectors.
        /// </summary>
        public virtual cVector3 AttitudeBinormal
        {
            get
            { return _attitude.column(2); }
        }

        /// <summary>
        /// The default thickness of a sprite which was made of a polygon.  The thickness adds a
        /// third dimension to what would otherwise be a 2D polygon.
        /// </summary>
        public virtual float DefaultPrismDz
        {
            get
            { return _defaultprismdz; }
        }

        /// <summary>
        /// Returns the name of this class as a string.  Useful for polymorphism.
        /// </summary>
        public virtual string RuntimeClass
        {
            get
            {
                return "cCritter";
            }
        }

        static public float Maxspeed
        {
            get { return MAXSPEED; }
            set { MAXSPEED = value; }
        }

        static public float MinRadius
        {
            get { return MINRADIUS; }
            set { MINRADIUS = value; }
        }
        
        static public float MaxRadius
        {
            get { return MAXRADIUS; }
            set { MAXRADIUS = value; }
        }
        
        static public float BulletRadius
        {
            get { return BULLETRADIUS; }
            set { BULLETRADIUS = value; }
        }

    }
}
//                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            