// For AC Framework 1.2, ZEROVECTOR and other vectors were removed,
// default parameters were added -- JC

using System;
using ACFramework;


namespace ACFramework
{

    class cRealBox2
    {
        public static readonly int BOX_INVALIDCODE = -1;
        public static readonly int BOX_INSIDE = 0;
        public static readonly int BOX_LOX = 1;
        public static readonly int BOX_HIX = 2;
        public static readonly int BOX_LOY = 4;
        public static readonly int BOX_HIY = 8;
        public static readonly int BOX_LOZ = 16;
        public static readonly int BOX_HIZ = 32;
        public static readonly int BOX_HIY_LOX = (BOX_HIY | BOX_LOX);
        public static readonly int BOX_HIY_HIX = (BOX_HIY | BOX_HIX);
        public static readonly int BOX_LOY_LOX = (BOX_LOY | BOX_LOX);
        public static readonly int BOX_LOY_HIX = (BOX_LOY | BOX_HIX);
        public static readonly int BOX_ALL = (BOX_LOY_HIX | BOX_HIY_LOX);
        public static readonly int BOX_X = (BOX_LOX | BOX_HIX);
        public static readonly int BOX_Y = (BOX_LOY | BOX_HIY);
        public static readonly int BOX_Z = (BOX_LOZ | BOX_HIZ);
        public static readonly int BOX_INVALIDOUTCODE = -1;

        public static readonly short LOX = 0;
        public static readonly short HIX = 1;
        public static readonly short LOY = 2;
        public static readonly short HIY = 3;
        public static readonly short LOZ = 4;
        public static readonly short HIZ = 5;

        public static readonly float MINSIZE = 0.001f; /*It messes up our outcode computations for surface points 
		if we can have boxes with any degenerate 0 size, so we enforce a minimum
		of about a thousandth. */
        protected float _lox, _hix, _loy, _hiy;

        protected void _arrange()
        { //Helper function 
            float temp;
            if (_lox > _hix)
            {
                temp = _lox;
                _lox = _hix;
                _hix = temp;
            }
            if (_loy > _hiy) //In Real land loy IS < hiy.
            {
                temp = _loy;
                _loy = _hiy;
                _hiy = temp;
            }
            if (_lox == _hix)
            {
                _lox -= 0.5f * cRealBox2.MINSIZE;
                _hix += 0.5f * cRealBox2.MINSIZE;
            }
            if (_loy == _hiy)
            {
                _loy -= 0.5f * cRealBox2.MINSIZE;
                _hiy += 0.5f * cRealBox2.MINSIZE;
            }
        }

        //helper function 

        protected void _initialize(float px, float py, float qx, float qy)
        {
            _lox = px;
            _loy = py;
            _hix = qx;
            _hiy = qy;
            _arrange();
        }

        /* Make this private and use a more
            dimension-independnt form as the public method*/

        public virtual void copy(cRealBox2 box)
        {
            _initialize(box.Lox, box.Loy, box.Hix, box.Hiy);
        }


        public void copy(cRealBox3 box)
        {
            _initialize(box.Lox, box.Loy, box.Hix, box.Hiy);
        }


        //constructors 
        /* Makes a box of these dimensions centered on the origin. We give the default box a
            typical 4:3 screen aspect , as is seen in 800x600 pixel resolution. 
            Often this won't fill a typical window nicely, what with all the
            menu, tool, and status bars, so we change it at	the program start.*/

        public cRealBox2(float lox, float loy, float hix, float hiy)
        {
            _initialize(lox, loy, hix, hiy);
        }


        public cRealBox2()
        {
            set(4.0f, 3.0f, 0.0f);
        }

        public cRealBox2(float xsize)
        {
            set(xsize, 3.0f, 0.0f);
        }

        public cRealBox2(float xsize, float ysize)
        {
            set(xsize, ysize, 0.0f);
        }

        public cRealBox2(float xsize, float ysize, float zsize)
        {
            set(xsize, ysize, zsize);
        }

        public cRealBox2(cVector2 locorner, cVector2 hicorner)
        {
            set(locorner, hicorner);
        }

        public cRealBox2(cVector2 center, float xsize, float ysize, float zsize = 0.0f)
        {
            set(center, xsize, ysize, zsize);
        }

        public cRealBox2(cVector2 center, float edge)
        {
            set(center, edge, edge);
        }

        //square 

        public cRealBox2(cRealBox3 box) { copy(box); }

        public void set(cVector2 locorner, cVector2 hicorner)
        {
            _initialize(locorner.X, locorner.Y, hicorner.X, hicorner.Y);
        }

        public void set(cVector2 center, float xsize, float ysize, float zsize = 0.0f)
        // zsize is a dummy
        {
            if (xsize < 0.0f)
                xsize = -xsize;
            if (ysize < 0.0f)
                ysize = -ysize;
            float lox, loy, hix, hiy;
            lox = center.X - xsize / 2.0f;
            loy = center.Y - ysize / 2.0f;
            hix = center.X + xsize / 2.0f;
            hiy = center.Y + ysize / 2.0f;
            _initialize(lox, loy, hix, hiy);
        }

        public void set(float xsize, float ysize, float zsize = 0.0f)
        {
            set(new cVector2(0.0f, 0.0f), xsize, ysize, zsize);
        }

        /* Have the dummy zsize arg in these constructors
            for consistency with the cRealBox3 interface. */
        //mutators 

        public void moveTo(cVector2 newcenter) { set(newcenter, XSize, YSize); }

        public void matchAspect(int cx, int cy)
        {
            float oldaspect, newaspect, centery;

            if ((_hiy - _loy == 0.0f) || (cy == 0))
                return;
            oldaspect = (_hix - _lox) / (_hiy - _loy);
            if (oldaspect == 0.0f)
                return;
            newaspect = (float)cx / (float)cy;
            if (newaspect == 0.0f)
                return;
            centery = (_hiy - _loy) / 2.0f;
            //stretch or shrink the box vertically 
            _hiy = centery + (oldaspect / newaspect) * (_hiy - centery);
            _loy = centery - (oldaspect / newaspect) * (centery - _loy);
            /*  If we instead wanted to stretch or shrink the box horizontally:
            _hix = centerx + (newaspect/oldaspect)*(_hix - centerx);
            _lox = centerx - (newaspect/oldaspect)*(centerx - _lox);
        */
        }


        public void setYRange(float loy, float hiy) { _loy = loy; _hiy = hiy; _arrange(); }

        public void setZRange(float loz, float hiz) { } //does nothing, just here to match cRealBox3 
        //accessors 

        public cVector2 corner(int i)
        {
            switch (i)
            {
                case 0:
                    return new cVector2(_lox, _loy);
                case 1:
                    return new cVector2(_lox, _hiy);
                case 2:
                    return new cVector2(_hix, _loy);
                default: //case 3 
                    return new cVector2(_hix, _hiy);
            }
        }

        //Method for listing corners 0 to 3 when you need to check all.

