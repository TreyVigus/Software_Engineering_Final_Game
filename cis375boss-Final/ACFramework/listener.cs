using System;
using OpenTK.Input;

// For ACFramework Version 1.2, I've fixed a bug that occurred when the player's maximum speed
// was set differently in the player's constructor -- in this bug, the hop strength used to be
// severely affected in the hopper classes
// ZEROVECTOR and others vectors were removed
// default parameters were added -- JC

namespace ACFramework
{

    class cListener
    {
        public static readonly float CRITTERTURNSPEEDSLOW = 0.2f * (float)Math.PI; //Slow Radians per second to turn. Try 0.5*PI 
        public static readonly float CRITTERTURNSPEEDFAST = 2.0f * (float)Math.PI; //Fast Radians per second to turn. Try 2.0*PI 
        public static readonly float TURNSPEEDUPWAIT = 0.1f; //Time in secs of turning before you turn faster.
        public static readonly float RIDESTEP = 0.05f;
        public static readonly int MOVEVIEW = 1;
        /* MOVEVIEW must be 1 or -1. 
        If MOVEVIEW is 1, then the cCritterViewerFly and
    cCritterViewerOrtho	will move the cCritterViewer in such a way that what you see 
    scrolls left when you press Ctrl+left arrow, scrolls right with Ctrl+right arrow,
    up with Ctrl+up arrow, down with Ctrl+down arrow.  The effect is that these keys
    are moving the world, as opposed to moving the viewer.  This seems better, as the
    typical user will not think in terms of the viewer as a separate object.  Also
    the Ctrl+Shift+left arrow will rotate the world to the left, and so on.  To do
    all this, the cCritterViewer actually is moving or rotating in the 
    opposite directions of the arrow, that is, a left arrow moves the critter right, which
    makes the view that the critter sees move to the left and so on.
        If MOVEVIEW is -1, all this is undone, and the arrows move the criter viewer
    in the natural way and the visual motions of the world are reversed. 
        I'm still not 100% sure that the MOVEVIEW = 1 makes the best interface */


        public cListener() { }

        public float turnspeed(float keydowntime)
        {
            if (keydowntime < TURNSPEEDUPWAIT)
                return CRITTERTURNSPEEDSLOW;
            else
                return CRITTERTURNSPEEDFAST;
        }


        public virtual void install(cCritter pcritter)
        {
            pcritter.restoreMaxspeed(); //In case you were just using cListenerCursor 
            pcritter.copyAttitudeMatrixToMotionMatrix();
            /* In case the two don't match, give your attitude
                precendence, just to get off to a proper start with the listener. */
        }

        public virtual void listen(float dt, cCritter pcritter) { }

        public virtual bool IsKindOf(string str)
        {
            return str == "cListener";
        }

        public virtual string RuntimeClass
        {
            get
            {
                return "cListener";
            }
        }


    }

    class cListenerArrow : cListener
    {

        public cListenerArrow() { }

        public override void listen(float dt, cCritter pcritter)
        {
            /* Note that since I set the velocity to 0.0 when I'm not
            pressing an arrow key, this means that acceraltion forces don't get
            to have accumulating effects on a critter with a cListenerScooter listener. So rather
            than having some very half-way kinds of acceleration effects, I go aheand and
            set acceleration to 0.0 in here. */
            pcritter.Acceleration = new cVector3(0.0f, 0.0f, 0.0f);

            bool left = Framework.Keydev[vk.Left];
            bool right = Framework.Keydev[vk.Right];
            bool up = Framework.Keydev[vk.Up];
            bool down = Framework.Keydev[vk.Down];
            bool pageup = Framework.Keydev[vk.PageUp];
            bool pagedown = Framework.Keydev[vk.PageDown];
            if (!left && !right && !down && !up && !pagedown && !pageup)
            {
                pcritter.Velocity = new cVector3(0.0f, 0.0f, 0.0f);
                return;
            }
            /* If you get here, you've pressed an arrow key.  First match the velocity to 
            the arrow key direction, and then match the attitude. */
            if (left)
                pcritter.Velocity = new cVector3(-pcritter.MaxSpeed, 0.0f, 0.0f);
            if (right)
                pcritter.Velocity = new cVector3(pcritter.MaxSpeed, 0.0f, 0.0f);
            if (down)
                pcritter.Velocity = new cVector3(0.0f, -pcritter.MaxSpeed, 0.0f);
            if (up)
                pcritter.Velocity = new cVector3(0.0f, pcritter.MaxSpeed, 0.0f);
            if (pagedown)
                pcritter.Velocity = new cVector3(0.0f, 0.0f, -pcritter.MaxSpeed);
            if (pageup)
                pcritter.Velocity = new cVector3(0.0f, 0.0f, pcritter.MaxSpeed);
            //Now match the attitude.
            if (pcritter.AttitudeToMotionLock)
                /* Need this condition if you want
            to have a "spaceinvaders" type shooter that always points up as in 
            the textbook problem 3.11 */
                pcritter.copyMotionMatrixToAttitudeMatrix();
            //Note that if pcritter is cCritterArmed*, then the cCritterArmed.listen does more stuff.
        }

        public override bool IsKindOf(string str)
        {
            return str == "cListenerArrow" || base.IsKindOf(str);
        }

        public override string RuntimeClass
        {
            get
            {
                return "cListenerArrow";
            }
        }


    }

    /* cListenerArrowAttitude is like class cListenerArrow except that makes the arow motions
    relative to the player's attitude rather than relative to the axies.  Also it changes
    the roles of the key pairs to be intuitive if you ride a player in 3D, where "up"
    is the ZAXIS, not the YAXIS. */
    class cListenerArrowAttitude : cListener
    {

        public cListenerArrowAttitude() { }

        public override void listen(float dt, cCritter pcritter) /* used in Defender3d */
        {
            /* Note that since I set the velocity to 0.0 when I'm not
            pressing an arrow key, this means that acceraltion forces don't get
            to have accumulating effects on a critter with a cListenerScooter listener. So rather
            than having some very half-way kinds of acceleration effects, I go aheand and
            set acceleration to 0.0 in here. */
            bool left = Framework.Keydev[vk.Left];
            bool right = Framework.Keydev[vk.Right];
            bool up = Framework.Keydev[vk.Up];
            bool down = Framework.Keydev[vk.Down];
            bool pageup = Framework.Keydev[vk.PageUp];
            bool pagedown = Framework.Keydev[vk.PageDown];
            pcritter.Acceleration = new cVector3(0.0f, 0.0f, 0.0f);
            if (!left && !right && !down && !up && !pagedown && !pageup)
            {
                pcritter.Velocity = new cVector3(0.0f, 0.0f, 0.0f);
                return;
            }
            /* If you get here, you've pressed an arrow key.  First match the velocity to 
            the arrow key direction, and then match the attitude. */
            if (left)
                pcritter.Velocity = pcritter.AttitudeNormal.mult(pcritter.MaxSpeed);
            if (right)
                pcritter.Velocity = pcritter.AttitudeNormal.mult(-pcritter.MaxSpeed);
            if (pagedown)
                pcritter.Velocity = pcritter.AttitudeTangent.mult(-pcritter.MaxSpeed);
            if (pageup)
                pcritter.Velocity = pcritter.AttitudeTangent.mult(pcritter.MaxSpeed);
            if (down)
                pcritter.Velocity = pcritter.AttitudeBinormal.mult(-pcritter.MaxSpeed);
            if (up)
                pcritter.Velocity = pcritter.AttitudeBinormal.mult(pcritter.MaxSpeed);
            //DON'T match the attitude to the motion.
            //Note that if pcritter is cCritterArmed*, then the cCritterArmed.listen does more stuff.
        }

