// For AC Framework 1.2, ZEROVECTOR and other vectors were removed,
// default parameters were added -- JC


using System;
using System.Drawing;

namespace ACFramework
{

    // Sprite.h: interface for the cSprite class.
    // 
    ////////////////////////////////////////////////////////////////////// 

    class cSprite
    {
        //Randomization and mutation flags.  MF stands for mutation flag.
        //The MF_ flags are defined in static readonly.cpp.
        public static readonly int MF_RADIUS = 0x00000008;
        public static readonly int MF_ROTATION = 0x00000010;
        public static readonly int MF_ALL = cSprite.MF_RADIUS | cSprite.MF_ROTATION; //MF_RADIUS | MF_ROTATION.
        //Angular velocity bounds used in mutate(...MF_ROTATION...) 
        public static readonly float MINANGVELOCITY = (float)-Math.PI / 2;
        public static readonly float MAXANGVELOCITY = (float)Math.PI / 2;
        //For drawing highlight circles in draw(...DF_FOCUS...) 
        public static readonly float HIGHLIGHTRATIO = 1.1f;
        public static readonly uint INVALIDID = 0; // Use 0 for this, see SPRITEID 
        //Prism static readonlys 
        public static readonly float CRITTERPRISMDZ = 0.3f; /* The default Z depth to use for a cCritter sprite if we are
			in 3D and have a sprite that uses its _prismdz field. */
        public static readonly float BULLETPRISMDZ = 0.2f; //Default z-depth to use for drawing cCritterBullet.
        //static readonly Real WALLPRISMDZ; //Define this one in critterwall.h instead.
        public static readonly float PLAYERPRISMDZ = 0.5f; //Default z-depth to use for drawing cCritterArmedPlayer.
        public static readonly float MAXPRISMDZ = 1.0f; /* We may make some graphical thickness additions that
			 do to make a 2D game look 3D, such as thick sprites or putting
			foreground or background at a z offset.  Keep all of these 
			within +-MAXPRISMDZ on the z axis so we know to set znear and 
			zfar to include them.  This isn't so much of an issue for a true 3D game,
			it's only an issue for 2D games that we dress up with prism sprites. */
        protected static uint SPRITEID = 1; /* Use this to assign each cSprite its own ID when its
			constructed. If you make four billion sprites, you'll wrap the ID around and trigger
			a ASSERT. We'll let the valid values go from 1 to 0xFFFFFFFF */
        protected uint _spriteID; /* Don't serialize or copy this one, it's unique for each sprite,
			and is set by the constructor. We need it for use in a lookup method for display list
			IDs in cGraphicsOpenGL class. */
        protected cColorStyle _pcolorstyle; //To hold things like fillcolor, texture map, etc.
        protected int _resourceID; /* _resourceID holds the id of any bitmap textue this
			sprite may want to use.  */
        protected float _radius; /* Relates to the internal geometry of the sprite.  Will not usually
			equal the value returned by cSprite::radius(), as this method
			multiplies in the _spriteattiude.scalefactor() */
        protected cMatrix3 _spriteattitude; /* We draw the sprite in the sprite attitude*critter attitude.
			This lets us position the sprite as we like relative to the critter's orientation.
			This is particularly useful in cSpriteComposite where I can group several sprites
			together. */
        protected float _prismdz; /* _prismdz is a kludge variable that I put in so that flat sprites will
			pop out to look 3D when I switch to a 3D graphics like cGraphicsOpenGL.  The
			default is 0.0.  If nonzero _prsimz, it turns a	2D sprite like a cPolygon or a 
			cBubble into a 3D prism or cylinder. */
        protected bool _newgeometryflag; /* This is for use with sophisticated graphics like 
			cGraphicsOpenGL, where we speed up the drawing
			of some kinds of sprites by saving a display list index that encapsulates the steps
			used to draw the sprite.  If you change the intrinsic geometry of the sprite, for
			instance by changing the number of vertices in a polygon, then you need to 
			make a new display list for that sprite, and the _newgeometryflag is the flag
			we have a cSprite use so as to signal this to the cGraphicsOpenGL. 
			At construction, _newgeometryflag is TRUE. We don't serialize this field. */

