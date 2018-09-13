using System;
using System.Drawing;

namespace ACFramework
{

    // begin Rucker's comment
    /*	
        1994.  The cPolygon class originally maintained a polygon as a "hand-made"
    resizable array of cVector vertices.  It maintained an equally large hand-made
    resizable array of Point pixel vertices as to support a 
    realToPixel(CRealPixelConverter &) function which is called before using a
    draw(CDC *) function to put the polygon on the screen.  The  Point _pointvert
    array is used only in realToPixel and draw, we don't maintain its correctness
    as we go along, only focussing on keeping the correct geometry of the polygon in
    the cVector array.
        June 20, 1999.  The hand-made arrays were replaced with CArrays. The "dot"
    feature was added to draw circles at the vertices.  Note that the dots as
    implemented would not have the correct appearance for representing cPolygon3
    projections. RR.
        August 4, 1999.  Changed it to a child of the cSprite class.
        June, 2001.  Gave it a general cVector which can be thought of as three-dimenisonal.
    */
    // end Rucker's comment

    // begin Childs comment
    /*  
		June, 2008.  Converted to C# and used a LinkedList for CArrays.
     * 
     * The cPolygon class can be used for 3-dimensional objects.  An example of this 
     * is found in cCritterTreasure.  However, the hardware platform determines 
     * whether or not filling polygons with colors is successful.  It is not 
     * successful on my computer, and I can only see it if I disable my graphics
     * accelerator and use software for graphics, but this really kills my frames
     * per second.  Realizing that other hardware platforms will probably have the
     * same issues with OpenGL, I changed the bullets in critterarmed.cs from
     * their original polygon versions to sphere versions, which worked out well.
     * This class is still useful for making 3-dimensional wireframes for such
     * platforms. -- JC
     */

    // For AC Framework 1.2, default parameters were added

    // end Childs comment


    //--------------TWO DIMENSIONAL--------------

    class cPolygon : cSprite
    {
        //const static readonlys  
        public static readonly int MF_COLOR = 0x00000020;
        public static readonly int MF_FILLING = 0x00000040;
        public static readonly int MF_LINEWIDTH = 0x00000080;
        public static readonly int MF_DOTS = 0x00000100;
        public static readonly int MF_VERTCOUNT = 0x00000200;
        public new static readonly int MF_ALL = cPolygon.MF_COLOR | cPolygon.MF_FILLING |
            cPolygon.MF_LINEWIDTH | cPolygon.MF_DOTS | cPolygon.MF_VERTCOUNT;
        //Special cPolygon variables 
        //Main array field and helper array field 
        protected LinkedList<cVector3> _vectorvert;
        protected LinkedList<cVector3> _transformedvert; /* We use this only in the transformedVert(Matrix)
			call.  We don't need to serialize it, as it is strictly a helper that gets updated before
			each use. */
        protected bool _convex;
        //Special Polygon Data fields about dot.
        protected bool _dotted;
        protected cColorStyle _pdotcolorstyle;
        protected float _realdotradiusweight; //Ratio of vertex marker dot size to _polygonradius.
        /*	DON'T put in a Real _radius or Real _angle!  C++ will let you do
        this even though these fields exist in cSprite, but what a mistake.  If you
        put a second _radius in here, then the cPolygon methods change the
        cPolygon _radius, but the cSprite::radius() accessor returns the cSprite
        _radius, which will still be 0.0 */
        //Private helper functions 

        protected void _initializer() //used by the constructors 
        {
            //The two CArrays are set to size 0 by their default constructors 
            //Nine decoration fields 
            _dotted = false;
            _pdotcolorstyle = new cColorStyle();
            _pdotcolorstyle.FillColor = Color.Yellow;
            _realdotradiusweight = 0.05f;
            // helper fields 
            _convex = true;
            _vectorvert = new LinkedList<cVector3>(
                delegate(out cVector3 v1, cVector3 v2)
                {
                    v1 = new cVector3();
                    v1.copy(v2);
                }
                );
            _transformedvert = new LinkedList<cVector3>(
                delegate(out cVector3 v1, cVector3 v2)
                {
                    v1 = new cVector3();
                    v1.copy(v2);
                }
                );
        }