        public override bool IsKindOf(string str)
        {
            return str == "cListenerArrowAttitude" || base.IsKindOf(str);
        }

        public override string RuntimeClass
        {
            get
            {
                return "cListenerArrowAttitude";
            }
        }


    }

    class cListenerHopper : cListener
    {
        protected float _walkspeed; // how fast you can walk 
        protected float _hopstrength; // how high you can hop 
        protected bool _hopping; // TRUE if hopping 
        protected bool _falling; // TRUE if falling 
        protected float _lastSpeed; // your hopping speed on previous frame 
        protected float saveMaxSpeed;

        public cListenerHopper(float walksp = 6.0f, float hopst = 2.0f)
        {
            _walkspeed = walksp;
            _hopstrength = hopst;
            _lastSpeed = 0.0f;
            _hopping = false;
            _falling = false;
        }

        public override void listen(float dt, cCritter pcritter)
        {
            /* We design this primarily for use in 2D where you use left/right to move
            along the x axis and use the up key (or you coudl have instead used
            the spacebar if you don't need it for shooting) to hop in the air.
            Simply as a default extension to 3D, we'll assume that in 3D the z direction
            would be on the ground and the y could continue to be the hopping direction.
            We set the x (and the z velocity) to 0.0 when I'm not
            pressing an arrow key, this means that acceleration forces don't get
            to have accumulating effects on a critter with a cListenerScooter listener. 
            So rather than having some very half-way kinds of acceleration effects, I go 
            ahead and set x and z acceleration to 0.0 in here. But I leave y alone. */
            pcritter.Acceleration = new cVector3(0.0f, pcritter.Acceleration.Y, 0.0f);
            cVector3 curvel = pcritter.Velocity; //Just to save typing 
            bool left = Framework.Keydev[vk.Left];
            bool right = Framework.Keydev[vk.Right];
            bool up = Framework.Keydev[vk.Up];
            bool down = Framework.Keydev[vk.Down];
            bool pageup = Framework.Keydev[vk.PageUp];
            bool pagedown = Framework.Keydev[vk.PageDown];
            if (!left && !right && !down && !up && !pagedown && !pageup)
                pcritter.Velocity = new cVector3(0.0f, curvel.Y, 0.0f); //don't keep left right momentum.
            if (!_hopping && left)
                pcritter.Velocity = new cVector3(-WalkSpeed, curvel.Y, curvel.Z);
            if (!_hopping && right)
                pcritter.Velocity = new cVector3(WalkSpeed, curvel.Y, curvel.Z);
            /* In a 2D world we'll assume critter hops in Y direction
                with VK_UP. In a 3D world, we'll assume critter hops in Y direction with
                VK_PAGEUP, and uses regular UP/DOWN to move "forward" in the negative
                Z direction, walking into the sceen from hiz to loz. */
            if (!_hopping && down)
                /* In the 3D World, let's by default assume hopper is "walking" form hiz to loz */
                pcritter.Velocity = new cVector3(curvel.X, curvel.Y, WalkSpeed);
            if (!_hopping && up)
                pcritter.Velocity = new cVector3(curvel.X, curvel.Y, -WalkSpeed);

            // I've rewritten this code so the player can't hop again in midair --  
            // unless forces other than gravity act on it in midair -- oh, well, it's  
            // still not perfect -- JC 
            bool hopkeypressed = pageup;

            if (hopkeypressed && !_hopping)
            {
                //Pulse upwards 
                saveMaxSpeed = pcritter.MaxSpeed;
                pcritter.MaxSpeed = cGame3D.MAXPLAYERSPEED;
                pcritter.addAcceleration(new cVector3(0.0f, _hopstrength * pcritter.ListenerAcceleration, 0.0f));
                _hopping = true;
                _falling = false;
                _lastSpeed = pcritter.MaxSpeed;
            }
            else
            {
                if (_hopping && _lastSpeed < pcritter.Speed)
                {
                    _falling = true;
                }
                else if (_hopping && _falling)
                {
                    _hopping = false; // player has landed  
                    _falling = false;
                    pcritter.MaxSpeed = saveMaxSpeed;
                }
                _lastSpeed = pcritter.Speed;
            }
        }

        public override bool IsKindOf(string str)
        {
            return str == "cListenerHopper" || base.IsKindOf(str);
        }

        public virtual float WalkSpeed
        {
            get
                { return _walkspeed; }
        }

        public override string RuntimeClass
        {
            get
            {
                return "cListenerHopper";
            }
        }


    }

    class cListenerScooterYHopper : cListener
    {
        protected float _walkspeed; // how fast you can walk 
        protected float _hopstrength; // how high you can hop 
        protected bool _hopping; // TRUE if hopping 
        protected bool _falling; // TRUE if falling 
        protected float _lastSpeed; // your hopping speed on previous frame 
        protected float saveMaxSpeed;

        public cListenerScooterYHopper(float walksp = 6.0f, float hopst = 2.0f)
        {
            _walkspeed = walksp;
            _hopstrength = hopst;
            _lastSpeed = 0.0f;
            _hopping = false;
            _falling = false;
        }

        public override void listen(float dt, cCritter pcritter)
        {
            cKeyInfo pcontroller = Framework.Keydev;
            /* Note that since I set the velocity to 0.0 when I'm not
            pressing an arrow key, this means that acceraltion forces don't get
            to have accumulating effects on a critter with a cListenerScooter listener. So rather
            than having some very half-way kinds of acceleration effects, I go aheand and
            set acceleration to 0.0 in here. */
            //	pcritter->setAcceleration(cVector(0.0, pcritter->acceleration().y(), 0.0));	 
            float yvelocity = pcritter.Velocity.Y; /* Save this and restore it before we leave
			this call, so that gravity can act in the y direction. */
            //Translate 
            /* I want to move the critter position. But I don't
            just use a moveTo because I want to have a correct _velocity inside the 
            critter so I can use it to hit things and bounce and so on.  So I change
            the velocity.*/
            bool inreverse = false; //Only set TRUE if currently pressing VK_DOWN 
            bool left = Framework.Keydev[vk.Left];
            bool right = Framework.Keydev[vk.Right];
            bool up = Framework.Keydev[vk.Up];
            bool down = Framework.Keydev[vk.Down];
            bool pageup = Framework.Keydev[vk.PageUp];
            bool pagedown = Framework.Keydev[vk.PageDown];
            if (!_hopping && up)
                pcritter.Velocity = pcritter.AttitudeTangent.mult(pcritter.MaxSpeed);
            if (!_hopping && down)
            {
                pcritter.Velocity = pcritter.AttitudeTangent.mult(-pcritter.MaxSpeed);
                inreverse = true;
            }
            if (!up && !down)
                pcritter.Velocity = new cVector3(0.0f, 0.0f, 0.0f);

            //Now restore the y velocity.
            pcritter.Velocity = new cVector3(pcritter.Velocity.X, yvelocity, pcritter.Velocity.Z);
            //	Real inreversesign = inreverse?-1.0:1.0; 

            if (!_hopping && !left && !right && !pagedown && !pageup)
                return;
            /* If you get here, you've pressed an arrow key or a hop key. */
            if (!_hopping && (left || right))
            {
                /* To rotate, Do three things.
            (a) Match the motion vectors to the visible attitude.
            (b) Rotate the motion vectors withe the arrow keys.
            (c) Match the attitude to the altered motion vectors. */
                //(a) Match the motion matrix to the attitude.
                pcritter.copyAttitudeMatrixToMotionMatrix(); //Changes _velocity 
                if (inreverse) //Keep the tangent and atttitudeTangent in opposite directions.
                    pcritter.yaw((float)Math.PI); //This puts _velocity back in the correct direction.
                //(b) Alter the motion matrix.
                if (left)
                    pcritter.yaw(dt * turnspeed(pcontroller.keystateage(vk.Left)));
                if (right)
                    pcritter.yaw(-dt * turnspeed(pcontroller.keystateage(vk.Right)));
                //(c) Match the attitude to the motion matrix.
                pcritter.copyMotionMatrixToAttitudeMatrix();
                if (inreverse) //Keep the tangent and atttitudeTangent in opposite directions.
                    pcritter.rotateAttitude((float)-Math.PI);
                pcritter.Velocity = //Restore y velocity in case you changed it again.
                    new cVector3(pcritter.Velocity.X, yvelocity, pcritter.Velocity.Z);
            }
            //Hopping code 
            // I've rewritten this code so the player can't hop again in midair --  
            // unless forces other than gravity act on it in midair -- oh, well, it's  
            // still not perfect -- JC 
            bool hopkeypressed = pageup;

            if (hopkeypressed && !_hopping)
            {
                //Pulse upwards 
                saveMaxSpeed = pcritter.MaxSpeed;
                pcritter.MaxSpeed = cGame3D.MAXPLAYERSPEED;
                pcritter.addAcceleration(new cVector3(0.0f, _hopstrength * pcritter.ListenerAcceleration, 0.0f));
                _hopping = true;
                _falling = false;
                _lastSpeed = pcritter.MaxSpeed;
            }
            else
            {
                if (_hopping && _lastSpeed < pcritter.Speed)
                {
                    _falling = true;
                }
                else if (_hopping && _falling)
                {
                    _hopping = false; // player has landed  
                    _falling = false;
                    pcritter.MaxSpeed = saveMaxSpeed;
                }
                _lastSpeed = pcritter.Speed;
            }
        }