        public cRealBox2 innerBox(float radius)
        { // Return a box offset inward by radius 
            float xoffset, yoffset;
            xoffset = yoffset = radius;
            if (xoffset < 0.0f) //Don't offset further than there's room for.
                xoffset = 0.0f;
            else if (xoffset > XSize / 2.0f)
                xoffset = XSize / 2.0f;
            if (yoffset < 0.0f) //Don't offset further than there's room for.
                yoffset = 0.0f;
            else if (yoffset > YSize / 2.0f)
                yoffset = YSize / 2.0f;
            cRealBox2 returnbox = new cRealBox2();
            returnbox.copy(this);
            returnbox._initialize(_lox + xoffset, _loy + yoffset, _hix - xoffset, _hiy - yoffset);
            return returnbox;
        }

        /* Return a box offset inward by radius
            distance from each wall.  We clamp radius be less than half the size() */

        public cRealBox2 outerBox(float radius)
        { // Return a box offset outward by radius 
            cRealBox2 returnbox = new cRealBox2();
            returnbox.copy(this);
            returnbox._initialize(_lox - radius, _loy - radius, _hix + radius, _hiy + radius);
            return returnbox;
        }

        /* Return a box offset outward by radius */
        //methods 

        public cVector2 randomVector()
        {
            return new cVector2(Framework.randomOb.randomReal(_lox, _hix),
                Framework.randomOb.randomReal(_loy, _hiy));
        }


        public bool inside(cVector2 testpos)
        {
            /* We prefer to think of the surface as still outside. */
            return (_lox < testpos.X && testpos.X < _hix &&
                _loy < testpos.Y && testpos.Y < _hiy);
        }


        public int outcode(cVector2 testpos)
        { /* This tells you which of the nine possible positions testpos has
	relative to the cRealBox2.  We think of the surface as outside.*/
            int outcode = BOX_INSIDE; //This is 0.

            if (testpos.X <= _lox)
                outcode |= BOX_LOX;
            if (testpos.X >= _hix)
                outcode |= BOX_HIX;
            if (testpos.Y <= _loy)
                outcode |= BOX_LOY;
            if (testpos.Y >= _hiy)
                outcode |= BOX_HIY;
            return outcode;
        }

        //Outcodes are defined in realbox.h 

        public float distanceTo(cVector2 testpos)
        {
            int dummy;
            return distanceToOutcode(testpos, out dummy);
        }


        public float distanceToOutcode(cVector2 testpos, out int posoutcode)
        { /*This gives the distance from testpos to the closest point of the
	cRealBox3.  If you are in a "side" zone your nearest distance is a point
	on the side.  If you are in a "corner" zone your nearest distance is a
	corner.  If you are inside, we call the distance 0.*/
            posoutcode = outcode(testpos);
            float dx, dy;
            dx = dy = 0.0f;
            if ((posoutcode & BOX_LOX) != 0)
                dx = _lox - testpos.X;
            if ((posoutcode & BOX_HIX) != 0)
                dx = testpos.X - _hix;
            if ((posoutcode & BOX_LOY) != 0)
                dy = _loy - testpos.Y;
            if ((posoutcode & BOX_HIY) != 0)
                dy = testpos.Y - _hiy;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }


        public float maxDistanceToCorner(cVector2 testpos)
        { /*This gives the distance from testpos to the furthest corner of the
	cRealBox2. */
            int posoutcode = outcode(testpos);
            float dx, dy;
            if ((posoutcode & BOX_LOX) != 0)
                dx = _hix - testpos.X;
            else if ((posoutcode & BOX_HIX) != 0)
                dx = testpos.X - _lox;
            else
            {
                float val1 = testpos.X - _lox;
                float val2 = _hix - testpos.X;
                dx = (val1 > val2) ? val1 : val2;
            }
            if ((posoutcode & BOX_LOY) != 0)
                dy = _hiy - testpos.Y;
            else if ((posoutcode & BOX_HIY) != 0)
                dy = testpos.Y - _loy;
            else
            {
                float val1 = testpos.Y - _loy;
                float val2 = _hiy - testpos.Y;
                dy = (val1 > val2) ? val1 : val2;
            }
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }


        public int addBounce(cVector2 position, cVector2 velocity, float bounciness)
        {
            int outcode = BOX_INSIDE;

            position.addassign(velocity);
            /* The basic idea is to reverse appropriate velocity component if you've passed an edge,
        and reflect the motion past the edge into motion away from the edge.  Reflection means
        at the right newx would be _hix - (position.x()-_hix), or 2*_hix - position.x()
        and at the newx would be _lox + (_lox - position.x()), or 2*_lox - position.x(). 
            Now add in the notion of a bounciness between 0.0 and 1.0.  First of all the
        reflected newvel is attentuated to -bounciness*oldvel.  Second, the reflected
        position should take into account the fact that the object is moving slower after
        the reflection, so we get, on the right, 
        newx  = _hix - bounciness*(position.x()-_hix) and on the left 
        newx = _lox + bounciness*(_lox - position.x())	*/

            if (position.X >= _hix)
            {
                velocity.set(-bounciness * velocity.X, velocity.Y);
                position.set(_hix - bounciness * (position.X - _hix), position.Y);
                outcode |= BOX_HIX;
            }
            if (position.X <= _lox)
            {
                velocity.set(-bounciness * velocity.X, velocity.Y);
                position.set(_lox - bounciness * (position.X - _lox), position.Y);
                outcode |= BOX_LOX;
            }
            if (position.Y >= _hiy)
            {
                velocity.set(velocity.X, -bounciness * velocity.Y);
                position.set(position.X, _hiy - bounciness * (position.Y - _hiy));
                outcode |= BOX_HIY;
            }
            if (position.Y <= _loy)
            {
                velocity.set(velocity.X, -bounciness * velocity.Y);
                position.set(position.X, _loy - bounciness * (position.Y - _loy));
                outcode |= BOX_LOY;
            }
            if (outcode != BOX_INSIDE)
                clamp(position); /* Just so some screwy high velocity bounce can't 
				leave you outside the box */
            return outcode;
        }


        /*add velocity to position, but if you pass the border, then
                reflect velocity and get a reflected position. */

        public int addBounce(cVector2 position, ref cVector2 velocity, float bounciness, float dt)
        {
            if (Math.Abs(dt) < 0.00001f)
                return BOX_INSIDE; //You're not moving.
            cVector2 dtvelocity = velocity.mult(dt);
            int outcode = addBounce(position, dtvelocity, bounciness);
            velocity = dtvelocity.div(dt);
            return outcode;
        }


        /* add dt*velocity to position, and do the bounce to velocity the same. */

        public int wrap(cVector2 position)
        {
            int outcode = BOX_INSIDE;
            if (position.X <= _lox)
            {
                outcode |= BOX_LOX;
                position.set(_hix - _lox + position.X, position.Y);
            }
            if (position.X >= _hix)
            {
                outcode |= BOX_HIX;
                position.set(_lox - _hix + position.X, position.Y);
            }
            if (position.Y <= _loy)
            {
                outcode |= BOX_LOY;
                position.set(position.X, _hiy - _loy + position.Y);
            }
            if (position.Y >= _hiy)
            {
                outcode |= BOX_HIY;
                position.set(position.X, _loy - _hiy + position.Y);
            }
            return outcode;
        }