        //Used by the two constructors.

        protected void _fixConvex()
        {
            // I rewrote this to make it efficient for a linked list 
            // First, test a bunch using a foreach -- JC
            bool first = true;
            bool second = true;
            cVector3 v1 = new cVector3();
            cVector3 v2 = new cVector3();
            float test;
            foreach (cVector3 v3 in _vectorvert)
            {
                if (!first && !second)
                {
                    test = ((v2.sub(v1)).mult((v3.sub(v1)))).Z;
                    /* Take the cross product and look at the z component.
                If it is positive, the cross product points up, so this is 
                counterclockwise rotation and you aren't convex. */
                    if (test < 0)
                    {
                        _convex = false;
                        break;
                    }
                    v1.copy(v2);
                    v2.copy(v3);
                }
                else if (first)
                {
                    v1.copy(v3);
                    first = false;
                }
                else // second
                {
                    v2.copy(v3);
                    second = false;
                }
            }

            // finally, do two special tests -- JC
            cVector3 last = _vectorvert[0];

            test = ((v2.sub(v1)).mult((last.sub(v1)))).Z;
            if (test < 0)
            {
                _convex = false;
                return;
            }

            v1.copy(v2);
            v2.copy(last);
            last = _vectorvert[1];
            test = ((v2.sub(v1)).mult((last.sub(v1)))).Z;
            if (test < 0)
                _convex = false;
        }


        //Constructor, destructor, operator= 

        public cPolygon()
        {
            _initializer();
        }

        //Default constructor calls initializer 

        public cPolygon(int vertcount)
        {
            _initializer();
            setRegularPolygon(vertcount, new cVector3(0.0f, 0.0f), 1.0f, 0.0f);
        }


        //Makes a default regular polygon with n verts.

        public cPolygon(int n, cVector3[] pverts, cColorStyle pcolorstyle = null)
        {
            _initializer();
            for (int i = 0; i < n; i++)
                _vectorvert.Add(pverts[i]);
            if (pcolorstyle != null)
            {
                cColorStyle c = new cColorStyle();
                c.copy(pcolorstyle);
                ColorStyle = c; //Otherwise keep the default.
            }
            fixCenterAndRadius();
        }

        /* Useful for wrapping 
            verts in a polygon class to pass to pgraphics->draw. */
        //Overloaded cSprite methods 

        public override void copy(cSprite psprite) //Use this in copy constructor and operator= 
        {
            /* Because our class has some CArray fields, we can't use the default overloaded
        copy constructor and operator=.  So as to avoid having to maintain similar code
        for these two different methods, we write a helper copy function that both
        the copy constructor and the operator= can use. */
            base.copy(psprite); //does center(), _radius, _angle, _rotationspeed.
            if (!psprite.IsKindOf("cPolygon"))
                return; //You're done if psprite isn't a cPolygon*.
            cPolygon ppolygon = (cPolygon)(psprite); /* I know it is a cPolygon
			at this point, but I need to do a cast, so the compiler will let me
			call a bunch of cPolygon methods. */
            //Arrays 
            _vectorvert.Copy(ppolygon._vectorvert);
            //Decoration fields
            cColorStyle c = new cColorStyle();
            c.copy(ppolygon.pcolorstyle());
            ColorStyle = c;
            cColorStyle c2 = new cColorStyle();
            c2.copy(ppolygon.DotColorStyle);
            DotColorStyle = c2;
            _dotted = ppolygon._dotted;
            _realdotradiusweight = ppolygon._realdotradiusweight;
            //Helper fields 
            _convex = ppolygon._convex;
        }

        public override cSprite copy()
        {
            cPolygon p = new cPolygon();
            p.copy(this);
            return p;
        }

        public override bool IsKindOf(string str)
        {
            return (str == "cPolygon") || base.IsKindOf(str);
        }

        /* Checks if csprite is a cPolygon,
            and, if so, copies all the fields.  */

        public static int max(int a, int b)
        {
            return (a > b) ? a : b;
        }