        public override bool IsKindOf(string str)
        {
            return str == "cListenerScooterYHopper" || base.IsKindOf(str);
        }

        public virtual float WalkSpeed
        {
            get
            { return _walkspeed; }
        }

        public override string RuntimeClass
        {
            get
            {
                return "cListenerScooterYHopper";
            }
        }


    }

    class cListenerScooterYHopperAir : cListenerScooterYHopper
    {
        // what way the player is facing. true is right and false is left
        private bool facing = true;

        public bool faceLeft = false;
        public bool faceRight = true;


        public cListenerScooterYHopperAir(float walksp = 6.0f, float hopst = 2.0f) :
            base(walksp, hopst)
        {
        }

        public override void listen(float dt, cCritter pcritter)
        {
            cKeyInfo pcontroller = Framework.Keydev;
            /* Note that since I set the velocity to 0.0 when I'm not
            pressing an arrow key, this means that acceraltion forces don't get
            to have accumulating effects on a critter with a cListenerScooter listener. So rather
            than having some very half-way kinds of acceleration effects, I go aheand and
            set acceleration to 0.0 in here. */
            //	pcritter->setAcceleration(cVector(0.0, pcritter->acceleration().y(), 0.0));	 
            float yvelocity = pcritter.Velocity.Y; /* Save this and restore it before we leave
			this call, so that gravity can act in the y direction. */
            //Translate 
            /* I want to move the critter position. But I don't
            just use a moveTo because I want to have a correct _velocity inside the 
            critter so I can use it to hit things and bounce and so on.  So I change
            the velocity.*/
            bool inreverse = false; //Only set TRUE if currently pressing VK_DOWN 

            bool left = Framework.Keydev[vk.A];
            bool right = Framework.Keydev[vk.D];
            bool up = Framework.Keydev[vk.W];
            bool down = Framework.Keydev[vk.S];
            bool pageup = Framework.Keydev[vk.PageUp];
            bool pagedown = Framework.Keydev[vk.PageDown];
            bool super = Framework.Keydev[vk.L];


            //Console.WriteLine("Age: " + pcritter.Age + " Shot: " + (pcritter.Age - lastHit));
            // player invicibility
            if (super)
            {
                
                if (pcritter.Age - pcritter.LastShield >= 0.5f)
                {
                    //Console.WriteLine("Age: " + pcritter.Age + " Shot: " + (pcritter.Age - pcritter.LastShield));
                    pcritter.LastShield = pcritter.Age;
                    //Console.WriteLine("Age: " + pcritter.Age + " Shot: " + (pcritter.Age - pcritter.LastShield));
                    pcritter.Shield = !pcritter.Shield;
                    if (pcritter.Shield)
                    {
                        pcritter.Sprite = new cSpriteQuake(ModelsMD2.MrSkeltalBlue);
                        pcritter.setRadius(cGame3D.PLAYERRADIUS);
                    }
                    else
                    {
                        pcritter.Sprite = new cSpriteQuake(ModelsMD2.MrSkeltal);
                        pcritter.setRadius(cGame3D.PLAYERRADIUS);
                    }
                }
            }

            //Hopping code 
            // I've rewritten this code so the player can't hop again in midair --  
            // unless forces other than gravity act on it in midair -- oh, well, it's  
            // still not perfect -- JC 
            if (up && !_hopping)
            {
                //Pulse upwards 
                saveMaxSpeed = pcritter.MaxSpeed;
                pcritter.MaxSpeed = cGame3D.MAXPLAYERSPEED;
                pcritter.addAcceleration(new cVector3(0.0f, _hopstrength * pcritter.ListenerAcceleration, 0.0f));
                _hopping = true;
                _falling = false;
                _lastSpeed = pcritter.MaxSpeed;
                pcritter.Sprite.ModelState = State.Jump;
            }
            else
            {
                if (_hopping && _lastSpeed < pcritter.Speed)
                {
                    _falling = true;
                }
                else if (_hopping && _falling)
                {
                    _hopping = false; // player has landed  
                    _falling = false;
                    pcritter.MaxSpeed = saveMaxSpeed;
                }
                _lastSpeed = pcritter.Speed;
            }

            if (down)
            {
                pcritter.Sprite.ModelState = State.Crouch;
            }
            if (right)
            {
                faceRight = true;
                faceLeft = false;
                pcritter.Velocity = pcritter.AttitudeTangent.mult(WalkSpeed);
                pcritter.Sprite.ModelState = State.Run;
            }
            if (left)
            {
                faceLeft = true;
                faceRight = false;
                pcritter.Velocity = pcritter.AttitudeTangent.mult(WalkSpeed);
                pcritter.Sprite.ModelState = State.Run;
                //inreverse = true;
            }
            
            if (!left && !right) {
                pcritter.Velocity = new cVector3(0.0f, 0.0f, 0.0f);
            }

            if (!left && !right && !down)
            {
                pcritter.Sprite.ModelState = State.Idle;
            }

            //Now restore the y velocity.
            pcritter.Velocity = new cVector3(pcritter.Velocity.X, yvelocity, pcritter.Velocity.Z);
            //	Real inreversesign = inreverse?-1.0:1.0; 

            if (!_hopping && !left && !right && !pagedown && !pageup)
                return;
            /* If you get here, you've pressed an arrow key or a hop key. */
            if (!_hopping && (left || right))
            {
                /* To rotate, Do three things.
            (a) Match the motion vectors to the visible attitude.
            (b) Rotate the motion vectors withe the arrow keys.
            (c) Match the attitude to the altered motion vectors. */
                //(a) Match the motion matrix to the attitude.
                pcritter.copyAttitudeMatrixToMotionMatrix(); //Changes _velocity 
                if (inreverse) //Keep the tangent and atttitudeTangent in opposite directions.
                    pcritter.yaw((float)Math.PI); //This puts _velocity back in the correct direction.
                //(b) Alter the motion matrix.
                if (right && !facing)
                {
                    cCritterViewer.customOffset = new cVector3(1.75f, -5.0f, 0.0f);
                    pcritter.yaw((float)Math.PI);
                    //pcritter.Sprite.rotate(new cSpin((float)Math.PI));
                    pcritter.rotateAttitude(pcritter.Tangent.rotationAngle(pcritter.AttitudeTangent));
                    facing = !facing;
                }

                else if (left && facing)
                {
                    cCritterViewer.customOffset = new cVector3(1.75f, 5.0f, 0.0f);
                   // Offset = (new cVector3(1.5f, 5.0f, 0.0f));
                    pcritter.yaw(-(float)Math.PI);
                    //pcritter.Sprite.rotate(new cSpin((float)Math.PI));
                    pcritter.rotateAttitude(pcritter.Tangent.rotationAngle(pcritter.AttitudeTangent));
                    facing = !facing;
                }
                //(c) Match the attitude to the motion matrix.
                pcritter.copyMotionMatrixToAttitudeMatrix();
                if (inreverse) //Keep the tangent and atttitudeTangent in opposite directions.
                    pcritter.rotateAttitude((float)-Math.PI);
                pcritter.Velocity = //Restore y velocity in case you changed it again.
                    new cVector3(pcritter.Velocity.X, yvelocity, pcritter.Velocity.Z);
            }
        }