        /* If you move off one edge,
            then come back in the same amount from the other side */

        public int wrap(cVector2 position, cVector2 wrapposition1, cVector2 wrapposition2,
            cVector2 wrapposition3, float radius)
        {
            wrapposition3.copy(position);
            wrapposition2.copy(position);
            wrapposition1.copy(position);
            int outcode = wrap(position);
            cRealBox2 smallbox = this.innerBox(radius);
            //First wrap the x 
            if (position.X <= smallbox._lox)
            {
                outcode |= BOX_LOX;
                wrapposition1.set(_hix - _lox + position.X, position.Y);
            }
            if (position.X >= smallbox._hix)
            {
                outcode |= BOX_HIX;
                wrapposition1.set(_lox - _hix + position.X, position.Y);
            }
            //If there's no x wrap, wrap the y for wrapposition1 
            if (wrapposition1.equal(position))
            {
                if (position.Y <= smallbox._loy)
                {
                    outcode |= BOX_LOY;
                    wrapposition1.set(position.X, _hiy - _loy + position.Y);
                }
                if (position.Y >= smallbox._hiy)
                {
                    outcode |= BOX_HIY;
                    wrapposition1.set(position.X, _loy - _hiy + position.Y);
                }
            }
            //If there's x wrap in wrapposition1, do the y wrap of position and wrapposition1 
            else //wrapposition1 not equal to position 
            {
                if (position.Y <= smallbox._loy)
                {
                    outcode |= BOX_LOY;
                    wrapposition2.set(position.X, _hiy - _loy + position.Y);
                    wrapposition3.set(wrapposition1.X, _hiy - _loy + position.Y);
                }
                if (position.Y >= smallbox._hiy)
                {
                    outcode |= BOX_HIY;
                    wrapposition2.set(position.X, _loy - _hiy + position.Y);
                    wrapposition3.set(wrapposition1.X, _loy - _hiy + position.Y);
                }
            }
            //Don't bother doing this for z.
            return outcode;
        }

        /* If you are within
            radius of an edge, put the possible wrap values in wrappositions. */

        public int clamp(cVector2 position)
        {
            int outcode = BOX_INSIDE;
            if (position.X < _lox)
            {
                outcode |= BOX_LOX;
                position.set(_lox, position.Y);
            }
            if (position.X > _hix)
            {
                outcode |= BOX_HIX;
                position.set(_hix, position.Y);
            }
            if (position.Y < _loy)
            {
                outcode |= BOX_LOY;
                position.set(position.X, _loy);
            }
            if (position.Y > _hiy)
            {
                outcode |= BOX_HIY;
                position.set(position.X, _hiy);
            }
            return outcode;
        }

        /*Make sure position is inside*/

        public int clamp(cVector2 position, cVector2 velocity)
        {
            int outcode = clamp(position);
            if ((outcode & BOX_LOX) != 0) //If you hit a LO X wall, zero out any neg X velocity.
                velocity.set(Math.Max(0.0f, velocity.X), velocity.Y);
            if ((outcode & BOX_HIX) != 0) //If you hit an HI X wall, zero out any pos X velocity.
                velocity.set(Math.Min(0.0f, velocity.X), velocity.Y);
            if ((outcode & BOX_LOY) != 0) //Same deal for Y.
                velocity.set(velocity.X, Math.Max(0.0f, velocity.Y));
            if ((outcode & BOX_HIY) != 0)
                velocity.set(velocity.X, Math.Min(0.0f, velocity.Y));
            return outcode;
        }

        /*Make sure position is inside
            and kill any velocity in a wall-hitting direction. */
        //	CRect realToPixel(const cRealPixelConverter &crealpixelconverter)const; 

        public virtual void draw(cGraphics pgraphics, int drawflags)
        {
            cColorStyle dummy = new cColorStyle();
            pgraphics.drawXYrectangle(new cVector3(LoCorner),
                new cVector3(HiCorner), dummy, drawflags);
        }


        public cRealBox2 box2mult(float scale, cRealBox2 box)
        {
            return new cRealBox2(box.Center, scale * box.XSize,
                scale * box.YSize);
        }


        /* Return scale times as big as box. */

        public bool box2equal(cRealBox2 brect)
        {
            return (_lox == brect._lox && _loy == brect._loy &&
                _hix == brect._hix && _hiy == brect._hiy);
        }


        public bool box2notequal(cRealBox2 brect)
        { return !box2equal(brect); }



        public virtual float Lox
        {
            get
                { return _lox; }
        }

        public virtual float Hix
        {
            get
                { return _hix; }
        }

        public virtual float Loy
        {
            get
                { return _loy; }
        }

        public virtual float Hiy
        {
            get
                { return _hiy; }
        }

        public virtual float Loz
        {
            get
                { return 0.0f; }
        }

        public virtual float Hiz
        {
            get
                { return 0.0f; }
        }

        public virtual float Midx
        {
            get
                { return (_hix + _lox) * 0.5f; }
        }

        public virtual float Midy
        {
            get
                { return (_hiy + _loy) * 0.5f; }
        }

        public virtual float Midz
        {
            get
                { return 0.0f; }
        }

        public virtual cVector2 Center
        {
            get
                { return new cVector2((_lox + _hix) * 0.5f, (_loy + _hiy) * 0.5f); }
        }

        public virtual cVector2 LoCorner
        {
            get
                { return new cVector2(_lox, _loy); }
        }

        public virtual cVector2 HiCorner
        {
            get
                { return new cVector2(_hix, _hiy); }
        }

        public virtual float XSize
        {
            get
                { return _hix - _lox; }
        }

        public virtual float YSize
        {
            get
                { return _hiy - _loy; }
        }

        public virtual float ZSize
        {
            get
                { return 0.0f; }
        }

        public virtual float MinSize
        {
            get
            {
                float xs = XSize;
                float ys = YSize;
                return (xs < ys) ? xs : ys;
            }
        }

        public virtual float MaxSize
        {
            get
            {
                float xs = XSize;
                float ys = YSize;
                return (xs > ys) ? xs : ys;
            }
        }

        public virtual float XRadius
        {
            get
                { return (_hix - _lox) * 0.5f; }
        }

        public virtual float YRadius
        {
            get
                { return (_hiy - _loy) * 0.5f; }
        }

        public virtual float ZRadius
        {
            get
                { return 0.0f; }
        }

        public virtual float Radius
        {
            get
                { return (float)Math.Sqrt(XRadius * XRadius + YRadius * YRadius); }
        }

        public virtual float AverageRadius
        {
            get
                { return (XRadius + YRadius) * 0.5f; }
        }


    }

    //3D================================================================= 