        public cSprite()
        {
            _prismdz = CRITTERPRISMDZ;
            _newgeometryflag = true;
            _resourceID = -1;
            _spriteattitude = new cMatrix3();
            Center = new cVector3(0.0f, 0.0f); //Sets last column of _spriteattitude.
            /* We start the cSprite with a nonzero radius so that we can readily adjust
        its size by scaling it, that is, by multiplying it by scale factors. Let's start
        it as the average of the cCritter min and max.*/
            _radius = (cCritter.MinRadius + cCritter.MaxRadius) / 2.0f;
            _pcolorstyle = new cColorStyle();
            //Default constructor of _spriteattidute will be an identity matrix.
            _spriteID = cSprite.SPRITEID++; // The the first one gets a value of 1.
        }


        public virtual void setstate(short state, int begf, int endf, short type) { }

        public virtual void copy(cSprite psprite)
        {
            _radius = psprite._radius;
            _prismdz = psprite._prismdz;
            _spriteattitude.copy(psprite._spriteattitude);
            _resourceID = psprite._resourceID;
            cColorStyle c = new cColorStyle();
            c.copy(psprite.pcolorstyle());
            ColorStyle = c;
        }

        public virtual cSprite copy()
        {
            cSprite c = new cSprite();
            c.copy(this);
            return c;
        }


        //Attitude mutators 

        /* The default cSprite method for setRadius 
            does the miniumum, to reset _radius.   cPolygon and cIcon override setRadius, by
            changing the intrinsic geometry of the polygon or the bitmap rectangle. 
            cSpriteComposite overrides to leave _radius alone and instead to scale the
            _spriteattitude method, though this	is in turn overridden for cSpriteShowOneChild.*/

        public virtual void rotate(float angle) { _spriteattitude.multassign(cMatrix3.zRotation(angle)); }

        public virtual void rotate(cSpin spin) { _spriteattitude.multassign(cMatrix3.rotation(spin)); }
        //3D Mutators 

        //Randomizing mutators 

        public virtual void mutate(int mutationflags, float mutationstrength)
        {
            if ((mutationflags & MF_RADIUS) != 0)
                Radius = Framework.randomOb.mutate(
                    _radius, cCritter.MinRadius, cCritter.MaxRadius,
                    mutationstrength);
            if ((mutationflags & MF_ROTATION) != 0)
                rotate(Framework.randomOb.mutate(Angle, 0.0f, (float)Math.PI, mutationstrength));
            _pcolorstyle.mutate(mutationflags, mutationstrength);
            _newgeometryflag = true;
        }


        public virtual void randomizeColor() { }

        public virtual void randomize(int mutationflags) { mutate(mutationflags, 1.0f); }
        //cColorstyle Mutators 

        //Texture mutator 

        public void setResourceID(int resourceid) { _resourceID = resourceid; } /* We'll count
			on the cGraphics to free the old resource bitmap memory and allocate the
			new resrouce bitmap memory as part of this sprite's next call to draw. */

        public virtual void fixResourceID() { } /* This virtual helper method is needed
			because I plan to use _resourceID in cBiota::Add to see if
			two sprites use the same textures.  A cSpriteComposite
			will adopt as its _resourceID the first non-trivial resourceID of
			a member, and a cSpriteQuake will set a _resoruceID keyed to
			its skin file. */
        //accessors 

        public virtual bool IsKindOf(string name)
        { return name == "cSprite"; }

        /* Default is _radius * _spriteattitude.scalefactor() */

        public cColorStyle pcolorstyle() { return _pcolorstyle; } /* Don't call this accessor const,
			as we usually use it to then do something to _pcolorstyle. */

        /* enabledisplaylist specifies whetehr or not you want to
            try and use a display list to show this sprites. By default 
            this is TRUE.  We make this virtual so I can turn it off
            for cSpriteIcon, which seems to runs slower with display 
            lists. */

        //Attribute accessors 
        /* The purpose of the attribute accessors is to turn on and off
        attributes of the cGraphics in one cGraphics::adjustAttributes call.
        It's especially necessary to turn the texture attribute on and off
        according to whether its used, as you can't fill polygons and draw
        textures without changing. In some cases turning off an attribute you
        don't need might help the speed. */

        //cColorstyle accessors 

        //methods 

        public virtual void animate(float dt, cCritter powner) { } /* Might choose sprite
			based on age, change the radius of the sprite with time, or choose a sprite based
			on direction or health of powner, or maybe spin. */

        public virtual void draw(cGraphics pgraphics)
        {
            draw(pgraphics, 0);
        }