        public override bool IsKindOf(string str)
        {
            return str == "cListenerScooterYHopperAir" || base.IsKindOf(str);
        }

        public override string RuntimeClass
        {
            get
            {
                return "cListenerScooterYHopperAir";
            }
        }
    }

    class cListenerScooter : cListener /* cListenerScooter works well in 2D and in 3D. */
    {

        public cListenerScooter() { }

        public override void listen(float dt, cCritter pcritter)
        {
            cKeyInfo pcontroller = Framework.Keydev;
            /* Note that since I set the velocity to 0.0 when I'm not
            pressing an arrow key, this means that acceraltion forces don't get
            to have accumulating effects on a critter with a cListenerScooter listener. So rather
            than having some very half-way kinds of acceleration effects, I go aheand and
            set acceleration to 0.0 in here. */
            pcritter.Acceleration = new cVector3(0.0f, 0.0f, 0.0f);
            //Translate 
            /* I want to move the critter position. But I don't
            just use a moveTo because I want to have a correct _velocity inside the 
            critter so I can use it to hit things and bounce and so on.  So I change
            the velocity.*/
            bool inreverse = false; //Only set TRUE if currently pressing VK_DOWN 
            bool left = Framework.Keydev[vk.Left];
            bool right = Framework.Keydev[vk.Right];
            bool up = Framework.Keydev[vk.Up];
            bool down = Framework.Keydev[vk.Down];
            bool pageup = Framework.Keydev[vk.PageUp];
            bool pagedown = Framework.Keydev[vk.PageDown];
            bool home = Framework.Keydev[vk.Home];
            bool end = Framework.Keydev[vk.End];
            if (up)
                pcritter.Velocity = pcritter.AttitudeTangent.mult(pcritter.MaxSpeed);
            if (down)
            {
                pcritter.Velocity = pcritter.AttitudeTangent.mult(-pcritter.MaxSpeed);
                inreverse = true;
            }
            if (!up && !down)
                pcritter.Velocity = new cVector3(0.0f, 0.0f, 0.0f);
            //	Real inreversesign = inreverse?-1.0:1.0; 
            //Turn 
            if (!left && !right && !home && !end && !pagedown && !pageup)
                return;
            /* If you get here, you've pressed an arrow key.  Do three things.
            (a) Match the motion vectors to the visible attitude.
            (b) Rotate the motion vectors withe the arrow keys.
            (c) Match the attitude to the altered motion vectors. */
            //(a) Match the motion matrix to the attitude.
            pcritter.copyAttitudeMatrixToMotionMatrix(); //Changes _velocity 
            if (inreverse) //Keep the tangent and atttitudeTangent in opposite directions.
                pcritter.yaw((float)Math.PI); //This puts _velocity back in the correct direction.
            //(b) Alter the motion matrix.
            if (left)
                pcritter.yaw(dt * turnspeed(pcontroller.keystateage(vk.Left)));
            if (right)
                pcritter.yaw(-dt * turnspeed(pcontroller.keystateage(vk.Right)));
            if (pagedown)
                pcritter.pitch(dt * turnspeed(pcontroller.keystateage(vk.PageDown)));
            if (pageup)
                pcritter.pitch(-dt * turnspeed(pcontroller.keystateage(vk.PageUp)));
            if (home)
                pcritter.roll(-dt * turnspeed(pcontroller.keystateage(vk.Home)));
            if (end)
                pcritter.roll(dt * turnspeed(pcontroller.keystateage(vk.End)));
            //(c) Match the attitude to the motion matrix.
            pcritter.copyMotionMatrixToAttitudeMatrix();
            if (inreverse) //Keep the tangent and atttitudeTangent in opposite directions.
                pcritter.rotateAttitude((float)-Math.PI);
        }

        public override bool IsKindOf(string str)
        {
            return str == "cListenerScooter" || base.IsKindOf(str);
        }

        public override string RuntimeClass
        {
            get
            {
                return "cListenerScooter";
            }
        }


    }

    class cListenerCar : cListener
    {

        public cListenerCar() { }

        public override void listen(float dt, cCritter pcritter)
        {
            //Translate 
            cKeyInfo pcontroller = Framework.Keydev;
            bool left = Framework.Keydev[vk.Left];
            bool right = Framework.Keydev[vk.Right];
            bool up = Framework.Keydev[vk.Up];
            bool down = Framework.Keydev[vk.Down];
            bool pageup = Framework.Keydev[vk.PageUp];
            bool pagedown = Framework.Keydev[vk.PageDown];
            bool home = Framework.Keydev[vk.Home];
            bool end = Framework.Keydev[vk.End];
            if (up)
                pcritter.addAcceleration(
                    pcritter.AttitudeTangent.mult(pcritter.ListenerAcceleration));
            if (down)
                pcritter.addAcceleration(
                    pcritter.AttitudeTangent.mult(-pcritter.ListenerAcceleration));
            bool inreverse = pcritter.Tangent.mod(pcritter.AttitudeTangent) < 0.0f;

            if (!left && !right && !home && !end && !pagedown && !pageup)
                return;
            /* If you get here, you've pressed an arrow key.  Do three things.
            (a) Match the motion vectors to the visible attitude.
            (b) Rotate the motion vectors with the arrow keys.
            (c) Match the attitude to the altered motion vectors. */
            //(a) Match the motion matrix to the attitude.
            pcritter.copyAttitudeMatrixToMotionMatrix();
            if (inreverse) //Keep the tangent and atttitudeTangent in opposite directions.
                pcritter.yaw((float)Math.PI);
            //(b) Alter the motion matrix.
            //2D Turn 
            if (left)
                pcritter.yaw(dt * turnspeed(pcontroller.keystateage(vk.Left)));
            if (right)
                pcritter.yaw(dt * -turnspeed(pcontroller.keystateage(vk.Right)));
            //Note that Left and Right Cancel out if both are down.
            //3D Turn 
            if (pagedown)
                pcritter.pitch(dt * turnspeed(pcontroller.keystateage(vk.PageDown)));
            if (pageup)
                pcritter.pitch(-dt * turnspeed(pcontroller.keystateage(vk.PageUp)));
            if (home)
                pcritter.roll(-dt * turnspeed(pcontroller.keystateage(vk.Home)));
            if (end)
                pcritter.roll(dt * turnspeed(pcontroller.keystateage(vk.End)));
            //(c) match the attitude to the motion matrix.
            pcritter.copyMotionMatrixToAttitudeMatrix();
            if (inreverse) //Keep the tangent and atttitudeTangent in opposite directions.
                pcritter.rotateAttitude((float)Math.PI);
            //Note that if pcritter is cCritterArmed*, then the cCritterArmed.listen does more stuff.
        }