    class cRealBox3
    {
        public static readonly int BOX_INVALIDCODE = -1;
        public static readonly int BOX_INSIDE = 0;
        public static readonly int BOX_LOX = 1;
        public static readonly int BOX_HIX = 2;
        public static readonly int BOX_LOY = 4;
        public static readonly int BOX_HIY = 8;
        public static readonly int BOX_LOZ = 16;
        public static readonly int BOX_HIZ = 32;
        public static readonly int BOX_HIY_LOX = (BOX_HIY | BOX_LOX);
        public static readonly int BOX_HIY_HIX = (BOX_HIY | BOX_HIX);
        public static readonly int BOX_LOY_LOX = (BOX_LOY | BOX_LOX);
        public static readonly int BOX_LOY_HIX = (BOX_LOY | BOX_HIX);
        public static readonly int BOX_ALL = (BOX_LOY_HIX | BOX_HIY_LOX);
        public static readonly int BOX_X = (BOX_LOX | BOX_HIX);
        public static readonly int BOX_Y = (BOX_LOY | BOX_HIY);
        public static readonly int BOX_Z = (BOX_LOZ | BOX_HIZ);
        public const int BOX_INVALIDOUTCODE = -1;

        public static readonly short LOX = 0;
        public static readonly short HIX = 1;
        public static readonly short LOY = 2;
        public static readonly short HIY = 3;
        public static readonly short LOZ = 4;
        public static readonly short HIZ = 5;

        public static readonly float MINSIZE = 0.001f; /*It messes up our outcode computations for surface points 
		if we can have boxes with any degenerate 0 size, so we enforce a minimum
		of about a thousandth. */
        protected float _lox, _hix, _loy, _hiy, _loz, _hiz;

        protected void _arrange()
        { //Helper function 
            float temp;
            if (_lox > _hix)
            {
                temp = _lox;
                _lox = _hix;
                _hix = temp;
            }
            if (_loy > _hiy)
            {
                temp = _loy;
                _loy = _hiy;
                _hiy = temp;
            }
            if (_loz > _hiz)
            {
                temp = _loz;
                _loz = _hiz;
                _hiz = temp;
            }
            if (_lox == _hix)
            {
                _lox -= 0.5f * cRealBox3.MINSIZE;
                _hix += 0.5f * cRealBox3.MINSIZE;
            }
            if (_loy == _hiy)
            {
                _loy -= 0.5f * cRealBox3.MINSIZE;
                _hiy += 0.5f * cRealBox3.MINSIZE;
            }
            if (_loz == _hiz)
            {
                _loz -= 0.5f * cRealBox3.MINSIZE;
                _hiz += 0.5f * cRealBox3.MINSIZE;
            }
        }

        //helper function 

        protected void _initialize(float px, float py, float pz, float qx, float qy, float qz)
        {
            _lox = px;
            _loy = py;
            _loz = pz;
            _hix = qx;
            _hiy = qy;
            _hiz = qz;
            _arrange();
        }

        /* Make this private and use 
            a more dimension-independnt form as the public method*/

        public void copy(cRealBox2 box)
        {
            _initialize(box.Lox, box.Loy, 0.0f, box.Hix, box.Hiy, 0.0f);
        }


        public virtual void copy(cRealBox3 box)
        {
            _initialize(box.Lox, box.Loy, box.Loz, box.Hix, box.Hiy, box.Hiz);
        }


        public static bool isFaceOutcode(int outcode)
        {
            return outcode == BOX_LOX || outcode == BOX_HIX ||
            outcode == BOX_LOY || outcode == BOX_HIY || outcode == BOX_LOZ ||
            outcode == BOX_HIZ;
        }
        //constructors 

        public cRealBox3(float xsize = 4.0f, float ysize = 3.0f, float zsize = 0.0f)
        {
            set(xsize, ysize, zsize);
        }

        public cRealBox3(cVector3 locorner, cVector3 hicorner)
        {
            set(locorner, hicorner);
        }

        public cRealBox3(cVector3 center, float xsize, float ysize, float zsize = 0.0f)
        {
            set(center, xsize, ysize, zsize);
        }

        public cRealBox3(cVector3 center, float edge)
        {
            set(center, edge, edge, edge);
        }

        //cube 

        public cRealBox3(cRealBox2 box) { copy(box); }

        public void set(cVector3 locorner, cVector3 hicorner)
        {
            _initialize(locorner.X, locorner.Y, locorner.Z, hicorner.X, hicorner.Y, hicorner.Z);
        }

        public void set(cVector3 center, float xsize, float ysize, float zsize = 0.0f)
        {
            if (xsize < 0.0f)
                xsize = -xsize;
            if (ysize < 0.0f)
                ysize = -ysize;
            if (zsize < 0.0f)
                zsize = -zsize;
            float lox, loy, hix, hiy, loz, hiz;
            lox = center.X - xsize / 2.0f;
            loy = center.Y - ysize / 2.0f;
            loz = center.Z - zsize / 2.0f;
            hix = center.X + xsize / 2.0f;
            hiy = center.Y + ysize / 2.0f;
            hiz = center.Z + zsize / 2.0f;
            _initialize(lox, loy, loz, hix, hiy, hiz);
        }


        public void set(float xsize, float ysize, float zsize = 0.0f)
        {
            set(new cVector3(0.0f, 0.0f, 0.0f), xsize, ysize, zsize);
        }

        /* Have the dummy zsize arg in these constructors
            for consistency with the cRealBox3 interface. */

        //accessors 

        public cVector3 corner(int i)
        {
            switch (i)
            {
                case 0:
                    return new cVector3(_lox, _loy, _loz);
                case 1:
                    return new cVector3(_lox, _loy, _hiz);
                case 2:
                    return new cVector3(_lox, _hiy, _loz);
                case 3:
                    return new cVector3(_lox, _hiy, _hiz);
                case 4:
                    return new cVector3(_hix, _loy, _loz);
                case 5:
                    return new cVector3(_hix, _loy, _hiz);
                case 6:
                    return new cVector3(_hix, _hiy, _loz);
                default: //case 7 
                    return new cVector3(_hix, _hiy, _hiz);
            }
        }

        //Method for listing corners 0 to 7 when you need to check all.

        //Return smallest non-zero dimension.

        public cRealBox3 innerBox(float radius)
        { // Return a box offset inward by radius, but don't offset more than would fit.
            float xoffset, yoffset, zoffset;
            xoffset = yoffset = zoffset = radius;
            if (xoffset < 0.0f) //Don't offset further than there's room for.
                xoffset = 0.0f;
            else
            {
                float val = XSize / 2.0f;
                if (xoffset > val)
                    xoffset = val;
            }
            if (yoffset < 0.0f) //Don't offset further than there's room for.
                yoffset = 0.0f;
            else
            {
                float val = YSize / 2.0f;
                if (yoffset > val)
                    yoffset = val;
            }
            if (zoffset < 0.0f) //Don't offset further than there's room for.
                zoffset = 0.0f;
            else
            {
                float val = ZSize / 2.0f;
                if (zoffset > val)
                    zoffset = val;
            }

            cRealBox3 returnbox = new cRealBox3();
            returnbox.copy(this);
            returnbox._initialize(_lox + xoffset, _loy + yoffset, _loz + zoffset,
                _hix - xoffset, _hiy - yoffset, _hiz - zoffset);
            return returnbox;
        }