        public virtual void draw(cGraphics pgraphics, int drawflags)
        { /* This is an example of the Template Method.  For the primitive (non-composite)
		 sprites we only overload the imagedraw method and use this template code. */
            if ((drawflags & ACView.DF_WIREFRAME) != 0)
            {
                if (IsKindOf("cSpriteQuake"))
                {
                    cSpriteSphere sphere = new cSpriteSphere(_radius, 6, 6);
                    sphere.LineColor = Color.Black;
                    sphere.draw(pgraphics, drawflags);
                }
            }
            pgraphics.pushMatrix();
            pgraphics.multMatrix(_spriteattitude);
            /* If I don't have UNCONDITIONAL_ADJUSTATTRIBUTES turned on in 
        cGraphicsOpenGL, then I should actually call pgraphics.adjustAttributes(this);
        right here instead of down inside the display list --- see the comment where
        UNCONDITIONAL_ADJUSTATTRIBUTES is defined. */
            if (EnabledDisplayList && pgraphics.SupportsDisplayList)
            {
                if (!pgraphics.activateDisplayList(this)) /* If you plan to use display lists,
				look if one's ready, and if not, open one and draw into it. */
                {
                    pgraphics.adjustAttributes(this); //See comment above.
                    imagedraw(pgraphics, drawflags);
                }
                pgraphics.callActiveDisplayList(this); //Now call the display list.
            }
            else //Not trying to use display lists for this kind of sprite.  Just draw it.
            {
                pgraphics.adjustAttributes(this); //See comment above.
                imagedraw(pgraphics, drawflags);
            }
            pgraphics.popMatrix();
            //After the draw, tell the sprite that its current geometry has now been drawn once.
            NewGeometryFlag = false; /* This is for use by the cGraphicsOpenGL for
			knowing when it may need to change any display list id being used for the sprites.  */
        }


        /* This gets called by the owner cCritter::draw which does multMatrix
        (on the right) with the critter's own _attitude and calls psprite()->draw. 
        cSprite::draw does a multMatrix (on the right) with its own _spriteattitude
        and calls its special overloaded imagedraw method. It's effectively a
        Template Method. */

        public virtual void imagedraw(cGraphics pgraphics, int drawflags)
        {
            /*This is a default approach, just draws a circle. */
            cColorStyle tempcolorstyle = new cColorStyle(true, true, Color.Blue,
                Color.LightGray, Radius / 4.0f);
            /* Params are filled, edged, fillcolor, linecolor,
         linewidth, etc. */
            pgraphics.drawcircle(new cVector3(0.0f, 0.0f, 0.0f), _radius,
                tempcolorstyle, drawflags);
        }


        public virtual short ModelState
        {
            get
                { return 0;  }
            set
                { }
        }

        public virtual cMatrix3 SpriteAttitude
        {
            get
                { return _spriteattitude; }
            set
            {
                _spriteattitude.copy(value);
            }
        }

        public virtual cVector3 Center
        {
            get
                { return _spriteattitude.LastColumn; }
            set
                { _spriteattitude.LastColumn = value; }
        }

        public virtual float Radius
        {
            get
            {
                //	return _radius; 
                return _spriteattitude.ScaleFactor * _radius;
            }
            set
            {
                _radius = value;
                NewGeometryFlag =true;
            }
        }

        public virtual float PrismDz
        {
            get
                { return _prismdz; }
            set
                { _prismdz = value; }
        }

        public virtual bool NewGeometryFlag
        {
            get
                { return _newgeometryflag; }
            set
                { _newgeometryflag = value; }
        }

        public virtual cColorStyle ColorStyle
        {
            get
                { return _pcolorstyle; }
            set
            {
                _pcolorstyle.copy(value);
                _newgeometryflag = true;
            }
        }

        public virtual bool Filled
        {
            get
                { return _pcolorstyle.Filled; }
            set
            {
                _pcolorstyle.Filled = value;
                NewGeometryFlag = true;
            }
        }

        public virtual bool Edged
        {
            get
                { return _pcolorstyle.Edged; }
            set
            {
                _pcolorstyle.Edged = value;
                NewGeometryFlag = true;
            }
        }

        public virtual Color FillColor
        {
            get
                { return _pcolorstyle.FillColor; }
            set
            {
                _pcolorstyle.FillColor = value;
                NewGeometryFlag = true;
            }
        }