        public static int min(int a, int b)
        {
            return (a < b) ? a : b;
        }

        public override void mutate(int mutationflags, float mutationstrength)
        {
            base.mutate(mutationflags, mutationstrength);
            //Also mutates pcolorstyle fields.
            /* Mutates _radius
                and calls setRadius, which does a scaling operation. */
            _pdotcolorstyle.mutate(mutationflags, mutationstrength);
            //The dots 
            if ((mutationflags & MF_DOTS) != 0)
            {
                Dotted = _dotted ^ Framework.randomOb.randomBOOL(mutationstrength / 2.0f);
                _realdotradiusweight =
                    Framework.randomOb.mutate(_realdotradiusweight, 0.2f, 0.4f, mutationstrength);
            }
            //Number of vertices 
            if ((mutationflags & MF_VERTCOUNT) != 0)
            {
                int newcount;
                if (vertCount() == 0)
                { //Creating a new polygon  
                    if (Framework.randomOb.randomBOOL(0.5f))
                        setRegularPolygon(Framework.randomOb.random(2, 9), Center, _radius, Angle);
                    else
                    {
                        newcount = Framework.randomOb.random(5, 14);
                        setRandomStarPolygon(newcount, newcount);
                    }
                }
                else if (Framework.randomOb.randomBOOL(mutationstrength)) //Mutate an existing one 
                {
                    int minsides = max(2, vertCount() - 2);
                    int maxsides = min(9, vertCount() + 2);
                    if (Framework.randomOb.randomBOOL(0.5f))
                    {
                        newcount = Framework.randomOb.random(minsides, maxsides);
                        setRegularPolygon(newcount, Center, _radius, Angle );
                    }
                    else
                    {
                        minsides = max(5, vertCount() - 2);
                        maxsides = min(14, vertCount() + 2);
                        newcount = Framework.randomOb.random(minsides, maxsides);
                        setRandomStarPolygon(newcount, newcount);
                    }
                }
            }
            //Fixups 
            if (vertCount() == 2) 	//Don't want any plain old line segments.
                Dotted = true;
            Radius = _radius; /* Set the radius last, as it depends on the line width, 
			dots, etc. */
            _newgeometryflag = true;
        }


        //Mutators 

        public void fixCenterAndRadius()
        {
            //Calculate the centroid 
            /* We plan to divide by vertCount, so don't allow it to be zero. */
            if (vertCount() == 0)
                return;
            cVector3 centroid = new cVector3(); //Default constuctor starts at zeroVector.
            foreach (cVector3 v in _vectorvert)
                centroid.addassign(_vectorvert.ElementAt());
            centroid.divassign(vertCount()); // We already made sure this divisor isn't zero.
            //Move the centroid to the origin 
            foreach (cVector3 v in _vectorvert)
                _vectorvert.ElementAt().subassign(centroid);
            //Fix Radius as furthest vertex from the origin.
            float distance = 0.0f; //Start with this and look for the biggest one.
            float testdistance;
            foreach (cVector3 v in _vectorvert)
            {
                testdistance = v.Magnitude;
                if (testdistance > distance)
                    distance = testdistance;
            }
            _radius = distance; //_radius is a raw number.
            _fixConvex();
        }

        /* Helper function calculates the centroid as the
            average of the vertices, moves the centroid the origin, and sets the _radius as the
            max distance of a vertex from the origin. */

        //Allocates arrays, preserves old info.

        public void setVertex(int n, cVector3 vect)
        {
            if (vertCount() < n)
            {
                while (vertCount() < n)
                    _vectorvert.Add(new cVector3());
                _vectorvert.Add(vect);
            }
            else if (vertCount() == n)
                _vectorvert.Add(vect);
            else
            {
                int i = 0;
                foreach (cVector3 v in _vectorvert)
                {
                    if (i == n)
                    {
                        _vectorvert.ElementAt().copy(vect);
                        break;
                    }
                    i++;
                }
            }
            _newgeometryflag = true;
        }


        public void addVertex(cVector3 vect)
        {
            _vectorvert.Add(vect);
            _newgeometryflag = true;
        }