        /* Return a box offset inward by radius
            distance from each wall, though we don't offset more than will fit. */

        public cRealBox3 outerBox(float radius)
        { // Return a box offset outward by radius 
            cRealBox3 returnbox = new cRealBox3();
            returnbox.copy(this);
            returnbox._initialize(_lox - radius, _loy - radius, _loz - radius,
                _hix + radius, _hiy + radius, _hiz + radius);
            return returnbox;
        }

        /* Return a box offset outward by radius. */

        public cRealBox2 side(int iside)
        {
            switch (iside)
            {
                case 0:
                case 1:
                    return (new cRealBox2(_loy, _loz, _hiy, _hiz));
                case 2:
                case 3:
                    return (new cRealBox2(_lox, _loz, _hix, _hiz));
                case 4:
                case 5:
                default:
                    return (new cRealBox2(_lox, _loy, _hix, _hiy));
            }
        }

        /* iside should be 0 through 5, that is, 
            an int(e) where e is one of the BOXSIDE enums */
        //mutators 

        public void matchAspect(int cx, int cy)
        {
            float oldaspect, newaspect, centery;

            if ((_hiy - _loy == 0.0f) || (cy == 0.0f))
                return;
            oldaspect = (_hix - _lox) / (_hiy - _loy);
            if (oldaspect == 0.0f)
                return;
            newaspect = (float)cx / (float)cy;
            if (newaspect == 0.0f)
                return;
            centery = (_hiy - _loy) / 2.0f;
            //stretch or shrink the box vertically 
            _hiy = centery + (oldaspect / newaspect) * (_hiy - centery);
            _loy = centery - (oldaspect / newaspect) * (centery - _loy);
            /*  If we instead wanted to stretch or shrink the box horizontally:
            _hix = centerx + (newaspect/oldaspect)*(_hix - centerx);
            _lox = centerx - (newaspect/oldaspect)*(centerx - _lox);
        */
        }


        public void setYRange(float loy, float hiy) { _loy = loy; _hiy = hiy; _arrange(); }

        public void setZRange(float loz, float hiz) { _loz = loz; _hiz = hiz; _arrange(); }
        /* Convenient for making a 2D box thick. */
        //methods 

        public cVector3 randomVector()
        {
            return new cVector3(Framework.randomOb.randomReal(_lox, _hix),
            Framework.randomOb.randomReal(_loy, _hiy),
            Framework.randomOb.randomReal(_loz, _hiz));
        }


        public bool inside(cVector3 testpos)
        {
            return (_lox < testpos.X && testpos.X < _hix &&
                _loy < testpos.Y && testpos.Y < _hiy &&
                _loz < testpos.Z && testpos.Z < _hiz);
        }


        public int outcode(cVector3 testpos)
        { /* This tells you which of the nine possible positions testpos has
	relative to the cRealBox3 */
            int outcode = BOX_INSIDE; //This is 0.

            if (testpos.X <= _lox)
                outcode |= BOX_LOX;
            if (testpos.X >= _hix)
                outcode |= BOX_HIX;
            if (testpos.Y <= _loy)
                outcode |= BOX_LOY;
            if (testpos.Y >= _hiy)
                outcode |= BOX_HIY;
            if (testpos.Z <= _loz)
                outcode |= BOX_LOZ;
            if (testpos.Z >= _hiz)
                outcode |= BOX_HIZ;
            return outcode;
        }

        //Outcodes are defined in realbox.h 

        public float distanceTo(cVector3 testpos)
        {
            int dummy;
            return distanceToOutcode(testpos, out dummy);
        }