        public override bool IsKindOf(string str)
        {
            return str == "cListenerCar" || base.IsKindOf(str);
        }

        public override string RuntimeClass
        {
            get
            {
                return "cListenerCar";
            }
        }


    }

    class cListenerSpaceship : cListener
    {

        public cListenerSpaceship() { }

        public override void listen(float dt, cCritter pcritter)
        {
            cKeyInfo pcontroller = Framework.Keydev;
            //Note that for the cListenerSpaceship, the pcritter->_direction is NOT locked to the _attitude 
            //Blast rocket 
            cVector3 blastacceleration = new cVector3();
            bool left = Framework.Keydev[vk.Left];
            bool right = Framework.Keydev[vk.Right];
            bool up = Framework.Keydev[vk.Up];
            bool down = Framework.Keydev[vk.Down];
            bool pageup = Framework.Keydev[vk.PageUp];
            bool pagedown = Framework.Keydev[vk.PageDown];
            bool home = Framework.Keydev[vk.Home];
            bool end = Framework.Keydev[vk.End];
            if (up)
                blastacceleration =
                    pcritter.AttitudeTangent.mult(pcritter.ListenerAcceleration);
            if (down)
                blastacceleration =
                    pcritter.AttitudeTangent.mult(-pcritter.ListenerAcceleration);
            if (up == down) //both on or both off 
                blastacceleration = new cVector3(0.0f, 0.0f, 0.0f);
            pcritter.addAcceleration(blastacceleration);

            //Rotate 2D 
            if (left)
            {
                pcritter.rotateAttitude(
                    new cSpin(dt * turnspeed(pcontroller.keystateage(vk.Left)),
                        pcritter.AttitudeBinormal));
            }
            if (right)
                pcritter.rotateAttitude(
                    new cSpin(-dt * turnspeed(pcontroller.keystateage(vk.Right)),
                        pcritter.AttitudeBinormal));
            //3D Turn  
            if (pagedown)
                pcritter.rotateAttitude(
                    new cSpin(dt * turnspeed(pcontroller.keystateage(vk.PageDown)),
                        pcritter.AttitudeNormal));
            if (up)
                pcritter.rotateAttitude(
                    new cSpin(-dt * turnspeed(pcontroller.keystateage(vk.PageUp)),
                        pcritter.AttitudeNormal));
            if (home)
                pcritter.rotateAttitude(
                    new cSpin(dt * turnspeed(pcontroller.keystateage(vk.Home)),
                        pcritter.AttitudeTangent));
            if (end)
                pcritter.rotateAttitude(
                    new cSpin(-dt * turnspeed(pcontroller.keystateage(vk.End)),
                        pcritter.AttitudeTangent));
            //Note that if pcritter is cCritterArmed*, then the cCritterArmed.listen does more stuff.
        }

        public override bool IsKindOf(string str)
        {
            return str == "cListenerSpaceship" || base.IsKindOf(str);
        }

        public override string RuntimeClass
        {
            get
            {
                return "cListenerSpaceship";
            }
        }


    }

    class cListenerCursor : cListener
    {
        public static readonly float TOOSMALLAMOVE = 0.1f; /* Accumulate mouse moves till you get a 
			magnitude larger than this, otherwise you'll see too much wobble. */
        public static readonly float CURSORSPEED = 100.0f;
        protected bool _attached;

        public cListenerCursor()
        {
            _attached = true;
        }

        public override void install(cCritter pcritter)
        {
            base.install(pcritter);
            pcritter.TempMaxSpeed = cListenerCursor.CURSORSPEED; /* Give a critter with a cursor listener a very high max speed
			so that I can use large velocities of the form displacement/dt . */
        }


        public override void listen(float dt, cCritter pcritter)
        {
            cKeyInfo pcontroller = Framework.Keydev;
            /* Unlike the other listeners, the cListenerCursor is going to have
             pcritter ignore
            any forces.  This is so it can strictly move with the cursor.
             So we zero out
            the acceleration each time.  We also zero out the velocity so the thing 
            doesn't
            have inertia. */
            pcritter.Velocity = new cVector3(0.0f, 0.0f, 0.0f);
            pcritter.Acceleration = new cVector3(0.0f, 0.0f, 0.0f);
            if (_attached && dt > 0.00001f) //Check dt so don't divide by zero. (_attached always TRUE) 
            {
                cVector3 cursorposclamp = pcritter.Game.CursorPos;
                cRealBox3 effectivebox = pcritter.MoveBox.innerBox(pcritter.Radius);
                //		ASSERT (pcritter->in3DWorld() || effectivebox.zsize() == 0.0); 
                effectivebox.clamp(cursorposclamp);
                cVector3 displacement = (cursorposclamp.sub(pcritter.Position));
                if (displacement.Magnitude > cListenerCursor.TOOSMALLAMOVE)
                //The if condition is because you get wobble if you adjust for too small a step.
                {
                    /* I want to move the critter position to cursorposclamp.  But I don't
                    just use a moveTo because I want to have a correct _velocity inside the 
                    critter so I can use it to hit things and bounce and so on.  So I change
                    the velocity.  Alternately I could change the acceleration, but for sudden
                    moves, it is preferagle to make  an "impulse" change by using velocity and
                    not acceleration. */
                    pcritter.Velocity = displacement.mult(1.0f / dt);
                    pcritter.copyMotionMatrixToAttitudeMatrix();
                }
            }
            //If no rotation keys have been pressed, you're done.
            //Note that if pcritter is cCritterArmed*, then the cCritterArmed.listen does more stuff.
            bool left = Framework.Keydev[vk.Left];
            bool right = Framework.Keydev[vk.Right];
            bool pageup = Framework.Keydev[vk.PageUp];
            bool pagedown = Framework.Keydev[vk.PageDown];
            bool home = Framework.Keydev[vk.Home];
            bool end = Framework.Keydev[vk.End];
            if (left)
                pcritter.yaw(dt * turnspeed(pcontroller.keystateage(vk.Left)));
            if (right)
                pcritter.yaw(-dt * turnspeed(pcontroller.keystateage(vk.Right)));
            if (pagedown)
                pcritter.pitch(dt * turnspeed(pcontroller.keystateage(vk.PageDown)));
            if (pageup)
                pcritter.pitch(-dt * turnspeed(pcontroller.keystateage(vk.PageUp)));
            if (home)
                pcritter.roll(-dt * turnspeed(pcontroller.keystateage(vk.Home)));
            if (end)
                pcritter.roll(dt * turnspeed(pcontroller.keystateage(vk.End)));
            //(c) Match the attitude to the motion matrix.
            pcritter.copyMotionMatrixToAttitudeMatrix();
        }