        public void changeVertexcount(int updown)
        {
            int newvertcount = _vectorvert.Size + updown;
            if (newvertcount < 2)
                newvertcount = 2;
            setRegularPolygon(newvertcount, Center, Radius, Angle);
            _newgeometryflag = true;
        }

        //Add updown to vert count.

        /*If you don't specify the center, radius, initangle arguments for
        RegularPolygon & StarPolygon, use default arguments */

        public void setRegularPolygon(int vertexcount)
        {
            float angle = 0.0f;
            _vectorvert.RemoveAll();
            for (int i = 0; i < vertexcount; i++)
            {
                _vectorvert.Add(new cVector3((float)Math.Cos(angle), (float)Math.Sin(angle)));
                angle += (2 * (float)Math.PI) / vertexcount;
            }
            _convex = true;
            Center = new cVector3(0.0f, 0.0f);
            _radius = 1.0f;
            Radius = _radius;
            _newgeometryflag = true;
        }

        public void setRegularPolygon(int vertexcount, cVector3 center,
            float newradius = 1.0f, float initangle = 0.0f)
        {
            float angle = initangle;
            _vectorvert.RemoveAll();
            for (int i = 0; i < vertexcount; i++)
            {
                _vectorvert.Add(center.add(
                    (new cVector3((float)Math.Cos(angle), (float)Math.Sin(angle))).mult(newradius)));
                angle += (2 * (float)Math.PI) / vertexcount;
            }
            _convex = true;
            Center = center;
            _radius = newradius;
            Radius = _radius;
            _newgeometryflag = true;
        }

        public void setStarPolygon(int vertexcount, float dentpercent)
        {
            _vectorvert.RemoveAll();
            float angle = 0.0f;
            float anglestep = (float)Math.PI / vertexcount;
            bool tipvertex = true;
            float innerradius = dentpercent;
            float currentradius = 1.0f;
            for (int i = 0; i < 2 * vertexcount; i++)
            {
                if (tipvertex)
                    currentradius = 1.0f;
                else
                    currentradius = innerradius;
                _vectorvert.Add((new cVector3((float)Math.Cos(angle), (float)Math.Sin(angle))).mult(currentradius));
                angle += anglestep;
                tipvertex = !tipvertex;
            }
            Center = new cVector3(0.0f, 0.0f);
            Radius = 1.0f;
            _newgeometryflag = true;
        }

        public void setStarPolygon(int vertexcount, float dentpercent, cVector3 center,
            float newradius = 1.0f, float initangle = 0.0f)
        {
            _vectorvert.RemoveAll();
            float angle = initangle;
            float anglestep = (float)Math.PI / vertexcount;
            bool tipvertex = true;
            float innerradius = newradius * dentpercent;
            float currentradius = newradius;
            for (int i = 0; i < 2 * vertexcount; i++)
            {
                if (tipvertex)
                    currentradius = newradius;
                else
                    currentradius = innerradius;
                _vectorvert.Add(center.add(
                    (new cVector3((float)Math.Cos(angle), (float)Math.Sin(angle))).mult(currentradius)));
                angle += anglestep;
                tipvertex = !tipvertex;
            }
            Center = center;
            Radius = newradius;
            _newgeometryflag = true;
        }


        public void setRandomStarPolygon(int mincount, int maxcount)
        {
            setStarPolygon(Framework.randomOb.random(mincount, maxcount),
                Framework.randomOb.randomReal(0.25f, 0.9f), Center,
                _radius, Angle);
            _newgeometryflag = true;
        }


        public void setRandomRegularPolygon(int mincount, int maxcount)
        {
            setRegularPolygon(Framework.randomOb.random(mincount, maxcount), Center,
                _radius, Angle);
            _newgeometryflag = true;
        }