        public float distanceToOutcode(cVector3 testpos, out int posoutcode)
        { /*This gives the distance from testpos to the closest point of the
	cRealBox3.  If you are in a "side" zone your nearest distance is a point
	on the side.  If you are in a "corner" zone your nearest distance is a
	corner.  If you are inside, we call the distance 0.*/
            posoutcode = outcode(testpos);
            float dx, dy, dz;
            dx = dy = dz = 0.0f;
            if ((posoutcode & BOX_LOX) != 0)
                dx = _lox - testpos.X;
            if ((posoutcode & BOX_HIX) != 0)
                dx = testpos.X - _hix;
            if ((posoutcode & BOX_LOY) != 0)
                dy = _loy - testpos.Y;
            if ((posoutcode & BOX_HIY) != 0)
                dy = testpos.Y - _hiy;
            if ((posoutcode & BOX_LOZ) != 0)
                dz = _loz - testpos.Z;
            if ((posoutcode & BOX_HIZ) != 0)
                dz = testpos.Z - _hiz;
            return (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }


        public float maxDistanceToCorner(cVector3 testpos)
        { /*This gives the distance from testpos to the furthest corenr of the
	cRealBox3. */
            int posoutcode = outcode(testpos);
            float dx, dy, dz;
            if ((posoutcode & BOX_LOX) != 0)
                dx = _hix - testpos.X;
            else if ((posoutcode & BOX_HIX) != 0)
                dx = testpos.X - _lox;
            else
                dx = Math.Max(testpos.X - _lox, _hix - testpos.X);
            if ((posoutcode & BOX_LOY) != 0)
                dy = _hiy - testpos.Y;
            else if ((posoutcode & BOX_HIY) != 0)
                dy = testpos.Y - _loy;
            else
                dy = Math.Max(testpos.Y - _loy, _hiy - testpos.Y);
            if ((posoutcode & BOX_LOZ) != 0)
                dz = _hiz - testpos.Z;
            else if ((posoutcode & BOX_HIZ) != 0)
                dz = testpos.Z - _loz;
            else
                dz = Math.Max(testpos.Z - _loz, _hiz - testpos.Z);
            return (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }


        public cVector3 closestSurfacePoint(cVector3 oldpos, int oldoutcode, cVector3 newpos, int newoutcode, bool crossedwall)
        {
            /* closestSurfacePoint will move the newpos
        from the far new side (or inside, or overlapping) of the box back to 
        the surface on the old near	side, edge, or corner given by oldoutcode.
        This would prevent going through the wall, but it isn't quite satisfactory
        in the following case:
            If oldoutcode is a corner position and you are in fact heading
        towards a face near the corner, our method bounces you off the corner
        even though visually you can see you should bounce off the
        face.  This has the effect of making a scooter player get hung up on
        a corner sometimes.
            So to avoid this situation, we consider change oldoutcode before
        we start, if oldoutcode is in a corner or edge zone, we can pick a
        surface point on the side that the critter meant to get to.  */
            //We do an if condition here, starting with newpos not being inside.
            if ((newoutcode != BOX_INSIDE) && !crossedwall && (oldoutcode != BOX_INSIDE) &&
                !isFaceOutcode(oldoutcode))
                /*If we're in a mild case, with newoutcode  reasonable, 
            we'll use newoutcode if oldoutcode is corneror edge.
            we'll use newoutcode whenever oldoutcode is corner or edge.
            This will prevent getting hung up on corners.
            Go ahead and move out of the corner or edge zone and use the
            the closest surface point to the newpos in the newoutcode zone.
            I have found that if I do this in gameAirHockey I can drag
            the player thorugh the goal wall, so I have a kludge fix
            in cCritterWall::collide, to pass in crossedwall = FALSE
            whenever the listener is a cListerCursor */
                oldoutcode = newoutcode;
            /* Base your surface point on the newpos so you can slide along.
         I worry about case where we crossedwall, tho.  In this case we'll still
        be using the unchanged oldoutcode, so the surface point we get 
        from newpos should still be on the correct side of the box. 
        Should I even so set newpos to oldpos in the crossedwall case?
        Turns out, NO, I shouoldn't because if I do, then when I try and
        scoot my player along a wall, he doesn't budge, as his newpos is
        always on the otherside of the wall.  */
            float px = newpos.X, py = newpos.Y, pz = newpos.Z;
            if ((oldoutcode & BOX_LOX) != 0)
                px = _lox;
            if ((oldoutcode & BOX_HIX) != 0)
                px = _hix;
            if ((oldoutcode & BOX_LOY) != 0)
                py = _loy;
            if ((oldoutcode & BOX_HIY) != 0)
                py = _hiy;
            if ((oldoutcode & BOX_LOZ) != 0)
                pz = _loz;
            if ((oldoutcode & BOX_HIZ) != 0)
                pz = _hiz;
            if (oldoutcode != BOX_INSIDE) //You've already hit one of the cases 
                return new cVector3(px, py, pz);
            //Otherwise you're in a pathological case where the oldoutcode was 
            //inside.  You need to find a point on the surface.
            //I never enter the following code as of Feb, 2004,because when I call this 
            //method from cCritterWall, I've alrady fixed oldoutcode if it was INSIDE.
            float mindist, newmindist;
            newmindist = mindist = Math.Abs(px - _lox);
            cVector3 ev = new cVector3(-newmindist, 0, 0);
            if ((newmindist = Math.Abs(px - _hix)) < mindist)
            {
                ev.set(newmindist, 0, 0);
                mindist = newmindist;
            }
            if ((newmindist = Math.Abs(py - _loy)) < mindist)
            {
                ev.set(0, -newmindist, 0);
                mindist = newmindist;
            }
            if ((newmindist = Math.Abs(py - _hiy)) < mindist)
            {
                ev.set(0, newmindist, 0);
                mindist = newmindist;
            }
            if ((newmindist = Math.Abs(pz - _loz)) < mindist)
            {
                ev.set(0, 0, -newmindist);
                mindist = newmindist;
            }
            if ((newmindist = Math.Abs(pz - _hiz)) < mindist)
            {
                ev.set(0, 0, newmindist);
                mindist = newmindist;
            }
            return newpos.add(ev);
        }

        public cVector3 escapeVector(cVector3 testpos, int posoutcode = BOX_INVALIDOUTCODE)
        {
            if (posoutcode == BOX_INVALIDOUTCODE)
                posoutcode = outcode(testpos);
            cVector3 escape = new cVector3();
            cVector3 surfacepoint = closestSurfacePoint(testpos, posoutcode, testpos,
                posoutcode, false);
            if (surfacepoint.notequal(testpos))
            {
                if (posoutcode != BOX_INSIDE)
                    escape = testpos.sub(surfacepoint);
                else
                    escape = surfacepoint.sub(testpos);
                return escape.normalize();
            }
            //surfacepoint == testpos case 
            float ex = 0, ey = 0, ez = 0;
            if ((posoutcode & BOX_LOX) != 0)
                ex = -1;
            if ((posoutcode & BOX_HIX) != 0)
                ex = 1;
            if ((posoutcode & BOX_LOY) != 0)
                ey = -1;
            if ((posoutcode & BOX_HIY) != 0)
                ey = 1;
            if ((posoutcode & BOX_LOZ) != 0)
                ez = -1;
            if ((posoutcode & BOX_HIZ) != 0)
                ez = 1;
            escape.set(ex, ey, ez);
            return escape.normalize();
        }

        /* points away from box, or to closest face if you're inside. Computes
        the posoutcode if you dont' feed a good one.  */

        public void reflect(cVector3 velocity, int posoutcode)
        {
            /* The idea here is that we have a critter with posoutcode hitting this cRealBox from the
        outside with a velocity and we want to reflect the velocity off the box.  Idea is
        to combine two or three reflections if you hit an edge or a corner. Assume you already
        calculated posoutcode. Be sure to use the fabs function in the code as every now and then
        the velocity will in fact be pointing the wrong way due to other collisions. 
            Added Feb 22, 2004: My player is getting hung up trying to bounce off
        edges of 3D Walls. This is because if, for instance, I'm bouncing off an 
        xz edge, my old code reversed both vx and vz, and if vy happens to be 0,
        this means you reverse your velocity no matter what angle you hit the edge 
        from.  I'm going to try only reversing the SMALLER of the two components
        at edges, and the SMALLEST of the three at corners.   My approach is to 
        keep only the outcode flag of the smallest velocity component, and then
        do the outcode-based component changes.*/

            float vx = velocity.X, vy = velocity.Y, vz = velocity.Z;
            float avx = Math.Abs(vx), avy = Math.Abs(vy), avz = Math.Abs(vz);
            //xyz corner 
            if (((posoutcode & BOX_X) != 0) && ((posoutcode & BOX_Y) != 0) && ((posoutcode & BOX_Z) != 0))
            {
                float minval = avx;
                if (avy < minval)
                    minval = avy;
                if (avz < minval)
                    minval = avz;
                if (avx == minval)
                {
                    posoutcode &= ~BOX_Y; //Ignore the Y collide 
                    posoutcode &= ~BOX_Z; //Ignore the Z collide 
                }
                else if (avy == minval)
                {
                    posoutcode &= ~BOX_X; //Ignore the X collide 
                    posoutcode &= ~BOX_Z; //Ignore the Z collide 
                }
                else
                {
                    posoutcode &= ~BOX_X; //Ignore the X collide 
                    posoutcode &= ~BOX_Y; //Ignore the Y collide 
                }
            }
            //xy edge 
            else if (((posoutcode & BOX_X) != 0) && ((posoutcode & BOX_Y) != 0))
            {
                if (avx < avy)
                    posoutcode &= ~BOX_Y; //Ignore the Y collide 
                else
                    posoutcode &= ~BOX_X; //Ignore the X collide 
            }
            //xz edge 
            else if (((posoutcode & BOX_X) != 0) && ((posoutcode & BOX_Z) != 0))
            {
                if (avx < avz)
                    posoutcode &= ~BOX_Z; //Ignore the Z collide 
                else
                    posoutcode &= ~BOX_X; //Ignore the X collide 
            }
            //yz edge 
            else if (((posoutcode & BOX_Y) != 0) && ((posoutcode & BOX_Z) != 0))
            {
                if (avy < avz)
                    posoutcode &= ~BOX_Z; //Ignore the Z collide 
                else
                    posoutcode &= ~BOX_Y; //Ignore the Y collide 
            }
            //Now do the bounce by changing one velocity component.  Rather than flipping  
            //the sign of the velocity component, I actually use a signed version of the 
            // absolute value to be sure I'm going in the correct direction.
            if ((posoutcode & BOX_LOX) != 0)
                vx = -avx;
            else if ((posoutcode & BOX_HIX) != 0)
                vx = avx;
            else if ((posoutcode & BOX_LOY) != 0)
                vy = -avy;
            else if ((posoutcode & BOX_HIY) != 0)
                vy = avy;
            else if ((posoutcode & BOX_LOZ) != 0)
                vz = -avz;
            else if ((posoutcode & BOX_HIZ) != 0)
                vz = avz;

            velocity.set(vx, vy, vz);
        }

        /* You must feed in a 
            correct posoutcode for reflect to work. */

        public int addBounce(cVector3 position, cVector3 velocity, float bounciness)
        {
            int outcode = BOX_INSIDE;

            position.addassign(velocity);
            /* The basic idea is to reverse appropriate velocity component if you've passed an edge,
        and reflect the motion past the edge into motion away from the edge.  Reflection means
        at the right newx would be _hix - (position.x()-_hix), or 2*_hix - position.x()
        and at the newx would be _lox + (_lox - position.x()), or 2*_lox - position.x(). 
            Now add in the notion of a bounciness between 0.0 and 1.0.  First of all the
        reflected newvel is attentuated to -bounciness*oldvel.  Second, the reflected
        position should take into account the fact that the object is moving slower after
        the reflection, so we get, on the right, 
        newx  = _hix - bounciness*(position.x()-_hix) and on the left 
        newx = _lox + bounciness*(_lox - position.x()) or
        newx = 	_lox - bounciness*(position.x() - _lox)*/

            if (position.X >= _hix)
            {
                velocity.set(-bounciness * velocity.X, velocity.Y);
                position.set(_hix - bounciness * (position.X - _hix), position.Y);
                outcode |= BOX_HIX;
            }
            if (position.X <= _lox)
            {
                velocity.set(-bounciness * velocity.X, velocity.Y);
                position.set(_lox - bounciness * (position.X - _lox), position.Y);
                outcode |= BOX_LOX;
            }
            if (position.Y >= _hiy)
            {
                velocity.set(velocity.X, -bounciness * velocity.Y);
                position.set(position.X, _hiy - bounciness * (position.Y - _hiy));
                outcode |= BOX_HIY;
            }
            if (position.Y <= _loy)
            {
                velocity.set(velocity.X, -bounciness * velocity.Y);
                position.set(position.X, _loy - bounciness * (position.Y - _loy));
                outcode |= BOX_LOY;
            }
            if (position.Z >= _hiz)
            {
                velocity.set(velocity.X, velocity.Y, -bounciness * velocity.Z);
                position.set(position.X, position.Y, _hiz - bounciness * (position.Z - _hiz));
                outcode |= BOX_HIZ;
            }
            if (position.Z <= _loz)
            {
                velocity.set(velocity.X, velocity.Y, -bounciness * velocity.Z);
                position.set(position.X, position.Y, _loz - bounciness * (position.Z - _loz));
                outcode |= BOX_LOZ;
            }
            if (outcode != BOX_INSIDE)
                clamp(position); /* Just so some screwy high velocity bounce can't 
				leave you outside the box */
            return outcode;
        }


        /*add velocity to position, but if you pass the border, then
                reflect velocity and get a reflected position. */

        public int addBounce(cVector3 position, ref cVector3 velocity, float bounciness, float dt)
        {
            if (Math.Abs(dt) < 0.00001f)
                return BOX_INSIDE; //You're not moving.
            cVector3 dtvelocity = velocity.mult(dt);
            int outcode = addBounce(position, dtvelocity, bounciness);
            velocity = dtvelocity.div(dt);
            return outcode;
        }


        /* add dt*velocity to position, and do the bounce to velocity the same. */

        public int wrap(cVector3 position)
        {
            /* There's a slight chance you might have a very thin world where a critter
    has raced out so far past the edge that if you wrap it, then the wrapped position
    is still past the edge. This would be particulary likely if you have a zero
    zsize cRealBox3.  In this wrap code, we call our CLAMP macro after 
    finding our wrapped positions. */
            int outcode = BOX_INSIDE;
            float newx, newy, newz;
            newx = position.X; newy = position.Y; newz = position.Z;
            if (position.X <= _lox)
            {
                outcode |= BOX_LOX;
                newx = _hix - _lox + position.X;
                if (newx < _lox)
                    newx = _lox;
                else if (newx > _hix)
                    newx = _hix;
            }
            else if (position.X >= _hix)
            {
                outcode |= BOX_HIX;
                newx = _lox - _hix + position.X;
                if (newx < _lox)
                    newx = _lox;
                else if (newx > _hix)
                    newx = _hix;
            }
            if (position.Y <= _loy)
            {
                outcode |= BOX_LOY;
                newy = _hiy - _loy + position.Y;
                if (newy < _loy)
                    newy = _loy;
                else if (newy > _hiy)
                    newy = _hiy;
            }
            else if (position.Y >= _hiy)
            {
                outcode |= BOX_HIY;
                newy = _loy - _hiy + position.Y;
                if (newy < _loy)
                    newy = _loy;
                else if (newy > _hiy)
                    newy = _hiy;
            }
            if (position.Z <= _loz)
            {
                outcode |= BOX_LOZ;
                newz = _hiz - _loz + position.Z;
                if (newz < _loz)
                    newz = _loz;
                else if (newz > _hiz)
                    newz = _hiz;
            }
            else if (position.Z >= _hiz)
            {
                outcode |= BOX_HIZ;
                newz = _loz - _hiz + position.Z;
                if (newz < _loz)
                    newz = _loz;
                else if (newz > _hiz)
                    newz = _hiz;
            }
            position.set(newx, newy, newz);
            return outcode;
        }

        /* If you move off one edge,
            then come back in the same amount from the other side */

        public int wrap(cVector3 position, cVector3 wrapposition1, cVector3 wrapposition2,
            cVector3 wrapposition3, float radius)
        {
            wrapposition3.copy(position);
            wrapposition2.copy(position);
            wrapposition1.copy(position);
            int outcode = wrap(position);
            cRealBox3 smallbox = this.innerBox(radius);
            //First wrap the x 
            if (position.X <= smallbox._lox)
            {
                outcode |= BOX_LOX;
                wrapposition1.set(_hix - _lox + position.X, position.Y, position.Z);
            }
            if (position.X >= smallbox._hix)
            {
                outcode |= BOX_HIX;
                wrapposition1.set(_lox - _hix + position.X, position.Y, position.Z);
            }
            //If there's no x wrap, wrap the y for wrapposition1 
            if (wrapposition1.equal(position))
            {
                if (position.Y <= smallbox._loy)
                {
                    outcode |= BOX_LOY;
                    wrapposition1.set(position.X, _hiy - _loy + position.Y, position.Z);
                }
                if (position.Y >= smallbox._hiy)
                {
                    outcode |= BOX_HIY;
                    wrapposition1.set(position.X, _loy - _hiy + position.Y, position.Z);
                }
            }
            //If there's x wrap in wrapposition1, do the y wrap of position and wrapposition1 
            else //wrapposition1 not equal to position 
            {
                if (position.Y <= smallbox._loy)
                {
                    outcode |= BOX_LOY;
                    wrapposition2.set(position.X, _hiy - _loy + position.Y, position.Z);
                    wrapposition3.set(wrapposition1.X, _hiy - _loy + position.Y, position.Z);
                }
                if (position.Y >= smallbox._hiy)
                {
                    outcode |= BOX_HIY;
                    wrapposition2.set(position.X, _loy - _hiy + position.Y, position.Z);
                    wrapposition3.set(wrapposition1.X, _loy - _hiy + position.Y, position.Z);
                }
            }
            if (position.Z <= _loz)
            {
                outcode |= BOX_LOZ;
                position.set(position.X, position.Y, _hiz - _loz + position.Z);
            }
            if (position.Z >= _hiz)
            {
                outcode |= BOX_HIZ;
                position.set(position.X, position.Y, _loz - _hiz + position.Z);
            }
            return outcode;
        }

        /* If you are within
            radius of an edge, put the possible wrap values in wrappositions. */

        public int clamp(cVector3 position)
        {
            int outcode = BOX_INSIDE;
            if (position.X < _lox)
            {
                outcode |= BOX_LOX;
                position.set(_lox, position.Y, position.Z);
            }
            if (position.X > _hix)
            {
                outcode |= BOX_HIX;
                position.set(_hix, position.Y, position.Z);
            }
            if (position.Y < _loy)
            {
                outcode |= BOX_LOY;
                position.set(position.X, _loy, position.Z);
            }
            if (position.Y > _hiy)
            {
                outcode |= BOX_HIY;
                position.set(position.X, _hiy, position.Z);
            }
            if (position.Z < _loz)
            {
                outcode |= BOX_LOY;
                position.set(position.X, position.Y, _loz);
            }
            if (position.Z > _hiz)
            {
                outcode |= BOX_HIY;
                position.set(position.X, position.Y, _hiz);
            }
            return outcode;
        }

        /*Make sure position is inside*/

        public int clamp(cVector3 position, cVector3 velocity)
        {
            int outcode = clamp(position);
            if ((outcode & BOX_LOX) != 0) //If you hit a LO X wall, zero out any neg X velocity.
                velocity.set(Math.Max(0.0f, velocity.X), velocity.Y);
            if ((outcode & BOX_HIX) != 0) //If you hit an HI X wall, zero out any pos X velocity.
                velocity.set(Math.Min(0.0f, velocity.X), velocity.Y);
            if ((outcode & BOX_LOY) != 0) //Same deal for Y.
                velocity.set(velocity.X, 0.0f); //__max(0.0, velocity.y()) ); 
            if ((outcode & BOX_HIY) != 0)
                velocity.set(velocity.X, Math.Min(0.0f, velocity.Y));
            if ((outcode & BOX_LOZ) != 0) //Same deal for Z.
                velocity.set(velocity.X, velocity.Y, Math.Max(0.0f, velocity.Z));
            if ((outcode & BOX_HIZ) != 0)
                velocity.set(velocity.X, velocity.Y, Math.Min(0.0f, velocity.Z));
            return outcode;
        }

        /*Make sure position is inside
            and kill any velocity in a wall-hitting direction. */

        public virtual void draw(cGraphics pgraphics, int drawflags)
        {
            /*Draw a basic unfilled rectangle at z=0. */
            cRealBox2 pgbox = new cRealBox2();
            pgbox.copy(this);
            pgbox.draw(pgraphics, drawflags);
        }


        public cRealBox3 box3mult(float scale, cRealBox3 box)
        {
            return new cRealBox3(box.Center, scale * box.XSize,
                scale * box.YSize, scale * box.ZSize);
        }

        /* Return
            a box scale times as big as box. */

        public bool box3equal(cRealBox3 brect)
        {
            // does not check 3 dimensions in Rucker's code, I left it the same -- JC 
            return (_lox == brect._lox && _loy == brect._loy &&
                _hix == brect._hix && _hiy == brect._hiy);
        }


        public bool box3notequal(cRealBox3 brect)
        { return !box3equal(brect); }

        public virtual float Lox
        {
            get
                { return _lox; }
        }

        public virtual float Hix
        {
            get
                { return _hix; }
        }

        public virtual float Loy
        {
            get
                { return _loy; }
        }

        public virtual float Hiy
        {
            get
                { return _hiy; }
        }

        public virtual float Loz
        {
            get
                { return _loz; }
        }

        public virtual float Hiz
        {
            get
                { return _hiz; }
        }

        public virtual cVector3 LoCorner
        {
            get
                { return new cVector3(_lox, _loy, _loz); }
        }

        public virtual cVector3 HiCorner
        {
            get
                { return new cVector3(_hix, _hiy, _hiz); }
        }

        public virtual float Midx
        {
            get
                { return (_hix + _lox) * 0.5f; }
        }

        public virtual float Midy
        {
            get
                { return (_hiy + _loy) * 0.5f; }
        }

        public virtual float Midz
        {
            get
                { return (_hiz + _loz) * 0.5f; }
        }

        public virtual cVector3 Center
        {
            get
                { return new cVector3(Midx, Midy, Midz); }
        }

        public virtual float XSize
        {
            get
                { return _hix - _lox; }
        }

        public virtual float YSize
        {
            get
                { return _hiy - _loy; }
        }

        public virtual float ZSize
        {
            get
                { return _hiz - _loz; }
        }

        public virtual float MinSize
        {
            get
            {
                if (ZSize != 0.0f)
                    return Math.Min(Math.Min(XSize, YSize), ZSize);
                else
                    return Math.Min(XSize, YSize);
            }
        }

        public virtual float MaxSize
        {
            get
                { return Math.Max(Math.Max(XSize, YSize), ZSize); }
        }

        public virtual float XRadius
        {
            get
                { return (_hix - _lox) * 0.5f; }
        }

        public virtual float YRadius
        {
            get
                { return (_hiy - _loy) * 0.5f; }
        }

        public virtual float ZRadius
        {
            get
                { return (_hiz - _loz) * 0.5f; }
        }

        public virtual float Radius
        {
            get
                { return (float)Math.Sqrt(XRadius * XRadius + YRadius * YRadius + ZRadius * ZRadius); }
        }

        public virtual float AverageRadius
        {
            get
                { return (XRadius + YRadius + ZRadius) * 0.33333333f; }
        }


    }
}

	
//                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                         