        public override bool IsKindOf(string str)
        {
            return str == "cListenerCursor" || base.IsKindOf(str);
        }

        public virtual bool Attached
        {
            get
            { return _attached; }
        }

        public override string RuntimeClass
        {
            get
            {
                return "cListenerCursor";
            }
        }


    }

    //The following are special listeners used by the cCritterViewer 

    class cListenerViewerRide : cListener //Assume 3D 
    {
        //Some static readonlys for adjusting the riderview 
        public static readonly cVector3 OFFSETDIR = new cVector3(-1.1f, 0.0f, 2.0f);
        public static readonly float PLAYERLOOKAHEAD = 8.0f;
        protected cVector3 _offset; //offset from the player, if you are riding on him.

        public cListenerViewerRide() { } // We will initialize _offset inside the install call.

        public override void install(cCritter pcritter)
        {
            base.install(pcritter);
            cCritterViewer pcritterv = (cCritterViewer)(pcritter);
            pcritterv.Perspective = true; //Assume we always use this with _perspective on.
            pcritter.AttitudeToMotionLock = false;
            _offset = cListenerViewerRide.OFFSETDIR.mult(pcritter.Player.Radius);
        }


        public override void listen(float dt, cCritter pcritter)
        {
            cCritterViewer pcritterv = (cCritterViewer)(pcritter);
            //Need the cast to use cCritterViewer.zoom.
            //Read the keys and maybe adjust the _offset 
            bool controlLeft = Framework.Keydev[vk.ControlLeft];
            bool controlRight = Framework.Keydev[vk.ControlRight];
            bool up = Framework.Keydev[vk.Up];
            bool down = Framework.Keydev[vk.Down];
            bool insert = Framework.Keydev[vk.Insert];
            bool delete = Framework.Keydev[vk.Delete];
            bool control = controlLeft || controlRight;
            if (up && control)
                _offset.addassign(new cVector3(0.0f, 0.0f, RIDESTEP));
            else if (down && control)
                _offset.addassign(new cVector3(0.0f, 0.0f, -RIDESTEP));
            /* The idea here is to not let the offset get unreasonably high or low. */
            if (_offset.Z < -15.0f * pcritter.Radius)
                _offset.Z = -15.0f * pcritter.Radius;
            if (_offset.Z > 15.0f * pcritter.Radius)
                _offset.Z = 15.0f * pcritter.Radius;
            /* Use the Insert, Delete keys to zoom. Control should be off; as it happens the Control key actually
        blocks processing of the Insert key. */
            if (insert)
                pcritterv.zoom(cCritterViewer.DEFAULTZOOMFACTOR);
            if (delete)
                pcritterv.zoom(1.0f / cCritterViewer.DEFAULTZOOMFACTOR);
            /* We make it the responsibility of the pcritter to match position to player()+_offset in the
        cCritterViewer.update method.  Why don't we do this here in cListenerViewerRide.listen?  The
        reason is that if I have two views of a game with a cListenerViewerRide viewpoint in each
        view, I want them to both be updating, even though I don't want to have both of them
        them listening. */
        }

        public override bool IsKindOf(string str)
        {
            return str == "cListenerViewerRide" || base.IsKindOf(str);
        }

        public virtual cVector3 Offset
        {
            get
            {
                cVector3 offset = new cVector3();
                offset.copy(_offset);
                return offset;
            }
            set
            {
                _offset.copy(value);
            }
        }

        public override string RuntimeClass
        {
            get
            {
                return "cListenerViewerRide";
            }
        }


    }


    class cListenerViewerFly : cListener //Assume 3D 
    {

        public cListenerViewerFly() { }

        public override void install(cCritter pcritter)
        {
            base.install(pcritter);
            cCritterViewer pcritterv = (cCritterViewer)(pcritter);
            pcritterv.Perspective = true; //Assume we always use this with _perspective on.
            pcritter.AttitudeToMotionLock = true;
            /* We will move the 
        critter around and match the attitude to the position. */
        }


        public override void listen(float dt, cCritter pcritter)
        {

            //Translate 
            bool controlLeft = Framework.Keydev[vk.ControlLeft];
            bool controlRight = Framework.Keydev[vk.ControlRight];
            bool shiftLeft = Framework.Keydev[vk.ShiftLeft];
            bool shiftRight = Framework.Keydev[vk.ShiftRight];
            bool up = Framework.Keydev[vk.Up];
            bool down = Framework.Keydev[vk.Down];
            bool left = Framework.Keydev[vk.Left];
            bool right = Framework.Keydev[vk.Right];
            bool pageup = Framework.Keydev[vk.PageUp];
            bool pagedown = Framework.Keydev[vk.PageDown];
            bool home = Framework.Keydev[vk.Home];
            bool end = Framework.Keydev[vk.End];
            bool insert = Framework.Keydev[vk.Insert];
            bool delete = Framework.Keydev[vk.Delete];
            bool control = controlLeft || controlRight;
            bool shift = shiftLeft || shiftRight;
            if (up && control && !shift)
                pcritter.moveTo(pcritter.Position.sub(pcritter.Binormal.mult(MOVEVIEW * dt * pcritter.MaxSpeed)));
            if (down && control && !shift)
                pcritter.moveTo(pcritter.Position.add(pcritter.Binormal.mult(MOVEVIEW * dt * pcritter.MaxSpeed)));
            if (left && control && !shift)
                pcritter.moveTo(pcritter.Position.sub(pcritter.Normal.mult(MOVEVIEW * dt * pcritter.MaxSpeed)));
            if (right && control && !shift)
                pcritter.moveTo(pcritter.Position.add(pcritter.Normal.mult(MOVEVIEW * dt * pcritter.MaxSpeed)));
            if (pageup && control && !shift)
                pcritter.moveTo(pcritter.Position.add(pcritter.Tangent.mult(dt * pcritter.MaxSpeed)));
            if (pagedown && control && !shift)
                pcritter.moveTo(pcritter.Position.sub(pcritter.Tangent.mult(dt * pcritter.MaxSpeed)));
            //Turn 
            if (left && control && shift)
                pcritter.yaw(-MOVEVIEW * dt * (float)Math.PI / 3.0f);
            if (right && control && shift)
                pcritter.yaw(MOVEVIEW * dt * (float)Math.PI / 3.0f);
            if (up && control && shift)
                pcritter.pitch(MOVEVIEW * dt * (float)Math.PI / 3.0f);
            if (down && control && shift)
                pcritter.pitch(-MOVEVIEW * dt * (float)Math.PI / 3.0f);
            if (home && control && shift)
                pcritter.roll(MOVEVIEW * dt * (float)Math.PI / 3.0f);
            if (end && control && shift)
                pcritter.roll(-MOVEVIEW * dt * (float)Math.PI / 3.0f);
            /* Use the Insert, Delete keys to zoom. Control should be off; as it happens the Control key actually
        blocks processing of the Insert key. */
            cCritterViewer pcritterv = (cCritterViewer)(pcritter);
            //Need the cast to use zoom 
            if (insert)
                pcritterv.zoom(cCritterViewer.DEFAULTZOOMFACTOR);
            if (delete)
                pcritterv.zoom(1.0f / cCritterViewer.DEFAULTZOOMFACTOR);
        }

        public override bool IsKindOf(string str)
        {
            return str == "cListenerViewerFly" || base.IsKindOf(str);
        }

        public override string RuntimeClass
        {
            get
            {
                return "cListenerViewerFly";
            }
        }


    }

    class cListenerViewerOrtho : cListener //Assume 2D 
    {

        public cListenerViewerOrtho() { }