        public void setRandomAsteroidPolygon(int mincount = 5, int maxcount = 30,
            float spikiness = 0.3f)
        {
            float angle = 0.0f, anglestep;
            int vertexcount = Framework.randomOb.random(mincount, maxcount);
            _vectorvert.RemoveAll();
            anglestep = 2 * (float)Math.PI / vertexcount;
            int i;
            for (i = 0; i < vertexcount && angle < 2.0f * (float)Math.PI; i++)
            {
                _vectorvert.Add(
                    (new cVector3((float)Math.Cos(angle), (float)Math.Sin(angle))).mult(Framework.randomOb.randomReal(1.0f - spikiness, 1.0f + spikiness)));
                angle += Framework.randomOb.randomReal(0.0f, 2.0f * anglestep);
            }
            fixCenterAndRadius();
            Radius = _radius;
            _newgeometryflag = true;
        }


        //Accessors 

        public int vertCount() { return _vectorvert.Size; } /*the const means the
			funtion doesn't change the class. I need this because some
			functions that	call (const cPolygon3 &p) use the cPolygon::getSize function.*/

        public cVector3 getVertex(int n)
        {
            return _vectorvert.ElementAt(n);
        }


        //Matrix methods.

        public LinkedList<cVector3> transformedVert(cMatrix3 M)
        {
            _transformedvert.RemoveAll();
            foreach (cVector3 v in _vectorvert)
                _transformedvert.Add(M.mult(v));
            return _transformedvert;
        }

        /* Fix our member array
            _transformedvert so it holds all the M*vert from the _vectorvert array, and then
            return a pointer to _transformedvert.  We can't return a copy of this array because
            MFC doesn't supply a CArray copy constructor. This method is needed by the
            cGraphicsMFC::draw(cPolygon) call. */
        //cSprite overloads 

        public override void imagedraw(cGraphics pgraphics, int drawflags)
        {
            if (_convex)
                pgraphics.drawpolygon(this, drawflags);
            else
                pgraphics.drawstarpolygon(this, drawflags);
        }


        public override bool UsesSmoothing
        {
            get
            { return false; }
        }

        public override float Angle
        {
            get
            {
                if (vertCount() == 0)
                    return 0.0f;
                return _vectorvert[0].sub(Center).angleBetween(new cVector3(1.0f, 0.0f, 0.0f));
            }
        }

        public override float Radius
        {
            set
            {
                fixCenterAndRadius(); /* In case you haven't done this yet, for instance if you've built
			the polygon with a bunch of setVertex or addVertex calls and you are now calling setRadius. */
                if (value == 0.0f)
                    return; //Don't allow zero radius.
                float scalefactor = value / Radius;
                foreach (cVector3 v in _vectorvert)
                    _vectorvert.ElementAt().multassign(scalefactor); // Centroid is origin so we can scale around it.
                _radius = scalefactor * _radius; /* _radius measures the newly scaled geometry of the
			polygon around the origin.  Note  value = spriteattitude._scalefactor()*_radius
			rather than value = _radius, although quite often the scalefactor is in fact 1.0. */
                NewGeometryFlag = true;
            }
        }

        public override float LineWidthWeight
        {
            set
            {
                _pcolorstyle.LineWidthWeight = value;
                _newgeometryflag = true;
            }
        }

        public virtual cColorStyle DotColorStyle
        {
            get
            { return _pdotcolorstyle; }
            set
            {
                _pdotcolorstyle = new cColorStyle();
                _pdotcolorstyle.copy(value);
                _newgeometryflag = true;
            }
        }

        public virtual bool Dotted
        {
            get
            { return _dotted; }
            set
            {
                _dotted = value;
                _newgeometryflag = true;
            }
        }

        public virtual float DotRadiusWeight
        {
            get
            { return _realdotradiusweight; }
            set
            {
                _realdotradiusweight = value;
                _newgeometryflag = true;
            }
        }

        public virtual bool Convex
        {
            get
            { return _convex; }
        }

        public virtual int VertCount
        {
            get
            { return _vectorvert.Size; }
        }

        public virtual float DotRadius
        {
            get
            { return _realdotradiusweight * _radius; }
        }


    }

    class cPolyPolygon : cSpriteComposite
    {
        public static readonly float MIN_TIP_RADIUS_RATIO = 0.2f;
        public static readonly float MAX_TIP_RADIUS_RATIO = 0.5f;
        public static readonly float MINTIPANGLEVELOCITY = 1.0f;
        public static readonly float MAXTIPANGLEVELOCITY = 4.0f;
        public static readonly float TIPPRISMDZMULTIPLIER = 1.3f; /* Make the tips have slightly larger prismdz. */
        protected float _tipangvelocity;