        public virtual Color LineColor
        {
            get
                { return _pcolorstyle.LineColor; }
            set
            {
                _pcolorstyle.LineColor = value;
                NewGeometryFlag = true;
            }
        }

        public virtual float LineWidthWeight
        {
            get
                { return _pcolorstyle.LineWidthWeight; }
            set
            {
                _pcolorstyle.LineWidthWeight = value;
                NewGeometryFlag = true;
            }
        }

        public virtual float LineWidth
        {
            get
                { return _pcolorstyle.LineWidth; }
            set
                { _pcolorstyle.LineWidth = value; NewGeometryFlag = true; }
        }

        public virtual int ResourceID
        {
            get
                { return _resourceID; }
            set
                { _resourceID = value; }
        }

        public virtual uint SpriteID
        {
            get
                { return _spriteID; }
        }

        public virtual float Angle
        {
            get
                { return 0.0f; }
        }

        public virtual bool EnabledDisplayList
        {
            get
                { return true; }
        }

        public virtual bool UsesSmoothing
        {
            get
                { return true; }
        }

        public virtual bool UsesTexture
        {
            get
                { return false; }
        }

        public virtual bool UsesTransparentMask
        {
            get
                { return false; }
        }

        public virtual bool UsesAlpha
        {
            get
                { return false; }
        }

        public virtual bool UsesLighting
        {
            get
               { return true; }
        }


    }

    class cSpriteComposite : cSprite
    {
        protected LinkedList<cSprite> _childspriteptr;

        public cSpriteComposite()
            : base()
        {
            _childspriteptr = new LinkedList<cSprite>(
                delegate(out cSprite s1, cSprite s2)
                {
                    s1 = s2.copy(); // to get polymorphism
                }
                );
        }
        //Default constructor of _childspriteptr will be a zero-size array.
        //Mutators  
        /* By default the inherited cSprite mutators will only affect the base sprite.
        Depending on whether we plan to show all of the child sprites all the time, and
        on whether we might prefer to individually mutate them we might or might not
        want to cascade the mutators down to all the children.  For the following ones
        that we pretty definitely want to be uniform, we cascade them to mutate both
        the base and all the children. You might want to overload more of these later. */

        /* setPrismDz needs to take the scaling of 
            the _spriteattitudematrix into account. */

        /* The default cSpriteComposite method for setRadius is
            to scale _spriteattitude matrix.  This gets used by cPolyPolygon and the
            various cSpriteBubble.  We override setRadius for the cSpriteShowOneChild 
            child of cSpriteComposite. */
        //Accessor 

        /* For now, we view a cSpriteComposite as having the
             radius of its largest piece.  But often we will often overload this. */

        public cSprite member(int n) { return _childspriteptr[n]; }
        //Methods 

        public override void fixResourceID()
        {
            _resourceID = -1;
            foreach (cSprite s in _childspriteptr)
                if (s.ResourceID != -1)
                {
                    _resourceID = s.ResourceID;
                    break;
                }
        }

        /*  A cSpriteComposite
            will adopt as its _resourceID the first non-trivial resourceID of
            a member. */

        public virtual void add(cSprite psprite)
        {
            _childspriteptr.Add(psprite);
            NewGeometryFlag = true;
            fixResourceID();
        }


        public virtual void add(int resourceid)
        {
            add(new cSpriteIcon(resourceid));
            NewGeometryFlag = true;
            fixResourceID();
        }

        /* Doesn't really need to be virtual as it
            calls the virtual add(new cSpriteIcon(resourceid)).*/

        public virtual void changeMember(int index, cSprite psprite)
        {
            if (index < 0 || index >= _childspriteptr.Size)
                return;
            _childspriteptr.RemoveAt(index);
            _childspriteptr.SetAt(psprite);
            NewGeometryFlag = true;
            fixResourceID();
        }

        /* If there is 
            a sprite at index, delete it and put in psprite new.  Else do nothing. */

        public override cSprite copy()
        {
            cSpriteComposite sc = new cSpriteComposite();
            sc.copy(this);
            return sc;
        }

        public override void copy(cSprite psprite)
        {
            base.copy(psprite);
            _childspriteptr.RemoveAll();
            if (!psprite.IsKindOf("cSpriteComposite"))
                return; //You're done if psprite isn't a cSpriteComposite.
            cSpriteComposite pspritecomposite = (cSpriteComposite)psprite; /* I know it is a
			cSpriteComposite at this point, but I need to do a cast, so the compiler will 
			let me access it's cSpriteComposite member _childspriteptr. */

            foreach (cSprite s in pspritecomposite._childspriteptr)
                add(s);

            NewGeometryFlag = true;
        }