        public override void install(cCritter pcritter)
        {
            base.install(pcritter);
            cCritterViewer pcritterv = (cCritterViewer)(pcritter);

            pcritter.AttitudeToMotionLock = false;
            pcritterv.Perspective = false; //2D 
        }


        public override void listen(float dt, cCritter pcritter)
        {
            cCritterViewer pcritterv = (cCritterViewer)(pcritter);
            //Need the cast to use zoom 
            //Use the Control + (Arrow keys, Insert or Delete) to translate.
            bool controlLeft = Framework.Keydev[vk.ControlLeft];
            bool controlRight = Framework.Keydev[vk.ControlRight];
            bool up = Framework.Keydev[vk.Up];
            bool down = Framework.Keydev[vk.Down];
            bool left = Framework.Keydev[vk.Left];
            bool right = Framework.Keydev[vk.Right];
            bool insert = Framework.Keydev[vk.Insert];
            bool delete = Framework.Keydev[vk.Delete];
            bool control = controlLeft || controlRight;
            if (left && control)
                pcritter.Velocity = new cVector3(MOVEVIEW * pcritter.MaxSpeed, 0.0f, 0.0f);
            if (right && control)
                pcritter.Velocity = new cVector3(-MOVEVIEW * pcritter.MaxSpeed, 0.0f, 0.0f);
            if (down && control)
                pcritter.Velocity = new cVector3(0.0f, MOVEVIEW * pcritter.MaxSpeed, 0.0f);
            if (up && control)
                pcritter.Velocity = new cVector3(0.0f, -MOVEVIEW * pcritter.MaxSpeed, 0.0f);
            if (!(control && (left || right || down || up)))
                pcritter.Velocity = new cVector3(0.0f, 0.0f, 0.0f);
            /* Use the Insert, Delete keys to zoom. Control should be off; as it happens the Control key actually
        blocks processing of the Insert key. */
            if (insert)
                pcritterv.zoom(cCritterViewer.DEFAULTZOOMFACTOR);
            if (delete)
                pcritterv.zoom(1.0f / cCritterViewer.DEFAULTZOOMFACTOR);

            if (pcritter.Velocity.Z != 0.0f) //Pointless to move in z if its a nonperspective viewer? 
                pcritter.Velocity = new cVector3(pcritter.Velocity.X, pcritter.Velocity.Y, 0.0f);
        }

        public override bool IsKindOf(string str)
        {
            return str == "cListenerViewerOrtho" || base.IsKindOf(str);
        }

        public override string RuntimeClass
        {
            get
            {
                return "cListenerViewerOrtho";
            }
        }


    }

    class cListenerViewerTranslate : cListener //Assume 3D.  Not used yet.
    {

        public cListenerViewerTranslate() { }

        public override void install(cCritter pcritter)
        {
            base.install(pcritter);
            cCritterViewer pcritterv = (cCritterViewer)(pcritter);

            pcritter.copyAttitudeMatrixToMotionMatrix();
            pcritter.AttitudeToMotionLock = false;
            pcritterv.Perspective = true;
        }


        public override void listen(float dt, cCritter pcritter)
        {
            cCritterViewer pcritterv = (cCritterViewer)(pcritter);
            //Need the cast to use zoom 
            //Use the Control + (Arrow keys, Insert or Delete) to translate.
            bool controlLeft = Framework.Keydev[vk.ControlLeft];
            bool controlRight = Framework.Keydev[vk.ControlRight];
            bool shiftLeft = Framework.Keydev[vk.ShiftLeft];
            bool shiftRight = Framework.Keydev[vk.ShiftRight];
            bool up = Framework.Keydev[vk.Up];
            bool down = Framework.Keydev[vk.Down];
            bool left = Framework.Keydev[vk.Left];
            bool right = Framework.Keydev[vk.Right];
            bool pageup = Framework.Keydev[vk.PageUp];
            bool pagedown = Framework.Keydev[vk.PageDown];
            bool insert = Framework.Keydev[vk.Insert];
            bool delete = Framework.Keydev[vk.Delete];
            bool control = controlLeft || controlRight;
            bool shift = shiftLeft || shiftRight;
            if (left && control && !shift)
                pcritter.Velocity = new cVector3(pcritter.MaxSpeed, 0.0f, 0.0f);
            if (right && control && !shift)
                pcritter.Velocity = new cVector3(-pcritter.MaxSpeed, 0.0f, 0.0f);
            if (down && control && !shift)
                pcritter.Velocity = new cVector3(0.0f, pcritter.MaxSpeed, 0.0f);
            if (up && control && !shift)
                pcritter.Velocity = new cVector3(0.0f, -pcritter.MaxSpeed, 0.0f);
            if (pageup && control && !shift)
                pcritter.Velocity = new cVector3(0.0f, 0.0f, pcritter.MaxSpeed);
            if (pagedown && control && !shift)
                pcritter.Velocity = new cVector3(0.0f, 0.0f, -pcritter.MaxSpeed);
            if (!(control && !shift && (left || right || down || up || pagedown || pageup)))
                pcritter.Velocity = new cVector3(0.0f, 0.0f, 0.0f);
            /* Use the Insert, Delete keys to zoom. Control should be off; as it happens the Control key actually
        blocks processing of the Insert key. */
            if (insert)
                pcritterv.zoom(cCritterViewer.DEFAULTZOOMFACTOR);
            if (delete)
                pcritterv.zoom(1.0f / cCritterViewer.DEFAULTZOOMFACTOR);

            //Use Control + Shift +  (Arrow keys, Insert or Delete) to turn.
            if (left && control && shift)
                pcritter.setSpin(((float)Math.PI / 3.0f), new cVector3(0.0f, 0.0f, 1.0f));
            else if (right && control && shift)
                pcritter.setSpin(-((float)Math.PI / 3.0f), new cVector3(0.0f, 0.0f, 1.0f));
            else if (down && control && shift)
                pcritter.setSpin(-((float)Math.PI / 3.0f), new cVector3(0.0f, 1.0f, 0.0f)); //odd 
            else if (up && control && shift)
                pcritter.setSpin(((float)Math.PI / 3.0f), new cVector3(0.0f, 1.0f, 0.0f)); //odd 
            else if (pageup && control && shift)
                pcritter.setSpin(-((float)Math.PI / 3.0f), new cVector3(1.0f, 0.0f, 0.0f)); //fine 
            else if (pagedown && control && shift)
                pcritter.setSpin(((float)Math.PI / 3.0f), new cVector3(1.0f, 0.0f, 0.0f)); //fine 
            else
                pcritter.setSpin(0.0f, new cVector3(0.0f, 0.0f, 1.0f));
            if (pcritter.Velocity.Z != 0.0f) //Pointless to move in z if its a nonperspective viewer? 
                pcritter.Velocity = new cVector3(pcritter.Velocity.X, pcritter.Velocity.Y, 0.0f);
        }

        public override bool IsKindOf(string str)
        {
            return str == "cListenerViewerTranslate" || base.IsKindOf(str);
        }

        public override string RuntimeClass
        {
            get
            {
                return "cListenerViewerTranslate";
            }
        }


    }

    class cListenerViewerOrbit : cListener //3D, but not used yet.
    {

        public cListenerViewerOrbit() { }

        public override void install(cCritter pcritter)
        {
            base.install(pcritter);
            cCritterViewer pcritterv = (cCritterViewer)(pcritter);
            pcritterv.Perspective = true; //Assume we always use this with _perspective on.
            pcritter.copyAttitudeMatrixToMotionMatrix();
            pcritter.AttitudeToMotionLock = true;
            /* We keep the critter pointed at the origin, and lock the attitude. */
        }