        public cPolyPolygon()
        {
            _tipangvelocity = 0.0f;
        } /* Start out with no basepoly and no tipshape, user must
			fix this with a call to mutate or to setBasePoly and setTipShape. */

        public cPolyPolygon(int baseverts, int tipverts)
        {
            BasePoly = new cPolygon(baseverts);
            TipShape = new cPolygon(tipverts);
            randomize(cSprite.MF_RADIUS | cPolygon.MF_COLOR);
            //Randomize the tip rotations.
            _tipangvelocity = Framework.randomOb.randomReal(cPolyPolygon.MINTIPANGLEVELOCITY,
                cPolyPolygon.MAXTIPANGLEVELOCITY);
            _tipangvelocity *= Framework.randomOb.randomSign();
        }

        public override cSprite copy()
        {
            cPolyPolygon p = new cPolyPolygon();
            p.copy(this);
            return p;
        }

        public override bool IsKindOf(string str)
        {
            return (str == "cPolyPolygon") || base.IsKindOf(str);
        }

        /* setTipShape puts a clone of pshape at each vertex of the
            pbasepoly(), and then it deletes pshape.  So don't use a pshape as argument that you need to save. */
        //cSprite overloads 

        /* Overload the standard cSpriteComposite radius() method. */

        public override void mutate(int mutationflags, float mutationstrength)
        {
            if (BasePoly == null)
                add(new cPolygon());
            BasePoly.mutate(mutationflags, mutationstrength);
            cSprite ptipshapenew;
            if (TipShape != null)
                ptipshapenew = TipShape.copy(); //Use this as a model 
            else
                ptipshapenew = new cPolygon();
            ptipshapenew.mutate(mutationflags, mutationstrength);
            //Make the tip shape have smaller radius than the basepoly.
            ptipshapenew.Radius = Framework.randomOb.mutate(ptipshapenew.Radius,
                MIN_TIP_RADIUS_RATIO * BasePoly.Radius, MAX_TIP_RADIUS_RATIO * BasePoly.Radius,
                mutationstrength);
            //Randomize the tip rotations.
            _tipangvelocity = Framework.randomOb.randomSign() * Framework.randomOb.mutate(_tipangvelocity, cPolyPolygon.MINTIPANGLEVELOCITY,
                cPolyPolygon.MAXTIPANGLEVELOCITY, mutationstrength);
            //Make all the tips the same.
            TipShape = ptipshapenew; //This deletes ptipshapenew 
            //More fixup 
            _newgeometryflag = true; //Because you changed the relative positions of the tips.
        }


        public override void animate(float dt, cCritter powner)
        {
            bool first = true;
            foreach (cSprite s in _childspriteptr)
            { //Tp oOnly rotate the tip shapes and not base, start with 2nd element
                if (!first)
                    _childspriteptr.ElementAt().rotate(dt * _tipangvelocity); /* this multiplies 
                          _spriteattitude by the given zRotation amount on the right. */
                else
                    first = false;
            }
        }

        public virtual cPolygon BasePoly
        {
            get
            {
                if (_childspriteptr.Size == 0)
                    return null;
                return (cPolygon)(_childspriteptr[0]);
            }
            set
            {
                cSprite ptipshapeold = null;
                if (TipShape != null)
                    ptipshapeold = TipShape.copy();
                _childspriteptr.RemoveAll();
                add(value);
                TipShape = ptipshapeold;
            }
        }