        public override void draw(cGraphics pgraphics, int drawflags = 0)
        { /* Use the base _spriteattitude and then walk the array of child sprites and call their
		imagedraw methods.  Don't try and imagedraw the base cSprite, as it's only there to hold
		the _spriteattitude.  Note that the cSpriteShowOneChild child class only cascades 
		the imagedraw to one child. */
            pgraphics.pushMatrix();
            pgraphics.multMatrix(_spriteattitude);
            /* For now, let's not try and use display lists for the composites, but only for the
        individual pieces. */
            foreach (cSprite s in _childspriteptr)
                _childspriteptr.ElementAt().draw(pgraphics, drawflags); //If there happen to be any.
            //If you don't want to draw the first sprite, like wiht a polypoly, start with i at 1, 
            pgraphics.popMatrix();
        }

        /* Default is to cascade
            calls to the _childspriteptr members. */

        public override void animate(float dt, cCritter powner)
        {
            foreach (cSprite s in _childspriteptr)
                _childspriteptr.ElementAt().animate(dt, powner);
        }

        // Default is to cascade.

        public override bool IsKindOf(string name)
        {
            return (name == "cSpriteComposite") || base.IsKindOf(name);
        }

        public override Color LineColor
        {
            set
            {
                base.LineColor = value;
                foreach (cSprite s in _childspriteptr)
                    _childspriteptr.ElementAt().LineColor = value;
            }
        }

        public override Color FillColor
        {
            set
            {
                base.FillColor = value;
                foreach (cSprite s in _childspriteptr)
                    _childspriteptr.ElementAt().FillColor = value;
            }
        }

        public override bool Edged
        {
            set
            {
                base.Edged = value;
                foreach (cSprite s in _childspriteptr)
                    _childspriteptr.ElementAt().Edged = value;
            }
        }

        public override bool Filled
        {
            set
            {
                base.Filled = value;
                foreach (cSprite s in _childspriteptr)
                    _childspriteptr.ElementAt().Filled = value;
            }
        }

        public override float LineWidthWeight
        {
            set
            {
                base.LineWidthWeight = value;
                foreach (cSprite s in _childspriteptr)
                    _childspriteptr.ElementAt().LineWidthWeight = value;
            }
        }

        public override float LineWidth
        {
            set
            {
                base.LineWidth = value;
                foreach (cSprite s in _childspriteptr)
                    _childspriteptr.ElementAt().LineWidth = value;
            }
        }

        public override bool NewGeometryFlag
        {
            set
            {
                base.NewGeometryFlag = value;
                foreach (cSprite s in _childspriteptr)
                    _childspriteptr.ElementAt().NewGeometryFlag = value;
            }
        }

        public override float PrismDz
        {
            set
            {
                _prismdz = value;
                float scalefactor = _spriteattitude.ScaleFactor;
                float scaledprismdz = _prismdz / scalefactor;
                /* We divide by the scalefactor becuase, for a composite sprite, the
            attitude scales the whole assembly from the "outside," and we want
            the value thickness to be aboslute. */
                foreach (cSprite s in _childspriteptr)
                    _childspriteptr.ElementAt().PrismDz = scaledprismdz;
                NewGeometryFlag = true;
            }
        }

        public override float Radius
        {
            get
            {
                /* This is just a start of a method that we'll need to overload for the indivdiual cases.
            For now, we view a cSpriteComposite as having the radius of its largest piece.  But often we
            need to think about how the radii sum up with each other.  We do this in cPolyPolygon::radius() 
            for instance, which is already overloaded from this. */
                float maxchildradius = 0.0f;
                foreach (cSprite s in _childspriteptr)
                    if (s.Radius > maxchildradius)
                        maxchildradius = s.Radius;

                return _spriteattitude.ScaleFactor * maxchildradius;
            }
            set
            {
                if (value == 0.0f)
                    return; //Don't allow zero radius.
                float oldradius = Radius;
                float scalefactor = value / oldradius;
                _spriteattitude.multassign(cMatrix3.scale(scalefactor));
                PrismDz = _prismdz; /* We need to do this so that the prismdz thickness takes
			the new scalefactor into account. */
                NewGeometryFlag = true;
            }
        }


    }
}
//                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        