        public override void listen(float dt, cCritter pcritter)
        {
            cVector3 newposition = pcritter.Position; //default 
            bool controlLeft = Framework.Keydev[vk.ControlLeft];
            bool controlRight = Framework.Keydev[vk.ControlRight];
            bool up = Framework.Keydev[vk.Up];
            bool down = Framework.Keydev[vk.Down];
            bool left = Framework.Keydev[vk.Left];
            bool right = Framework.Keydev[vk.Right];
            bool pageup = Framework.Keydev[vk.PageUp];
            bool pagedown = Framework.Keydev[vk.PageDown];
            bool control = controlLeft || controlRight;
            if (left && control)
                newposition.rotate(
                    cSpin.mult(dt, new cSpin(-((float)Math.PI / 3.0f), new cVector3(0.0f, 1.0f, 0.0f))));
            else if (right && control)
                newposition.rotate(
                    cSpin.mult(dt, new cSpin(((float)Math.PI / 3.0f), new cVector3(0.0f, 1.0f, 0.0f))));
            else if (down && control)
                newposition.rotate(
                    cSpin.mult(dt, new cSpin(-((float)Math.PI / 3.0f), new cVector3(1.0f, 0.0f, 0.0f))));
            else if (up && control)
                newposition.rotate(
                    cSpin.mult(dt, new cSpin(((float)Math.PI / 3.0f), new cVector3(1.0f, 0.0f, 0.0f))));
            else if (pageup && control)
                newposition.rotate(
                    cSpin.mult(dt, new cSpin(-((float)Math.PI / 3.0f), new cVector3(0.0f, 0.0f, 1.0f))));
            else if (pagedown && control)
                newposition.rotate(
                    cSpin.mult(dt, new cSpin(((float)Math.PI / 3.0f), new cVector3(0.0f, 0.0f, 1.0f))));
            if (newposition.notequal(pcritter.Position))
            {
                pcritter.moveTo(newposition);
                //Do something extra, try and always look at the origin.
                //		cCritter *pplayer = pcritter->pgame()->pplayer(); 
                //		pcritter->lookAt(pplayer->position()); 
                pcritter.lookAt(new cVector3(0.0f, 0.0f, 0.0f));
                //		pcritter->setTangent(-pcritter->position());  
                /* Point at the origin.  Unfortunately, as currently implemented,
                this call has the unpleasant side effect of sometimes flipping your visual up and down, as when
                you move from, say, (0, 0.1, 10) to (0, -0.1, 10).  Using setAttitudeTangent(-position())
                has the same effect.  This is because both use the cMatrix.rotationFromUnitToUnit
                to turn the normal and binormal along with the tangent. */
            }
        }

        public override bool IsKindOf(string str)
        {
            return str == "cListenerViewerOrbit" || base.IsKindOf(str);
        }

        public override string RuntimeClass
        {
            get
            {
                return "cListenerViewerOrbit";
            }
        }


    }

    // used to get coordinates for helping to set up objects in a game
    class cListenerScooterLevitator : cListener
    {
        protected float _walkspeed; // how fast you can walk 

        public cListenerScooterLevitator(float walksp = 6.0f)
        {
            _walkspeed = walksp;
        }

        public override void listen(float dt, cCritter pcritter)
        {
            cKeyInfo pcontroller = Framework.Keydev;
            /* Note that since I set the velocity to 0.0 when I'm not
            pressing an arrow key, this means that acceraltion forces don't get
            to have accumulating effects on a critter with a cListenerScooter listener. So rather
            than having some very half-way kinds of acceleration effects, I go aheand and
            set acceleration to 0.0 in here. */
            //	pcritter->setAcceleration(cVector(0.0, pcritter->acceleration().y(), 0.0));	 
            float yvelocity = pcritter.Velocity.Y; /* Save this and restore it before we leave
			    this call, so that gravity can act in the y direction. */
            //Translate 
            /* I want to move the critter position. But I don't
            just use a moveTo because I want to have a correct _velocity inside the 
            critter so I can use it to hit things and bounce and so on.  So I change
            the velocity.*/
            bool inreverse = false; //Only set TRUE if currently pressing VK_DOWN 
            bool left = Framework.Keydev[vk.Left];
            bool right = Framework.Keydev[vk.Right];
            bool up = Framework.Keydev[vk.Up];
            bool down = Framework.Keydev[vk.Down];
            bool flyup = Framework.Keydev[vk.U];
            bool flydown = Framework.Keydev[vk.I];
            bool d = Framework.Keydev[vk.D];
            pcritter.clearForcelist();  // in case forces were added somewhere
            pcritter.Game.AddOn = "Position: (" + pcritter.Position.X.ToString() + ", " +
                pcritter.Position.Y.ToString() + ", " + pcritter.Position.Z.ToString() + ")";
            if (up)
                pcritter.Velocity = pcritter.AttitudeTangent.mult(0.1f * pcritter.MaxSpeed);
            if (down)
                pcritter.Velocity = pcritter.AttitudeTangent.mult(0.1f * -pcritter.MaxSpeed);
            if (flyup)
                pcritter.Velocity = new cVector3(0.0f, 0.1f * pcritter.MaxSpeed, 0.0f);
            if (flydown)
                pcritter.Velocity = new cVector3(0.0f, -0.1f * pcritter.MaxSpeed, 0.0f);
            if (!up && !down && !flyup && !flydown)
                pcritter.Velocity = new cVector3(0.0f, 0.0f, 0.0f);
            if (d)
                Framework.view.pviewpointcritter().Listener = new cListenerViewerFly();
            //Now restore the y velocity.
            //            pcritter.Velocity = new cVector3(pcritter.Velocity.X, yvelocity, pcritter.Velocity.Z);
            //	Real inreversesign = inreverse?-1.0:1.0; 

            if (!left && !right)
                return;
            /* If you get here, you've pressed an arrow key or a hop key. */
            if (left || right)
            {
                /* To rotate, Do three things.
            (a) Match the motion vectors to the visible attitude.
            (b) Rotate the motion vectors withe the arrow keys.
            (c) Match the attitude to the altered motion vectors. */
                //(a) Match the motion matrix to the attitude.
                pcritter.copyAttitudeMatrixToMotionMatrix(); //Changes _velocity 
                if (inreverse) //Keep the tangent and atttitudeTangent in opposite directions.
                    pcritter.yaw((float)Math.PI); //This puts _velocity back in the correct direction.
                //(b) Alter the motion matrix.
                if (left)
                    pcritter.yaw(dt * turnspeed(pcontroller.keystateage(vk.Left)));
                if (right)
                    pcritter.yaw(-dt * turnspeed(pcontroller.keystateage(vk.Right)));
                //(c) Match the attitude to the motion matrix.
                pcritter.copyMotionMatrixToAttitudeMatrix();
                if (inreverse) //Keep the tangent and atttitudeTangent in opposite directions.
                    pcritter.rotateAttitude((float)-Math.PI);
                //                pcritter.Velocity = //Restore y velocity in case you changed it again.
                //                    new cVector3(pcritter.Velocity.X, yvelocity, pcritter.Velocity.Z);
            }
        }

        public override bool IsKindOf(string str)
        {
            return str == "cListenerScooterLevitator" || base.IsKindOf(str);
        }

        public virtual float WalkSpeed
        {
            get
            { return _walkspeed; }
        }

        public override string RuntimeClass
        {
            get
            {
                return "cListenerScooterLevitator";
            }
        }


    }


}
//                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            