        public virtual cSprite TipShape
        {
            get
            {
                if (_childspriteptr.Size < 2)
                    return null;
                return _childspriteptr[1];
            }

            set
            {
                int i;
                if (BasePoly == null || value == null)
                    return;

                cSprite s = member(0);
                _childspriteptr.RemoveAll();
                add(s);
                float angle = 0.0f, angleincrement = 2 * (float)Math.PI;
                if (BasePoly.vertCount() != 0)
                    angleincrement /= BasePoly.vertCount();
                for (i = 0; i < BasePoly.vertCount(); i++)
                {
                    cSprite ptipshapenew = value.copy();
                    cMatrix3 tipattitude = cMatrix3.zRotation(angle);
                    angle += angleincrement;
                    tipattitude.LastColumn = BasePoly.getVertex(i);
                    ptipshapenew.SpriteAttitude = tipattitude;
                    add(ptipshapenew);
                }
                PrismDz = _prismdz; //Cascade the prismdz out to the tips.
                value = null;
            }
        }

        public override float Radius
        {
            get
            {
                float tempradius = 0.0f;
                float scaledradius;
                if (BasePoly != null)
                    tempradius += BasePoly.Radius;
                if (TipShape != null)
                    tempradius += TipShape.Radius;
                scaledradius = (_spriteattitude.ScaleFactor) * tempradius;
                return scaledradius;
            }
        }

        public override float PrismDz
        {
            set
            {
                /* If the tipshapes are exactly the same thcikness at the baseshape, we have their faces coinciding
            with each otehr, which gives an ugly drawing effect.  So in cPolyPolygon.setPrismDz, we make the
            tipshape thicker than the base shape.  To make it stick out above the base shape on both sides,
            we translate each tipshape downward in the z direction a bit as well. */
                _prismdz = value;
                float scalefactor = _spriteattitude.ScaleFactor;
                float baseprismdz = _prismdz / scalefactor; /* We 
			divide by the scalefactor becuase, for a composite sprite, the
			attitude scales the whole assembly from the "outside," and we want
			the value thickness to be aboslute. */
                BasePoly.PrismDz = baseprismdz;
                float tipprismdz = cPolyPolygon.TIPPRISMDZMULTIPLIER * baseprismdz;
                float tipoffsetdz = 0.5f * (tipprismdz - baseprismdz);
                bool first = true;
                foreach (cSprite s in _childspriteptr)
                {
                    //To only get the tip shapes and not base, start i with 1 not 0.
                    if (!first)
                    {
                        //Make thicker.
                        _childspriteptr.ElementAt().PrismDz = cPolyPolygon.TIPPRISMDZMULTIPLIER * baseprismdz;
                        //Slide so tipshape sticks out equally above and below .
                        cMatrix3 tipattitude = _childspriteptr.ElementAt().SpriteAttitude;
                        tipattitude.ZTranslation = -tipoffsetdz; /* Use this absolute method rather than a
				    relative call to tranlsate(cVector(0,0,-tipoffsetdz) because it's possible I might
				    call setPrismDz more than once on a given sprite, so I don't watn the z corretions
				    to accumulate. */
                        _childspriteptr.ElementAt().SpriteAttitude = tipattitude;
                        /* Slide down the z axis a bit.  Don't try to do this by multiplying in 
                        a cMatrix::translation matrix. */
                    }
                }

            }
        }
        /* makes the tipshapes a bit thicker. */

        public virtual float TipAngleVelocity
        {
            set
                { _tipangvelocity = value; }
        }

    }


    class cSpriteRectangle : cPolygon
    {

        public cSpriteRectangle(float lox = -0.5f, float loy = -0.5f,
            float hix = 0.5f, float hiy = 0.5f)
        {
            /* I used to call _initializer() here, but that was mistake
        as the baseclass cPolygon constructor calls _initializer first.
        Calling _initializer twice made a resrource leak. */
            _vectorvert.Add(new cVector3(lox, loy));
            _vectorvert.Add(new cVector3(hix, loy));
            _vectorvert.Add(new cVector3(hix, hiy));
            _vectorvert.Add(new cVector3(lox, hiy));
            _newgeometryflag = true;
            PrismDz = 0.0f;
            _convex = true;
            fixCenterAndRadius();
        }

        public override cSprite copy()
        {
            cSpriteRectangle r = new cSpriteRectangle();
            r.copy(this);
            return r;
        }

        public override bool IsKindOf(string str)
        {
            return (str == "cSpriteRectangle") || base.IsKindOf(str);
        }


    }
}
//                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   